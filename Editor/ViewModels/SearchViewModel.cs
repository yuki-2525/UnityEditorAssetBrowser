// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Services;

namespace UnityEditorAssetBrowser.ViewModels
{
    public class SearchViewModel
    {
        private readonly AvatarExplorerDatabase? _aeDatabase;
        private readonly KonoAssetAvatarsDatabase? _kaAvatarsDatabase;
        private readonly KonoAssetWearablesDatabase? _kaWearablesDatabase;
        private readonly KonoAssetWorldObjectsDatabase? _kaWorldObjectsDatabase;
        private int _currentTab;
        private readonly Dictionary<int, SearchCriteria> _tabSearchCriteria =
            new Dictionary<int, SearchCriteria>();

        public SearchCriteria SearchCriteria { get; private set; } = new SearchCriteria();

        public SearchViewModel(
            AvatarExplorerDatabase? aeDatabase,
            KonoAssetAvatarsDatabase? kaAvatarsDatabase,
            KonoAssetWearablesDatabase? kaWearablesDatabase,
            KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase
        )
        {
            _aeDatabase = aeDatabase;
            _kaAvatarsDatabase = kaAvatarsDatabase;
            _kaWearablesDatabase = kaWearablesDatabase;
            _kaWorldObjectsDatabase = kaWorldObjectsDatabase;
        }

        public void SetCurrentTab(int tab)
        {
            // 現在のタブの検索条件を保存
            if (!_tabSearchCriteria.ContainsKey(_currentTab))
            {
                _tabSearchCriteria[_currentTab] = new SearchCriteria();
            }
            _tabSearchCriteria[_currentTab] = SearchCriteria.Clone();

            // 新しいタブの検索条件を読み込む
            _currentTab = tab;
            if (_tabSearchCriteria.TryGetValue(tab, out var criteria))
            {
                SearchCriteria = criteria.Clone();
            }
            else
            {
                SearchCriteria = new SearchCriteria();
                _tabSearchCriteria[tab] = SearchCriteria.Clone();
            }
        }

        public void ClearSearchCriteria()
        {
            SearchCriteria = new SearchCriteria();
            _tabSearchCriteria[_currentTab] = SearchCriteria.Clone();
        }

        public bool IsItemMatchSearch(object item)
        {
            if (
                string.IsNullOrEmpty(SearchCriteria.SearchQuery)
                && !SearchCriteria.ShowAdvancedSearch
            )
                return true;

            if (item is AvatarExplorerItem aeItem)
            {
                return IsAEItemMatchSearch(aeItem);
            }
            else if (item is KonoAssetAvatarItem kaAvatarItem)
            {
                return IsKAAvatarItemMatchSearch(kaAvatarItem);
            }
            else if (item is KonoAssetWearableItem kaWearableItem)
            {
                return IsKAWearableItemMatchSearch(kaWearableItem);
            }
            else if (item is KonoAssetWorldObjectItem kaWorldObjectItem)
            {
                return IsKAWorldObjectItemMatchSearch(kaWorldObjectItem);
            }

            return false;
        }

        private bool IsAEItemMatchSearch(AvatarExplorerItem item)
        {
            // 基本検索
            if (!string.IsNullOrEmpty(SearchCriteria.SearchQuery))
            {
                var searchQuery = SearchCriteria.SearchQuery.ToLower();
                var title = item.Title ?? "";
                var authorName = item.AuthorName ?? "";
                var category = item.Category ?? "";
                var memo = item.Memo ?? "";

                if (
                    !title.ToLower().Contains(searchQuery)
                    && !authorName.ToLower().Contains(searchQuery)
                    && !category.ToLower().Contains(searchQuery)
                    && !(item.Tags != null && item.Tags.Any(t => t.ToLower().Contains(searchQuery)))
                    && !memo.ToLower().Contains(searchQuery)
                )
                {
                    return false;
                }
            }

            // 詳細検索
            if (SearchCriteria.ShowAdvancedSearch)
            {
                var title = item.Title ?? "";
                var authorName = item.AuthorName ?? "";
                var category = item.Category ?? "";
                var memo = item.Memo ?? "";

                // タイトル検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.TitleSearch)
                    && !title.ToLower().Contains(SearchCriteria.TitleSearch.ToLower())
                )
                {
                    return false;
                }

                // 作者名検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.AuthorSearch)
                    && !authorName.ToLower().Contains(SearchCriteria.AuthorSearch.ToLower())
                )
                {
                    return false;
                }

                // カテゴリ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.CategorySearch)
                    && !category.ToLower().Contains(SearchCriteria.CategorySearch.ToLower())
                )
                {
                    return false;
                }

                // 対応アバター検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.SupportedAvatarsSearch)
                    && (
                        item.SupportedAvatars == null
                        || !item.SupportedAvatars.Any(a =>
                            a.ToLower().Contains(SearchCriteria.SupportedAvatarsSearch.ToLower())
                        )
                    )
                )
                {
                    return false;
                }

                // タグ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.TagsSearch)
                    && (
                        item.Tags == null
                        || !item.Tags.Any(t =>
                            t.ToLower().Contains(SearchCriteria.TagsSearch.ToLower())
                        )
                    )
                )
                {
                    return false;
                }

                // メモ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.MemoSearch)
                    && !memo.ToLower().Contains(SearchCriteria.MemoSearch.ToLower())
                )
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsKAAvatarItemMatchSearch(KonoAssetAvatarItem item)
        {
            // 基本検索
            if (!string.IsNullOrEmpty(SearchCriteria.SearchQuery))
            {
                var searchQuery = SearchCriteria.SearchQuery.ToLower();
                var name = item.description?.name ?? "";
                var creator = item.description?.creator ?? "";
                var memo = item.description?.memo ?? "";

                if (
                    !name.ToLower().Contains(searchQuery)
                    && !creator.ToLower().Contains(searchQuery)
                    && !(
                        item.description?.tags != null
                        && item.description.tags.Any(t => t.ToLower().Contains(searchQuery))
                    )
                    && !memo.ToLower().Contains(searchQuery)
                )
                {
                    return false;
                }
            }

            // 詳細検索
            if (SearchCriteria.ShowAdvancedSearch)
            {
                var name = item.description?.name ?? "";
                var creator = item.description?.creator ?? "";
                var memo = item.description?.memo ?? "";

                // タイトル検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.TitleSearch)
                    && !name.ToLower().Contains(SearchCriteria.TitleSearch.ToLower())
                )
                {
                    return false;
                }

                // 作者名検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.AuthorSearch)
                    && !creator.ToLower().Contains(SearchCriteria.AuthorSearch.ToLower())
                )
                {
                    return false;
                }

                // タグ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.TagsSearch)
                    && (
                        item.description?.tags == null
                        || !item.description.tags.Any(t =>
                            t.ToLower().Contains(SearchCriteria.TagsSearch.ToLower())
                        )
                    )
                )
                {
                    return false;
                }

                // メモ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.MemoSearch)
                    && !memo.ToLower().Contains(SearchCriteria.MemoSearch.ToLower())
                )
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsKAWearableItemMatchSearch(KonoAssetWearableItem item)
        {
            // 基本検索
            if (!string.IsNullOrEmpty(SearchCriteria.SearchQuery))
            {
                var searchQuery = SearchCriteria.SearchQuery.ToLower();
                var name = item.description?.name ?? "";
                var creator = item.description?.creator ?? "";
                var category = item.category ?? "";
                var memo = item.description?.memo ?? "";

                if (
                    !name.ToLower().Contains(searchQuery)
                    && !creator.ToLower().Contains(searchQuery)
                    && !category.ToLower().Contains(searchQuery)
                    && !(
                        item.description?.tags != null
                        && item.description.tags.Any(t => t.ToLower().Contains(searchQuery))
                    )
                    && !memo.ToLower().Contains(searchQuery)
                )
                {
                    return false;
                }
            }

            // 詳細検索
            if (SearchCriteria.ShowAdvancedSearch)
            {
                var name = item.description?.name ?? "";
                var creator = item.description?.creator ?? "";
                var category = item.category ?? "";
                var memo = item.description?.memo ?? "";

                // タイトル検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.TitleSearch)
                    && !name.ToLower().Contains(SearchCriteria.TitleSearch.ToLower())
                )
                {
                    return false;
                }

                // 作者名検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.AuthorSearch)
                    && !creator.ToLower().Contains(SearchCriteria.AuthorSearch.ToLower())
                )
                {
                    return false;
                }

                // カテゴリ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.CategorySearch)
                    && !category.ToLower().Contains(SearchCriteria.CategorySearch.ToLower())
                )
                {
                    return false;
                }

                // 対応アバター検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.SupportedAvatarsSearch)
                    && (
                        item.supportedAvatars == null
                        || !item.supportedAvatars.Any(a =>
                            a.ToLower().Contains(SearchCriteria.SupportedAvatarsSearch.ToLower())
                        )
                    )
                )
                {
                    return false;
                }

                // タグ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.TagsSearch)
                    && (
                        item.description?.tags == null
                        || !item.description.tags.Any(t =>
                            t.ToLower().Contains(SearchCriteria.TagsSearch.ToLower())
                        )
                    )
                )
                {
                    return false;
                }

                // メモ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.MemoSearch)
                    && !memo.ToLower().Contains(SearchCriteria.MemoSearch.ToLower())
                )
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsKAWorldObjectItemMatchSearch(KonoAssetWorldObjectItem item)
        {
            // 基本検索
            if (!string.IsNullOrEmpty(SearchCriteria.SearchQuery))
            {
                var searchQuery = SearchCriteria.SearchQuery.ToLower();
                var name = item.description?.name ?? "";
                var creator = item.description?.creator ?? "";
                var category = item.category ?? "";
                var memo = item.description?.memo ?? "";

                if (
                    !name.ToLower().Contains(searchQuery)
                    && !creator.ToLower().Contains(searchQuery)
                    && !category.ToLower().Contains(searchQuery)
                    && !(
                        item.description?.tags != null
                        && item.description.tags.Any(t => t.ToLower().Contains(searchQuery))
                    )
                    && !memo.ToLower().Contains(searchQuery)
                )
                {
                    return false;
                }
            }

            // 詳細検索
            if (SearchCriteria.ShowAdvancedSearch)
            {
                var name = item.description?.name ?? "";
                var creator = item.description?.creator ?? "";
                var category = item.category ?? "";
                var memo = item.description?.memo ?? "";

                // タイトル検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.TitleSearch)
                    && !name.ToLower().Contains(SearchCriteria.TitleSearch.ToLower())
                )
                {
                    return false;
                }

                // 作者名検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.AuthorSearch)
                    && !creator.ToLower().Contains(SearchCriteria.AuthorSearch.ToLower())
                )
                {
                    return false;
                }

                // カテゴリ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.CategorySearch)
                    && !category.ToLower().Contains(SearchCriteria.CategorySearch.ToLower())
                )
                {
                    return false;
                }

                // タグ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.TagsSearch)
                    && (
                        item.description?.tags == null
                        || !item.description.tags.Any(t =>
                            t.ToLower().Contains(SearchCriteria.TagsSearch.ToLower())
                        )
                    )
                )
                {
                    return false;
                }

                // メモ検索
                if (
                    !string.IsNullOrEmpty(SearchCriteria.MemoSearch)
                    && !memo.ToLower().Contains(SearchCriteria.MemoSearch.ToLower())
                )
                {
                    return false;
                }
            }

            return true;
        }
    }
}
