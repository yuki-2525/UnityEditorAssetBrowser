// Copyright (c) 2025 yuki-2525
// This code is borrowed from AssetLibraryManager (https://github.com/MAIOTAchannel/AssetLibraryManager)
// Used with permission from MAIOTAchannel

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
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

        /// <summary>キャッシュの最大サイズ</summary>
        private int MAX_CACHE_SIZE = 50;

        /// <summary>
        /// 画像のキャッシュ
        /// キーは画像パス、値は読み込まれたテクスチャ
        /// </summary>
        public Dictionary<string, Texture2D> imageCache { get; } =
            new Dictionary<string, Texture2D>();

        /// <summary>LRU管理用のアクセス順序</summary>
        private readonly LinkedList<string> accessOrder = new LinkedList<string>();

        /// <summary>現在表示中の画像パス</summary>
        private readonly HashSet<string> currentVisibleImages = new HashSet<string>();

        /// <summary>画像パスとLinkedListNodeのマッピング</summary>
        private readonly Dictionary<string, LinkedListNode<string>> nodeMap = new Dictionary<string, LinkedListNode<string>>();

        /// <summary>現在読み込み中の画像パス</summary>
        private readonly HashSet<string> loadingImages = new HashSet<string>();

        /// <summary>プレースホルダーテクスチャ</summary>
        private Texture2D? placeholderTexture;

        /// <summary>メインスレッド処理キュー</summary>
        private readonly Queue<System.Action> mainThreadQueue = new Queue<System.Action>();

        /// <summary>
        /// シングルトンインスタンスを取得
        /// インスタンスが存在しない場合は新規作成する
        /// </summary>
        public static ImageServices Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ImageServices();
                    instance.InitializePlaceholder();
                    EditorApplication.update += instance.ProcessMainThreadQueue;
                }
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
                return placeholderTexture;

            if (imageCache.TryGetValue(path, out var cachedTexture))
            {
                // LRU更新: 最近使用したアイテムをリストの末尾に移動
                UpdateAccessOrder(path);
                return cachedTexture;
            }

            // 即座に同期読み込みを試行（小さいファイル用）
            if (TryLoadSmallImageSync(path, out var texture))
            {
                return texture;
            }

            // 大きいファイルは非同期読み込み
            LoadTextureAsync(path, priority: 1);
            return placeholderTexture;
        }

        /// <summary>
        /// 小さい画像の同期読み込みを試行
        /// </summary>
        private bool TryLoadSmallImageSync(string path, out Texture2D? texture)
        {
            texture = null;
            
            try
            {
                if (!File.Exists(path)) return false;
                
                var fileInfo = new FileInfo(path);
                // 2MB以下のファイルは同期読み込み（ほとんどの画像をカバー）
                if (fileInfo.Length > 2 * 1024 * 1024) return false;
                
                var bytes = File.ReadAllBytes(path);
                texture = new Texture2D(2, 2);
                
                if (UnityEngine.ImageConversion.LoadImage(texture, bytes))
                {
                    AddToCache(path, texture);
                    return true;
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                    texture = null;
                    return false;
                }
            }
            catch
            {
                if (texture != null)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                    texture = null;
                }
                return false;
            }
        }

        /// <summary>
        /// メインスレッドキューの処理
        /// </summary>
        private void ProcessMainThreadQueue()
        {
            var processCount = 0;
            while (mainThreadQueue.Count > 0 && processCount < 10) // 1フレームで最大10個処理
            {
                var action = mainThreadQueue.Dequeue();
                action?.Invoke();
                processCount++;
            }
        }

        /// <summary>
        /// 大きい画像の非同期読み込み（Task.Run版）
        /// </summary>
        private void LoadLargeImageAsync(string path, Action<Texture2D?>? onComplete)
        {
            try
            {
                if (!File.Exists(path))
                {
                    mainThreadQueue.Enqueue(() => {
                        loadingImages.Remove(path);
                        onComplete?.Invoke(null);
                    });
                    return;
                }

                var bytes = File.ReadAllBytes(path);
                
                // メインスレッドでテクスチャ作成
                mainThreadQueue.Enqueue(() => {
                    CreateTextureFromBytesSync(path, bytes, onComplete);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"Large image load failed for {path}: {ex.Message}");
                mainThreadQueue.Enqueue(() => {
                    loadingImages.Remove(path);
                    onComplete?.Invoke(null);
                });
            }
        }

        /// <summary>
        /// バイト配列からテクスチャを作成（同期版）
        /// </summary>
        private void CreateTextureFromBytesSync(string path, byte[] bytes, Action<Texture2D?>? onComplete)
        {
            Texture2D? texture = null;
            try
            {
                texture = new Texture2D(2, 2);
                if (UnityEngine.ImageConversion.LoadImage(texture, bytes))
                {
                    AddToCache(path, texture);
                }
                else
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                    texture = null;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Texture creation failed for {path}: {ex.Message}");
                if (texture != null)
                {
                    UnityEngine.Object.DestroyImmediate(texture);
                    texture = null;
                }
            }
            
            loadingImages.Remove(path);
            onComplete?.Invoke(texture);
            
            // UI更新
            EditorWindow.focusedWindow?.Repaint();
        }

        /// <summary>
        /// テクスチャを非同期で読み込む
        /// </summary>
        /// <param name="path">読み込むテクスチャのパス</param>
        /// <param name="onComplete">読み込み完了時のコールバック</param>
        /// <param name="priority">読み込み優先度（高い値ほど優先）</param>
        public void LoadTextureAsync(string path, Action<Texture2D?>? onComplete = null, int priority = 0)
        {
            if (string.IsNullOrEmpty(path))
            {
                onComplete?.Invoke(null);
                return;
            }

            // 既にキャッシュに存在する場合
            if (imageCache.TryGetValue(path, out var cachedTexture))
            {
                UpdateAccessOrder(path);
                onComplete?.Invoke(cachedTexture);
                return;
            }

            // 既に読み込み中の場合はスキップ
            if (loadingImages.Contains(path))
            {
                return;
            }

            // 大きいファイルは直接Task.Runで処理（EditorCoroutineより高速）
            loadingImages.Add(path);
            Task.Run(() => LoadLargeImageAsync(path, onComplete));
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
                KonoAssetOtherAssetItem otherItem => otherItem.description.imageFilename,
                _ => string.Empty,
            };
        }

        /// <summary>
        /// 画像キャッシュをクリア
        /// メモリ使用量を削減するために使用する
        /// </summary>
        public void ClearCache()
        {
            // 全テクスチャを適切に解放
            foreach (var texture in imageCache.Values)
            {
                if (texture != null && texture != placeholderTexture)
                {
                    try
                    {
                        UnityEngine.Object.DestroyImmediate(texture);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogWarning($"テクスチャの破棄に失敗しました: {ex.Message}");
                    }
                }
            }
            imageCache.Clear();
            accessOrder.Clear();
            nodeMap.Clear();
            currentVisibleImages.Clear();
            loadingImages.Clear();
        }

        /// <summary>
        /// ImageServiceインスタンスを破棄し、イベントハンドラーをクリーンアップ
        /// </summary>
        public void Dispose()
        {
            try
            {
                EditorApplication.update -= ProcessMainThreadQueue;
                ClearCache();
                
                if (placeholderTexture != null)
                {
                    UnityEngine.Object.DestroyImmediate(placeholderTexture);
                    placeholderTexture = null;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"ImageService dispose中にエラーが発生しました: {ex.Message}");
            }
            finally
            {
                instance = null;
            }
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
        /// 表示中アイテムの画像を更新し、不要な画像を削除
        /// </summary>
        /// <param name="visibleItems">現在表示中のアイテム</param>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        public void UpdateVisibleImages(
            IEnumerable<object> visibleItems,
            string aeDatabasePath,
            string kaDatabasePath
        )
        {
            var newVisibleImages = new HashSet<string>();

            // 新しく表示されるアイテムの画像パス収集
            foreach (var item in visibleItems)
            {
                string imagePath = GetItemImagePath(item);
                if (!string.IsNullOrEmpty(imagePath))
                {
                    string fullImagePath = GetFullImagePath(
                        imagePath,
                        aeDatabasePath,
                        kaDatabasePath
                    );
                    newVisibleImages.Add(fullImagePath);

                    // まだキャッシュにない場合のみ読み込み
                    if (!imageCache.ContainsKey(fullImagePath) && File.Exists(fullImagePath))
                    {
                        LoadTexture(fullImagePath);
                    }
                }
            }

            // 不要になった画像をキャッシュから削除
            var imagesToRemove = currentVisibleImages.Except(newVisibleImages).ToList();
            foreach (var imagePath in imagesToRemove)
            {
                RemoveFromCache(imagePath);
            }

            currentVisibleImages.Clear();
            foreach (var path in newVisibleImages)
            {
                currentVisibleImages.Add(path);
            }

            // 表示中画像の読み込み完了後にEditorWindowを再描画
            if (newVisibleImages.Any())
            {
                EditorApplication.delayCall += () => EditorWindow.focusedWindow?.Repaint();
            }
        }

        /// <summary>
        /// 検索結果に応じてキャッシュサイズを適応的に調整
        /// </summary>
        /// <param name="searchResultCount">検索結果の件数</param>
        public void AdaptCacheSizeToSearchResults(int searchResultCount)
        {
            var newMaxSize = GetOptimalCacheSize(searchResultCount);
            if (newMaxSize < imageCache.Count)
            {
                // キャッシュサイズを削減
                EvictOldestItems(imageCache.Count - newMaxSize);
            }
            MAX_CACHE_SIZE = newMaxSize;
        }

        /// <summary>
        /// キャッシュにテクスチャを追加
        /// </summary>
        private void AddToCache(string path, Texture2D texture)
        {
            // キャッシュサイズ制限チェック
            while (imageCache.Count >= MAX_CACHE_SIZE)
            {
                EvictOldestItem();
            }

            var node = accessOrder.AddLast(path);
            imageCache[path] = texture;
            nodeMap[path] = node;
        }

        /// <summary>
        /// LRUアクセス順序を更新
        /// </summary>
        private void UpdateAccessOrder(string path)
        {
            if (nodeMap.TryGetValue(path, out var node))
            {
                accessOrder.Remove(node);
                node = accessOrder.AddLast(path);
                nodeMap[path] = node;
            }
        }

        /// <summary>
        /// 最も古いアイテムをキャッシュから削除
        /// </summary>
        private void EvictOldestItem()
        {
            if (accessOrder.First == null) return;

            var oldestPath = accessOrder.First.Value;
            RemoveFromCache(oldestPath);
        }

        /// <summary>
        /// 指定された数の古いアイテムを削除
        /// </summary>
        private void EvictOldestItems(int count)
        {
            for (int i = 0; i < count && accessOrder.Count > 0; i++)
            {
                EvictOldestItem();
            }
        }

        /// <summary>
        /// キャッシュから画像を削除
        /// </summary>
        private void RemoveFromCache(string path)
        {
            if (imageCache.TryGetValue(path, out var texture))
            {
                UnityEngine.Object.DestroyImmediate(texture);
                imageCache.Remove(path);
            }

            if (nodeMap.TryGetValue(path, out var node))
            {
                accessOrder.Remove(node);
                nodeMap.Remove(path);
            }
        }

        /// <summary>
        /// 検索結果数に応じた最適なキャッシュサイズを取得
        /// </summary>
        private int GetOptimalCacheSize(int searchResultCount)
        {
            if (searchResultCount <= 10) return 20;      // 小さい結果: 2ページ分
            if (searchResultCount <= 100) return 50;     // 中程度: 適度なキャッシュ
            return 30;                                   // 大きい結果: 省メモリ
        }

        /// <summary>
        /// 画像の完全なパスを取得
        /// </summary>
        /// <summary>
        /// プレースホルダーテクスチャを初期化
        /// </summary>
        private void InitializePlaceholder()
        {
            if (placeholderTexture != null) return;

            placeholderTexture = new Texture2D(100, 100);
            var pixels = new Color32[100 * 100];
            
            // シンプルなチェッカーボードパターンを生成
            for (int y = 0; y < 100; y++)
            {
                for (int x = 0; x < 100; x++)
                {
                    var isWhite = (x / 10 + y / 10) % 2 == 0;
                    pixels[y * 100 + x] = isWhite 
                        ? new Color32(240, 240, 240, 255) 
                        : new Color32(200, 200, 200, 255);
                }
            }
            
            placeholderTexture.SetPixels32(pixels);
            placeholderTexture.Apply();
        }


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
