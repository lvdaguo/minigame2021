using UnityEngine;
using Utilities.Enum;

namespace Utilities.Event
{
    [System.Serializable]
    public class EventDef
    {
        [SerializeField] private EventSpaceEnum _eventSpaceEnum;
        [SerializeField] private string _eventName;

        public EventDef(EventSpaceEnum eventSpaceEnumEnum, string eventName)
        {
            _eventSpaceEnum = eventSpaceEnumEnum;
            _eventName = eventName;
        }
        
        public EventSpaceEnum EventSpaceEnumEnum => _eventSpaceEnum;
        public string EventName => _eventName;
    }
}