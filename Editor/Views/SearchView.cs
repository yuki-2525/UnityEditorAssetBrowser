// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using UnityEditor;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.ViewModels;
using UnityEngine;

namespace UnityEditorAssetBrowser.Views
{
    public class SearchView
    {
        private readonly SearchViewModel _searchViewModel;
        private readonly AssetBrowserViewModel _assetBrowserViewModel;
        private readonly PaginationViewModel _paginationViewModel;

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
                () => _assetBrowserViewModel.GetFilteredWorldObjects()
            );
            EditorGUILayout.LabelField($"検索結果: {totalItems}件");
            EditorGUILayout.Space(10);
        }

        public void DrawDatabasePathFields(
            ref string aeDatabasePath,
            ref string kaDatabasePath,
            Action onAePathChanged,
            Action onKaPathChanged
        )
        {
            DrawDatabasePathField("AE Database Path:", ref aeDatabasePath, onAePathChanged);
            DrawDatabasePathField("KA Database Path:", ref kaDatabasePath, onKaPathChanged);

            // リフレッシュボタンを追加（一つにまとめる）
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("更新", GUILayout.Width(100)))
            {
                onAePathChanged();
                onKaPathChanged();
                GUI.changed = true;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
        }

        private void DrawDatabasePathField(string label, ref string path, Action onPathChanged)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(100));
            EditorGUILayout.TextField(path, EditorStyles.textField);

            // パスが入力されている時のみ削除ボタンを表示
            if (!string.IsNullOrEmpty(path))
            {
                if (GUILayout.Button("×", GUILayout.Width(20)))
                {
                    path = string.Empty;
                    onPathChanged?.Invoke();
                    // パスが削除されたらデータベースをクリア
                    if (label.Contains("AE"))
                    {
                        _assetBrowserViewModel.ClearAEDatabase();
                    }
                    else if (label.Contains("KA"))
                    {
                        _assetBrowserViewModel.ClearKADatabase();
                    }
                }
            }

            if (GUILayout.Button("参照", GUILayout.Width(60)))
            {
                string newPath = EditorUtility.OpenFolderPanel(label, "", "");
                if (!string.IsNullOrEmpty(newPath))
                {
                    path = newPath;
                    onPathChanged?.Invoke();
                    // パスが変更されたらデータベースを読み込む
                    if (label.Contains("AE"))
                    {
                        _assetBrowserViewModel.LoadAEDatabase(path);
                    }
                    else if (label.Contains("KA"))
                    {
                        _assetBrowserViewModel.LoadKADatabase(path);
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
