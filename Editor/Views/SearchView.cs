// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using UnityEditor;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Services;
using UnityEditorAssetBrowser.ViewModels;
using UnityEngine;

namespace UnityEditorAssetBrowser.Views
{
    /// <summary>
    /// 検索機能のビュー
    /// 基本検索、詳細検索、データベースパス設定などのUIを提供する
    /// </summary>
    public class SearchView
    {
        /// <summary>検索のViewModel</summary>
        private readonly SearchViewModel _searchViewModel;

        /// <summary>アセットブラウザーのViewModel</summary>
        private readonly AssetBrowserViewModel _assetBrowserViewModel;

        /// <summary>ページネーションのViewModel</summary>
        private readonly PaginationViewModel _paginationViewModel;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="searchViewModel">検索のViewModel</param>
        /// <param name="assetBrowserViewModel">アセットブラウザーのViewModel</param>
        /// <param name="paginationViewModel">ページネーションのViewModel</param>
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

        /// <summary>
        /// 検索フィールドの描画
        /// </summary>
        public void DrawSearchField()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawBasicSearch();
            DrawAdvancedSearch();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// 基本検索フィールドの描画
        /// </summary>
        private void DrawBasicSearch()
        {
            EditorGUILayout.BeginHorizontal();
            DrawSearchQueryField();
            DrawAdvancedSearchToggle();
            DrawClearButton();
            DrawSortButton();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 検索クエリフィールドの描画
        /// </summary>
        private void DrawSearchQueryField()
        {
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
        }

        /// <summary>
        /// 詳細検索トグルの描画
        /// </summary>
        private void DrawAdvancedSearchToggle()
        {
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
        }

        /// <summary>
        /// クリアボタンの描画
        /// </summary>
        private void DrawClearButton()
        {
            if (GUILayout.Button("クリア", GUILayout.Width(60)))
            {
                _searchViewModel.ClearSearchCriteria();
                _paginationViewModel.ResetPage();
                GUI.changed = true;
            }
        }

        /// <summary>
        /// ソートボタンの描画
        /// </summary>
        private void DrawSortButton()
        {
            if (GUILayout.Button("▼ 表示順", GUILayout.Width(80)))
            {
                ShowSortMenu();
            }
        }

        /// <summary>
        /// ソートメニューの表示
        /// </summary>
        private void ShowSortMenu()
        {
            var menu = new GenericMenu();
            AddSortMenuItem(
                menu,
                "追加順（新しい順）",
                AssetBrowserViewModel.SortMethod.CreatedDateDesc
            );
            AddSortMenuItem(
                menu,
                "追加順（古い順）",
                AssetBrowserViewModel.SortMethod.CreatedDateAsc
            );
            AddSortMenuItem(menu, "アセット名（A-Z順）", AssetBrowserViewModel.SortMethod.TitleAsc);
            AddSortMenuItem(
                menu,
                "アセット名（Z-A順）",
                AssetBrowserViewModel.SortMethod.TitleDesc
            );
            AddSortMenuItem(
                menu,
                "ショップ名（A-Z順）",
                AssetBrowserViewModel.SortMethod.AuthorAsc
            );
            AddSortMenuItem(
                menu,
                "ショップ名（Z-A順）",
                AssetBrowserViewModel.SortMethod.AuthorDesc
            );
            menu.ShowAsContext();
        }

        /// <summary>
        /// ソートメニューアイテムの追加
        /// </summary>
        /// <param name="menu">メニュー</param>
        /// <param name="label">ラベル</param>
        /// <param name="sortMethod">ソート方法</param>
        private void AddSortMenuItem(
            GenericMenu menu,
            string label,
            AssetBrowserViewModel.SortMethod sortMethod
        )
        {
            menu.AddItem(
                new GUIContent(label),
                _assetBrowserViewModel.CurrentSortMethod == sortMethod,
                () => _assetBrowserViewModel.SetSortMethod(sortMethod)
            );
        }

        /// <summary>
        /// 詳細検索フィールドの描画
        /// </summary>
        private void DrawAdvancedSearch()
        {
            if (!_searchViewModel.SearchCriteria.ShowAdvancedSearch)
                return;

            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            DrawAdvancedSearchFields();
            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        /// <summary>
        /// 詳細検索フィールドの描画
        /// </summary>
        private void DrawAdvancedSearchFields()
        {
            DrawSearchField(
                "タイトル:",
                _searchViewModel.SearchCriteria.TitleSearch,
                value => _searchViewModel.SearchCriteria.TitleSearch = value
            );
            DrawSearchField(
                "作者名:",
                _searchViewModel.SearchCriteria.AuthorSearch,
                value => _searchViewModel.SearchCriteria.AuthorSearch = value
            );

            if (_paginationViewModel.SelectedTab != 0)
            {
                DrawSearchField(
                    "カテゴリ:",
                    _searchViewModel.SearchCriteria.CategorySearch,
                    value => _searchViewModel.SearchCriteria.CategorySearch = value
                );
            }

            if (_paginationViewModel.SelectedTab == 1)
            {
                DrawSearchField(
                    "対応アバター:",
                    _searchViewModel.SearchCriteria.SupportedAvatarsSearch,
                    value => _searchViewModel.SearchCriteria.SupportedAvatarsSearch = value
                );
            }

            DrawSearchField(
                "タグ:",
                _searchViewModel.SearchCriteria.TagsSearch,
                value => _searchViewModel.SearchCriteria.TagsSearch = value
            );
            DrawSearchField(
                "メモ:",
                _searchViewModel.SearchCriteria.MemoSearch,
                value => _searchViewModel.SearchCriteria.MemoSearch = value
            );
        }

        /// <summary>
        /// 検索フィールドの描画
        /// </summary>
        /// <param name="label">ラベル</param>
        /// <param name="value">現在の値</param>
        /// <param name="onValueChanged">値変更時のコールバック</param>
        private void DrawSearchField(string label, string value, Action<string> onValueChanged)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(100));
            var newValue = EditorGUILayout.TextField(value);
            if (newValue != value)
            {
                onValueChanged(newValue);
                GUI.changed = true;
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 検索結果件数の描画
        /// </summary>
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

        /// <summary>
        /// データベースパス入力フィールドの描画
        /// </summary>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        /// <param name="loadAEDatabase">AEデータベース読み込みのコールバック</param>
        /// <param name="loadKADatabase">KAデータベース読み込みのコールバック</param>
        public void DrawDatabasePathFields(
            ref string aeDatabasePath,
            ref string kaDatabasePath,
            Action loadAEDatabase,
            Action loadKADatabase
        )
        {
            DrawDatabasePathField("AE Database Path:", ref aeDatabasePath, loadAEDatabase);
            DrawDatabasePathField("KA Database Path:", ref kaDatabasePath, loadKADatabase);
            DrawRefreshButton(aeDatabasePath, kaDatabasePath);
            EditorGUILayout.Space(10);
        }

        /// <summary>
        /// データベースパス入力フィールドの描画
        /// </summary>
        /// <param name="label">ラベル</param>
        /// <param name="path">パス</param>
        /// <param name="onPathChanged">パス変更時のコールバック</param>
        private void DrawDatabasePathField(string label, ref string path, Action onPathChanged)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(path);
            EditorGUI.EndDisabledGroup();

            DrawDeleteButton(label, ref path, onPathChanged);
            DrawBrowseButton(label, ref path, onPathChanged);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 削除ボタンの描画
        /// </summary>
        /// <param name="label">ラベル</param>
        /// <param name="path">パス</param>
        /// <param name="onPathChanged">パス変更時のコールバック</param>
        private void DrawDeleteButton(string label, ref string path, Action onPathChanged)
        {
            if (!string.IsNullOrEmpty(path) && GUILayout.Button("削除", GUILayout.Width(60)))
            {
                path = "";
                onPathChanged();
                _paginationViewModel.ResetPage();
                UpdateDatabaseServicePath(label, path);
            }
        }

        /// <summary>
        /// 参照ボタンの描画
        /// </summary>
        /// <param name="label">ラベル</param>
        /// <param name="path">パス</param>
        /// <param name="onPathChanged">パス変更時のコールバック</param>
        private void DrawBrowseButton(string label, ref string path, Action onPathChanged)
        {
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                var selectedPath = EditorUtility.OpenFolderPanel(
                    $"Select {label} Directory",
                    "",
                    ""
                );
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    HandleSelectedPath(label, ref path, selectedPath, onPathChanged);
                }
            }
        }

        /// <summary>
        /// 選択されたパスの処理
        /// </summary>
        /// <param name="label">ラベル</param>
        /// <param name="path">パス</param>
        /// <param name="selectedPath">選択されたパス</param>
        /// <param name="onPathChanged">パス変更時のコールバック</param>
        private void HandleSelectedPath(
            string label,
            ref string path,
            string selectedPath,
            Action onPathChanged
        )
        {
            string tempPath = path;
            path = selectedPath;
            UpdateDatabaseServicePath(label, path);

            if (label == "AE Database Path:")
            {
                DatabaseService.LoadAndUpdateAEDatabase();
                string currentPath = DatabaseService.GetAEDatabasePath();
                UpdatePathIfValid(ref path, currentPath, onPathChanged);
            }
            else if (label == "KA Database Path:")
            {
                DatabaseService.LoadAndUpdateKADatabase();
                string currentPath = DatabaseService.GetKADatabasePath();
                UpdatePathIfValid(ref path, currentPath, onPathChanged);
            }

            if (!string.IsNullOrEmpty(path))
            {
                DatabaseService.SaveSettings();
                _paginationViewModel.ResetPage();
            }
        }

        /// <summary>
        /// データベースサービスのパスを更新
        /// </summary>
        /// <param name="label">ラベル</param>
        /// <param name="path">パス</param>
        private void UpdateDatabaseServicePath(string label, string path)
        {
            if (label == "AE Database Path:")
            {
                DatabaseService.SetAEDatabasePath(path);
            }
            else if (label == "KA Database Path:")
            {
                DatabaseService.SetKADatabasePath(path);
            }
            DatabaseService.SaveSettings();
        }

        /// <summary>
        /// パスが有効な場合に更新
        /// </summary>
        /// <param name="path">パス</param>
        /// <param name="currentPath">現在のパス</param>
        /// <param name="onPathChanged">パス変更時のコールバック</param>
        private void UpdatePathIfValid(ref string path, string currentPath, Action onPathChanged)
        {
            if (string.IsNullOrEmpty(currentPath))
            {
                path = "";
            }
            else
            {
                onPathChanged();
            }
        }

        /// <summary>
        /// 更新ボタンの描画
        /// </summary>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        private void DrawRefreshButton(string aeDatabasePath, string kaDatabasePath)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("更新", GUILayout.Width(150)))
            {
                _assetBrowserViewModel.RefreshDatabases(aeDatabasePath, kaDatabasePath);
                _assetBrowserViewModel.RefreshImageCache(aeDatabasePath, kaDatabasePath);
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
