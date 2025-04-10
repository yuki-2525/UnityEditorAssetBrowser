// Copyright (c) 2025 yuki-2525
// This code is borrowed from AvatarExplorer(https://github.com/yuki-2525/AvatarExplorer)
// AvatarExplorer is licensed under the MIT License. https://github.com/yuki-2525/AvatarExplorer/blob/main/LICENSE

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
    }
    #endregion
}
