using UnityEngine;

namespace Utilities.Debugger
{
    [System.Serializable]
    public class LogSpaceItem
    {
        [SerializeField] private Color _color = Color.white;
        [SerializeField] private bool _enabled;

        public Color Color => _color;
        public bool Enabled => _enabled;
    }
}