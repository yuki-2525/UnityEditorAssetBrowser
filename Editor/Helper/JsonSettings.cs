// Copyright (c) 2025 yuki-2525
// This code is borrowed from AETools(https://github.com/puk06/AE-Tools)
// AETools is licensed under the MIT License. https://github.com/puk06/AE-Tools/blob/master/LICENSE.txt

#nullable enable

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditorAssetBrowser.Helper;

namespace UnityEditorAssetBrowser.Helper
{
    /// <summary>
    /// JSON設定を提供するクラス
    /// </summary>
    public static class JsonSettings
    {
        /// <summary>
        /// JSONシリアライズ設定
        /// </summary>
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            DateParseHandling = DateParseHandling.None,
            Converters = new List<JsonConverter> { new CustomDateTimeConverter() },
        };
    }
}
