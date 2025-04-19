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
using UnityEngine;

namespace UnityEditorAssetBrowser.Services
{
    /// <summary>
    /// データベース操作を支援するサービスクラス
    /// AvatarExplorerとKonoAssetのデータベースの読み込み、保存、更新を管理する
    /// </summary>
    public static class DatabaseService
    {
        /// <summary>
        /// AvatarExplorerデータベースパスのEditorPrefsキー
        /// </summary>
        private const string AE_DATABASE_PATH_KEY = "UnityEditorAssetBrowser_AEDatabasePath";

        /// <summary>
        /// KonoAssetデータベースパスのEditorPrefsキー
        /// </summary>
        private const string KA_DATABASE_PATH_KEY = "UnityEditorAssetBrowser_KADatabasePath";

        /// <summary>
        /// AvatarExplorerデータベースのパス
        /// </summary>
        private static string aeDatabasePath = "";

        /// <summary>
        /// KonoAssetデータベースのパス
        /// </summary>
        private static string kaDatabasePath = "";

        /// <summary>
        /// AvatarExplorerのデータベース
        /// </summary>
        private static AvatarExplorerDatabase? aeDatabase;

        /// <summary>
        /// KonoAssetのアバターデータベース
        /// </summary>
        private static KonoAssetAvatarsDatabase? kaAvatarsDatabase;

        /// <summary>
        /// KonoAssetのウェアラブルデータベース
        /// </summary>
        private static KonoAssetWearablesDatabase? kaWearablesDatabase;

        /// <summary>
        /// KonoAssetのワールドオブジェクトデータベース
        /// </summary>
        private static KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase;

        /// <summary>
        /// データベースの設定を読み込む
        /// 保存されたパスからデータベースを読み込み、更新する
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
        /// データベースの設定を保存する
        /// 現在のパスをEditorPrefsに保存する
        /// </summary>
        public static void SaveSettings()
        {
            EditorPrefs.SetString(AE_DATABASE_PATH_KEY, aeDatabasePath);
            EditorPrefs.SetString(KA_DATABASE_PATH_KEY, kaDatabasePath);
        }

        /// <summary>
        /// AvatarExplorerデータベースを読み込み、更新する
        /// パスが無効な場合はエラーメッセージを表示し、パスをリセットする
        /// </summary>
        public static void LoadAndUpdateAEDatabase()
        {
            if (string.IsNullOrEmpty(aeDatabasePath))
                return;

            aeDatabase = AEDatabaseHelper.LoadAEDatabaseFile(aeDatabasePath);
            if (aeDatabase == null)
            {
                ShowErrorDialog(
                    "パスエラー",
                    "入力したパスが誤っています\n\nAvatarExplorer-v1.0.x/Datas\nを指定してください"
                );
                ResetAEDatabasePath();
            }
        }

        /// <summary>
        /// KonoAssetデータベースを読み込み、更新する
        /// パスが無効な場合はエラーメッセージを表示し、パスをリセットする
        /// </summary>
        public static void LoadAndUpdateKADatabase()
        {
            if (string.IsNullOrEmpty(kaDatabasePath))
                return;

            var metadataPath = Path.Combine(kaDatabasePath, "metadata");
            if (!Directory.Exists(metadataPath))
            {
                ShowErrorDialog(
                    "パスエラー",
                    "入力したパスが誤っています\n\nKonoAssetの設定にある\n\"アプリデータの保存先\"と\n同一のディレクトリを指定してください"
                );
                ResetKADatabasePath();
                return;
            }

            var result = KADatabaseHelper.LoadKADatabaseFiles(metadataPath);
            kaAvatarsDatabase = result.avatarsDatabase;
            kaWearablesDatabase = result.wearablesDatabase;
            kaWorldObjectsDatabase = result.worldObjectsDatabase;
        }

        /// <summary>
        /// エラーダイアログを表示する
        /// </summary>
        /// <param name="title">ダイアログのタイトル</param>
        /// <param name="message">表示するメッセージ</param>
        private static void ShowErrorDialog(string title, string message)
        {
            EditorUtility.DisplayDialog(title, message, "OK");
        }

        /// <summary>
        /// AvatarExplorerデータベースのパスをリセットする
        /// </summary>
        private static void ResetAEDatabasePath()
        {
            aeDatabasePath = "";
            SaveSettings();
        }

        /// <summary>
        /// KonoAssetデータベースのパスをリセットする
        /// </summary>
        private static void ResetKADatabasePath()
        {
            kaDatabasePath = "";
            SaveSettings();
        }

        /// <summary>
        /// AvatarExplorerデータベースのパスを取得する
        /// </summary>
        /// <returns>データベースのパス</returns>
        public static string GetAEDatabasePath() => aeDatabasePath;

        /// <summary>
        /// KonoAssetデータベースのパスを取得する
        /// </summary>
        /// <returns>データベースのパス</returns>
        public static string GetKADatabasePath() => kaDatabasePath;

        /// <summary>
        /// AvatarExplorerデータベースのパスを設定する
        /// </summary>
        /// <param name="path">設定するパス</param>
        public static void SetAEDatabasePath(string path) => aeDatabasePath = path;

        /// <summary>
        /// KonoAssetデータベースのパスを設定する
        /// </summary>
        /// <param name="path">設定するパス</param>
        public static void SetKADatabasePath(string path) => kaDatabasePath = path;

        /// <summary>
        /// AvatarExplorerデータベースを取得する
        /// </summary>
        /// <returns>データベース（存在しない場合はnull）</returns>
        public static AvatarExplorerDatabase? GetAEDatabase() => aeDatabase;

        /// <summary>
        /// KonoAssetアバターデータベースを取得する
        /// </summary>
        /// <returns>データベース（存在しない場合はnull）</returns>
        public static KonoAssetAvatarsDatabase? GetKAAvatarsDatabase() => kaAvatarsDatabase;

        /// <summary>
        /// KonoAssetウェアラブルデータベースを取得する
        /// </summary>
        /// <returns>データベース（存在しない場合はnull）</returns>
        public static KonoAssetWearablesDatabase? GetKAWearablesDatabase() => kaWearablesDatabase;

        /// <summary>
        /// KonoAssetワールドオブジェクトデータベースを取得する
        /// </summary>
        /// <returns>データベース（存在しない場合はnull）</returns>
        public static KonoAssetWorldObjectsDatabase? GetKAWorldObjectsDatabase() =>
            kaWorldObjectsDatabase;
    }
}
