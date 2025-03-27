using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorAssetBrowser.Models;
using Newtonsoft.Json;

namespace UnityEditorAssetBrowser.Helper
{
    public static class DatabaseHelper
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

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

        public static void SaveKADatabase(string path, KonoAssetDatabase database)
        {
            try
            {
                var metadataPath = Path.Combine(path, "metadata");
                if (!Directory.Exists(metadataPath))
                {
                    Directory.CreateDirectory(metadataPath);
                }

                // ウェアラブルアイテムの保存
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

        public static string[] FindUnityPackages(string directory)
        {
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
            {
                return Array.Empty<string>();
            }

            return Directory.GetFiles(directory, "*.unitypackage", SearchOption.AllDirectories);
        }
    }
} 