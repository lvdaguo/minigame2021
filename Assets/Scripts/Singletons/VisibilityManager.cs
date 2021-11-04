using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.DesignPatterns;

namespace Singletons
{
    public class VisibilityManager : LSingleton<VisibilityManager>
    {
        [SerializeField] private List<GameObject> _controlGroup;

        private Dictionary<GameObject, CanvasGroup> _canvasGroupMap;
        
        private void Start()
        {
            InitMembers();
        }

        private void InitMembers()
        {
            _canvasGroupMap = new Dictionary<GameObject, CanvasGroup>();
            
            foreach (GameObject go in _controlGroup)
            {
                AddController(go);                
            }
        }
        
        private void AddController(GameObject go)
        {
             CanvasGroup canvasGroup = go.AddComponent<CanvasGroup>();
             _canvasGroupMap.Add(go, canvasGroup);
        }

        public void Show(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public void Hide(CanvasGroup canvasGroup)
        {
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        public void Show(GameObject go)
        {
            Show(_canvasGroupMap[go]);
        }

        public void Hide(GameObject go)
        {
            Hide(_canvasGroupMap[go]);
        }
    }
}