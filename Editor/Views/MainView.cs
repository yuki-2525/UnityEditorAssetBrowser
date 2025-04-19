#nullable enable

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorAssetBrowser.Helper;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Services;
using UnityEditorAssetBrowser.ViewModels;
using UnityEngine;

namespace UnityEditorAssetBrowser.Views
{
    /// <summary>
    /// メインウィンドウのビュー
    /// アセットブラウザーのメインUIを管理し、タブ切り替えやコンテンツ表示を制御する
    /// </summary>
    public class MainView
    {
        /// <summary>アセットブラウザーのViewModel</summary>
        private readonly AssetBrowserViewModel _assetBrowserViewModel;

        /// <summary>検索のViewModel</summary>
        private readonly SearchViewModel _searchViewModel;

        /// <summary>ページネーションのViewModel</summary>
        private readonly PaginationViewModel _paginationViewModel;

        /// <summary>検索ビュー</summary>
        private readonly SearchView _searchView;

        /// <summary>ページネーションビュー</summary>
        private readonly PaginationView _paginationView;

        /// <summary>アセットアイテムビュー</summary>
        private AssetItemView _assetItemView;

        /// <summary>スクロール位置</summary>
        private Vector2 scrollPosition;

        /// <summary>AEデータベースのパス</summary>
        private string aeDatabasePath;

        /// <summary>KAデータベースのパス</summary>
        private string kaDatabasePath;

        /// <summary>AEデータベース</summary>
        private readonly AvatarExplorerDatabase? aeDatabase;

        /// <summary>タブのラベル</summary>
        private readonly string[] tabs = { "アバター", "アイテム", "ワールドオブジェクト" };

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="assetBrowserViewModel">アセットブラウザーのViewModel</param>
        /// <param name="searchViewModel">検索のViewModel</param>
        /// <param name="paginationViewModel">ページネーションのViewModel</param>
        /// <param name="searchView">検索ビュー</param>
        /// <param name="paginationView">ページネーションビュー</param>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        /// <param name="aeDatabase">AEデータベース</param>
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
            _assetItemView = new AssetItemView(aeDatabase);
        }

        /// <summary>
        /// メインウィンドウの描画
        /// </summary>
        public void DrawMainWindow()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);

            DrawDatabasePathFields();
            DrawTabBar();
            _searchView.DrawSearchField();
            _searchView.DrawSearchResultCount();
            DrawContentArea();

            if (GUI.changed)
            {
                SaveDatabasePaths();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// データベースパスフィールドの描画
        /// </summary>
        private void DrawDatabasePathFields()
        {
            _searchView.DrawDatabasePathFields(
                ref aeDatabasePath,
                ref kaDatabasePath,
                () =>
                {
                    _assetBrowserViewModel.LoadAEDatabase(aeDatabasePath);
                    _assetItemView = new AssetItemView(aeDatabase);
                },
                () =>
                {
                    _assetBrowserViewModel.LoadKADatabase(kaDatabasePath);
                    _assetItemView = new AssetItemView(aeDatabase);
                }
            );
        }

        /// <summary>
        /// データベースパスの保存
        /// </summary>
        private void SaveDatabasePaths()
        {
            DatabaseService.SetAEDatabasePath(aeDatabasePath);
            DatabaseService.SetKADatabasePath(kaDatabasePath);
            DatabaseService.SaveSettings();
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
                _searchViewModel.SetCurrentTab(newTab);
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

        /// <summary>
        /// アバターコンテンツの表示
        /// </summary>
        private void ShowAvatarsContent()
        {
            var items = _assetBrowserViewModel.GetFilteredAvatars();
            foreach (var item in items)
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
            var items = _assetBrowserViewModel.GetFilteredItems();
            foreach (var item in items)
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
            var items = _assetBrowserViewModel.GetFilteredWorldObjects();
            foreach (var item in items)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    _assetItemView.ShowAvatarItem(aeItem, false, false);
                }
                else if (item is KonoAssetWorldObjectItem kaItem)
                {
                    _assetItemView.ShowKonoAssetWorldObjectItem(kaItem);
                }
            }
        }
    }
}
