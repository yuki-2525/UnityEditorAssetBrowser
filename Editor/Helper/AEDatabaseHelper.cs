// Copyright (c) 2025 yuki-2525
// This code is borrowed from AETools(https://github.com/puk06/AE-Tools)
// AETools is licensed under the MIT License. https://github.com/puk06/AE-Tools/blob/master/LICENSE.txt

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditorAssetBrowser.Models;
using UnityEngine;

namespace UnityEditorAssetBrowser.Helper
{
    /// <summary>
    /// AEデータベース操作を支援するヘルパークラス
    /// </summary>
    public static class AEDatabaseHelper
    {
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
                    Debug.LogWarning($"ItemsData.json not found at: {jsonPath}");
                    return null;
                }

                var json = File.ReadAllText(jsonPath);
                var items = JsonConvert.DeserializeObject<AvatarExplorerItem[]>(
                    json,
                    JsonSettings.Settings
                );

                if (items == null)
                {
                    Debug.LogWarning("Failed to deserialize AE database");
                    return null;
                }

                return new AvatarExplorerDatabase { Items = new List<AvatarExplorerItem>(items) };
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error loading AE database: {ex.Message}");
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
                var json = JsonConvert.SerializeObject(data, JsonSettings.Settings);
                File.WriteAllText(jsonPath, json);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error saving AE database: {ex.Message}");
            }
        }
    }
}
