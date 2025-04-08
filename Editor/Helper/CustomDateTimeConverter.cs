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

        /// <summary>
        /// 日付文字列をDateTimeに変換
        /// </summary>
        /// <param name="date">日付文字列</param>
        /// <returns>DateTime</returns>
        public static DateTime GetDate(string date)
        {
            try
            {
                bool allDigits = true;
                foreach (char c in date)
                {
                    if (!char.IsDigit(c))
                    {
                        allDigits = false;
                        break;
                    }
                }

                if (allDigits)
                    return DateTimeOffset.FromUnixTimeMilliseconds(long.Parse(date)).DateTime;

                var allDigitsStr = "";
                foreach (var c in date)
                {
                    if (char.IsDigit(c))
                        allDigitsStr += c;
                }

                if (allDigitsStr.Length != 14)
                    return DateTimeOffset.FromUnixTimeMilliseconds(0).DateTime;

                var year = allDigitsStr.Substring(0, 4);
                var month = allDigitsStr.Substring(4, 2);
                var day = allDigitsStr.Substring(6, 2);
                var hour = allDigitsStr.Substring(8, 2);
                var minute = allDigitsStr.Substring(10, 2);
                var second = allDigitsStr.Substring(12, 2);

                var dateTime = new DateTime(
                    int.Parse(year),
                    int.Parse(month),
                    int.Parse(day),
                    int.Parse(hour),
                    int.Parse(minute),
                    int.Parse(second),
                    DateTimeKind.Unspecified
                );

                // ローカルのタイムゾーンの時間をUTCに変換
                var utcDateTime = TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.Local);

                return utcDateTime;
            }
            catch
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(0).DateTime;
            }
        }
    }
}
