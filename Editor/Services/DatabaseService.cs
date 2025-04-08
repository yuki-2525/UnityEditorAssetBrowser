// Copyright (c) 2025 yuki-2525
// This code is borrowed from AETools(https://github.com/puk06/AE-Tools)
// AETools is licensed under the MIT License. https://github.com/puk06/AE-Tools/blob/master/LICENSE.txt

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorAssetBrowser.Helper;
using UnityEditorAssetBrowser.Models;

namespace UnityEditorAssetBrowser.Services
{
    /// <summary>
    /// データベース操作を支援するサービスクラス
    /// </summary>
    public static class DatabaseService
    {
        /// <summary>AEデータベースパスのEditorPrefsキー</summary>
        private const string AE_DATABASE_PATH_KEY = "UnityEditorAssetBrowser_AEDatabasePath";

        /// <summary>KAデータベースパスのEditorPrefsキー</summary>
        private const string KA_DATABASE_PATH_KEY = "UnityEditorAssetBrowser_KADatabasePath";

        /// <summary>AEデータベースのパス</summary>
        private static string aeDatabasePath = "";

        /// <summary>KAデータベースのパス</summary>
        private static string kaDatabasePath = "";

        /// <summary>AvatarExplorerのデータベース</summary>
        private static AvatarExplorerDatabase? aeDatabase;

        /// <summary>KonoAssetのアバターデータベース</summary>
        private static KonoAssetAvatarsDatabase? kaAvatarsDatabase;

        /// <summary>KonoAssetのウェアラブルデータベース</summary>
        private static KonoAssetWearablesDatabase? kaWearablesDatabase;

        /// <summary>KonoAssetのワールドオブジェクトデータベース</summary>
        private static KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase;

        /// <summary>
        /// 設定の読み込み
        /// </summary>
        public static void LoadSettings()
        {
            aeDatabasePath = EditorPrefs.GetString(AE_DATABASE_PATH_KEY, "");
            kaDatabasePath = EditorPrefs.GetString(KA_DATABASE_PATH_KEY, "");

            if (!string.IsNullOrEmpty(aeDatabasePath))
                LoadAndUpdateAEDatabase();
            if (!string.IsNullOrEmpty(kaDatabasePath))
                LoadAndUpdateKADatabase();
        }

        /// <summary>
        /// 設定の保存
        /// </summary>
        public static void SaveSettings()
        {
            EditorPrefs.SetString(AE_DATABASE_PATH_KEY, aeDatabasePath);
            EditorPrefs.SetString(KA_DATABASE_PATH_KEY, kaDatabasePath);
        }

        /// <summary>
        /// AEデータベースの読み込みと更新
        /// </summary>
        public static void LoadAndUpdateAEDatabase()
        {
            if (string.IsNullOrEmpty(aeDatabasePath))
                return;

            var databasePath = Path.Combine(aeDatabasePath, "ItemsData.json");

            if (!File.Exists(databasePath))
            {
                // エラーポップアップを表示
                EditorUtility.DisplayDialog(
                    "パスエラー",
                    "入力したパスが誤っています\n\nAvatarExplorerの設定にある\n\"データベースの保存先\"と\n同一のディレクトリを指定してください",
                    "OK"
                );

                // パスを空欄に戻す
                aeDatabasePath = "";
                SaveSettings();
                return;
            }

            aeDatabase = AEDatabaseHelper.LoadAEDatabaseFile(aeDatabasePath);
        }

        /// <summary>
        /// KAデータベースの読み込みと更新
        /// </summary>
        public static void LoadAndUpdateKADatabase()
        {
            if (string.IsNullOrEmpty(kaDatabasePath))
                return;

            var metadataPath = Path.Combine(kaDatabasePath, "metadata");

            if (!Directory.Exists(metadataPath))
            {
                // エラーポップアップを表示
                EditorUtility.DisplayDialog(
                    "パスエラー",
                    "入力したパスが誤っています\n\nKonoAssetの設定にある\n\"アプリデータの保存先\"と\n同一のディレクトリを指定してください",
                    "OK"
                );

                // パスを空欄に戻す
                kaDatabasePath = "";
                SaveSettings();
                return;
            }

            var result = KADatabaseHelper.LoadKADatabaseFiles(metadataPath);
            if (result != null)
            {
                kaAvatarsDatabase = result.avatarsDatabase;
                kaWearablesDatabase = result.wearablesDatabase;
                kaWorldObjectsDatabase = result.worldObjectsDatabase;
            }
        }

        /// <summary>
        /// AEデータベースパスを取得
        /// </summary>
        public static string GetAEDatabasePath()
        {
            return aeDatabasePath;
        }

        /// <summary>
        /// KAデータベースパスを取得
        /// </summary>
        public static string GetKADatabasePath()
        {
            return kaDatabasePath;
        }

        /// <summary>
        /// AEデータベースパスを設定
        /// </summary>
        public static void SetAEDatabasePath(string path)
        {
            aeDatabasePath = path;
        }

        /// <summary>
        /// KAデータベースパスを設定
        /// </summary>
        public static void SetKADatabasePath(string path)
        {
            kaDatabasePath = path;
        }

        /// <summary>
        /// AEデータベースを取得
        /// </summary>
        public static AvatarExplorerDatabase? GetAEDatabase()
        {
            return aeDatabase;
        }

        /// <summary>
        /// KAアバターデータベースを取得
        /// </summary>
        public static KonoAssetAvatarsDatabase? GetKAAvatarsDatabase()
        {
            return kaAvatarsDatabase;
        }

        /// <summary>
        /// KAウェアラブルデータベースを取得
        /// </summary>
        public static KonoAssetWearablesDatabase? GetKAWearablesDatabase()
        {
            return kaWearablesDatabase;
        }

        /// <summary>
        /// KAワールドオブジェクトデータベースを取得
        /// </summary>
        public static KonoAssetWorldObjectsDatabase? GetKAWorldObjectsDatabase()
        {
            return kaWorldObjectsDatabase;
        }
    }
}
