using System.Collections.Generic;

namespace Core.Localization.Runtime
{
    public interface ILocalizationController
    {
        void Initialize();
        void SetLanguage(string languageCode);
        string GetCurrentLanguage();
        string GetLocalizedText(string key);
        string GetLocalizedText(string key, params object[] args);
        bool HasKey(string key);
        List<string> GetAvailableLanguages();
    }
}