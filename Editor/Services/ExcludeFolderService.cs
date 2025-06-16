using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnityEditorAssetBrowser.Services
{
    /// <summary>
    /// 除外フォルダ判定の共通サービス
    /// </summary>
    public static class ExcludeFolderService
    {
        private const string PREFS_KEY_EXCLUDE_FOLDERS = "UnityEditorAssetBrowser_ExcludeFolders";
        private const string PREFS_KEY_EXCLUDE_FOLDERS_COMBINED =
            "UnityEditorAssetBrowser_ExcludeFolders_Combined";

        // デフォルト除外フォルダ（s?付き正規表現）
        private static readonly List<string> DefaultExcludePatterns = new List<string>
        {
            "Assets?",
            "Packages?",
            "Animes?",
            "Animations?",
            "AnimationControllers?",
            "Animators?",
            "Commons?",
            "Data",
            "Expressions?",
            "Fbxs?",
            "Mats?",
            "Matcaps?",
            "Materials?",
            "Motions?",
            "Partcles?",
            "Prefabs?",
            "Shaders?",
            "Texs?",
            "Textures?",
        };

        /// <summary>
        /// 除外フォルダかどうか判定（正規表現でなければ完全一致、大文字小文字区別なし）
        /// </summary>
        public static bool IsExcludedFolder(string folderName)
        {
            var patterns = GetCombinedExcludePatterns();
            string normFolderName = Normalize(folderName);
            foreach (var pattern in patterns)
            {
                if (string.IsNullOrEmpty(pattern))
                    continue;
                string normPattern = Normalize(pattern);
                try
                {
                    if (IsRegexPattern(normPattern))
                    {
                        if (Regex.IsMatch(normFolderName, normPattern, RegexOptions.IgnoreCase))
                            return true;
                    }
                    else
                    {
                        if (
                            string.Equals(
                                normFolderName,
                                normPattern,
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                            return true;
                    }
                }
                catch
                { /* 無効な正規表現は無視 */
                }
            }
            return false;
        }

        /// <summary>
        /// 除外判定用の合成リスト（ユーザー追加分＋ONの初期設定分）
        /// </summary>
        public static List<string> GetExcludeFolderPatterns()
        {
            var prefs = LoadPrefs();
            var result = new List<string>();
            if (prefs != null)
            {
                result.AddRange(prefs.userFolders);
                result.AddRange(prefs.enabledDefaults);
            }
            return result;
        }

        /// <summary>
        /// 正規表現かどうかを判定する（特殊文字が含まれていれば正規表現とみなす）
        /// </summary>
        public static bool IsRegexPattern(string pattern)
        {
            string regexChars = @".*+?^${}()|[]\\";
            return pattern.IndexOfAny(regexChars.ToCharArray()) >= 0;
        }

        /// <summary>
        /// 除外判定用に空白・全角空白・アンダースコアを除去
        /// </summary>
        private static string Normalize(string s)
        {
            if (s == null)
                return "";
            return s.Replace(" ", "").Replace("　", "").Replace("_", "");
        }

        /// <summary>
        /// 除外フォルダの初期設定リスト（s?付き正規表現、abc順でUIに表示）を取得
        /// </summary>
        public static List<string> GetAllDefaultExcludeFolders()
        {
            return new List<string>(DefaultExcludePatterns);
        }

        /// <summary>
        /// 除外フォルダ設定を初期化（EditorPrefsに値がなければデフォルト値を登録）
        /// </summary>
        public static void InitializeDefaultExcludeFolders()
        {
            string excludeFoldersJson = EditorPrefs.GetString(PREFS_KEY_EXCLUDE_FOLDERS, "");
            if (string.IsNullOrEmpty(excludeFoldersJson))
            {
                var data = new ExcludeFoldersPrefsData
                {
                    userFolders = new List<string>(),
                    enabledDefaults = new List<string>(DefaultExcludePatterns),
                };
                string json = JsonUtility.ToJson(data);
                EditorPrefs.SetString(PREFS_KEY_EXCLUDE_FOLDERS, json);
            }
            else
            {
                // マイグレーション: 新しいデフォルト項目があればONで追加、不要な項目は削除
                var data = JsonUtility.FromJson<ExcludeFoldersPrefsData>(excludeFoldersJson);
                bool updated = false;
                foreach (var def in DefaultExcludePatterns)
                {
                    if (!data.enabledDefaults.Contains(def))
                    {
                        data.enabledDefaults.Add(def);
                        updated = true;
                    }
                }
                var toRemove = data
                    .enabledDefaults.Where(x => !DefaultExcludePatterns.Contains(x))
                    .ToList();
                foreach (var rem in toRemove)
                {
                    data.enabledDefaults.Remove(rem);
                    updated = true;
                }
                if (updated)
                {
                    string json = JsonUtility.ToJson(data);
                    EditorPrefs.SetString(PREFS_KEY_EXCLUDE_FOLDERS, json);
                }
            }
        }

        /// <summary>
        /// 除外フォルダ設定データ（ユーザー追加分・ONの初期設定分）をEditorPrefsから読み込む
        /// </summary>
        public static ExcludeFoldersPrefsData LoadPrefs()
        {
            string json = EditorPrefs.GetString(PREFS_KEY_EXCLUDE_FOLDERS, "");
            if (string.IsNullOrEmpty(json))
            {
                return null;
            }
            try
            {
                var data = JsonUtility.FromJson<ExcludeFoldersPrefsData>(json);
                if (
                    data != null
                    && (data.enabledDefaults == null || data.enabledDefaults.Count == 0)
                )
                {
                    data.enabledDefaults = new List<string>(DefaultExcludePatterns);
                }
                return data;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 除外フォルダ設定データ（ユーザー追加分・ONの初期設定分）をEditorPrefsに保存する
        /// </summary>
        public static void SaveExcludeFolders(
            List<string> userFolders,
            List<string> enabledDefaults
        )
        {
            var data = new ExcludeFoldersPrefsData
            {
                userFolders = userFolders,
                enabledDefaults = enabledDefaults,
            };
            string json = JsonUtility.ToJson(data);
            EditorPrefs.SetString(PREFS_KEY_EXCLUDE_FOLDERS, json);
        }

        /// <summary>
        /// 判定用の合成済みリスト（ユーザー追加分＋ONの初期設定分）を保存
        /// </summary>
        public static void SaveCombinedExcludePatterns(List<string> combined)
        {
            string json = JsonUtility.ToJson(new StringListWrapper { list = combined });
            EditorPrefs.SetString(PREFS_KEY_EXCLUDE_FOLDERS_COMBINED, json);
        }

        /// <summary>
        /// 判定用の合成済みリストを取得
        /// </summary>
        public static List<string> GetCombinedExcludePatterns()
        {
            string json = EditorPrefs.GetString(PREFS_KEY_EXCLUDE_FOLDERS_COMBINED, "");
            if (string.IsNullOrEmpty(json))
                return new List<string>();
            try
            {
                var data = JsonUtility.FromJson<StringListWrapper>(json);
                return data?.list ?? new List<string>();
            }
            catch
            {
                return new List<string>();
            }
        }

        [Serializable]
        public class ExcludeFoldersPrefsData
        {
            public List<string> userFolders = new List<string>();
            public List<string> enabledDefaults = new List<string>();
        }

        [Serializable]
        private class StringListWrapper
        {
            public List<string> list = new List<string>();
        }
    }
}
