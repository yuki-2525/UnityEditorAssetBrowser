// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using UnityEditor;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Services;
using UnityEditorAssetBrowser.ViewModels;
using UnityEditorAssetBrowser.Windows;
using UnityEngine;

namespace UnityEditorAssetBrowser.Views
{
    public class SearchView
    {
        private readonly SearchViewModel _searchViewModel;
        private readonly AssetBrowserViewModel _assetBrowserViewModel;
        private readonly PaginationViewModel _paginationViewModel;
        private int _lastSelectedTab = -1; // 前回選択されていたタブを記録

        public SearchView(
            SearchViewModel searchViewModel,
            AssetBrowserViewModel assetBrowserViewModel,
            PaginationViewModel paginationViewModel
        )
        {
            _searchViewModel = searchViewModel;
            _assetBrowserViewModel = assetBrowserViewModel;
            _paginationViewModel = paginationViewModel;
        }

        public void DrawSearchField()
        {
            // タブが変更された場合の処理
            CheckTabChange();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 基本検索フィールド
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("検索:", GUILayout.Width(60));
            var newSearchQuery = EditorGUILayout.TextField(
                _searchViewModel.SearchCriteria.SearchQuery
            );
            if (newSearchQuery != _searchViewModel.SearchCriteria.SearchQuery)
            {
                _searchViewModel.SearchCriteria.SearchQuery = newSearchQuery;
                _paginationViewModel.ResetPage();
                OnSearchResultChanged();
                GUI.changed = true;
            }

            // 詳細検索のトグル
            var newShowAdvancedSearch = EditorGUILayout.ToggleLeft(
                "詳細検索",
                _searchViewModel.SearchCriteria.ShowAdvancedSearch,
                GUILayout.Width(100)
            );
            if (newShowAdvancedSearch != _searchViewModel.SearchCriteria.ShowAdvancedSearch)
            {
                _searchViewModel.SearchCriteria.ShowAdvancedSearch = newShowAdvancedSearch;
                _paginationViewModel.ResetPage();
                GUI.changed = true;
            }

            // クリアボタン
            if (GUILayout.Button("クリア", GUILayout.Width(60)))
            {
                _searchViewModel.ClearSearchCriteria();
                _paginationViewModel.ResetPage();
                OnSearchResultChanged();
                GUI.changed = true;
            }

            // ソートボタン
            if (GUILayout.Button("▼ 表示順", GUILayout.Width(80)))
            {
                var menu = new GenericMenu();
                menu.AddItem(
                    new GUIContent("追加順（新しい順）"),
                    _assetBrowserViewModel.CurrentSortMethod
                        == AssetBrowserViewModel.SortMethod.CreatedDateDesc,
                    () =>
                        _assetBrowserViewModel.SetSortMethod(
                            AssetBrowserViewModel.SortMethod.CreatedDateDesc
                        )
                );
                menu.AddItem(
                    new GUIContent("追加順（古い順）"),
                    _assetBrowserViewModel.CurrentSortMethod
                        == AssetBrowserViewModel.SortMethod.CreatedDateAsc,
                    () =>
                        _assetBrowserViewModel.SetSortMethod(
                            AssetBrowserViewModel.SortMethod.CreatedDateAsc
                        )
                );
                menu.AddItem(
                    new GUIContent("アセット名（A-Z順）"),
                    _assetBrowserViewModel.CurrentSortMethod
                        == AssetBrowserViewModel.SortMethod.TitleAsc,
                    () =>
                        _assetBrowserViewModel.SetSortMethod(
                            AssetBrowserViewModel.SortMethod.TitleAsc
                        )
                );
                menu.AddItem(
                    new GUIContent("アセット名（Z-A順）"),
                    _assetBrowserViewModel.CurrentSortMethod
                        == AssetBrowserViewModel.SortMethod.TitleDesc,
                    () =>
                        _assetBrowserViewModel.SetSortMethod(
                            AssetBrowserViewModel.SortMethod.TitleDesc
                        )
                );
                menu.AddItem(
                    new GUIContent("ショップ名（A-Z順）"),
                    _assetBrowserViewModel.CurrentSortMethod
                        == AssetBrowserViewModel.SortMethod.AuthorAsc,
                    () =>
                        _assetBrowserViewModel.SetSortMethod(
                            AssetBrowserViewModel.SortMethod.AuthorAsc
                        )
                );
                menu.AddItem(
                    new GUIContent("ショップ名（Z-A順）"),
                    _assetBrowserViewModel.CurrentSortMethod
                        == AssetBrowserViewModel.SortMethod.AuthorDesc,
                    () =>
                        _assetBrowserViewModel.SetSortMethod(
                            AssetBrowserViewModel.SortMethod.AuthorDesc
                        )
                );
                menu.AddItem(
                    new GUIContent("BOOTHID順（新しい順）"),
                    _assetBrowserViewModel.CurrentSortMethod
                        == AssetBrowserViewModel.SortMethod.BoothIdDesc,
                    () =>
                        _assetBrowserViewModel.SetSortMethod(
                            AssetBrowserViewModel.SortMethod.BoothIdDesc
                        )
                );
                menu.AddItem(
                    new GUIContent("BOOTHID順（古い順）"),
                    _assetBrowserViewModel.CurrentSortMethod
                        == AssetBrowserViewModel.SortMethod.BoothIdAsc,
                    () =>
                        _assetBrowserViewModel.SetSortMethod(
                            AssetBrowserViewModel.SortMethod.BoothIdAsc
                        )
                );
                menu.ShowAsContext();
            }
            EditorGUILayout.EndHorizontal();

            // 詳細検索フィールド
            if (_searchViewModel.SearchCriteria.ShowAdvancedSearch)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // タイトル検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("タイトル:", GUILayout.Width(100));
                var newTitleSearch = EditorGUILayout.TextField(
                    _searchViewModel.SearchCriteria.TitleSearch
                );
                if (newTitleSearch != _searchViewModel.SearchCriteria.TitleSearch)
                {
                    _searchViewModel.SearchCriteria.TitleSearch = newTitleSearch;
                    _paginationViewModel.ResetPage();
                    OnSearchResultChanged();
                    GUI.changed = true;
                }
                EditorGUILayout.EndHorizontal();

                // 作者名検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("作者名:", GUILayout.Width(100));
                var newAuthorSearch = EditorGUILayout.TextField(
                    _searchViewModel.SearchCriteria.AuthorSearch
                );
                if (newAuthorSearch != _searchViewModel.SearchCriteria.AuthorSearch)
                {
                    _searchViewModel.SearchCriteria.AuthorSearch = newAuthorSearch;
                    _paginationViewModel.ResetPage();
                    OnSearchResultChanged();
                    GUI.changed = true;
                }
                EditorGUILayout.EndHorizontal();

                // カテゴリ検索（アバタータブ以外で表示）
                if (_paginationViewModel.SelectedTab != 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("カテゴリ:", GUILayout.Width(100));
                    var newCategorySearch = EditorGUILayout.TextField(
                        _searchViewModel.SearchCriteria.CategorySearch
                    );
                    if (newCategorySearch != _searchViewModel.SearchCriteria.CategorySearch)
                    {
                        _searchViewModel.SearchCriteria.CategorySearch = newCategorySearch;
                        _paginationViewModel.ResetPage();
                        OnSearchResultChanged();
                        GUI.changed = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // 対応アバター検索（アイテムタブのみで表示）
                if (_paginationViewModel.SelectedTab == 1)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("対応アバター:", GUILayout.Width(100));
                    var newSupportedAvatarsSearch = EditorGUILayout.TextField(
                        _searchViewModel.SearchCriteria.SupportedAvatarsSearch
                    );
                    if (
                        newSupportedAvatarsSearch
                        != _searchViewModel.SearchCriteria.SupportedAvatarsSearch
                    )
                    {
                        _searchViewModel.SearchCriteria.SupportedAvatarsSearch =
                            newSupportedAvatarsSearch;
                        _paginationViewModel.ResetPage();
                        OnSearchResultChanged();
                        GUI.changed = true;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                // タグ検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("タグ:", GUILayout.Width(100));
                var newTagsSearch = EditorGUILayout.TextField(
                    _searchViewModel.SearchCriteria.TagsSearch
                );
                if (newTagsSearch != _searchViewModel.SearchCriteria.TagsSearch)
                {
                    _searchViewModel.SearchCriteria.TagsSearch = newTagsSearch;
                    _paginationViewModel.ResetPage();
                    OnSearchResultChanged();
                    GUI.changed = true;
                }
                EditorGUILayout.EndHorizontal();

                // メモ検索
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("メモ:", GUILayout.Width(100));
                var newMemoSearch = EditorGUILayout.TextField(
                    _searchViewModel.SearchCriteria.MemoSearch
                );
                if (newMemoSearch != _searchViewModel.SearchCriteria.MemoSearch)
                {
                    _searchViewModel.SearchCriteria.MemoSearch = newMemoSearch;
                    _paginationViewModel.ResetPage();
                    OnSearchResultChanged();
                    GUI.changed = true;
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        public void DrawSearchResultCount()
        {
            int totalItems = _paginationViewModel.GetCurrentTabItemCount(
                () => _assetBrowserViewModel.GetFilteredAvatars(),
                () => _assetBrowserViewModel.GetFilteredItems(),
                () => _assetBrowserViewModel.GetFilteredWorldObjects(),
                () => _assetBrowserViewModel.GetFilteredOthers()
            );
            EditorGUILayout.LabelField($"検索結果: {totalItems}件");
            EditorGUILayout.Space(10);
        }

        public void DrawDatabaseButtons()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("設定", GUILayout.Width(100)))
            {
                SettingsWindow.ShowWindow(
                    _assetBrowserViewModel,
                    _searchViewModel,
                    _paginationViewModel
                );
            }
            if (GUILayout.Button("更新", GUILayout.Width(100)))
            {
                // データベースを更新
                DatabaseService.LoadAEDatabase();
                DatabaseService.LoadKADatabase();
                _searchViewModel.SetCurrentTab(_paginationViewModel.SelectedTab);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// 検索結果が変更された時の処理
        /// 画像キャッシュを新しい表示アイテムに更新する
        /// </summary>
        private void OnSearchResultChanged()
        {
            // 現在のタブの新しい検索結果を取得
            var filteredItems = GetCurrentTabFilteredItems();
            var sortedItems = _assetBrowserViewModel.SortItems(filteredItems);
            var pageItems = _paginationViewModel.GetCurrentPageItems(sortedItems);

            // 検索結果数に応じてキャッシュサイズを調整
            ImageServices.Instance.AdaptCacheSizeToSearchResults(filteredItems.Count);

            // 画像キャッシュを新しい表示アイテムに更新
            ImageServices.Instance.UpdateVisibleImages(
                pageItems,
                DatabaseService.GetAEDatabasePath(),
                DatabaseService.GetKADatabasePath()
            );
        }

        /// <summary>
        /// 現在のタブのフィルターされたアイテムを取得
        /// </summary>
        private System.Collections.Generic.List<object> GetCurrentTabFilteredItems()
        {
            return _paginationViewModel.SelectedTab switch
            {
                0 => _assetBrowserViewModel.GetFilteredAvatars(),
                1 => _assetBrowserViewModel.GetFilteredItems(),
                2 => _assetBrowserViewModel.GetFilteredWorldObjects(),
                3 => _assetBrowserViewModel.GetFilteredOthers(),
                _ => new System.Collections.Generic.List<object>()
            };
        }

        /// <summary>
        /// タブの変更をチェックし、変更された場合は検索欄をリセット
        /// </summary>
        private void CheckTabChange()
        {
            int currentTab = _paginationViewModel.SelectedTab;
            
            // タブが変更された場合
            if (_lastSelectedTab != -1 && _lastSelectedTab != currentTab)
            {
                // 検索条件をクリア
                _searchViewModel.ClearSearchCriteria();
                _paginationViewModel.ResetPage();
                OnSearchResultChanged();
                GUI.changed = true;
            }
            
            _lastSelectedTab = currentTab;
        }
    }
}
