// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using UnityEditorAssetBrowser.Helper;

namespace UnityEditorAssetBrowser.Models
{
    /// <summary>
    /// アセットアイテムの情報を管理するクラス
    /// 様々な形式のアセットアイテムから共通の情報を取得する機能を提供する
    /// </summary>
    public class AssetItem
    {
        /// <summary>
        /// ワールドカテゴリーの日本語名
        /// </summary>
        private const string WORLD_CATEGORY_JP = "ワールド";

        /// <summary>
        /// ワールドカテゴリーの英語名
        /// </summary>
        private const string WORLD_CATEGORY_EN = "world";

        /// <summary>
        /// アイテムのカテゴリー名を取得
        /// </summary>
        /// <param name="item">カテゴリー名を取得するアイテム</param>
        /// <returns>アイテムのカテゴリー名。取得できない場合は空文字列</returns>
        public string GetItemCategoryName(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.GetAECategoryName();
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.category;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.category;
            }
            else if (item is KonoAssetOtherAssetItem otherItem)
            {
                return otherItem.category;
            }
            return string.Empty;
        }

        /// <summary>
        /// アイテムのカテゴリー名を取得（エイリアス）
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>カテゴリー名</returns>
        public string GetAECategoryName(object item)
        {
            return GetItemCategoryName(item);
        }

        /// <summary>
        /// アイテムのタイトルを取得
        /// </summary>
        /// <param name="item">タイトルを取得するアイテム</param>
        /// <returns>アイテムのタイトル。取得できない場合は空文字列</returns>
        public string GetTitle(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.Title;
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return kaItem.description.name;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.description.name;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.description.name;
            }
            else if (item is KonoAssetOtherAssetItem otherItem)
            {
                return otherItem.description.name;
            }
            return string.Empty;
        }

        /// <summary>
        /// アイテムの作者名を取得
        /// </summary>
        /// <param name="item">作者名を取得するアイテム</param>
        /// <returns>アイテムの作者名。取得できない場合は空文字列</returns>
        public string GetAuthor(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.AuthorName;
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return kaItem.description.creator;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.description.creator;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.description.creator;
            }
            else if (item is KonoAssetOtherAssetItem otherItem)
            {
                return otherItem.description.creator;
            }
            return string.Empty;
        }

        /// <summary>
        /// アイテムの作成日を取得
        /// </summary>
        /// <param name="item">作成日を取得するアイテム</param>
        /// <returns>アイテムの作成日（UnixTimeMilliseconds）。取得できない場合は0</returns>
        public long GetCreatedDate(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                if (aeItem.CreatedDate == default)
                    return 0;

                // 日付文字列をUTCのDateTimeに変換
                var utcDateTime = CustomDateTimeConverter.GetDate(aeItem.CreatedDate.ToString());

                // UTCのDateTimeをUnixTimeMillisecondsに変換
                return new DateTimeOffset(utcDateTime, TimeSpan.Zero).ToUnixTimeMilliseconds();
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return kaItem.description.createdAt;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.description.createdAt;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.description.createdAt;
            }
            else if (item is KonoAssetOtherAssetItem otherItem)
            {
                return otherItem.description.createdAt;
            }
            return 0;
        }

        /// <summary>
        /// アイテムのメモを取得
        /// </summary>
        /// <param name="item">メモを取得するアイテム</param>
        /// <returns>アイテムのメモ。取得できない場合は空文字列</returns>
        public string GetMemo(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.Memo ?? string.Empty;
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return kaItem.description.memo ?? string.Empty;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.description.memo ?? string.Empty;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.description.memo ?? string.Empty;
            }
            else if (item is KonoAssetOtherAssetItem otherItem)
            {
                return otherItem.description.memo ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// カテゴリーがワールド関連かどうかを判定
        /// </summary>
        /// <param name="category">判定するカテゴリー名</param>
        /// <returns>ワールド関連のカテゴリーの場合はtrue、それ以外はfalse</returns>
        public bool IsWorldCategory(string category)
        {
            return category.Contains(WORLD_CATEGORY_JP, StringComparison.OrdinalIgnoreCase)
                || category.Contains(WORLD_CATEGORY_EN, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// アイテムのBooth Item IDを取得
        /// </summary>
        /// <param name="item">Booth IDを取得するアイテム</param>
        /// <returns>Booth Item ID。取得できない場合や0以下/nullの場合は0</returns>
        public int GetBoothItemId(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return (aeItem.BoothId > 0) ? aeItem.BoothId : 0;
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return (
                    kaItem.description.boothItemId.HasValue
                    && kaItem.description.boothItemId.Value > 0
                )
                    ? kaItem.description.boothItemId.Value
                    : 0;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return (
                    wearableItem.description.boothItemId.HasValue
                    && wearableItem.description.boothItemId.Value > 0
                )
                    ? wearableItem.description.boothItemId.Value
                    : 0;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return (
                    worldItem.description.boothItemId.HasValue
                    && worldItem.description.boothItemId.Value > 0
                )
                    ? worldItem.description.boothItemId.Value
                    : 0;
            }
            else if (item is KonoAssetOtherAssetItem otherItem)
            {
                return (
                    otherItem.description.boothItemId.HasValue
                    && otherItem.description.boothItemId.Value > 0
                )
                    ? otherItem.description.boothItemId.Value
                    : 0;
            }
            return 0;
        }
    }
}
