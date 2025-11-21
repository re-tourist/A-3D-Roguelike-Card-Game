// Assets/Scripts/UI/Localization/LanguageManager.cs
using UnityEngine;
using TMPro;
using System;
using System.Collections.Generic;

namespace Game.Localization
{
    public enum Language { Zh, En }

    public static class LanguageManager
    {
        const string PrefKey = "GameLang";
        static Language _current = LoadInitial();
        static TMP_FontAsset _zhFontAsset;
        static TMP_FontAsset _enFontAsset;

        public static Language Current => _current;

        public static event Action OnLanguageChanged;

        public static void SetLanguage(Language lang)
        {
            _current = lang;
            PlayerPrefs.SetString(PrefKey, lang.ToString());
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }

        static Language LoadInitial()
        {
            var s = PlayerPrefs.GetString(PrefKey, "");
            if (Enum.TryParse(s, out Language lang)) return lang;
            return Language.Zh;
        }

        public static TMP_FontAsset GetTMPFont(int size)
        {
            return _current == Language.Zh ? GetZhFont(size) : GetEnFont(size);
        }

        public static void SetZhFontAsset(TMP_FontAsset asset)
        {
            _zhFontAsset = asset;
            OnLanguageChanged?.Invoke();
        }

        public static void SetEnFontAsset(TMP_FontAsset asset)
        {
            _enFontAsset = asset;
            OnLanguageChanged?.Invoke();
        }

        public static void ApplyTo(Transform root, int size)
        {
            var font = GetTMPFont(size);
            if (root == null || font == null) return;
            var texts = root.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var t in texts)
            {
                t.font = font;
            }
        }

        static TMP_FontAsset GetZhFont(int size)
        {
            if (_zhFontAsset != null) return _zhFontAsset;
            return null;
        }

        static TMP_FontAsset GetEnFont(int size)
        {
            if (_enFontAsset != null) return _enFontAsset;
            return null;
        }

        static readonly Dictionary<string, string> Zh = new(StringComparer.OrdinalIgnoreCase)
        {
            { "continue", "继续" },
            { "new game", "新游戏" },
            { "give up current game", "放弃当前游戏" },
            { "settings", "设置" },
            { "exit", "退出" },
            { "resume", "继续" },
            { "back_to_menu", "返回主菜单" },
            { "back", "返回" }
        };

        static readonly Dictionary<string, string> En = new(StringComparer.OrdinalIgnoreCase)
        {
            { "continue", "Continue" },
            { "new game", "New Game" },
            { "give up current game", "Give Up Current Game" },
            { "settings", "Settings" },
            { "exit", "Exit" },
            { "resume", "Resume" },
            { "back_to_menu", "Back to Menu" },
            { "back", "Back" }
        };

        public static string Tr(string key)
        {
            if (_current == Language.Zh) return Zh.TryGetValue(key, out var v) ? v : key;
            return En.TryGetValue(key, out var v2) ? v2 : key;
        }
    }
}
