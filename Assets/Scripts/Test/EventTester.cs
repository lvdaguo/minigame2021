using UnityEngine;
using Utilities.Enum;
using Utilities.Event;

namespace Test
{
    public class TestArguments : EventArguments
    {
        public int Num { get; }

        public TestArguments(int num)
        {
            Num = num;
        }
    }
    
    public class EventTester : MonoBehaviour
    {
        private EventDef _testEvent = new EventDef(EventSpaceEnum.Global, "TestEvent");
        
        private void Awake()
        {
            EventCenter.AddListener(EventSpaceEnum.Global, "TestEvent", OnTestEvent);
        }

        private void OnTestEvent(EventArguments eventArguments)
        {
            if (eventArguments is TestArguments testArguments)
            {
                Debug.Log("Print: " + testArguments.Num);
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                EventCenter.Broadcast(_testEvent, new TestArguments(10));
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                EventCenter.RemoveListener(_testEvent, OnTestEvent);
            }
        }
    }
}