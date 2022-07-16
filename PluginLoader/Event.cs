using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace PluginLoader
{
    /// <summary>
    /// 事件基类
    /// </summary>
    public abstract class Event
    {
        /// <summary>
        /// 是否取消
        /// </summary>
        public bool IsCancel;
        /// <summary>
        /// 所有插件监听器
        /// </summary>
        public static List<Listener> Listeners = new List<Listener>();
        public virtual string Name { get; }
        public virtual void Do(bool IsCancel)
        {

        }
        public string GetEventName()
        {
            return Name;
        }
        /// <summary>
        /// 注册监听器
        /// </summary>
        /// <param name="listener">
        /// 监听器
        /// </param>
        public static void RegListener(Listener listener)
        {
            Listeners.Add(listener);
        }/// <summary>
        /// 发送全局事件
        /// </summary>
        /// <param name="event">
        /// 事件
        /// </param>
        public static void SetEvent(Event @event)
        {
            bool cancel = false;
            for(int i = 0; i < Listeners.Count; i++)
            {
                Event e = Listeners[i].GetEvent(@event);
                cancel = cancel && e.IsCancel;
                MethodInfo[] methods = Listeners[i].GetType().GetMethods();
                foreach(MethodInfo method in methods)
                {
                    bool IsEventMethod = false;
                    foreach(Attribute attribute in method.GetCustomAttributes())
                    {
                        if(attribute.GetType().Name == "EventHandle")
                        {
                            IsEventMethod = true;
                        }
                    }
                    if (IsEventMethod & method.GetParameters().Length == 1)
                    {
                        if(method.GetParameters()[0].ParameterType.BaseType.Name == "Event" & method.GetParameters()[0].ParameterType.Name == @event.GetType().Name & method.ReturnType.BaseType.Name == "Event")
                        {
                            e = (Event)method.Invoke(Listeners[i], new object[] {@event });
                            cancel = cancel && e.IsCancel;
                        }
                    }
                }
            }
            @event.Do(cancel);
        }
    }
}
