using System.Collections.Generic;
using DialogueSystem;
using Iphone;
using KeywordSystem;
using UnityEngine;
using Utilities;
using Utilities.DesignPatterns;

namespace Singletons
{
    public class GameConfigProxy : GSingleton<GameConfigProxy>
    {
        [SerializeField] private SceneConfigListSO _sceneConfigListSO;

        private Dictionary<string, GameConfig> _sceneGameConfigMap;
        private GameConfig CurGameConfig => _sceneGameConfigMap[SceneLoader.ActiveScene];
        
        private void Start()
        {
            _sceneGameConfigMap = new Dictionary<string, GameConfig>();
            foreach (SceneConfig sceneConfig in _sceneConfigListSO.SceneConfigs)
            {
                _sceneGameConfigMap.Add(sceneConfig.SceneName, sceneConfig.GameConfig);
            }
        }

        public DialogueSystemConfigSO DialogueSystemConfigSO => CurGameConfig.DialogueSystemConfigSO;
        public KeywordConfigSO KeywordConfigSO => CurGameConfig.KeywordConfigSO;
        public IphoneConfigSO IphoneConfigSO => CurGameConfig.IphoneConfigSO;
    }
}
