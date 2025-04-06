// Copyright (c) 2025 yuki-2525
// This code is borrowed from AETools(https://github.com/puk06/AE-Tools)
// AETools is licensed under the MIT License. https://github.com/puk06/AE-Tools/blob/master/LICENSE.txt

#nullable enable

using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditorAssetBrowser.Models;
using UnityEngine;

namespace UnityEditorAssetBrowser.Helper
{
    /// <summary>
    /// KAデータベース操作を支援するヘルパークラス
    /// </summary>
    public static class KADatabaseHelper
    {
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
                    Debug.LogWarning($"File not found: {path}");
                    return null;
                }

                var json = File.ReadAllText(path);
                var database = JsonConvert.DeserializeObject<KonoAssetDatabase>(
                    json,
                    JsonSettings.Settings
                );

                if (database == null)
                {
                    Debug.LogWarning("Failed to deserialize KA database");
                    return null;
                }

                return database;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error loading KA database: {ex.Message}");
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

                var jsonPath = Path.Combine(metadataPath, "database.json");
                var json = JsonConvert.SerializeObject(database, JsonSettings.Settings);
                File.WriteAllText(jsonPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error saving KA database: {ex.Message}");
            }
        }
    }
}
