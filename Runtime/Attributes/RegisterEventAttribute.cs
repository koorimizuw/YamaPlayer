using System;
using UnityEngine.EventSystems;

namespace Yamadev.YamaStream
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = true)]
    public class RegisterEventAttribute : Attribute
    {
        public Type TargetType { get; }
        public string UnityEvent { get; }
        public string EventName { get; }

        public RegisterEventAttribute(string unityEvent, string eventName)
            : this(null, unityEvent, eventName) { }

        public RegisterEventAttribute(Type targetType, string unityEvent, string eventName)
        {
            TargetType = targetType;
            UnityEvent = unityEvent;
            EventName = eventName;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class RegisterEventTriggerAttribute : Attribute
    {
        public EventTriggerType EventType { get; }
        public string EventName { get; }

        public RegisterEventTriggerAttribute(EventTriggerType eventType, string eventName)
        {
            EventType = eventType;
            EventName = eventName;
        }
    }
}