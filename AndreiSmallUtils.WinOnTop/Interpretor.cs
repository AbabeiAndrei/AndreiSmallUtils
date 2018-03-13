using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using AndreiSmallUtils.Utils;
using AndreiSmallUtils.Utils.Models;
using AndreiSmallUtils.WinOnTop.KeyModel;

namespace AndreiSmallUtils.WinOnTop
{
    public class Interpretor : IInterpretor
    {
        #region Fields

        private readonly IntPtr _consoleHandle;
        protected const int ITEMS_PER_PAGE = 10;
        private int _page;
        private IList<WindowInfo> _windows;

        #endregion

        #region Properties

        private int Pages => (_windows?.Count - 1) / ITEMS_PER_PAGE ?? 0;

        public string OnTopMarker => "*";

        public int? CursorPosition { get; protected set; }
     
        #endregion

        #region Events

        public event EventHandler Closing;

        public event CursorMoveHandler CursorMove;

        public event KeyPressHandler KeyPressed;

        #endregion

        #region Constructros

        public Interpretor(IntPtr consoleHandle)
        {
            _consoleHandle = consoleHandle;
        }

        #endregion

        #region Implementation of IInterpretor

        public virtual IEnumerable<string> GetMenu()
        {
            if (_windows == null)
                _windows = WindowsUtils.GetWindows().ToList();

            var index = 0;

            foreach (var window in _windows.Skip(ITEMS_PER_PAGE * _page).Take(ITEMS_PER_PAGE))
                yield return $" {index++}: {window.Title} {(window.TopMost ? OnTopMarker : "")}";

            yield return string.Empty;

            if(_page > 0)
                yield return "p: Prev page";

            if(_page < Pages)
                yield return "n: Next page";
            
            yield return "r: Refresh windows";
            yield return "q: Quit";
        }

        public virtual bool Response(ConsoleKeyInfo chr)
        {
            var type = GetType();
            var boolType = typeof(bool);

            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                              .Select(mi => new 
                              {
                                  Attributes = mi.GetCustomAttributes().OfType<KeyAttribute>(),
                                  MethodInfo = mi
                              })
                              .Where(a => a.Attributes.SelectMany(ca => ca.Chars).Contains(chr.Key))
                              .OrderBy(a => a.Attributes.Select(ka => ka.Order).DefaultIfEmpty(0).Sum())
                              .Select(a => a.MethodInfo)
                              .ToList();

            if (methods.Count <= 0)
                return false;

            foreach (var method in methods)
            {
                try
                {
                    object[] paramenters;
                    var paramsInfo = method.GetParameters();

                    switch (paramsInfo.Length)
                    {
                        case 0:
                            paramenters = null;
                            break;
                        case 1:
                        case 2:
                            paramenters = paramsInfo.Select(pi => ExtractParameter(chr, pi, method.Name)).ToArray();
                            break;
                        default:
                            throw new MethodAccessException($"Unknow parameters in the method {method.Name}");
                    }


                    var result = method.Invoke(this, paramenters);

                    if (method.ReturnType == boolType && result is bool ret)
                        return ret;
                }
                finally
                {
                    var args = new KeyPressArgs(chr);
                    KeyPressed?.Invoke(this, args);
                    CheckMethodAttachedEvent(method, chr);
                }
            }

            return true;
        }

        private void CheckMethodAttachedEvent(MethodInfo method, ConsoleKeyInfo keyInfo)
        {
            var keyAttributes = method.GetCustomAttributes().OfType<KeyAttribute>()
                               .Where(ka => !string.IsNullOrEmpty(ka.Event));

            var args = new KeyPressArgs(keyInfo);

            foreach (var attribute in keyAttributes)
                this.RaiseEvent(attribute.Event, args);
        }

        private static object ExtractParameter(ConsoleKeyInfo chr, ParameterInfo paramsInfo, string method)
        {
            if (paramsInfo.ParameterType == typeof(char))
                return chr.KeyChar;
            if (paramsInfo.ParameterType == typeof(ConsoleKey))
                return chr.Key;
            if (paramsInfo.ParameterType == typeof(ConsoleKeyInfo))
                return chr;

            throw new MethodAccessException($"Unknow parameters in the method {method}");
        }

        #endregion

        #region Protected methods

        [Key(ConsoleKey.N, ConsoleKey.RightArrow)]
        protected virtual bool Next()
        {
            if(_windows == null || _page >= Pages)
                return false;

            _page++;
            return true;
        }

        [Key(ConsoleKey.P, ConsoleKey.LeftArrow)]
        protected virtual bool Prev()
        {
            if(_page <= 0)
                return false;

            _page--;

            return true;
        }

        [Key(ConsoleKey.R, ConsoleKey.F5)]
        protected virtual void Refresh()
        {
            _windows = null;
        }

        [Key(ConsoleKey.D0, ConsoleKey.D1, ConsoleKey.D2, ConsoleKey.D4, ConsoleKey.D5, ConsoleKey.D6, ConsoleKey.D7, ConsoleKey.D8, 
             ConsoleKey.D9, ConsoleKey.NumPad0, ConsoleKey.NumPad1, ConsoleKey.NumPad2, ConsoleKey.NumPad3, ConsoleKey.NumPad4, 
             ConsoleKey.NumPad5, ConsoleKey.NumPad6, ConsoleKey.NumPad7, ConsoleKey.NumPad8, ConsoleKey.NumPad9)]
        protected virtual bool SelectTop(char win)
        {
            if(_windows == null)
                return false;

            var item = int.Parse(win.ToString());

            return SetWindowOnTop(item);
        }

        [Key(ConsoleKey.Q, ConsoleKey.Escape, Order = 0)]
        protected virtual void Quit(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Escape && keyInfo.Modifiers != ConsoleModifiers.Shift) 
                return;

            Closing?.Invoke(this, EventArgs.Empty);
            Environment.Exit(0);
        }

        [Key(ConsoleKey.Escape, Order = 1)]
        protected virtual bool HideCusor(ConsoleKeyInfo keyInfo)
        {
            if (CursorPosition == null)
                return false;

            var old = CursorPosition;
            CursorPosition = null;

            var args = new CursorInformation(old, CursorPosition,keyInfo);

            return CursorMove?.Invoke(this, args) ?? true;
        }

        [Key(ConsoleKey.UpArrow)]
        protected virtual bool CursorUp(ConsoleKeyInfo keyInfo)
        {
            if (CursorPosition == null)
            {
                CursorPosition = _windows.Skip(_page * ITEMS_PER_PAGE).Take(ITEMS_PER_PAGE)
                                         .Count();
            }

            if (CursorPosition <= 0)
                return false;

            var old = CursorPosition--;

            var args = new CursorInformation(old, CursorPosition,keyInfo);

            return CursorMove?.Invoke(this, args) ?? true;
        }

        [Key(ConsoleKey.DownArrow)]
        protected virtual bool CursorDown(ConsoleKeyInfo keyInfo)
        {
            if (CursorPosition == null)
                CursorPosition = -1;

            var page = _windows.Skip(_page * ITEMS_PER_PAGE).Take(ITEMS_PER_PAGE);

            if (CursorPosition >= page.Count() - 1)
                return false;
            

            var old = CursorPosition++;

            var args = new CursorInformation(old, CursorPosition,keyInfo);

            return CursorMove?.Invoke(this, args) ?? true;
        }
        
        [Key(ConsoleKey.Enter)]
        protected virtual bool SelectItem()
        {
            if(CursorPosition.HasValue)
                SetWindowOnTop(CursorPosition.Value);

            return false;
        }

        #endregion

        #region Private methods

        private bool SetWindowOnTop(int item)
        {
            var page = _windows.Skip(_page * ITEMS_PER_PAGE).Take(item + 1).ToArray();

            if (page.Length <= item)
                return false;

            var window = page[item];

            var location = window.TopMost
                               ? WinApi.HWND_NOTOPMOST
                               : WinApi.HWND_TOPMOST;

            if (!window.TopMost)
                WinApi.SetForegroundWindow(window.Handle);

            WinApi.SetWindowPos(window.Handle, location, 0, 0, 0, 0, WinApi.SWP_NOMOVE | WinApi.SWP_NOSIZE);

            window.TopMost = !window.TopMost;

            if (_consoleHandle != IntPtr.Zero)
                WinApi.SetForegroundWindow(_consoleHandle);

            return true;
        }

        #endregion
    }
}
