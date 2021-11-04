using System;
using UnityEngine;

namespace Utilities.Debugger
{
    [CreateAssetMenu(fileName = "LogConfig", menuName = "Utilities/Log Config")]
    public class LogConfigSO : ScriptableObject
    {
        [SerializeField] private LogSpace[] _logSpaces;
        [SerializeField] private bool _isBold;
        [SerializeField] private bool _isItalic;
        [SerializeField, Min(1)] private int _size;
        [SerializeField, Min(1)] private int _logBufferSize;
        [SerializeField] private bool _activated;
        
        public LogSpace[] LogSpaces => _logSpaces;
        public bool IsBold => _isBold;
        public bool IsItalic => _isItalic;
        public int Size => _size;
        
        public int LogBufferSize => _logBufferSize;
        
        public bool Activated => _activated;
        
        public event Action ValueChange = delegate { };
        private void OnValidate()
        {
            ValueChange.Invoke();
        }
    }
}