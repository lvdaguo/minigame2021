using System.Collections.Generic;
using DialogueSystem;
using Iphone;
using KeywordSystem;
using UnityEngine;

namespace Singletons
{
    [System.Serializable]
    public class GameConfig
    {
        [SerializeField] private DialogueSystemConfigSO _dialogueSystemConfigSO;
        [SerializeField] private KeywordConfigSO _keywordConfigSO;
        [SerializeField] private IphoneConfigSO _iphoneConfigSO;
        
        public DialogueSystemConfigSO DialogueSystemConfigSO => _dialogueSystemConfigSO;
        public KeywordConfigSO KeywordConfigSO => _keywordConfigSO;
        public IphoneConfigSO IphoneConfigSO => _iphoneConfigSO;
    }
    
    [System.Serializable]
    public class SceneConfig
    {
        [SerializeField] private string _sceneName;
        [SerializeField] private GameConfig _gameConfig;
        
        public string SceneName => _sceneName;
        public GameConfig GameConfig => _gameConfig;
    }
    
    [CreateAssetMenu(fileName = "SceneConfigList", menuName = "Util/Scene Config List")]
    public class SceneConfigListSO : ScriptableObject
    {
        [SerializeField] private List<SceneConfig> _sceneConfigs;

        public List<SceneConfig> SceneConfigs => _sceneConfigs;
    }
}