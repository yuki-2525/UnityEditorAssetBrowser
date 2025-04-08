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
        /// AvatarExplorerのデータベースファイルを読み込む
        /// </summary>
        /// <param name="path">データベースのパス</param>
        /// <returns>読み込んだデータベース</returns>
        public static AvatarExplorerDatabase? LoadAEDatabaseFile(string path)
        {
            try
            {
                // パスがディレクトリの場合は、ItemsData.jsonを探す
                string jsonPath;
                if (Directory.Exists(path))
                {
                    jsonPath = Path.Combine(path, "ItemsData.json");
                    if (!File.Exists(jsonPath))
                    {
                        Debug.LogWarning($"AE database file not found at: {jsonPath}");
                        return null;
                    }
                }
                else
                {
                    // パスがファイルの場合はそのまま使用
                    jsonPath = path;
                    if (!File.Exists(jsonPath))
                    {
                        Debug.LogWarning($"AE database file not found at: {jsonPath}");
                        return null;
                    }
                }

                var json = File.ReadAllText(jsonPath);

                // JSONが配列形式かどうかを確認
                if (json.TrimStart().StartsWith("["))
                {
                    // 配列形式の場合は、AvatarExplorerItem[]としてデシリアライズしてから
                    // AvatarExplorerDatabaseに変換
                    var items = JsonConvert.DeserializeObject<AvatarExplorerItem[]>(json);
                    if (items != null)
                    {
                        return new AvatarExplorerDatabase(items);
                    }
                }
                else
                {
                    // オブジェクト形式の場合は、そのままAvatarExplorerDatabaseとしてデシリアライズ
                    return JsonConvert.DeserializeObject<AvatarExplorerDatabase>(json);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load AE database: {ex.Message}");
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
