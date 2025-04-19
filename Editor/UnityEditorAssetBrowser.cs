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
        /// <summary>ページネーション情報</summary>
        private PaginationInfo _paginationInfo = new PaginationInfo();

        /// <summary>ページネーションのViewModel</summary>
        private PaginationViewModel _paginationViewModel = null!;

        /// <summary>アセットブラウザーのViewModel</summary>
        private AssetBrowserViewModel _assetBrowserViewModel = null!;

        /// <summary>検索のViewModel</summary>
        private SearchViewModel _searchViewModel = null!;

        /// <summary>検索ビュー</summary>
        private SearchView _searchView = null!;

        /// <summary>ページネーションビュー</summary>
        private PaginationView _paginationView = null!;

        /// <summary>メインビュー</summary>
        private MainView _mainView = null!;

        /// <summary>アイテム検索サービス</summary>
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
            InitializeServices();
            InitializeViewModels();
            InitializeViews();
            RegisterEvents();
        }

        /// <summary>
        /// サービスの初期化
        /// </summary>
        private void InitializeServices()
        {
            DatabaseService.LoadSettings();
            _itemSearchService = new ItemSearchService(DatabaseService.GetAEDatabase());
        }

        /// <summary>
        /// ViewModelの初期化
        /// </summary>
        private void InitializeViewModels()
        {
            _paginationViewModel = new PaginationViewModel(_paginationInfo);
            _searchViewModel = new SearchViewModel(DatabaseService.GetAEDatabase());
            _assetBrowserViewModel = new AssetBrowserViewModel(
                DatabaseService.GetAEDatabase(),
                DatabaseService.GetKAAvatarsDatabase(),
                DatabaseService.GetKAWearablesDatabase(),
                DatabaseService.GetKAWorldObjectsDatabase(),
                _paginationInfo,
                _searchViewModel
            );
        }

        /// <summary>
        /// Viewの初期化
        /// </summary>
        private void InitializeViews()
        {
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
        }

        /// <summary>
        /// イベントの登録
        /// </summary>
        private void RegisterEvents()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        /// <summary>
        /// ウィンドウが無効になった時の処理
        /// </summary>
        private void OnDisable()
        {
            UnregisterEvents();
        }

        /// <summary>
        /// イベントの解除
        /// </summary>
        private void UnregisterEvents()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        /// <summary>
        /// シーン階層が変更された時の処理
        /// </summary>
        private void OnHierarchyChanged()
        {
            if (AreDatabasePathsSet())
            {
                UpdateImageCache();
                UpdateSearchTab();
            }
        }

        /// <summary>
        /// データベースパスが設定されているか確認
        /// </summary>
        /// <returns>両方のパスが設定されている場合はtrue</returns>
        private bool AreDatabasePathsSet()
        {
            return !string.IsNullOrEmpty(DatabaseService.GetAEDatabasePath())
                && !string.IsNullOrEmpty(DatabaseService.GetKADatabasePath());
        }

        /// <summary>
        /// 画像キャッシュの更新
        /// </summary>
        private void UpdateImageCache()
        {
            _assetBrowserViewModel.RefreshImageCache(
                DatabaseService.GetAEDatabasePath(),
                DatabaseService.GetKADatabasePath()
            );
        }

        /// <summary>
        /// 検索タブの更新
        /// </summary>
        private void UpdateSearchTab()
        {
            _searchViewModel.SetCurrentTab(_paginationInfo.SelectedTab);
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
            ClearImageCache();
            ReloadDatabases();
            UpdateAssetBrowserDatabases();
            ResetPagination();
        }

        /// <summary>
        /// 画像キャッシュのクリア
        /// </summary>
        private void ClearImageCache()
        {
            ImageServices.Instance.ClearCache();
        }

        /// <summary>
        /// データベースの再読み込み
        /// </summary>
        private void ReloadDatabases()
        {
            DatabaseService.LoadAndUpdateAEDatabase();
            DatabaseService.LoadAndUpdateKADatabase();
        }

        /// <summary>
        /// アセットブラウザーのデータベースを更新
        /// </summary>
        private void UpdateAssetBrowserDatabases()
        {
            _assetBrowserViewModel.LoadAEDatabase(DatabaseService.GetAEDatabasePath());
            _assetBrowserViewModel.LoadKADatabase(DatabaseService.GetKADatabasePath());
        }

        /// <summary>
        /// ページネーションのリセット
        /// </summary>
        private void ResetPagination()
        {
            _paginationInfo.ResetPage();
        }
    }
}
