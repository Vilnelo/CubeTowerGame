namespace Core.Localization.Runtime
{
    public static class LocalizationExtensions
    {
        private static ILocalizationController s_LocalizationController;

        public static void Initialize(ILocalizationController controller)
        {
            s_LocalizationController = controller;
        }

        public static string Localize(this string key)
        {
            return s_LocalizationController?.GetLocalizedText(key) ?? key;
        }

        public static string Localize(this string key, params object[] args)
        {
            return s_LocalizationController?.GetLocalizedText(key, args) ?? key;
        }
    }
}