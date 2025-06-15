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
using UnityEditorAssetBrowser.ViewModels;
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
        /// KonoAssetのその他アセットデータベース
        /// </summary>
        private static KonoAssetOtherAssetsDatabase? kaOtherAssetsDatabase;

        private static AssetBrowserViewModel? _assetBrowserViewModel;
        private static SearchViewModel? _searchViewModel;
        private static PaginationViewModel? _paginationViewModel;

        /// <summary>
        /// ViewModelの参照を設定する
        /// </summary>
        public static void SetViewModels(
            AssetBrowserViewModel assetBrowserViewModel,
            SearchViewModel searchViewModel,
            PaginationViewModel paginationViewModel
        )
        {
            _assetBrowserViewModel = assetBrowserViewModel;
            _searchViewModel = searchViewModel;
            _paginationViewModel = paginationViewModel;
        }

        /// <summary>
        /// データベースの設定を読み込む
        /// 保存されたパスからデータベースを読み込み、更新する
        /// </summary>
        public static void LoadSettings()
        {
            aeDatabasePath = EditorPrefs.GetString(AE_DATABASE_PATH_KEY, "");
            kaDatabasePath = EditorPrefs.GetString(KA_DATABASE_PATH_KEY, "");

            if (!string.IsNullOrEmpty(aeDatabasePath))
                LoadAEDatabase();
            if (!string.IsNullOrEmpty(kaDatabasePath))
                LoadKADatabase();
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
        public static void LoadAEDatabase()
        {
            // データベースをクリア
            ClearAEDatabase();

            if (!string.IsNullOrEmpty(aeDatabasePath))
            {
                aeDatabase = AEDatabaseHelper.LoadAEDatabaseFile(aeDatabasePath);
                if (aeDatabase == null)
                {
                    OnAEDatabasePathChanged("");
                    ShowErrorDialog(
                        "パスエラー",
                        "入力したパスが誤っています\n\n\"AvatarExplorer-v1.x.x\" フォルダ\nを指定してください"
                    );
                    return;
                }
            }

            // データベースを更新
            if (
                _assetBrowserViewModel != null
                && _searchViewModel != null
                && _paginationViewModel != null
            )
            {
                _assetBrowserViewModel.UpdateDatabases(
                    GetAEDatabase(),
                    GetKAAvatarsDatabase(),
                    GetKAWearablesDatabase(),
                    GetKAWorldObjectsDatabase(),
                    GetKAOtherAssetsDatabase()
                );
                _searchViewModel.SetCurrentTab(_paginationViewModel.SelectedTab);
            }
        }

        /// <summary>
        /// KonoAssetデータベースを読み込み、更新する
        /// パスが無効な場合はエラーメッセージを表示し、パスをリセットする
        /// </summary>
        public static void LoadKADatabase()
        {
            // データベースをクリア
            ClearKADatabase();

            if (!string.IsNullOrEmpty(kaDatabasePath))
            {
                var metadataPath = Path.Combine(kaDatabasePath, "metadata");
                if (!Directory.Exists(metadataPath))
                {
                    OnKADatabasePathChanged("");
                    ShowErrorDialog(
                        "パスエラー",
                        "入力したパスが誤っています\n\nKonoAssetの設定にある\n\"アプリデータの保存先\"と\n同一のディレクトリを指定してください"
                    );
                    return;
                }

                var result = KADatabaseHelper.LoadKADatabaseFiles(metadataPath);
                kaAvatarsDatabase = result.avatarsDatabase;
                kaWearablesDatabase = result.wearablesDatabase;
                kaWorldObjectsDatabase = result.worldObjectsDatabase;
                kaOtherAssetsDatabase = result.otherAssetsDatabase;
            }

            // データベースを更新
            if (
                _assetBrowserViewModel != null
                && _searchViewModel != null
                && _paginationViewModel != null
            )
            {
                _assetBrowserViewModel.UpdateDatabases(
                    GetAEDatabase(),
                    GetKAAvatarsDatabase(),
                    GetKAWearablesDatabase(),
                    GetKAWorldObjectsDatabase(),
                    GetKAOtherAssetsDatabase()
                );
                _searchViewModel.SetCurrentTab(_paginationViewModel.SelectedTab);
            }
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

        public static void OnAEDatabasePathChanged(string path)
        {
            SetAEDatabasePath(path);
            if (string.IsNullOrEmpty(path))
            {
                // パスが空の場合は、データベースをクリアして即座に更新
                ClearAEDatabase();
                if (
                    _assetBrowserViewModel != null
                    && _searchViewModel != null
                    && _paginationViewModel != null
                )
                {
                    _assetBrowserViewModel.UpdateDatabases(
                        null,
                        GetKAAvatarsDatabase(),
                        GetKAWearablesDatabase(),
                        GetKAWorldObjectsDatabase(),
                        GetKAOtherAssetsDatabase()
                    );
                    // SetCurrentTabは呼ばない
                }
            }
            else
            {
                LoadAEDatabase();
            }
            SaveSettings();
        }

        public static void OnKADatabasePathChanged(string path)
        {
            SetKADatabasePath(path);
            if (string.IsNullOrEmpty(path))
            {
                // パスが空の場合は、データベースをクリアして即座に更新
                ClearKADatabase();
                if (
                    _assetBrowserViewModel != null
                    && _searchViewModel != null
                    && _paginationViewModel != null
                )
                {
                    _assetBrowserViewModel.UpdateDatabases(GetAEDatabase(), null, null, null, null);
                    // SetCurrentTabは呼ばない
                }
            }
            else
            {
                LoadKADatabase();
            }
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

        /// <summary>
        /// KonoAssetその他アセットデータベースを取得する
        /// </summary>
        /// <returns>データベース（存在しない場合はnull）</returns>
        public static KonoAssetOtherAssetsDatabase? GetKAOtherAssetsDatabase() =>
            kaOtherAssetsDatabase;

        /// <summary>
        /// AvatarExplorerデータベースをクリアする
        /// </summary>
        public static void ClearAEDatabase()
        {
            aeDatabase = null;
        }

        /// <summary>
        /// KonoAssetデータベースをクリアする
        /// </summary>
        public static void ClearKADatabase()
        {
            kaAvatarsDatabase = null;
            kaWearablesDatabase = null;
            kaWorldObjectsDatabase = null;
            kaOtherAssetsDatabase = null;
        }
    }
}
