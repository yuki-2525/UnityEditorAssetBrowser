// Copyright (c) 2025 yuki-2525
// This code is borrowed from AETools(https://github.com/puk06/AE-Tools)
// AETools is licensed under the MIT License. https://github.com/puk06/AE-Tools/blob/master/LICENSE.txt

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEditorAssetBrowser.Models;
using UnityEngine;

namespace UnityEditorAssetBrowser.Helper
{
    /// <summary>
    /// AEデータベース操作を支援するヘルパークラス
    /// AvatarExplorerのデータベースファイルの読み込み、保存、変換を行う
    /// </summary>
    public static class AEDatabaseHelper
    {
        /// <summary>
        /// AvatarExplorerのデータベースファイルを読み込む
        /// </summary>
        /// <param name="path">データベースのパス（ディレクトリまたはファイルパス）</param>
        /// <returns>読み込んだデータベース。読み込みに失敗した場合はnull</returns>
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
                        // Datasフォルダ内も探す
                        var datasJsonPath = Path.Combine(path, "Datas", "ItemsData.json");
                        if (File.Exists(datasJsonPath))
                        {
                            jsonPath = datasJsonPath;
                        }
                        else
                        {
                            Debug.LogWarning($"AE database file not found");
                            return null;
                        }
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

                // JSONシリアライザーの設定
                var settings = new JsonSerializerSettings
                {
                    Converters = new List<JsonConverter> { new CustomDateTimeConverter() },
                };

                // JSONが配列形式かどうかを確認
                if (json.TrimStart().StartsWith("["))
                {
                    // 配列形式の場合は、AvatarExplorerItem[]としてデシリアライズしてから
                    // AvatarExplorerDatabaseに変換
                    var items = JsonConvert.DeserializeObject<AvatarExplorerItem[]>(json, settings);
                    if (items != null)
                    {
                        // 対応アバターのパスを変換
                        foreach (var item in items)
                        {
                            if (item.SupportedAvatar != null && item.SupportedAvatar.Length > 0)
                            {
                                item.SupportedAvatar = ConvertSupportedAvatarPaths(
                                    items,
                                    item.SupportedAvatar
                                );
                            }
                        }
                        return new AvatarExplorerDatabase(items);
                    }
                }
                else
                {
                    // オブジェクト形式の場合は、そのままAvatarExplorerDatabaseとしてデシリアライズ
                    var database = JsonConvert.DeserializeObject<AvatarExplorerDatabase>(
                        json,
                        settings
                    );
                    if (database?.Items != null)
                    {
                        // 対応アバターのパスを変換
                        foreach (var item in database.Items)
                        {
                            if (item.SupportedAvatar != null && item.SupportedAvatar.Length > 0)
                            {
                                item.SupportedAvatar = ConvertSupportedAvatarPaths(
                                    database.Items.ToArray(),
                                    item.SupportedAvatar
                                );
                            }
                        }
                    }
                    return database;
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
        /// <param name="path">保存先のディレクトリパス</param>
        /// <param name="data">保存するデータ</param>
        /// <exception cref="Exception">保存に失敗した場合にスローされる</exception>
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

        /// <summary>
        /// 対応アバターのパスをアバター名に変換する
        /// </summary>
        /// <param name="items">全アイテムリスト</param>
        /// <param name="supportedAvatars">変換対象の対応アバターパス配列</param>
        /// <returns>変換後のアバター名配列</returns>
        private static string[] ConvertSupportedAvatarPaths(
            AvatarExplorerItem[] items,
            string[] supportedAvatars
        )
        {
            var supportedAvatarNames = new List<string>();
            foreach (var avatar in supportedAvatars)
            {
                var avatarData = items.FirstOrDefault(x => x.ItemPath == avatar);
                if (avatarData != null)
                {
                    supportedAvatarNames.Add(avatarData.Title.Replace(" ", ""));
                }
            }
            return supportedAvatarNames.ToArray();
        }
    }
}
