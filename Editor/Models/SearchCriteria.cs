// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditorAssetBrowser.Models
{
    /// <summary>
    /// 検索条件を管理するクラス
    /// </summary>
    public class SearchCriteria
    {
        // 基本検索
        private string _searchQuery = "";

        // 詳細検索
        private bool _showAdvancedSearch = false;
        private string _titleSearch = "";
        private string _authorSearch = "";
        private string _categorySearch = "";
        private string _supportedAvatarsSearch = "";
        private string _tagsSearch = "";
        private string _memoSearch = "";

        // ソート方法
        private SortMethod _sortMethod = SortMethod.TitleAsc;

        // フィルター
        private bool _showAvatars = true;
        private bool _showWearables = true;
        private bool _showWorldObjects = true;

        // ページネーション
        private int _currentPage = 1;
        private int _itemsPerPage = 20;

        /// <summary>
        /// 基本検索クエリ
        /// </summary>
        public string SearchQuery
        {
            get => _searchQuery;
            set => _searchQuery = value ?? "";
        }

        /// <summary>
        /// 詳細検索を表示するかどうか
        /// </summary>
        public bool ShowAdvancedSearch
        {
            get => _showAdvancedSearch;
            set => _showAdvancedSearch = value;
        }

        /// <summary>
        /// タイトル検索
        /// </summary>
        public string TitleSearch
        {
            get => _titleSearch;
            set => _titleSearch = value ?? "";
        }

        /// <summary>
        /// 作者名検索
        /// </summary>
        public string AuthorSearch
        {
            get => _authorSearch;
            set => _authorSearch = value ?? "";
        }

        /// <summary>
        /// カテゴリ検索
        /// </summary>
        public string CategorySearch
        {
            get => _categorySearch;
            set => _categorySearch = value ?? "";
        }

        /// <summary>
        /// 対応アバター検索
        /// </summary>
        public string SupportedAvatarsSearch
        {
            get => _supportedAvatarsSearch;
            set => _supportedAvatarsSearch = value ?? "";
        }

        /// <summary>
        /// タグ検索
        /// </summary>
        public string TagsSearch
        {
            get => _tagsSearch;
            set => _tagsSearch = value ?? "";
        }

        /// <summary>
        /// メモ検索
        /// </summary>
        public string MemoSearch
        {
            get => _memoSearch;
            set => _memoSearch = value ?? "";
        }

        /// <summary>
        /// ソート方法
        /// </summary>
        public SortMethod SortMethod
        {
            get => _sortMethod;
            set => _sortMethod = value;
        }

        /// <summary>
        /// アバターを表示するかどうか
        /// </summary>
        public bool ShowAvatars
        {
            get => _showAvatars;
            set => _showAvatars = value;
        }

        /// <summary>
        /// ウェアラブルを表示するかどうか
        /// </summary>
        public bool ShowWearables
        {
            get => _showWearables;
            set => _showWearables = value;
        }

        /// <summary>
        /// ワールドオブジェクトを表示するかどうか
        /// </summary>
        public bool ShowWorldObjects
        {
            get => _showWorldObjects;
            set => _showWorldObjects = value;
        }

        /// <summary>
        /// 現在のページ
        /// </summary>
        public int CurrentPage
        {
            get => _currentPage;
            set => _currentPage = Math.Max(1, value);
        }

        /// <summary>
        /// 1ページあたりのアイテム数
        /// </summary>
        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set => _itemsPerPage = Math.Max(1, value);
        }

        /// <summary>
        /// 基本検索のキーワードを取得する
        /// </summary>
        /// <returns>キーワードの配列</returns>
        public string[] GetKeywords()
        {
            return SearchQuery.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// タイトル検索のキーワードを取得する
        /// </summary>
        /// <returns>キーワードの配列</returns>
        public string[] GetTitleKeywords()
        {
            return TitleSearch.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 作者名検索のキーワードを取得する
        /// </summary>
        /// <returns>キーワードの配列</returns>
        public string[] GetAuthorKeywords()
        {
            return AuthorSearch.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// カテゴリ検索のキーワードを取得する
        /// </summary>
        /// <returns>キーワードの配列</returns>
        public string[] GetCategoryKeywords()
        {
            return CategorySearch.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 対応アバター検索のキーワードを取得する
        /// </summary>
        /// <returns>キーワードの配列</returns>
        public string[] GetSupportedAvatarsKeywords()
        {
            return SupportedAvatarsSearch.Split(
                new[] { ' ', '　' },
                StringSplitOptions.RemoveEmptyEntries
            );
        }

        /// <summary>
        /// タグ検索のキーワードを取得する
        /// </summary>
        /// <returns>キーワードの配列</returns>
        public string[] GetTagsKeywords()
        {
            return TagsSearch.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// メモ検索のキーワードを取得する
        /// </summary>
        /// <returns>キーワードの配列</returns>
        public string[] GetMemoKeywords()
        {
            return MemoSearch.Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// フィルター条件を取得する
        /// </summary>
        /// <returns>フィルター条件の辞書</returns>
        public Dictionary<string, bool> GetFilters()
        {
            return new Dictionary<string, bool>
            {
                { "Avatars", ShowAvatars },
                { "Wearables", ShowWearables },
                { "WorldObjects", ShowWorldObjects },
            };
        }

        /// <summary>
        /// ソート方法を取得する
        /// </summary>
        /// <returns>ソート方法</returns>
        public SortMethod GetSortMethod()
        {
            return SortMethod;
        }

        /// <summary>
        /// 検索条件をリセットする
        /// </summary>
        public void Reset()
        {
            SearchQuery = "";
            TitleSearch = "";
            AuthorSearch = "";
            CategorySearch = "";
            SupportedAvatarsSearch = "";
            TagsSearch = "";
            MemoSearch = "";
            SortMethod = SortMethod.TitleAsc;
            ShowAvatars = true;
            ShowWearables = true;
            ShowWorldObjects = true;
            CurrentPage = 1;
        }
    }

    /// <summary>
    /// ソート方法の列挙型
    /// </summary>
    public enum SortMethod
    {
        /// <summary>
        /// タイトル昇順
        /// </summary>
        TitleAsc,

        /// <summary>
        /// タイトル降順
        /// </summary>
        TitleDesc,

        /// <summary>
        /// 作者名昇順
        /// </summary>
        AuthorAsc,

        /// <summary>
        /// 作者名降順
        /// </summary>
        AuthorDesc,

        /// <summary>
        /// 作成日昇順
        /// </summary>
        DateAsc,

        /// <summary>
        /// 作成日降順
        /// </summary>
        DateDesc,
    }
}
