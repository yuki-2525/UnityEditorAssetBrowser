using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnityEditorAssetBrowser.Models
{
    #region Base Database Models
    /// <summary>
    /// KonoAssetの基本データベースモデル
    /// </summary>
    public class KonoAssetDatabase
    {
        public int version { get; set; }
        public object[] data { get; set; } = Array.Empty<object>();
    }
    #endregion

    #region Specific Database Models
    /// <summary>
    /// アバター用データベース
    /// </summary>
    public class KonoAssetAvatarsDatabase : KonoAssetDatabase
    {
        public new KonoAssetAvatarItem[] data { get; set; } = Array.Empty<KonoAssetAvatarItem>();
    }

    /// <summary>
    /// ウェアラブル用データベース
    /// </summary>
    public class KonoAssetWearablesDatabase : KonoAssetDatabase
    {
        public new KonoAssetWearableItem[] data { get; set; } =
            Array.Empty<KonoAssetWearableItem>();
    }

    /// <summary>
    /// ワールドオブジェクト用データベース
    /// </summary>
    public class KonoAssetWorldObjectsDatabase : KonoAssetDatabase
    {
        public new KonoAssetWorldObjectItem[] data { get; set; } =
            Array.Empty<KonoAssetWorldObjectItem>();
    }
    #endregion

    #region Item Models
    /// <summary>
    /// ウェアラブルアイテムモデル
    /// </summary>
    public class KonoAssetWearableItem
    {
        public string id { get; set; } = "";
        public KonoAssetDescription description { get; set; } = new KonoAssetDescription();
        public string category { get; set; } = "";
        public string[] supportedAvatars { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// アバターアイテムモデル
    /// </summary>
    public class KonoAssetAvatarItem
    {
        public string id { get; set; } = "";
        public KonoAssetDescription description { get; set; } = new KonoAssetDescription();
    }

    /// <summary>
    /// ワールドオブジェクトアイテムモデル
    /// </summary>
    public class KonoAssetWorldObjectItem
    {
        public string id { get; set; } = "";
        public KonoAssetDescription description { get; set; } = new KonoAssetDescription();
        public string category { get; set; } = "";
    }
    #endregion

    #region Description Model
    /// <summary>
    /// KonoAssetアイテムの詳細情報モデル
    /// </summary>
    public class KonoAssetDescription
    {
        public string name { get; set; } = "";
        public string creator { get; set; } = "";
        public string imageFilename { get; set; } = "";
        public string[] tags { get; set; } = Array.Empty<string>();
        public string? memo { get; set; }
        public int? boothItemId { get; set; }
        public string[] dependencies { get; set; } = Array.Empty<string>();
        public long createdAt { get; set; }
        public long publishedAt { get; set; }
    }
    #endregion
}
