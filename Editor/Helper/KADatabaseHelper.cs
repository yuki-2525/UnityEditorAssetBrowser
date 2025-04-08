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
    /// KonoAssetデータベースの読み込みを支援するヘルパークラス
    /// </summary>
    public class KADatabaseHelper
    {
        /// <summary>
        /// データベース読み込み結果
        /// </summary>
        public class DatabaseLoadResult
        {
            public KonoAssetAvatarsDatabase? avatarsDatabase;
            public KonoAssetWearablesDatabase? wearablesDatabase;
            public KonoAssetWorldObjectsDatabase? worldObjectsDatabase;
        }

        /// <summary>
        /// KonoAssetデータベースを読み込む
        /// </summary>
        /// <param name="metadataPath">メタデータパス</param>
        /// <returns>読み込んだデータベース</returns>
        public static DatabaseLoadResult? LoadKADatabase(string metadataPath)
        {
            var result = new DatabaseLoadResult();

            // アバターデータベースの読み込み
            var avatarsPath = Path.Combine(metadataPath, "avatars.json");
            if (File.Exists(avatarsPath))
            {
                var baseDb = LoadKADatabaseFile(avatarsPath);
                if (baseDb != null)
                {
                    result.avatarsDatabase =
                        JsonConvert.DeserializeObject<KonoAssetAvatarsDatabase>(
                            JsonConvert.SerializeObject(baseDb)
                        );
                }
            }

            // ウェアラブルデータベースの読み込み
            var wearablesPath = Path.Combine(metadataPath, "avatarWearables.json");
            if (File.Exists(wearablesPath))
            {
                var baseDb = LoadKADatabaseFile(wearablesPath);
                if (baseDb != null)
                {
                    result.wearablesDatabase =
                        JsonConvert.DeserializeObject<KonoAssetWearablesDatabase>(
                            JsonConvert.SerializeObject(baseDb)
                        );
                }
            }

            // ワールドオブジェクトデータベースの読み込み
            var worldObjectsPath = Path.Combine(metadataPath, "worldObjects.json");
            if (File.Exists(worldObjectsPath))
            {
                var baseDb = LoadKADatabaseFile(worldObjectsPath);
                if (baseDb != null)
                {
                    result.worldObjectsDatabase =
                        JsonConvert.DeserializeObject<KonoAssetWorldObjectsDatabase>(
                            JsonConvert.SerializeObject(baseDb)
                        );
                }
            }

            return result;
        }

        /// <summary>
        /// KonoAssetデータベースファイルを読み込む
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>読み込んだデータベース</returns>
        private static object? LoadKADatabaseFile(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject(json);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError(
                    $"Failed to load database file: {filePath}\n{ex.Message}"
                );
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
