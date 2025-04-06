// Copyright (c) 2025 yuki-2525
// This code is borrowed from AvatarExplorer(https://github.com/yuki-2525/AvatarExplorer)
// AvatarExplorer is licensed under the MIT License. https://github.com/yuki-2525/AvatarExplorer/blob/main/LICENSE

#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnityEditorAssetBrowser.Models
{
    #region Database Model
    /// <summary>
    /// AvatarExplorerのデータベースモデル
    /// </summary>
    public class AvatarExplorerDatabase
    {
        [JsonProperty("Items")]
        public List<AvatarExplorerItem> Items { get; set; } = new List<AvatarExplorerItem>();

        [JsonConstructor]
        public AvatarExplorerDatabase() { }

        /// <summary>
        /// 配列からデータベースを作成するための変換コンストラクタ
        /// </summary>
        /// <param name="items">アイテムの配列</param>
        public AvatarExplorerDatabase(AvatarExplorerItem[] items)
        {
            Items = new List<AvatarExplorerItem>(items);
        }
    }
    #endregion

    #region Item Model
    /// <summary>
    /// AvatarExplorerのアイテムモデル
    /// </summary>
    public class AvatarExplorerItem
    {
        [JsonProperty("Title")]
        public string Title { get; set; } = "";

        [JsonProperty("AuthorName")]
        public string AuthorName { get; set; } = "";

        [JsonProperty("ItemMemo")]
        public string ItemMemo { get; set; } = "";

        [JsonProperty("ItemPath")]
        public string ItemPath { get; set; } = "";

        [JsonProperty("ImagePath")]
        public string ImagePath { get; set; } = "";

        [JsonProperty("MaterialPath")]
        public string MaterialPath { get; set; } = "";

        [JsonProperty("SupportedAvatar")]
        public string[] SupportedAvatar { get; set; } = Array.Empty<string>();

        [JsonProperty("BoothId")]
        public int BoothId { get; set; } = -1;

        [JsonProperty("Type")]
        public string Type { get; set; } = "";

        [JsonProperty("CustomCategory")]
        public string CustomCategory { get; set; } = "";

        [JsonProperty("AuthorId")]
        public string AuthorId { get; set; } = "";

        [JsonProperty("ThumbnailUrl")]
        public string ThumbnailUrl { get; set; } = "";

        [JsonProperty("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.MinValue;

        // 追加プロパティ
        [JsonIgnore]
        public string Category => GetCategoryName();

        [JsonIgnore]
        public string[] SupportedAvatars => SupportedAvatar;

        [JsonIgnore]
        public string[] Tags => Array.Empty<string>();

        [JsonIgnore]
        public string Memo => ItemMemo;

        public string GetCategoryName()
        {
            // Typeを数値に変換
            if (int.TryParse(Type, out int typeValue))
            {
                if (typeValue == 9 && !string.IsNullOrEmpty(CustomCategory))
                {
                    return CustomCategory;
                }

                return typeValue switch
                {
                    0 => "アバター",
                    1 => "衣装",
                    2 => "テクスチャ",
                    3 => "ギミック",
                    4 => "アクセサリー",
                    5 => "ワールド",
                    6 => "アバターギミック",
                    7 => "アバターアクセサリー",
                    8 => "アバター衣装",
                    _ => "その他",
                };
            }

            return "その他";
        }
    }
    #endregion
}
