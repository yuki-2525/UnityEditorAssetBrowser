// Copyright (c) 2025 yuki-2525

#nullable enable

using System;

namespace UnityEditorAssetBrowser.Models
{
    public class AssetItem
    {
        /// <summary>
        /// アイテムのカテゴリー名を取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>カテゴリー名</returns>
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
        /// <param name="item">アイテム</param>
        /// <returns>タイトル</returns>
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
            return string.Empty;
        }

        /// <summary>
        /// アイテムの作者名を取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>作者名</returns>
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
            return string.Empty;
        }

        /// <summary>
        /// アイテムの作成日を取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>作成日（UnixTimeMilliseconds）</returns>
        public long GetCreatedDate(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                if (aeItem.CreatedDate == default)
                    return 0;

                // 日付文字列をUTCのDateTimeに変換
                var utcDateTime = GetDate(aeItem.CreatedDate.ToString());

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
            return 0;
        }

        /// <summary>
        /// 日付文字列をDateTimeに変換
        /// </summary>
        /// <param name="dateString">日付文字列</param>
        /// <returns>DateTime</returns>
        private DateTime GetDate(string dateString)
        {
            if (DateTime.TryParse(dateString, out DateTime result))
            {
                return result;
            }
            return DateTime.Now;
        }

        /// <summary>
        /// アイテムのメモを取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>メモ</returns>
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
            return string.Empty;
        }
    }
}
