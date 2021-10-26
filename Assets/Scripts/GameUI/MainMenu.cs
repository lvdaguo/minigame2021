using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using Utilities.DataStructures;
using Utilities.DesignPatterns;

namespace GameUI
{
    public class MainMenu : LSingleton<MainMenu>
    {
        [SerializeField] private string _startChapterName;

        [SerializeField] private GameObject _mainMenu;
        [SerializeField] private GameObject _achievement;
        [SerializeField] private GameObject _outTeam;

        [SerializeField, TextArea] private string[] _texts;

        [SerializeField] private Image _leftTitle;
        [SerializeField] private Image _leftBackground;
        [SerializeField] private TextMeshProUGUI _leftText;
        [SerializeField] private TextMeshProUGUI _rightText;

        [SerializeField] private Ease _fadeCurve;
        [SerializeField] private float _fadeTime;
        [SerializeField] private float _textStayTime;
        [SerializeField] private float _delayTime;

        public event Action GameStart = delegate { };
        public event Action InterfaceChange = delegate { };

        private bool _interactable;
        
        private void Start()
        {
            _interactable = true;
            ToMainMenu();
        }

        public void StartGame()
        {
            _leftText.alpha = 0f;
            _rightText.alpha = 0f;
            _leftBackground.color = new Color(1f, 1f, 1f, 0f);

            StartCoroutine(StartGameCo());
        }

        private IEnumerator FadeText(TextMeshProUGUI text)
        {
            yield return text
                .DOFade(1f, _fadeTime)
                .SetEase(_fadeCurve)
                .WaitForCompletion();
            yield return Wait.Seconds(_textStayTime);
            yield return text
                .DOFade(0f, _fadeTime)
                .SetEase(_fadeCurve)
                .WaitForCompletion();
        }
        
        private IEnumerator StartGameCo()
        {
            _interactable = false;
            GameStart.Invoke();
            
            yield return _mainMenu.GetComponent<CanvasGroup>()
                .DOFade(0f, _fadeTime)
                .SetEase(_fadeCurve)
                .WaitForCompletion();

            yield return Wait.Seconds(_delayTime);
            
            _rightText.text = _texts[0];
            yield return FadeText(_rightText);

            yield return Wait.Seconds(_delayTime);
            
            _rightText.text = _texts[1];
            yield return FadeText(_rightText);

            yield return Wait.Seconds(_delayTime);
                
            _leftTitle.DOFade(0f, _fadeTime).SetEase(_fadeCurve);
            _leftBackground.DOFade(1f, _fadeTime).SetEase(_fadeCurve).WaitForCompletion();

            float time = 0f;
            while (time < _fadeTime)
            {
                time += Time.deltaTime;
                yield return null;
            }
            
            yield return Wait.Seconds(_delayTime);
            
            _leftText.text = _texts[2];
            yield return FadeText(_leftText);
        
            yield return Wait.Seconds(_delayTime);

            _leftText.text = _texts[3];
            yield return FadeText(_leftText);

            yield return Wait.Seconds(_delayTime);
            
            SceneLoader.LoadScene(_startChapterName);
        }

        public void ContinueGame()
        {
        }

        public void ToAchievement()
        {
            if (_interactable == false)
            {
                return;
            }
            _mainMenu.SetActive(false);
            _achievement.SetActive(true);
            _outTeam.SetActive(false);
            InterfaceChange.Invoke();
        }

        public void ToOurTeam()
        {
            if (_interactable == false)
            {
                return;
            }
            _mainMenu.SetActive(false);
            _achievement.SetActive(false);
            _outTeam.SetActive(true);
            InterfaceChange.Invoke();
        }

        public void ToMainMenu()
        {
            _mainMenu.SetActive(true);
            _achievement.SetActive(false);
            _outTeam.SetActive(false);
            InterfaceChange.Invoke();
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}