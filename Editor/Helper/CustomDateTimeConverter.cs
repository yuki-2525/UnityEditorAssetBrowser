// Copyright (c) 2025 yuki-2525
// This code is borrowed from AETools(https://github.com/puk06/AE-Tools)
// AETools is licensed under the MIT License. https://github.com/puk06/AE-Tools/blob/master/LICENSE.txt

#nullable enable

using System;
using Newtonsoft.Json;

namespace UnityEditorAssetBrowser.Helper
{
    /// <summary>
    /// カスタム日付変換クラス
    /// </summary>
    public class CustomDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(
            JsonReader reader,
            Type objectType,
            DateTime existingValue,
            bool hasExistingValue,
            JsonSerializer serializer
        )
        {
            if (reader.TokenType == JsonToken.Null)
                return DateTime.MinValue;

            if (reader.TokenType == JsonToken.String)
            {
                string dateString = reader.Value?.ToString() ?? string.Empty;

                // 日付形式の変換
                if (DateTime.TryParse(dateString, out DateTime result))
                    return result;
            }

            throw new JsonSerializationException(
                $"Unexpected token type {reader.TokenType} when parsing DateTime"
            );
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
        }
    }
}
