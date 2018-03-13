using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using AndreiSmallUtils.Utils;
using AndreiSmallUtils.WinOnTop.KeyModel;

namespace AndreiSmallUtils.WinOnTop
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = Assembly.GetExecutingAssembly().GetName().Name;
            Console.OutputEncoding = Encoding.UTF8;

            var process = Process.GetCurrentProcess();

            var interpretor = new Interpretor(process.MainWindowHandle);

            interpretor.CursorMove += InterpretorOnCursorMove;
            //interpretor.KeyPressed += InterpretorOnKeyPressed;

            ConsoleKeyInfo? key = null;

            while (true)
            {
                Console.Clear();
                foreach (var menuItem in interpretor.GetMenu())
                    WriteMenuItem(menuItem, interpretor);

                Console.Write("Input > ");
                var continueLoop = false;
                
                if(key.HasValue)
                    InterpretorOnKeyPressed(interpretor, new KeyPressArgs(key.Value));

                while (!continueLoop)
                {
                    key = Console.ReadKey(true);
                    continueLoop = interpretor.Response(key.Value);
                    InterpretorOnKeyPressed(interpretor, new KeyPressArgs(key.Value));
                }
            }
        }

        private static void WriteMenuItem(string menuItem, Interpretor interpretor)
        {
            if (menuItem.EndsWith(interpretor.OnTopMarker))
                Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(menuItem.ReplaceLast(interpretor.OnTopMarker));
            if (Console.ForegroundColor != ConsoleColor.White)
                Console.ForegroundColor = ConsoleColor.White;
        }

        private static bool InterpretorOnCursorMove(IInterpretor sender, ICursorInformation args)
        {
            var orgPosition = (Console.CursorLeft, Console.CursorTop);

            if (args.OldCursorPosition.HasValue && args.OldCursorPosition.Value >= 0)
            {
                Console.SetCursorPosition(0, args.OldCursorPosition.Value);
                WriteMenuItem(' ' + sender.GetMenu().ElementAt(args.OldCursorPosition.Value)?.TrimStart(), sender as Interpretor);
            }

            if (args.NewCursorPosition.HasValue && args.NewCursorPosition.Value >= 0)
            {
                Console.SetCursorPosition(0, args.NewCursorPosition.Value);
                WriteMenuItem('>' + sender.GetMenu().ElementAt(args.NewCursorPosition.Value)?.TrimStart(), sender as Interpretor);
            }

            Console.SetCursorPosition(orgPosition.CursorLeft, orgPosition.CursorTop);

            return false; //handled
        }

        private static void InterpretorOnKeyPressed(IInterpretor sender, IKeyPressArgs args)
        {
            if(!(sender is Interpretor interpretor))
                return;

            InterpretorOnCursorMove(interpretor,
                                    new CursorInformation(null,
                                                          interpretor.CursorPosition,
                                                          args.Key));
        }
    }
}
