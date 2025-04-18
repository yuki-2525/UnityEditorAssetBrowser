#nullable enable

using UnityEditor;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Services;
using UnityEditorAssetBrowser.ViewModels;
using UnityEngine;

namespace UnityEditorAssetBrowser.Views
{
    public class MainView
    {
        private readonly AssetBrowserViewModel _assetBrowserViewModel;
        private readonly SearchViewModel _searchViewModel;
        private readonly PaginationViewModel _paginationViewModel;
        private readonly SearchView _searchView;
        private readonly PaginationView _paginationView;
        private AssetItemView _assetItemView;
        private Vector2 scrollPosition;
        private string aeDatabasePath;
        private string kaDatabasePath;
        private AvatarExplorerDatabase? aeDatabase;
        private readonly string[] tabs = new string[]
        {
            "アバター",
            "アイテム",
            "ワールドオブジェクト",
        };
        private GUIStyle? titleStyle;
        private GUIStyle? boxStyle;

        public MainView(
            AssetBrowserViewModel assetBrowserViewModel,
            SearchViewModel searchViewModel,
            PaginationViewModel paginationViewModel,
            SearchView searchView,
            PaginationView paginationView,
            string aeDatabasePath,
            string kaDatabasePath,
            AvatarExplorerDatabase? aeDatabase
        )
        {
            _assetBrowserViewModel = assetBrowserViewModel;
            _searchViewModel = searchViewModel;
            _paginationViewModel = paginationViewModel;
            _searchView = searchView;
            _paginationView = paginationView;
            this.aeDatabasePath = aeDatabasePath;
            this.kaDatabasePath = kaDatabasePath;
            this.aeDatabase = aeDatabase;
            _assetItemView = new AssetItemView(aeDatabasePath, kaDatabasePath, aeDatabase);
        }

        #region UI Drawing Methods
        /// <summary>
        /// メインウィンドウの描画
        /// </summary>
        public void DrawMainWindow()
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

                EditorWindow.focusedWindow?.Repaint();
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
            var filteredItems = _assetBrowserViewModel.GetFilteredAvatars();
            var sortedItems = _assetBrowserViewModel.SortItems(filteredItems);
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
            var filteredItems = _assetBrowserViewModel.GetFilteredItems();
            var sortedItems = _assetBrowserViewModel.SortItems(filteredItems);
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
            var filteredItems = _assetBrowserViewModel.GetFilteredWorldObjects();
            var sortedItems = _assetBrowserViewModel.SortItems(filteredItems);
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
    }
}
