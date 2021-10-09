﻿using UnityEngine;

namespace Iphone
{
    [CreateAssetMenu(fileName = "IphoneConfigSO", menuName = "Iphone/Iphone Config")]
    public class IphoneConfigSO : ScriptableObject
    {
        [SerializeField] private string _password;

        [SerializeField] private string _clientName;
        [SerializeField] private string _sexText;
        [SerializeField] private bool _isMale;
        [SerializeField] private string _age;
        [SerializeField] private string _occupation;
        [SerializeField] private string _educationBackground;

        [SerializeField] private string[] _clientBackgrounds;
        
        public string Password => _password;

        public string ClientName => _clientName;
        public string SexText => _sexText;
        public bool IsMale => _isMale;
        public string Age => _age;
        public string Occupation => _occupation;
        public string EducationBackground => _educationBackground;
        public string[] ClientBackgrounds => _clientBackgrounds;
    }
}