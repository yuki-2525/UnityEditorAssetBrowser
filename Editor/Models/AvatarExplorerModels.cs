// Copyright (c) 2025 yuki-2525
// This code is borrowed from AvatarExplorer(https://github.com/yuki-2525/AvatarExplorer)
// AvatarExplorer is licensed under the MIT License. https://github.com/yuki-2525/AvatarExplorer/blob/main/LICENSE

#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditorAssetBrowser.Models;

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
    /// AvatarExplorerのアイテムタイプ
    /// </summary>
    public enum AvatarExplorerItemType
    {
        /// <summary>
        /// アバター
        /// </summary>
        Avatar = 0,

        /// <summary>
        /// 衣装
        /// </summary>
        Clothing = 1,

        /// <summary>
        /// テクスチャ
        /// </summary>
        Texture = 2,

        /// <summary>
        /// ギミック
        /// </summary>
        Gimmick = 3,

        /// <summary>
        /// アクセサリー
        /// </summary>
        Accessory = 4,

        /// <summary>
        /// 髪型
        /// </summary>
        HairStyle = 5,

        /// <summary>
        /// アニメーション
        /// </summary>
        Animation = 6,

        /// <summary>
        /// ツール
        /// </summary>
        Tool = 7,

        /// <summary>
        /// シェーダー
        /// </summary>
        Shader = 8,

        /// <summary>
        /// カスタムカテゴリー
        /// </summary>
        Custom = 9,

        /// <summary>
        /// 不明
        /// </summary>
        Unknown = 10,
    }

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

        [JsonIgnore]
        public string Category => GetAECategoryName();

        [JsonIgnore]
        public string[] SupportedAvatars => SupportedAvatar;

        [JsonIgnore]
        public string[] Tags => Array.Empty<string>();

        [JsonIgnore]
        public string Memo => ItemMemo;

        /// <summary>
        /// AEアイテムのカテゴリー名を取得
        /// </summary>
        /// <returns>カテゴリー名</returns>
        public string GetAECategoryName()
        {
            // Typeが数値として保存されている場合の処理
            if (int.TryParse(Type, out int typeValue))
            {
                return GetCategoryNameByType((AvatarExplorerItemType)typeValue);
            }

            // Typeが文字列として保存されている場合の処理
            if (Enum.TryParse(Type, true, out AvatarExplorerItemType itemType))
            {
                return GetCategoryNameByType(itemType);
            }

            // デフォルトはカスタムカテゴリー
            return CustomCategory;
        }

        /// <summary>
        /// タイプに基づいてカテゴリー名を取得
        /// </summary>
        /// <param name="itemType">アイテムタイプ</param>
        /// <returns>カテゴリー名</returns>
        private string GetCategoryNameByType(AvatarExplorerItemType itemType)
        {
            return itemType switch
            {
                AvatarExplorerItemType.Avatar => "アバター",
                AvatarExplorerItemType.Clothing => "衣装",
                AvatarExplorerItemType.Texture => "テクスチャ",
                AvatarExplorerItemType.Gimmick => "ギミック",
                AvatarExplorerItemType.Accessory => "アクセサリー",
                AvatarExplorerItemType.HairStyle => "髪型",
                AvatarExplorerItemType.Animation => "アニメーション",
                AvatarExplorerItemType.Tool => "ツール",
                AvatarExplorerItemType.Shader => "シェーダー",
                AvatarExplorerItemType.Custom => CustomCategory,
                _ => "不明",
            };
        }
    }
    #endregion
}
