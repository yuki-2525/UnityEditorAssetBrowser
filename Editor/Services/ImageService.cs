// Copyright (c) 2025 yuki-2525
// This code is borrowed from AssetLibraryManager (https://github.com/MAIOTAchannel/AssetLibraryManager)
// Used with permission from MAIOTAchannel

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditorAssetBrowser.Models;
using UnityEngine;

namespace UnityEditorAssetBrowser.Services
{
    /// <summary>
    /// 画像操作を支援するサービスクラス
    /// テクスチャの読み込み、キャッシュ管理、アイテム画像パスの取得機能を提供する
    /// </summary>
    public class ImageServices
    {
        private static ImageServices? instance;

        /// <summary>
        /// 画像のキャッシュ
        /// キーは画像パス、値は読み込まれたテクスチャ
        /// </summary>
        public Dictionary<string, Texture2D> imageCache { get; } =
            new Dictionary<string, Texture2D>();

        /// <summary>
        /// シングルトンインスタンスを取得
        /// インスタンスが存在しない場合は新規作成する
        /// </summary>
        public static ImageServices Instance
        {
            get
            {
                instance ??= new ImageServices();
                return instance;
            }
        }

        /// <summary>
        /// テクスチャを読み込む
        /// キャッシュに存在する場合はキャッシュから返す
        /// </summary>
        /// <param name="path">読み込むテクスチャのパス</param>
        /// <returns>読み込まれたテクスチャ（読み込みに失敗した場合はnull）</returns>
        /// <exception cref="FileNotFoundException">指定されたパスにファイルが存在しない場合</exception>
        /// <exception cref="IOException">ファイルの読み込みに失敗した場合</exception>
        public Texture2D? LoadTexture(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (imageCache.TryGetValue(path, out var cachedTexture))
            {
                return cachedTexture;
            }

            if (!File.Exists(path))
            {
                Debug.LogWarning($"Texture file not found: {path}");
                return null;
            }

            try
            {
                var bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2);
                if (UnityEngine.ImageConversion.LoadImage(texture, bytes))
                {
                    imageCache[path] = texture;
                    return texture;
                }
                Debug.LogWarning($"Failed to load image: {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading texture from {path}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// アイテムの画像パスを取得
        /// アイテムの種類に応じて適切な画像パスを返す
        /// </summary>
        /// <param name="item">画像パスを取得するアイテム</param>
        /// <returns>アイテムの画像パス（取得できない場合は空文字列）</returns>
        public static string GetItemImagePath(object item)
        {
            return item switch
            {
                AvatarExplorerItem aeItem => aeItem.ImagePath,
                KonoAssetAvatarItem kaItem => kaItem.description.imageFilename,
                KonoAssetWearableItem wearableItem => wearableItem.description.imageFilename,
                KonoAssetWorldObjectItem worldItem => worldItem.description.imageFilename,
                _ => string.Empty,
            };
        }

        /// <summary>
        /// 画像キャッシュをクリア
        /// メモリ使用量を削減するために使用する
        /// </summary>
        public void ClearCache()
        {
            imageCache.Clear();
        }

        /// <summary>
        /// 現在表示中のアイテムの画像を再読み込み
        /// </summary>
        /// <param name="items">再読み込みするアイテムのリスト</param>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        public void ReloadCurrentItemsImages(
            IEnumerable<object> items,
            string aeDatabasePath,
            string kaDatabasePath
        )
        {
            foreach (var item in items)
            {
                string imagePath = GetItemImagePath(item);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullImagePath = GetFullImagePath(
                        imagePath,
                        aeDatabasePath,
                        kaDatabasePath
                    );
                    if (File.Exists(fullImagePath))
                    {
                        LoadTexture(fullImagePath);
                    }
                }
            }
        }

        /// <summary>
        /// 画像の完全なパスを取得
        /// </summary>
        private string GetFullImagePath(
            string imagePath,
            string aeDatabasePath,
            string kaDatabasePath
        )
        {
            if (string.IsNullOrEmpty(imagePath))
                return string.Empty;

            return imagePath.StartsWith("Datas")
                ? Path.Combine(aeDatabasePath, imagePath.Replace("Datas\\", ""))
                : Path.Combine(kaDatabasePath, "images", imagePath);
        }
    }
}
