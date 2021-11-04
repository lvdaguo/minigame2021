using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Enum;

namespace Utilities.Event
{
    [CreateAssetMenu(fileName = "EventCenterConfig", menuName = "Utilities/Event Center Config")]
    public class EventCenterConfigSO : ScriptableObject
    {
        [SerializeField] private List<EventSpaceEnum> _eventSpaceEnums;

        public List<EventSpaceEnum> EventSpaceEnums => _eventSpaceEnums;
    }
}