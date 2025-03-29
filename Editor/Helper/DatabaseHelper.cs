using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorAssetBrowser.Models;
using UnityEngine;

namespace UnityEditorAssetBrowser.Helper
{
    /// <summary>
    /// データベース操作を支援するヘルパークラス
    /// </summary>
    public static class DatabaseHelper
    {
        #region Constants
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
        };
        #endregion

        #region AE Database Operations
        /// <summary>
        /// AEデータベースを読み込む
        /// </summary>
        /// <param name="path">データベースのパス</param>
        /// <returns>読み込んだデータベース、失敗時はnull</returns>
        public static AvatarExplorerDatabase? LoadAEDatabase(string path)
        {
            try
            {
                var jsonPath = Path.Combine(path, "ItemsData.json");
                if (!File.Exists(jsonPath))
                {
                    Debug.LogError($"ItemsData.json not found at: {jsonPath}");
                    return null;
                }

                Debug.Log($"Loading AE database from: {jsonPath}");
                var json = File.ReadAllText(jsonPath);
                var items = JsonConvert.DeserializeObject<AvatarExplorerItem[]>(json, JsonSettings);

                if (items == null)
                {
                    Debug.LogError("Failed to deserialize AE database");
                    return null;
                }

                Debug.Log($"AE database loaded successfully. Items count: {items.Length}");
                return new AvatarExplorerDatabase { Items = new List<AvatarExplorerItem>(items) };
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading AE database: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// AEデータベースを保存する
        /// </summary>
        /// <param name="path">保存先のパス</param>
        /// <param name="data">保存するデータ</param>
        public static void SaveAEDatabase(string path, AvatarExplorerItem[] data)
        {
            try
            {
                var jsonPath = Path.Combine(path, "ItemsData.json");
                var json = JsonConvert.SerializeObject(data, JsonSettings);
                File.WriteAllText(jsonPath, json);
                Debug.Log($"AE database saved successfully to: {jsonPath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving AE database: {ex.Message}");
            }
        }
        #endregion

        #region KA Database Operations
        /// <summary>
        /// KAデータベースを読み込む
        /// </summary>
        /// <param name="path">データベースのパス</param>
        /// <returns>読み込んだデータベース、失敗時はnull</returns>
        public static KonoAssetDatabase? LoadKADatabase(string path)
        {
            try
            {
                if (!File.Exists(path))
                {
                    Debug.LogError($"File not found: {path}");
                    return null;
                }

                var json = File.ReadAllText(path);
                return JsonConvert.DeserializeObject<KonoAssetDatabase>(json, JsonSettings);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading KA database from {path}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// KAデータベースを保存する
        /// </summary>
        /// <param name="path">保存先のパス</param>
        /// <param name="database">保存するデータベース</param>
        public static void SaveKADatabase(string path, KonoAssetDatabase database)
        {
            try
            {
                var metadataPath = Path.Combine(path, "metadata");
                if (!Directory.Exists(metadataPath))
                {
                    Directory.CreateDirectory(metadataPath);
                }

                var wearablesPath = Path.Combine(metadataPath, "avatarWearables.json");
                var wearablesJson = JsonConvert.SerializeObject(database, JsonSettings);
                File.WriteAllText(wearablesPath, wearablesJson);

                Debug.Log($"KA database saved successfully to: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error saving KA database: {ex.Message}");
            }
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// 指定されたディレクトリ内のUnityPackageファイルを検索する
        /// </summary>
        /// <param name="directory">検索対象のディレクトリ</param>
        /// <returns>見つかったUnityPackageファイルのパス配列</returns>
        public static string[] FindUnityPackages(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(directory, "*.unitypackage", SearchOption.AllDirectories);
        }
        #endregion
    }
}
