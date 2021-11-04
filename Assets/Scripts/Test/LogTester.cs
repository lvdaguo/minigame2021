using UnityEngine;
using Utilities.Debugger;
using Utilities.Enum;

namespace Test
{
    public class LogTester : MonoBehaviour
    {
        private void Awake()
        {
            Log.Print("Hahaha", LogSpaceEnum.Utilities);
            Log.PrintError("What happened", LogSpaceEnum.Global);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                Log.PrintWarning("What's up", LogSpaceEnum.Utilities, gameObject);
            }
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                Log.Print("No im gonna buy a keyboard", LogSpaceEnum.Global, gameObject);
            }
        }
    }
}