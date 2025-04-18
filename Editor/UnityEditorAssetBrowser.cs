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
        private PaginationViewModel _paginationViewModel;
        private AssetBrowserViewModel _assetBrowserViewModel;
        private SearchViewModel _searchViewModel;
        private SearchView _searchView;
        private PaginationView _paginationView;

        /// <summary>AvatarExplorerのデータベース</summary>
        private AvatarExplorerDatabase? aeDatabase;

        /// <summary>KonoAssetのアバターデータベース</summary>
        private KonoAssetAvatarsDatabase? kaAvatarsDatabase;

        /// <summary>KonoAssetのウェアラブルデータベース</summary>
        private KonoAssetWearablesDatabase? kaWearablesDatabase;

        /// <summary>KonoAssetのワールドオブジェクトデータベース</summary>
        private KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase;

        /// <summary>スクロールビューの位置</summary>
        private Vector2 scrollPosition;

        /// <summary>AEデータベースのパス</summary>
        private string aeDatabasePath = "";

        /// <summary>KAデータベースのパス</summary>
        private string kaDatabasePath = "";

        /// <summary>詳細検索の表示状態</summary>
        private bool showAdvancedSearch => _searchViewModel.SearchCriteria.ShowAdvancedSearch;

        /// <summary>タブのラベル</summary>
        private string[] tabs = { "アバター", "アバター関連", "ワールド" };

        /// <summary>フォールドアウト状態の管理</summary>
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();

        /// <summary>画像のキャッシュ</summary>
        private Dictionary<string, Texture2D> imageCache => ImageServices.Instance.imageCache;

        /// <summary>タイトル用のスタイル</summary>
        private GUIStyle? titleStyle;

        /// <summary>ボックス用のスタイル</summary>
        private GUIStyle? boxStyle;

        /// <summary>メモのフォールドアウト状態の管理</summary>
        private Dictionary<string, bool> memoFoldouts = new Dictionary<string, bool>();

        /// <summary>UnityPackageのフォールドアウト状態の管理</summary>
        private Dictionary<string, bool> unityPackageFoldouts = new Dictionary<string, bool>();

        /// <summary>ソート方法の列挙型</summary>
        private enum SortMethod
        {
            /// <summary>追加順（新しい順）</summary>
            CreatedDateDesc,

            /// <summary>追加順（古い順）</summary>
            CreatedDateAsc,

            /// <summary>アセット名（A-Z順）</summary>
            TitleAsc,

            /// <summary>アセット名（Z-A順）</summary>
            TitleDesc,

            /// <summary>ショップ名（A-Z順）</summary>
            AuthorAsc,

            /// <summary>ショップ名（Z-A順）</summary>
            AuthorDesc,
        }

        /// <summary>現在のソート方法</summary>
        private SortMethod currentSortMethod = SortMethod.CreatedDateDesc;

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

        private AssetItem assetItem = new AssetItem();
        private AssetItemView _assetItemView;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public UnityEditorAssetBrowser()
        {
            _paginationViewModel = new PaginationViewModel(_paginationInfo);
            _searchViewModel = new SearchViewModel(null, null, null, null);
            _assetBrowserViewModel = new AssetBrowserViewModel(
                aeDatabase,
                kaAvatarsDatabase,
                kaWearablesDatabase,
                kaWorldObjectsDatabase,
                _paginationInfo,
                _searchViewModel
            );
            _searchView = new SearchView(
                _searchViewModel,
                _assetBrowserViewModel,
                _paginationViewModel
            );
            _paginationView = new PaginationView(_paginationViewModel, _assetBrowserViewModel);

            // パスを取得
            aeDatabasePath = DatabaseService.GetAEDatabasePath();
            kaDatabasePath = DatabaseService.GetKADatabasePath();

            // AssetItemViewのインスタンスを作成
            _assetItemView = new AssetItemView(aeDatabasePath, kaDatabasePath, aeDatabase);
        }
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
            // DatabaseServiceから設定を読み込む
            DatabaseService.LoadSettings();

            // パスを取得
            aeDatabasePath = DatabaseService.GetAEDatabasePath();
            kaDatabasePath = DatabaseService.GetKADatabasePath();

            // データベースを読み込む
            if (!string.IsNullOrEmpty(aeDatabasePath))
            {
                _assetBrowserViewModel.LoadAEDatabase(aeDatabasePath);
            }
            if (!string.IsNullOrEmpty(kaDatabasePath))
            {
                _assetBrowserViewModel.LoadKADatabase(kaDatabasePath);
            }

            // AssetItemViewのインスタンスを作成
            _assetItemView = new AssetItemView(aeDatabasePath, kaDatabasePath, aeDatabase);

            // シーン変更時に自動的に更新を実行
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
            if (!string.IsNullOrEmpty(aeDatabasePath) && !string.IsNullOrEmpty(kaDatabasePath))
            {
                RefreshImageCache();
                _searchViewModel.SetCurrentTab(_paginationViewModel.SelectedTab);
            }
        }

        /// <summary>
        /// GUIの描画処理
        /// </summary>
        private void OnGUI()
        {
            InitializeStyles();
            DrawMainWindow();
        }
        #endregion

        #region UI Drawing Methods
        /// <summary>
        /// メインウィンドウの描画
        /// </summary>
        private void DrawMainWindow()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);

            // データベースパスフィールドと検索フィールドを描画
            _searchView.DrawDatabasePathFields(
                ref aeDatabasePath,
                ref kaDatabasePath,
                () =>
                {
                    _assetBrowserViewModel.LoadAEDatabase(aeDatabasePath);
                    // AssetItemViewのインスタンスを再作成
                    _assetItemView = new AssetItemView(aeDatabasePath, kaDatabasePath, aeDatabase);
                },
                () =>
                {
                    _assetBrowserViewModel.LoadKADatabase(kaDatabasePath);
                    // AssetItemViewのインスタンスを再作成
                    _assetItemView = new AssetItemView(aeDatabasePath, kaDatabasePath, aeDatabase);
                }
            );

            DrawTabBar();
            _searchView.DrawSearchField();
            _searchView.DrawSearchResultCount();

            DrawContentArea();

            if (GUI.changed)
            {
                // DatabaseServiceにパスを保存
                DatabaseService.SetAEDatabasePath(aeDatabasePath);
                DatabaseService.SetKADatabasePath(kaDatabasePath);
                DatabaseService.SaveSettings();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// タブバーの描画
        /// </summary>
        private void DrawTabBar()
        {
            var newTab = GUILayout.Toolbar(_paginationViewModel.SelectedTab, tabs);
            if (newTab != _paginationViewModel.SelectedTab)
            {
                _paginationViewModel.SelectedTab = newTab;
                _paginationViewModel.ResetPage();

                // タブが切り替わったときにSearchViewModelに通知
                _searchViewModel.SetCurrentTab(_paginationViewModel.SelectedTab);

                Repaint();
            }
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// コンテンツエリアの描画
        /// </summary>
        private void DrawContentArea()
        {
            GUILayout.BeginVertical();
            DrawScrollView();
            _paginationView.DrawPaginationButtons();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// スクロールビューの描画
        /// </summary>
        private void DrawScrollView()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition,
                GUILayout.ExpandHeight(true)
            );
            DrawCurrentTabContent();
            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// 現在のタブのコンテンツを描画
        /// </summary>
        private void DrawCurrentTabContent()
        {
            switch (_paginationViewModel.SelectedTab)
            {
                case 0:
                    ShowAvatarsContent();
                    break;
                case 1:
                    ShowItemsContent();
                    break;
                case 2:
                    ShowWorldObjectsContent();
                    break;
            }
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
            return category.Contains(WORLD_CATEGORY_JP, StringComparison.OrdinalIgnoreCase)
                || category.Contains(WORLD_CATEGORY_EN, StringComparison.OrdinalIgnoreCase);
        }
        #endregion

        /// <summary>
        /// GUIスタイルの初期化
        /// </summary>
        private void InitializeStyles()
        {
            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 14,
                    margin = new RectOffset(4, 4, 4, 4),
                };
            }

            if (boxStyle == null)
            {
                boxStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(10, 10, 10, 10),
                    margin = new RectOffset(0, 0, 5, 5),
                };
            }
        }

        /// <summary>
        /// アバターコンテンツの表示
        /// </summary>
        private void ShowAvatarsContent()
        {
            var filteredItems = GetFilteredAvatars();
            var sortedItems = SortItems(filteredItems);
            var pageItems = _paginationViewModel.GetCurrentPageItems(sortedItems);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    _assetItemView.ShowAvatarItem(aeItem, false, false);
                }
                else if (item is KonoAssetAvatarItem kaItem)
                {
                    _assetItemView.ShowKonoAssetItem(kaItem, false, false);
                }
            }
        }

        /// <summary>
        /// アバター関連アイテムコンテンツの表示
        /// </summary>
        private void ShowItemsContent()
        {
            var filteredItems = GetFilteredItems();
            var sortedItems = SortItems(filteredItems);
            var pageItems = _paginationViewModel.GetCurrentPageItems(sortedItems);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    _assetItemView.ShowAvatarItem(aeItem, true, true);
                }
                else if (item is KonoAssetWearableItem kaItem)
                {
                    _assetItemView.ShowKonoAssetWearableItem(kaItem, true);
                }
            }
        }

        /// <summary>
        /// ワールドオブジェクトコンテンツの表示
        /// </summary>
        private void ShowWorldObjectsContent()
        {
            var filteredItems = GetFilteredWorldObjects();
            var sortedItems = SortItems(filteredItems);
            var pageItems = _paginationViewModel.GetCurrentPageItems(sortedItems);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    _assetItemView.ShowAvatarItem(aeItem, true, false);
                }
                else if (item is KonoAssetWorldObjectItem worldItem)
                {
                    _assetItemView.ShowKonoAssetWorldObjectItem(worldItem);
                }
            }
        }

        /// <summary>
        /// 総ページ数を取得
        /// </summary>
        /// <returns>総ページ数</returns>
        private int GetTotalPages()
        {
            var currentItems = _assetBrowserViewModel.GetCurrentTabItems(
                _paginationViewModel.SelectedTab
            );
            return _assetBrowserViewModel.GetTotalPages(currentItems);
        }

        /// <summary>
        /// フィルターされたアバターリストを取得
        /// </summary>
        /// <returns>フィルターされたアバターリスト</returns>
        private List<object> GetFilteredAvatars() => _assetBrowserViewModel.GetFilteredAvatars();

        /// <summary>
        /// フィルターされたアイテムリストを取得
        /// </summary>
        /// <returns>フィルターされたアイテムリスト</returns>
        private List<object> GetFilteredItems() => _assetBrowserViewModel.GetFilteredItems();

        /// <summary>
        /// フィルターされたワールドオブジェクトリストを取得
        /// </summary>
        /// <returns>フィルターされたワールドオブジェクトリスト</returns>
        private List<object> GetFilteredWorldObjects() =>
            _assetBrowserViewModel.GetFilteredWorldObjects();

        /// <summary>
        /// アイテムをソートする
        /// </summary>
        /// <param name="items">ソートするアイテムリスト</param>
        /// <returns>ソートされたアイテムリスト</returns>
        public List<object> SortItems(List<object> items) =>
            _assetBrowserViewModel.SortItems(items);

        /// <summary>
        /// 現在のタブのアイテムを取得
        /// </summary>
        /// <returns>現在のタブのアイテムリスト</returns>
        private List<object> GetCurrentTabItems() =>
            _assetBrowserViewModel.GetCurrentTabItems(_paginationViewModel.SelectedTab);

        /// <summary>
        /// ソート方法を設定
        /// </summary>
        private void SetSortMethod(SortMethod method)
        {
            if (currentSortMethod != method)
            {
                currentSortMethod = method;
                _assetBrowserViewModel.SetSortMethod((AssetBrowserViewModel.SortMethod)method);
                _paginationViewModel.ResetPage();
            }
        }

        /// <summary>
        /// データベースを再読み込みする
        /// </summary>
        private void RefreshDatabases()
        {
            // 画像キャッシュをクリア
            ImageServices.Instance.ClearCache();

            // データベースを再読み込み
            if (!string.IsNullOrEmpty(aeDatabasePath))
            {
                _assetBrowserViewModel.LoadAEDatabase(aeDatabasePath);
            }

            if (!string.IsNullOrEmpty(kaDatabasePath))
            {
                _assetBrowserViewModel.LoadKADatabase(kaDatabasePath);
            }

            // ページをリセット
            _paginationViewModel.ResetPage();
        }

        /// <summary>
        /// 画像キャッシュを再取得する
        /// </summary>
        private void RefreshImageCache()
        {
            _assetBrowserViewModel.RefreshImageCache(aeDatabasePath, kaDatabasePath);
        }

        /// <summary>
        /// AEデータベースの読み込みと表示
        /// </summary>
        private void LoadAndDisplayAEDatabase()
        {
            if (string.IsNullOrEmpty(aeDatabasePath))
            {
                aeDatabase = null;
                return;
            }

            DatabaseService.SetAEDatabasePath(aeDatabasePath);
            DatabaseService.LoadAndUpdateAEDatabase();
            aeDatabase = DatabaseService.GetAEDatabase();

            // パスが空欄になった場合（エラー時）に更新
            aeDatabasePath = DatabaseService.GetAEDatabasePath();
        }

        /// <summary>
        /// KAデータベースの読み込みと表示
        /// </summary>
        private void LoadAndDisplayKADatabase()
        {
            if (string.IsNullOrEmpty(kaDatabasePath))
            {
                kaAvatarsDatabase = null;
                kaWearablesDatabase = null;
                kaWorldObjectsDatabase = null;
                return;
            }

            DatabaseService.SetKADatabasePath(kaDatabasePath);
            DatabaseService.LoadAndUpdateKADatabase();
            kaAvatarsDatabase = DatabaseService.GetKAAvatarsDatabase();
            kaWearablesDatabase = DatabaseService.GetKAWearablesDatabase();
            kaWorldObjectsDatabase = DatabaseService.GetKAWorldObjectsDatabase();

            // パスが空欄になった場合（エラー時）に更新
            kaDatabasePath = DatabaseService.GetKADatabasePath();
        }
    }
}
