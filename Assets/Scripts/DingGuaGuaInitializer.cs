using Iphone;
using KeywordSystem;
using Singletons;
using UnityEngine;
using Utilities.DataStructures;
using Utilities.DesignPatterns;

public class DingGuaGuaInitializer : LSingleton<DingGuaGuaInitializer>
{
    public bool Working { get; set; }
    
    private void Start()
    {
        Working = false;
        Wait.Delayed(() =>
        {
            Working = true;
            KeywordConfigSO keywordConfigSO = GameConfigProxy.Instance.KeywordConfigSO;
            foreach (string keyword in keywordConfigSO.KeywordListSO.KeywordList)
            {
                KeywordCollector.Instance.Collect(keyword);
            }

            foreach (MergeOnlyKeyword mergeOnlyKeyword in keywordConfigSO.KeywordListSO.MergeOnlyKeywordList)
            {
                DingGuaGua.Instance.AddBackground(mergeOnlyKeyword.Keyword);
            }
            Working = false;
        }, 0.1f);
    }
}
