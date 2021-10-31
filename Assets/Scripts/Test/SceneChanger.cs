using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Utilities.DesignPatterns;

namespace Test
{
    [System.Serializable]
    public class SceneChangeSetting
    {
        [SerializeField] private string _sceneName;
        [SerializeField] private KeyCode _keyCode;

        public string SceneName => _sceneName;
        public KeyCode KeyCode => _keyCode;
    }
    
    public class SceneChanger : GSingleton<SceneChanger>
    {
        [SerializeField] private List<SceneChangeSetting> _sceneChangeSettings;
        
        private void Update()
        {
            foreach (SceneChangeSetting setting in _sceneChangeSettings)
            {
                if (Input.GetKeyDown(setting.KeyCode))
                {
                    SceneLoader.LoadScene(setting.SceneName);
                }
            }
        }
    }
}
