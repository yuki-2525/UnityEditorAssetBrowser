// Copyright (c) 2025 yuki-2525
// This code is borrowed from AETools(https://github.com/puk06/AE-Tools)
// AETools is licensed under the MIT License. https://github.com/puk06/AE-Tools/blob/master/LICENSE.txt

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorAssetBrowser.Models;
using UnityEngine;

namespace UnityEditorAssetBrowser.Models
{
    #region Base Database Models
    /// <summary>
    /// KonoAssetの基本データベースモデル
    /// データベースのバージョンとアイテムリストを管理する
    /// </summary>
    public class KonoAssetDatabase
    {
        /// <summary>
        /// データベースのバージョン
        /// </summary>
        public int version { get; set; }

        /// <summary>
        /// アイテムのリスト
        /// </summary>
        public object[] data { get; set; } = Array.Empty<object>();
    }
    #endregion

    #region Specific Database Models
    /// <summary>
    /// アバター用データベース
    /// アバターアイテムのリストを管理する
    /// </summary>
    public class KonoAssetAvatarsDatabase : KonoAssetDatabase
    {
        /// <summary>
        /// アバターアイテムのリスト
        /// </summary>
        public new KonoAssetAvatarItem[] data { get; set; } = Array.Empty<KonoAssetAvatarItem>();
    }

    /// <summary>
    /// ウェアラブル用データベース
    /// ウェアラブルアイテムのリストを管理する
    /// </summary>
    public class KonoAssetWearablesDatabase : KonoAssetDatabase
    {
        /// <summary>
        /// ウェアラブルアイテムのリスト
        /// </summary>
        public new KonoAssetWearableItem[] data { get; set; } =
            Array.Empty<KonoAssetWearableItem>();
    }

    /// <summary>
    /// ワールドオブジェクト用データベース
    /// ワールドオブジェクトアイテムのリストを管理する
    /// </summary>
    public class KonoAssetWorldObjectsDatabase : KonoAssetDatabase
    {
        /// <summary>
        /// ワールドオブジェクトアイテムのリスト
        /// </summary>
        public new KonoAssetWorldObjectItem[] data { get; set; } =
            Array.Empty<KonoAssetWorldObjectItem>();
    }

    /// <summary>
    /// その他アセット用データベース
    /// その他アセットアイテムのリストを管理する
    /// </summary>
    public class KonoAssetOtherAssetsDatabase : KonoAssetDatabase
    {
        /// <summary>
        /// その他アセットアイテムのリスト
        /// </summary>
        public new KonoAssetOtherAssetItem[] data { get; set; } =
            Array.Empty<KonoAssetOtherAssetItem>();
    }
    #endregion

    #region Item Models
    /// <summary>
    /// ウェアラブルアイテムモデル
    /// 衣装やアクセサリーなどのアイテム情報を管理する
    /// </summary>
    public class KonoAssetWearableItem
    {
        /// <summary>
        /// アイテムのID
        /// </summary>
        public string id { get; set; } = "";

        /// <summary>
        /// アイテムの詳細情報
        /// </summary>
        public KonoAssetDescription description { get; set; } = new KonoAssetDescription();

        /// <summary>
        /// アイテムのカテゴリー
        /// </summary>
        public string category { get; set; } = "";

        /// <summary>
        /// 対応アバターのリスト
        /// </summary>
        public string[] supportedAvatars { get; set; } = Array.Empty<string>();
    }

    /// <summary>
    /// アバターアイテムモデル
    /// アバターの情報を管理する
    /// </summary>
    public class KonoAssetAvatarItem
    {
        /// <summary>
        /// アバターのID
        /// </summary>
        public string id { get; set; } = "";

        /// <summary>
        /// アバターの詳細情報
        /// </summary>
        public KonoAssetDescription description { get; set; } = new KonoAssetDescription();
    }

    /// <summary>
    /// ワールドオブジェクトアイテムモデル
    /// ワールドオブジェクトの情報を管理する
    /// </summary>
    public class KonoAssetWorldObjectItem
    {
        /// <summary>
        /// オブジェクトのID
        /// </summary>
        public string id { get; set; } = "";

        /// <summary>
        /// オブジェクトの詳細情報
        /// </summary>
        public KonoAssetDescription description { get; set; } = new KonoAssetDescription();

        /// <summary>
        /// オブジェクトのカテゴリー
        /// </summary>
        public string category { get; set; } = "";
    }

    /// <summary>
    /// その他アセットアイテムモデル
    /// その他アセットの情報を管理する
    /// </summary>
    public class KonoAssetOtherAssetItem
    {
        /// <summary>
        /// アセットのID
        /// </summary>
        public string id { get; set; } = "";

        /// <summary>
        /// アセットの詳細情報
        /// </summary>
        public KonoAssetDescription description { get; set; } = new KonoAssetDescription();

        /// <summary>
        /// アセットのカテゴリー
        /// </summary>
        public string category { get; set; } = "";
    }
    #endregion

    #region Description Model
    /// <summary>
    /// KonoAssetアイテムの詳細情報モデル
    /// アイテムの基本情報を管理する
    /// </summary>
    public class KonoAssetDescription
    {
        /// <summary>
        /// アイテムの名前
        /// </summary>
        public string name { get; set; } = "";

        /// <summary>
        /// 作者名
        /// </summary>
        public string creator { get; set; } = "";

        /// <summary>
        /// 画像ファイル名
        /// </summary>
        public string imageFilename { get; set; } = "";

        /// <summary>
        /// タグのリスト
        /// </summary>
        public string[] tags { get; set; } = Array.Empty<string>();

        /// <summary>
        /// メモ
        /// </summary>
        public string? memo { get; set; }

        /// <summary>
        /// BOOTHのアイテムID
        /// </summary>
        public int? boothItemId { get; set; }

        /// <summary>
        /// 依存アイテムのリスト
        /// </summary>
        public string[] dependencies { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 作成日時（UnixTimeMilliseconds）
        /// </summary>
        public long createdAt { get; set; }

        /// <summary>
        /// 公開日時（UnixTimeMilliseconds）
        /// </summary>
        public long? publishedAt { get; set; }
    }
    #endregion
}
