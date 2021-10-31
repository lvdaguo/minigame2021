using System;
using System.Collections;
using UnityEngine;
// ReSharper disable PossibleNullReferenceException

namespace Utilities
{
    /// <summary>
    /// 选项保存器
    /// </summary>
    public static class OptionSaver
    {
        /// <summary> 保存事件 </summary>
        public static event Action SaveEvent = delegate {  };

        /// <summary> 加载事件 </summary>
        public static event Action LoadEvent = delegate {  };

        /// <summary> 初始化 </summary>
        internal static void Init(UtilInitValues utilInitValues)
        {
            if (PlayerPrefs.HasKey("FirstGame") == false)
            {
                PlayerPrefs.SetInt("FirstGame", 0);
                Save();
            }
            else
            {
                Load();
            }
        }

        /// <summary> 保存设置 </summary>
        public static void Save()
        {
            SaveEvent.Invoke();
            PlayerPrefs.Save();
        }

        /// <summary> 加载设置 </summary>
        public static void Load()
        {
            LoadEvent.Invoke();
        }
    }
}