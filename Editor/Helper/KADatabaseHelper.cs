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
        /// KonoAssetのデータベースを読み込む
        /// </summary>
        /// <param name="metadataPath">メタデータのパス</param>
        /// <returns>読み込んだデータベース</returns>
        public static DatabaseLoadResult LoadKADatabaseFiles(string metadataPath)
        {
            var result = new DatabaseLoadResult();

            // avatars.jsonの読み込み
            var avatarsPath = Path.Combine(metadataPath, "avatars.json");
            if (File.Exists(avatarsPath))
            {
                var json = File.ReadAllText(avatarsPath);
                result.avatarsDatabase = JsonConvert.DeserializeObject<KonoAssetAvatarsDatabase>(
                    json
                );
            }

            // avatarWearables.jsonの読み込み
            var wearablesPath = Path.Combine(metadataPath, "avatarWearables.json");
            if (File.Exists(wearablesPath))
            {
                var json = File.ReadAllText(wearablesPath);
                result.wearablesDatabase =
                    JsonConvert.DeserializeObject<KonoAssetWearablesDatabase>(json);
            }

            // worldObjects.jsonの読み込み
            var worldObjectsPath = Path.Combine(metadataPath, "worldObjects.json");
            if (File.Exists(worldObjectsPath))
            {
                var json = File.ReadAllText(worldObjectsPath);
                result.worldObjectsDatabase =
                    JsonConvert.DeserializeObject<KonoAssetWorldObjectsDatabase>(json);
            }

            return result;
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
