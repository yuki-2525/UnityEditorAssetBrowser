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
        private readonly PaginationInfo _paginationInfo = new();

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
        private readonly string[] tabs = { "アバター", "アバター関連", "ワールド", "その他" };

        /// <summary>フォールドアウト状態の管理</summary>
        private readonly Dictionary<string, bool> foldouts = new();

        /// <summary>画像のキャッシュ</summary>
        private Dictionary<string, Texture2D> imageCache => ImageServices.Instance.imageCache;

        /// <summary>メモのフォールドアウト状態の管理</summary>
        private readonly Dictionary<string, bool> memoFoldouts = new();

        /// <summary>UnityPackageのフォールドアウト状態の管理</summary>
        private readonly Dictionary<string, bool> unityPackageFoldouts = new();

        /// <summary>ソート方法のラベル</summary>
        private readonly string[] sortLabels =
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
            // 除外フォルダ初期化と合成済みリスト保存
            ExcludeFolderService.InitializeDefaultExcludeFolders();
            var prefs = ExcludeFolderService.LoadPrefs();
            var combined = new List<string>();
            if (prefs != null)
            {
                combined.AddRange(prefs.userFolders);
                combined.AddRange(prefs.enabledDefaults);
            }
            ExcludeFolderService.SaveCombinedExcludePatterns(combined);

            InitializeCategoryAssetTypes();
            InitializeServices();
            InitializeViewModels();
            InitializeViews();
            RegisterEventHandlers();
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
                DatabaseService.GetKAOtherAssetsDatabase(),
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
                DatabaseService.GetAEDatabase()
            );
        }

        /// <summary>
        /// イベントハンドラの登録
        /// </summary>
        private void RegisterEventHandlers()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        /// <summary>
        /// ウィンドウが無効になった時の処理
        /// </summary>
        private void OnDisable()
        {
            UnregisterEventHandlers();
        }

        /// <summary>
        /// イベントハンドラの解除
        /// </summary>
        private void UnregisterEventHandlers()
        {
            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        /// <summary>
        /// シーン階層が変更された時の処理
        /// </summary>
        private void OnHierarchyChanged()
        {
            string aeDatabasePath = DatabaseService.GetAEDatabasePath();
            string kaDatabasePath = DatabaseService.GetKADatabasePath();

            // 画像キャッシュをクリア
            ImageServices.Instance.ClearCache();

            // データベースを再読み込み
            DatabaseService.LoadAEDatabase();
            DatabaseService.LoadKADatabase();
            _searchViewModel.SetCurrentTab(_paginationViewModel.SelectedTab);

            // 現在表示中のアイテムの画像を再読み込み
            var currentItems = _assetBrowserViewModel.GetCurrentTabItems(
                _paginationViewModel.SelectedTab
            );
            ImageServices.Instance.ReloadCurrentItemsImages(
                currentItems,
                aeDatabasePath,
                kaDatabasePath
            );
        }

        /// <summary>
        /// 画像キャッシュの更新（非推奨 - 新しい実装では自動管理）
        /// 新しい実装では表示時に自動的に必要な画像のみ読み込まれる
        /// </summary>
        private void UpdateImageCache()
        {
            // 新しい実装では画像キャッシュは表示時に自動管理されるため、
            // ここではキャッシュクリアのみ実行
            ImageServices.Instance.ClearCache();
        }

        /// <summary>
        /// GUIの描画処理
        /// </summary>
        private void OnGUI()
        {
            _mainView.DrawMainWindow();
        }

        /// <summary>
        /// カテゴリごとのアセットタイプ設定を初期化
        /// EditorPrefsから値を読み込むか、デフォルト値を設定する
        /// </summary>
        private void InitializeCategoryAssetTypes()
        {
            // 初期値の設定
            var defaultTypes = new Dictionary<string, int>
            {
                { "アバター", AssetTypeConstants.AVATAR },
                { "衣装", AssetTypeConstants.AVATAR_RELATED },
                { "テクスチャ", AssetTypeConstants.AVATAR_RELATED },
                { "ギミック", AssetTypeConstants.AVATAR_RELATED },
                { "アクセサリー", AssetTypeConstants.AVATAR_RELATED },
                { "髪型", AssetTypeConstants.AVATAR_RELATED },
                { "アニメーション", AssetTypeConstants.AVATAR_RELATED },
                { "ツール", AssetTypeConstants.OTHER },
                { "シェーダー", AssetTypeConstants.OTHER },
            };

            // 指定された順序のカテゴリの初期化
            var orderedCategories = new[]
            {
                "アバター",
                "衣装",
                "テクスチャ",
                "ギミック",
                "アクセサリー",
                "髪型",
                "アニメーション",
                "ツール",
                "シェーダー",
            };

            foreach (var category in orderedCategories)
            {
                var key = "UnityEditorAssetBrowser_CategoryAssetType_" + category;
                if (!EditorPrefs.HasKey(key) && defaultTypes.ContainsKey(category))
                {
                    EditorPrefs.SetInt(key, defaultTypes[category]);
                }
            }

            // その他のカテゴリの初期化
            var aeDatabase = DatabaseService.GetAEDatabase();
            if (aeDatabase != null)
            {
                var otherCategories = aeDatabase
                    .Items.Select(item => item.GetAECategoryName())
                    .Distinct()
                    .Where(category => !orderedCategories.Contains(category))
                    .OrderBy(category => category);

                foreach (var category in otherCategories)
                {
                    var key = "UnityEditorAssetBrowser_CategoryAssetType_" + category;
                    if (!EditorPrefs.HasKey(key))
                    {
                        var defaultValue = GetDefaultAssetTypeForCategory(category);
                        EditorPrefs.SetInt(key, defaultValue);
                    }
                }
            }
        }

        /// <summary>
        /// カテゴリのデフォルトアセットタイプを取得
        /// </summary>
        /// <param name="category">カテゴリ名</param>
        /// <returns>デフォルトのアセットタイプのインデックス</returns>
        private int GetDefaultAssetTypeForCategory(string category)
        {
            if (category.Contains("ワールド") || category.Contains("world"))
            {
                return AssetTypeConstants.WORLD;
            }
            return AssetTypeConstants.OTHER;
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
    }
}
