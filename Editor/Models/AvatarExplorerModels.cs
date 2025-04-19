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
    /// アセットアイテムのリストを管理する
    /// </summary>
    public class AvatarExplorerDatabase
    {
        /// <summary>
        /// アセットアイテムのリスト
        /// </summary>
        [JsonProperty("Items")]
        public List<AvatarExplorerItem> Items { get; set; } = new List<AvatarExplorerItem>();

        /// <summary>
        /// デフォルトコンストラクタ
        /// </summary>
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
    /// アセットの種類を定義する
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
    /// アセットの詳細情報を管理する
    /// </summary>
    public class AvatarExplorerItem
    {
        /// <summary>
        /// アイテムのタイトル
        /// </summary>
        [JsonProperty("Title")]
        public string Title { get; set; } = "";

        /// <summary>
        /// 作者名
        /// </summary>
        [JsonProperty("AuthorName")]
        public string AuthorName { get; set; } = "";

        /// <summary>
        /// アイテムのメモ
        /// </summary>
        [JsonProperty("ItemMemo")]
        public string ItemMemo { get; set; } = "";

        /// <summary>
        /// アイテムのパス
        /// </summary>
        [JsonProperty("ItemPath")]
        public string ItemPath { get; set; } = "";

        /// <summary>
        /// 画像のパス
        /// </summary>
        [JsonProperty("ImagePath")]
        public string ImagePath { get; set; } = "";

        /// <summary>
        /// マテリアルのパス
        /// </summary>
        [JsonProperty("MaterialPath")]
        public string MaterialPath { get; set; } = "";

        /// <summary>
        /// 対応アバターのリスト
        /// </summary>
        [JsonProperty("SupportedAvatar")]
        public string[] SupportedAvatar { get; set; } = Array.Empty<string>();

        /// <summary>
        /// BOOTHのID
        /// </summary>
        [JsonProperty("BoothId")]
        public int BoothId { get; set; } = -1;

        /// <summary>
        /// アイテムのタイプ
        /// </summary>
        [JsonProperty("Type")]
        public string Type { get; set; } = "";

        /// <summary>
        /// カスタムカテゴリー
        /// </summary>
        [JsonProperty("CustomCategory")]
        public string CustomCategory { get; set; } = "";

        /// <summary>
        /// 作者のID
        /// </summary>
        [JsonProperty("AuthorId")]
        public string AuthorId { get; set; } = "";

        /// <summary>
        /// サムネイル画像のURL
        /// </summary>
        [JsonProperty("ThumbnailUrl")]
        public string ThumbnailUrl { get; set; } = "";

        /// <summary>
        /// 作成日時
        /// </summary>
        [JsonProperty("CreatedDate")]
        public DateTime CreatedDate { get; set; } = DateTime.MinValue;

        /// <summary>
        /// アイテムのカテゴリー
        /// </summary>
        [JsonIgnore]
        public string Category => GetAECategoryName();

        /// <summary>
        /// 対応アバターのリスト（エイリアス）
        /// </summary>
        [JsonIgnore]
        public string[] SupportedAvatars => SupportedAvatar;

        /// <summary>
        /// タグのリスト（現在は空配列）
        /// </summary>
        [JsonIgnore]
        public string[] Tags => Array.Empty<string>();

        /// <summary>
        /// アイテムのメモ（エイリアス）
        /// </summary>
        [JsonIgnore]
        public string Memo => ItemMemo;

        /// <summary>
        /// AEアイテムのカテゴリー名を取得
        /// Typeの値に基づいてカテゴリー名を決定する
        /// </summary>
        /// <returns>アイテムのカテゴリー名</returns>
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
        /// <param name="itemType">アイテムのタイプ</param>
        /// <returns>対応するカテゴリー名</returns>
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
