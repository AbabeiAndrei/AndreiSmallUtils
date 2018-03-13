using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AndreiSmallUtils.Utils
{
    public static class ReflexionEx
    {
        public static void RaiseEvent<TEventArgs>(this object source, string eventName, TEventArgs eventArgs) 
            where TEventArgs : EventArgs
        {
            var type = source.GetType();
            var @event = type.GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);

            if (@event == null)
                throw new ArgumentException("Event not found", nameof(eventName));
            
            if (!(@event.GetValue(source) is MulticastDelegate eventDelegate))
                return;

            foreach (var handler in eventDelegate.GetInvocationList())
                handler.Method.Invoke(handler.Target,
                                      new[]
                                      {
                                          source,
                                          eventArgs
                                      });
        }
    }
}
