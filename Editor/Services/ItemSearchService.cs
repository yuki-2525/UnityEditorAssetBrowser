#nullable enable

using System;
using System.IO;
using System.Linq;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.ViewModels;

namespace UnityEditorAssetBrowser.Services
{
    public class ItemSearchService
    {
        private readonly AssetItem assetItem = new AssetItem();
        private readonly AvatarExplorerDatabase? aeDatabase;

        public ItemSearchService(AvatarExplorerDatabase? aeDatabase = null)
        {
            this.aeDatabase = aeDatabase;
        }

        public bool IsItemMatchSearch(object item, SearchCriteria criteria)
        {
            if (criteria == null)
                return true;

            // 基本検索
            if (!string.IsNullOrEmpty(criteria.SearchQuery))
            {
                if (!IsBasicSearchMatch(item, criteria.SearchQuery))
                    return false;
            }

            // 詳細検索
            if (criteria.ShowAdvancedSearch)
            {
                if (!IsAdvancedSearchMatch(item, criteria))
                    return false;
            }

            return true;
        }

        private bool IsBasicSearchMatch(object item, string searchQuery)
        {
            var keywords = searchQuery.Split(
                new[] { ' ', '　' },
                StringSplitOptions.RemoveEmptyEntries
            );
            foreach (var keyword in keywords)
            {
                bool matchesKeyword = false;

                // タイトル
                if (
                    GetTitle(item).IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase)
                    >= 0
                )
                    matchesKeyword = true;

                // 作者名
                if (
                    GetAuthor(item).IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase)
                    >= 0
                )
                    matchesKeyword = true;

                // カテゴリ
                if (
                    item is AvatarExplorerItem aeItem
                    && aeItem
                        .GetAECategoryName()
                        .IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) >= 0
                )
                    matchesKeyword = true;

                // 対応アバター
                if (IsSupportedAvatarsMatch(item, keyword))
                    matchesKeyword = true;

                // タグ
                if (IsTagsMatch(item, keyword))
                    matchesKeyword = true;

                // メモ
                if (IsMemoMatch(item, keyword))
                    matchesKeyword = true;

                if (!matchesKeyword)
                    return false;
            }

            return true;
        }

        private bool IsAdvancedSearchMatch(object item, SearchCriteria criteria)
        {
            // タイトル検索
            if (
                !string.IsNullOrEmpty(criteria.TitleSearch)
                && !IsTitleMatch(item, criteria.TitleSearch)
            )
                return false;

            // 作者名検索
            if (
                !string.IsNullOrEmpty(criteria.AuthorSearch)
                && !IsAuthorMatch(item, criteria.AuthorSearch)
            )
                return false;

            // カテゴリ検索
            if (
                !string.IsNullOrEmpty(criteria.CategorySearch)
                && !IsCategoryMatch(item, criteria.CategorySearch)
            )
                return false;

            // 対応アバター検索
            if (
                !string.IsNullOrEmpty(criteria.SupportedAvatarsSearch)
                && !IsSupportedAvatarsMatch(item, criteria.SupportedAvatarsSearch)
            )
                return false;

            // タグ検索
            if (
                !string.IsNullOrEmpty(criteria.TagsSearch)
                && !IsTagsMatch(item, criteria.TagsSearch)
            )
                return false;

            // メモ検索
            if (
                !string.IsNullOrEmpty(criteria.MemoSearch)
                && !IsMemoMatch(item, criteria.MemoSearch)
            )
                return false;

            return true;
        }

        private string GetTitle(object item) => assetItem.GetTitle(item);

        private string GetAuthor(object item) => assetItem.GetAuthor(item);

        private bool IsTitleMatch(object item, string searchQuery)
        {
            var keywords = searchQuery.Split(
                new[] { ' ', '　' },
                StringSplitOptions.RemoveEmptyEntries
            );
            return keywords.All(keyword =>
                GetTitle(item).IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) >= 0
            );
        }

        private bool IsAuthorMatch(object item, string searchQuery)
        {
            var keywords = searchQuery.Split(
                new[] { ' ', '　' },
                StringSplitOptions.RemoveEmptyEntries
            );
            return keywords.All(keyword =>
                GetAuthor(item).IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) >= 0
            );
        }

        private bool IsCategoryMatch(object item, string searchQuery)
        {
            var keywords = searchQuery.Split(
                new[] { ' ', '　' },
                StringSplitOptions.RemoveEmptyEntries
            );
            string categoryName = "";

            if (item is AvatarExplorerItem aeItem)
                categoryName = aeItem.GetAECategoryName();
            else if (item is KonoAssetWearableItem wearableItem)
                categoryName = wearableItem.category ?? "";
            else if (item is KonoAssetWorldObjectItem worldItem)
                categoryName = worldItem.category ?? "";

            return keywords.All(keyword =>
                categoryName.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) >= 0
            );
        }

        private bool IsSupportedAvatarsMatch(object item, string searchQuery)
        {
            if (item is AvatarExplorerItem aeItem && aeItem.SupportedAvatars != null)
            {
                return aeItem.SupportedAvatars.Any(avatarPath =>
                {
                    var avatarItem = aeDatabase?.Items.FirstOrDefault(x =>
                        x.ItemPath == avatarPath
                    );
                    if (avatarItem != null)
                    {
                        return avatarItem.Title.IndexOf(
                                searchQuery,
                                StringComparison.InvariantCultureIgnoreCase
                            ) >= 0;
                    }
                    return Path.GetFileName(avatarPath)
                            .IndexOf(searchQuery, StringComparison.InvariantCultureIgnoreCase) >= 0;
                });
            }
            else if (
                item is KonoAssetWearableItem wearableItem
                && wearableItem.supportedAvatars != null
            )
            {
                return wearableItem.supportedAvatars.Any(avatar =>
                    avatar.IndexOf(searchQuery, StringComparison.InvariantCultureIgnoreCase) >= 0
                );
            }
            return true;
        }

        private bool IsTagsMatch(object item, string searchQuery)
        {
            var keywords = searchQuery.Split(
                new[] { ' ', '　' },
                StringSplitOptions.RemoveEmptyEntries
            );
            string[]? tags = null;

            if (item is KonoAssetAvatarItem kaItem)
                tags = kaItem.description.tags;
            else if (item is KonoAssetWearableItem wearableItem)
                tags = wearableItem.description.tags;
            else if (item is KonoAssetWorldObjectItem worldItem)
                tags = worldItem.description.tags;

            if (tags == null)
                return false;

            return keywords.All(keyword =>
                tags.Any(tag =>
                    tag.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) >= 0
                )
            );
        }

        private bool IsMemoMatch(object item, string searchQuery)
        {
            var keywords = searchQuery.Split(
                new[] { ' ', '　' },
                StringSplitOptions.RemoveEmptyEntries
            );
            string? memo = null;

            if (item is AvatarExplorerItem aeItem)
                memo = aeItem.Memo;
            else if (item is KonoAssetAvatarItem kaItem)
                memo = kaItem.description.memo;
            else if (item is KonoAssetWearableItem wearableItem)
                memo = wearableItem.description.memo;
            else if (item is KonoAssetWorldObjectItem worldItem)
                memo = worldItem.description.memo;

            if (string.IsNullOrEmpty(memo))
                return false;

            return keywords.All(keyword =>
                memo!.IndexOf(keyword, StringComparison.InvariantCultureIgnoreCase) >= 0
            );
        }
    }
}
