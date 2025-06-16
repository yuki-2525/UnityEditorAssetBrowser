#nullable enable

using System;
using System.IO;
using System.Linq;
using UnityEditorAssetBrowser.Models;
using UnityEngine;

namespace UnityEditorAssetBrowser.Services
{
    /// <summary>
    /// アイテム検索を支援するサービスクラス
    /// 基本検索と詳細検索の機能を提供し、アイテムの検索条件に基づいたフィルタリングを行う
    /// </summary>
    public class ItemSearchService : AssetItem
    {
        private readonly AvatarExplorerDatabase? aeDatabase;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="aeDatabase">AvatarExplorerデータベース（オプション）</param>
        public ItemSearchService(AvatarExplorerDatabase? aeDatabase = null)
        {
            this.aeDatabase = aeDatabase;
        }

        /// <summary>
        /// アイテムが検索条件に一致するか判定する
        /// </summary>
        /// <param name="item">判定するアイテム</param>
        /// <param name="criteria">検索条件</param>
        /// <param name="tabIndex">現在のタブインデックス</param>
        /// <returns>検索条件に一致する場合はtrue、それ以外はfalse</returns>
        public bool IsItemMatchSearch(object item, SearchCriteria criteria, int tabIndex = 0)
        {
            if (criteria == null)
            {
                return true;
            }

            // 基本検索
            if (!string.IsNullOrEmpty(criteria.SearchQuery))
            {
                bool basic = IsBasicSearchMatch(item, criteria.GetKeywords(), tabIndex);
                if (!basic)
                {
                    return false;
                }
            }

            // 詳細検索
            if (criteria.ShowAdvancedSearch)
            {
                bool adv = IsAdvancedSearchMatch(item, criteria);
                if (!adv)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 基本検索の条件に一致するか判定する
        /// </summary>
        /// <param name="item">判定するアイテム</param>
        /// <param name="keywords">検索キーワード</param>
        /// <param name="tabIndex">現在のタブインデックス</param>
        /// <returns>検索条件に一致する場合はtrue、それ以外はfalse</returns>
        private bool IsBasicSearchMatch(object item, string[] keywords, int tabIndex)
        {
            foreach (var keyword in keywords)
            {
                bool matchesKeyword = false;

                // タイトル
                if (GetTitle(item).Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
                {
                    matchesKeyword = true;
                }

                // 作者名
                if (GetAuthor(item).Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
                {
                    matchesKeyword = true;
                }

                // カテゴリ（アバタータブはスキップ＝matchesKeywordをtrueにしない）
                if (tabIndex != 0)
                {
                    if (IsCategoryMatch(item, new[] { keyword }))
                    {
                        matchesKeyword = true;
                    }
                }
                // アバタータブはスキップ

                // 対応アバター（アイテムタブのみ判定）
                if (tabIndex == 1)
                {
                    if (IsSupportedAvatarsMatch(item, new[] { keyword }))
                    {
                        matchesKeyword = true;
                    }
                }
                // それ以外のタブはスキップ

                // タグ
                if (IsTagsMatch(item, new[] { keyword }))
                {
                    matchesKeyword = true;
                }

                // メモ
                if (IsMemoMatch(item, new[] { keyword }))
                {
                    matchesKeyword = true;
                }

                if (!matchesKeyword)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 詳細検索の条件に一致するか判定する
        /// </summary>
        /// <param name="item">判定するアイテム</param>
        /// <param name="criteria">検索条件</param>
        /// <returns>検索条件に一致する場合はtrue、それ以外はfalse</returns>
        private bool IsAdvancedSearchMatch(object item, SearchCriteria criteria)
        {
            // タイトル検索
            if (
                !string.IsNullOrEmpty(criteria.TitleSearch)
                && !IsTitleMatch(item, criteria.GetTitleKeywords())
            )
                return false;

            // 作者名検索
            if (
                !string.IsNullOrEmpty(criteria.AuthorSearch)
                && !IsAuthorMatch(item, criteria.GetAuthorKeywords())
            )
                return false;

            // カテゴリ検索
            if (
                !string.IsNullOrEmpty(criteria.CategorySearch)
                && !IsCategoryMatch(item, criteria.GetCategoryKeywords())
            )
                return false;

            // 対応アバター検索
            if (
                !string.IsNullOrEmpty(criteria.SupportedAvatarsSearch)
                && !IsSupportedAvatarsMatch(item, criteria.GetSupportedAvatarsKeywords())
            )
                return false;

            // タグ検索
            if (
                !string.IsNullOrEmpty(criteria.TagsSearch)
                && !IsTagsMatch(item, criteria.GetTagsKeywords())
            )
                return false;

            // メモ検索
            if (
                !string.IsNullOrEmpty(criteria.MemoSearch)
                && !IsMemoMatch(item, criteria.GetMemoKeywords())
            )
                return false;

            return true;
        }

        /// <summary>
        /// タイトルが検索条件に一致するか判定する
        /// </summary>
        /// <param name="item">判定するアイテム</param>
        /// <param name="keywords">検索キーワード</param>
        /// <returns>検索条件に一致する場合はtrue、それ以外はfalse</returns>
        private bool IsTitleMatch(object item, string[] keywords)
        {
            return keywords.All(keyword =>
                GetTitle(item).Contains(keyword, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        /// <summary>
        /// 作者名が検索条件に一致するか判定する
        /// </summary>
        /// <param name="item">判定するアイテム</param>
        /// <param name="keywords">検索キーワード</param>
        /// <returns>検索条件に一致する場合はtrue、それ以外はfalse</returns>
        private bool IsAuthorMatch(object item, string[] keywords)
        {
            return keywords.All(keyword =>
                GetAuthor(item).Contains(keyword, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        /// <summary>
        /// カテゴリが検索条件に一致するか判定する
        /// </summary>
        /// <param name="item">判定するアイテム</param>
        /// <param name="keywords">検索キーワード</param>
        /// <returns>検索条件に一致する場合はtrue、それ以外はfalse</returns>
        private bool IsCategoryMatch(object item, string[] keywords)
        {
            string categoryName = GetItemCategoryName(item);

            return keywords.All(keyword =>
                categoryName.Contains(keyword, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        /// <summary>
        /// 対応アバターが検索条件に一致するか判定する
        /// </summary>
        /// <param name="item">判定するアイテム</param>
        /// <param name="keywords">検索キーワード</param>
        /// <returns>検索条件に一致する場合はtrue、それ以外はfalse</returns>
        private bool IsSupportedAvatarsMatch(object item, string[] keywords)
        {
            if (item is AvatarExplorerItem aeItem && aeItem.SupportedAvatars != null)
            {
                // すべてのキーワードが少なくとも1つの対応アバターに含まれていることを確認
                return keywords.All(keyword =>
                    aeItem.SupportedAvatars.Any(avatarPath =>
                    {
                        var avatarItem = aeDatabase?.Items.FirstOrDefault(x =>
                            x.ItemPath == avatarPath
                        );
                        if (avatarItem != null)
                        {
                            return avatarItem.Title.Contains(
                                keyword,
                                StringComparison.InvariantCultureIgnoreCase
                            );
                        }
                        return Path.GetFileName(avatarPath)
                            .Contains(keyword, StringComparison.InvariantCultureIgnoreCase);
                    })
                );
            }
            else if (
                item is KonoAssetWearableItem wearableItem
                && wearableItem.supportedAvatars != null
            )
            {
                // すべてのキーワードが少なくとも1つの対応アバターに含まれていることを確認
                return keywords.All(keyword =>
                    wearableItem.supportedAvatars.Any(avatar =>
                        avatar.Contains(keyword, StringComparison.InvariantCultureIgnoreCase)
                    )
                );
            }
            return true;
        }

        /// <summary>
        /// タグが検索条件に一致するか判定する
        /// </summary>
        /// <param name="item">判定するアイテム</param>
        /// <param name="keywords">検索キーワード</param>
        /// <returns>検索条件に一致する場合はtrue、それ以外はfalse</returns>
        private bool IsTagsMatch(object item, string[] keywords)
        {
            string[]? tags = null;

            if (item is KonoAssetAvatarItem kaItem)
                tags = kaItem.description.tags;
            else if (item is KonoAssetWearableItem wearableItem)
                tags = wearableItem.description.tags;
            else if (item is KonoAssetWorldObjectItem worldItem)
                tags = worldItem.description.tags;
            else if (item is KonoAssetOtherAssetItem otherItem)
                tags = otherItem.description.tags;

            if (tags == null || tags.Length == 0)
                return false;

            // すべてのキーワードが少なくとも1つのタグに含まれていることを確認
            return keywords.All(keyword =>
                tags.Any(tag => tag.Contains(keyword, StringComparison.InvariantCultureIgnoreCase))
            );
        }

        /// <summary>
        /// メモが検索条件に一致するか判定する
        /// </summary>
        /// <param name="item">判定するアイテム</param>
        /// <param name="keywords">検索キーワード</param>
        /// <returns>検索条件に一致する場合はtrue、それ以外はfalse</returns>
        private bool IsMemoMatch(object item, string[] keywords)
        {
            string memo = GetMemo(item);
            if (string.IsNullOrEmpty(memo))
                return false;

            // すべてのキーワードがメモに含まれていることを確認
            return keywords.All(keyword =>
                memo.Contains(keyword, StringComparison.InvariantCultureIgnoreCase)
            );
        }

        public bool IsDatabaseNull()
        {
            return aeDatabase == null;
        }
    }
}
