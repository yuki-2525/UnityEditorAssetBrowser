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
    /// KonoAssetデータベースの読み込みと保存を支援するヘルパークラス
    /// アバター、ウェアラブル、ワールドオブジェクトのデータベースを管理する
    /// </summary>
    public class KADatabaseHelper
    {
        /// <summary>
        /// データベース読み込み結果を保持するクラス
        /// 各データベースの読み込み状態を管理する
        /// </summary>
        public class DatabaseLoadResult
        {
            /// <summary>
            /// アバターデータベース
            /// </summary>
            public KonoAssetAvatarsDatabase? avatarsDatabase;

            /// <summary>
            /// ウェアラブルデータベース
            /// </summary>
            public KonoAssetWearablesDatabase? wearablesDatabase;

            /// <summary>
            /// ワールドオブジェクトデータベース
            /// </summary>
            public KonoAssetWorldObjectsDatabase? worldObjectsDatabase;

            /// <summary>
            /// その他アセットデータベース
            /// </summary>
            public KonoAssetOtherAssetsDatabase? otherAssetsDatabase;
        }

        /// <summary>
        /// KonoAssetのデータベースを読み込む
        /// 指定されたパスから各データベースファイルを読み込み、結果を返す
        /// </summary>
        /// <param name="metadataPath">メタデータが格納されているディレクトリのパス</param>
        /// <returns>読み込んだデータベースの結果</returns>
        public static DatabaseLoadResult LoadKADatabaseFiles(string metadataPath)
        {
            var result = new DatabaseLoadResult();

            try
            {
                // avatars.jsonの読み込み
                var avatarsPath = Path.Combine(metadataPath, "avatars.json");
                if (File.Exists(avatarsPath))
                {
                    var json = File.ReadAllText(avatarsPath);
                    result.avatarsDatabase =
                        JsonConvert.DeserializeObject<KonoAssetAvatarsDatabase>(json);
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

                // otherAssets.jsonの読み込み
                var otherAssetsPath = Path.Combine(metadataPath, "otherAssets.json");
                if (File.Exists(otherAssetsPath))
                {
                    var json = File.ReadAllText(otherAssetsPath);
                    result.otherAssetsDatabase =
                        JsonConvert.DeserializeObject<KonoAssetOtherAssetsDatabase>(json);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load KA database: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// KAデータベースを保存する
        /// 指定されたパスにデータベースをJSON形式で保存する
        /// </summary>
        /// <param name="path">保存先のディレクトリパス</param>
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
                Debug.LogError($"Error saving KA database: {ex.Message}");
            }
        }
    }
}
