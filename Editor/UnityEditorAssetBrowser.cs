// Copyright (c) 2025 yuki-2525
// This code is borrowed from AvatarExplorer(https://github.com/puk06/AvatarExplorer)
// AvatarExplorer is licensed under the MIT License. https://github.com/puk06/AvatarExplorer/blob/main/LICENSE
// This code is borrowed from AssetLibraryManager (https://github.com/MAIOTAchannel/AssetLibraryManager)
// Used with permission from MAIOTAchannel

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorAssetBrowser.Helper;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Services;
using UnityEditorAssetBrowser.ViewModels;
using UnityEditorAssetBrowser.Views;
using UnityEngine;

namespace UnityEditorAssetBrowser
{
    /// <summary>
    /// Unity Editor用のアセットブラウザーウィンドウ
    /// AvatarExplorerとKonoAssetのデータベースを統合して表示・管理する
    /// </summary>
    public class UnityEditorAssetBrowser : EditorWindow
    {
        #region Constants
        /// <summary>ウィンドウのタイトル</summary>
        private const string WINDOW_TITLE = "Asset Browser";

        /// <summary>AEデータベースパスのEditorPrefsキー</summary>
        private const string AE_DATABASE_PATH_KEY = "UnityEditorAssetBrowser_AEDatabasePath";

        /// <summary>KAデータベースパスのEditorPrefsキー</summary>
        private const string KA_DATABASE_PATH_KEY = "UnityEditorAssetBrowser_KADatabasePath";

        /// <summary>ワールドカテゴリーの日本語キーワード</summary>
        private const string WORLD_CATEGORY_JP = "ワールド";

        /// <summary>ワールドカテゴリーの英語キーワード</summary>
        private const string WORLD_CATEGORY_EN = "world";
        #endregion

        #region Fields
        private PaginationInfo _paginationInfo = new PaginationInfo();
        private PaginationViewModel _paginationViewModel = null!;
        private AssetBrowserViewModel _assetBrowserViewModel = null!;
        private SearchViewModel _searchViewModel = null!;
        private SearchView _searchView = null!;
        private PaginationView _paginationView = null!;
        private MainView _mainView = null!;
        private ItemSearchService _itemSearchService = null!;

        /// <summary>スクロールビューの位置</summary>
        private Vector2 scrollPosition;

        /// <summary>詳細検索の表示状態</summary>
        private bool showAdvancedSearch => _searchViewModel.SearchCriteria.ShowAdvancedSearch;

        /// <summary>タブのラベル</summary>
        private string[] tabs = { "アバター", "アバター関連", "ワールド" };

        /// <summary>フォールドアウト状態の管理</summary>
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        /// <summary>画像のキャッシュ</summary>
        private Dictionary<string, Texture2D> imageCache => ImageServices.Instance.imageCache;

        /// <summary>メモのフォールドアウト状態の管理</summary>
        private Dictionary<string, bool> memoFoldouts = new Dictionary<string, bool>();

        /// <summary>UnityPackageのフォールドアウト状態の管理</summary>
        private Dictionary<string, bool> unityPackageFoldouts = new Dictionary<string, bool>();

        /// <summary>ソート方法のラベル</summary>
        private string[] sortLabels =
        {
            "追加順（新しい順）",
            "追加順（古い順）",
            "アセット名（A-Z順）",
            "アセット名（Z-A順）",
            "ショップ名（A-Z順）",
            "ショップ名（Z-A順）",
        };
        #endregion

        #region Unity Editor Window Methods
        /// <summary>
        /// メニューからウィンドウを表示する
        /// </summary>
        [MenuItem("Window/Unity Editor Asset Browser")]
        public static void ShowWindow()
        {
            GetWindow<UnityEditorAssetBrowser>(WINDOW_TITLE);
        }

        /// <summary>
        /// ウィンドウが有効になった時の処理
        /// </summary>
        private void OnEnable()
        {
            // 設定を読み込み
            DatabaseService.LoadSettings();

            // サービスの初期化
            _itemSearchService = new ItemSearchService(DatabaseService.GetAEDatabase());

            // ViewModelの初期化
            _paginationViewModel = new PaginationViewModel(_paginationInfo);
            _searchViewModel = new SearchViewModel(
                DatabaseService.GetAEDatabase(),
                DatabaseService.GetKAAvatarsDatabase(),
                DatabaseService.GetKAWearablesDatabase(),
                DatabaseService.GetKAWorldObjectsDatabase()
            );
            _assetBrowserViewModel = new AssetBrowserViewModel(
                DatabaseService.GetAEDatabase(),
                DatabaseService.GetKAAvatarsDatabase(),
                DatabaseService.GetKAWearablesDatabase(),
                DatabaseService.GetKAWorldObjectsDatabase(),
                _paginationInfo,
                _searchViewModel
            );

            // Viewの初期化
            _searchView = new SearchView(
                _searchViewModel,
                _assetBrowserViewModel,
                _paginationViewModel
            );
            _paginationView = new PaginationView(_paginationViewModel, _assetBrowserViewModel);
            _mainView = new MainView(
                _assetBrowserViewModel,
                _searchViewModel,
                _paginationViewModel,
                _searchView,
                _paginationView,
                DatabaseService.GetAEDatabasePath(),
                DatabaseService.GetKADatabasePath(),
                DatabaseService.GetAEDatabase()
            );

            // シーン変更イベントの登録
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        /// <summary>
        /// ウィンドウが無効になった時の処理
        /// </summary>
        private void OnDisable()
        {
            // イベントの解除
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        /// <summary>
        /// シーン階層が変更された時の処理
        /// </summary>
        private void OnHierarchyChanged()
        {
            // パスが設定されている場合のみ画像キャッシュを更新
            if (
                !string.IsNullOrEmpty(DatabaseService.GetAEDatabasePath())
                && !string.IsNullOrEmpty(DatabaseService.GetKADatabasePath())
            )
            {
                _assetBrowserViewModel.RefreshImageCache(
                    DatabaseService.GetAEDatabasePath(),
                    DatabaseService.GetKADatabasePath()
                );
                _searchViewModel.SetCurrentTab(_paginationInfo.SelectedTab);
            }
        }

        /// <summary>
        /// GUIの描画処理
        /// </summary>
        private void OnGUI()
        {
            _mainView.DrawMainWindow();
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// カテゴリーがワールド関連かどうかを判定
        /// </summary>
        /// <param name="category">カテゴリー名</param>
        /// <returns>ワールド関連の場合true</returns>
        private bool IsWorldCategory(string category)
        {
            return _itemSearchService.IsWorldCategory(category);
        }
        #endregion

        /// <summary>
        /// データベースを再読み込みする
        /// </summary>
        private void RefreshDatabases()
        {
            // 画像キャッシュをクリア
            ImageServices.Instance.ClearCache();

            // データベースを再読み込み
            DatabaseService.LoadAndUpdateAEDatabase();
            DatabaseService.LoadAndUpdateKADatabase();

            // データベースを更新
            _assetBrowserViewModel.LoadAEDatabase(DatabaseService.GetAEDatabasePath());
            _assetBrowserViewModel.LoadKADatabase(DatabaseService.GetKADatabasePath());

            // ページをリセット
            _paginationInfo.ResetPage();
        }
    }
}
