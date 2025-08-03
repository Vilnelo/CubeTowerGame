using System;
using System.Collections.Generic;
using System.Linq;
using Core.AssetManagement.Runtime;
using Core.Localization.Runtime;
using UnityEngine;
using Zenject;

namespace Core.Localization.External
{
    public class LocalizationController : ILocalizationController, IInitializable, IDisposable
    {
        [Inject] private IAssetLoader m_AssetLoader;
        
        public static event Action OnLocalizationReady;

        private const string m_LocalizationCsvAddress = "localization_data";
        private const string m_LanguagePrefsKey = "Language";
        private const string m_DefaultLanguage = "EN";

        private Dictionary<string, Dictionary<string, string>> m_LocalizationData = new();
        private string m_CurrentLanguage = m_DefaultLanguage;
        private List<string> m_AvailableLanguages = new();
        private bool m_IsInitialized = false;

        public void Initialize()
        {
            Debug.Log("[LocalizationController] Initializing...");
            
            LocalizationExtensions.Initialize(this);
            
            LoadLocalizationData();
            
            m_CurrentLanguage = PlayerPrefs.GetString(m_LanguagePrefsKey, m_DefaultLanguage);
            
            m_IsInitialized = true;
            Debug.Log($"[LocalizationController] Initialized with language: {m_CurrentLanguage}");
            
            //TODO: Выставлен русский для проверки локализации. Реализовать выбор языка устройства.
            SetLanguage("RU");
            
            OnLocalizationReady?.Invoke();
        }

        private void LoadLocalizationData()
        {
            try
            {
                var csvAsset = m_AssetLoader.LoadSync<TextAsset>(m_LocalizationCsvAddress);
                if (csvAsset == null)
                {
                    Debug.LogError($"[LocalizationController] Failed to load CSV file: {m_LocalizationCsvAddress}");
                    return;
                }

                ParseCSVData(csvAsset.text);
                Debug.Log($"[LocalizationController] Loaded localization data with {m_AvailableLanguages.Count} languages");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LocalizationController] Error loading localization data: {ex.Message}");
            }
        }

        private void ParseCSVData(string csvText)
        {
            m_LocalizationData.Clear();
            m_AvailableLanguages.Clear();

            string[] lines = csvText.Split('\n');
            if (lines.Length < 2)
            {
                Debug.LogError("[LocalizationController] CSV file is empty or has no header");
                return;
            }
            
            string[] headers = ParseCSVLine(lines[0]);
            if (headers.Length < 2)
            {
                Debug.LogError("[LocalizationController] CSV header must have at least Key and one language column");
                return;
            }
            
            for (int i = 1; i < headers.Length; i++)
            {
                string language = headers[i].Trim().ToUpper();
                m_AvailableLanguages.Add(language);
                m_LocalizationData[language] = new Dictionary<string, string>();
            }
            
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex].Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#"))
                    continue;

                string[] values = ParseCSVLine(line);
                if (values.Length < 2)
                    continue;

                string key = values[0].Trim();
                if (string.IsNullOrEmpty(key))
                    continue;
                
                for (int i = 1; i < values.Length && i < headers.Length; i++)
                {
                    string language = m_AvailableLanguages[i - 1];
                    string value = values[i].Trim();
                    
                    value = value.Replace("\\n", "\n");
                    value = value.Replace("\\t", "\t");
                    value = value.Replace("\"\"", "\"");
                    
                    m_LocalizationData[language][key] = value;
                }
            }

            Debug.Log($"[LocalizationController] Parsed {m_LocalizationData.Values.FirstOrDefault()?.Count ?? 0} localization keys");
        }

        private string[] ParseCSVLine(string line)
        {
            List<string> result = new List<string>();
            bool inQuotes = false;
            string currentField = "";

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentField += '"';
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(currentField);
                    currentField = "";
                }
                else
                {
                    currentField += c;
                }
            }

            result.Add(currentField);
            return result.ToArray();
        }

        public void SetLanguage(string languageCode)
        {
            if (!m_IsInitialized)
            {
                Debug.LogWarning("[LocalizationController] Not initialized yet");
                return;
            }

            string upperLanguage = languageCode.ToUpper();
            
            if (!m_AvailableLanguages.Contains(upperLanguage))
            {
                Debug.LogWarning($"[LocalizationController] Language '{upperLanguage}' not available. Available: {string.Join(", ", m_AvailableLanguages)}");
                return;
            }

            if (m_CurrentLanguage != upperLanguage)
            {
                m_CurrentLanguage = upperLanguage;
                PlayerPrefs.SetString(m_LanguagePrefsKey, m_CurrentLanguage);
                PlayerPrefs.Save();
                
                Debug.Log($"[LocalizationController] Language changed to: {m_CurrentLanguage}");
            }
        }

        public string GetCurrentLanguage()
        {
            return m_CurrentLanguage;
        }

        public string GetLocalizedText(string key)
        {
            if (!m_IsInitialized || string.IsNullOrEmpty(key))
            {
                return key;
            }

            if (m_LocalizationData.TryGetValue(m_CurrentLanguage, out var languageData) && 
                languageData.TryGetValue(key, out var localizedText))
            {
                return localizedText;
            }
            
            if (m_CurrentLanguage != m_DefaultLanguage && 
                m_LocalizationData.TryGetValue(m_DefaultLanguage, out var englishData) && 
                englishData.TryGetValue(key, out var englishText))
            {
                Debug.LogWarning($"[LocalizationController] Key '{key}' not found for language '{m_CurrentLanguage}', using English fallback");
                return englishText;
            }

            Debug.LogWarning($"[LocalizationController] Key '{key}' not found");
            return key;
        }

        public string GetLocalizedText(string key, params object[] args)
        {
            string localizedText = GetLocalizedText(key);
            
            if (args == null || args.Length == 0)
                return localizedText;

            try
            {
                return string.Format(localizedText, args);
            }
            catch (FormatException ex)
            {
                Debug.LogWarning($"[LocalizationController] Format error for key '{key}': {ex.Message}");
                return localizedText;
            }
        }

        public bool HasKey(string key)
        {
            if (!m_IsInitialized || string.IsNullOrEmpty(key))
                return false;

            return m_LocalizationData.TryGetValue(m_CurrentLanguage, out var languageData) && 
                   languageData.ContainsKey(key);
        }

        public List<string> GetAvailableLanguages()
        {
            return new List<string>(m_AvailableLanguages);
        }

        public void Dispose()
        {
            m_AssetLoader?.Release(m_LocalizationCsvAddress);
            Debug.Log("[LocalizationController] Disposed");
        }
    }
}