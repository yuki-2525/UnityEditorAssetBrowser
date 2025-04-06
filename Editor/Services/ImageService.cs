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
    /// </summary>
    public class ImageServices
    {
        private static ImageServices? instance;

        /// <summary>
        /// 画像キャッシュ
        /// </summary>
        public Dictionary<string, Texture2D> imageCache { get; } =
            new Dictionary<string, Texture2D>();

        /// <summary>
        /// シングルトンインスタンスを取得
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
        /// </summary>
        /// <param name="path">テクスチャパス</param>
        /// <returns>読み込んだテクスチャ</returns>
        public Texture2D? LoadTexture(string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            if (imageCache.TryGetValue(path, out var cachedTexture))
            {
                return cachedTexture;
            }

            if (!File.Exists(path))
                return null;

            try
            {
                var bytes = File.ReadAllBytes(path);
                var texture = new Texture2D(2, 2);
                if (UnityEngine.ImageConversion.LoadImage(texture, bytes))
                {
                    imageCache[path] = texture;
                    return texture;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Error loading texture from {path}: {ex.Message}");
            }

            return null;
        }

        /// <summary>
        /// アイテムの画像パスを取得
        /// </summary>
        /// <param name="item">アイテム</param>
        /// <returns>画像パス</returns>
        public static string GetItemImagePath(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.ImagePath;
            }
            else if (item is KonoAssetAvatarItem kaItem)
            {
                return kaItem.description.imageFilename;
            }
            else if (item is KonoAssetWearableItem wearableItem)
            {
                return wearableItem.description.imageFilename;
            }
            else if (item is KonoAssetWorldObjectItem worldItem)
            {
                return worldItem.description.imageFilename;
            }

            return string.Empty;
        }

        /// <summary>
        /// キャッシュをクリアする
        /// </summary>
        public void ClearCache()
        {
            imageCache.Clear();
        }
    }
}
