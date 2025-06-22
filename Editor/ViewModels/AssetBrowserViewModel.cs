// Copyright (c) 2025 yuki-2525
// This code is borrowed from AvatarExplorer(https://github.com/puk06/AvatarExplorer)
// AvatarExplorer is licensed under the MIT License. https://github.com/puk06/AvatarExplorer/blob/main/LICENSE
// This code is borrowed from AssetLibraryManager (https://github.com/MAIOTAchannel/AssetLibraryManager)
// Used with permission from MAIOTAchannel

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditorAssetBrowser.Helper;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Services;
using UnityEngine;

namespace UnityEditorAssetBrowser.ViewModels
{
    /// <summary>
    /// アセットブラウザのビューモデル
    /// アセットの検索、フィルタリング、ソート、データベースの管理を行う
    /// </summary>
    public class AssetBrowserViewModel
    {
        // イベント定義
        public event Action? SortMethodChanged;
        public event Action<string>? ErrorOccurred;

        private AvatarExplorerDatabase? _aeDatabase;
        private KonoAssetAvatarsDatabase? _kaAvatarsDatabase;
        private KonoAssetWearablesDatabase? _kaWearablesDatabase;
        private KonoAssetWorldObjectsDatabase? _kaWorldObjectsDatabase;
        private KonoAssetOtherAssetsDatabase? _kaOtherAssetsDatabase;
        private readonly PaginationInfo _paginationInfo;
        private SortMethod _currentSortMethod = SortMethod.CreatedDateDesc;
        private readonly SearchViewModel _searchViewModel;
        private string? _lastError;

        /// <summary>ソート処理用のAssetItemヘルパーインスタンス（パフォーマンス最適化）</summary>
        private static readonly AssetItem _assetItemHelper = new AssetItem();

        public string? LastError => _lastError;

        public SortMethod CurrentSortMethod => _currentSortMethod;

        public AssetBrowserViewModel(
            AvatarExplorerDatabase? aeDatabase,
            KonoAssetAvatarsDatabase? kaAvatarsDatabase,
            KonoAssetWearablesDatabase? kaWearablesDatabase,
            KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase,
            KonoAssetOtherAssetsDatabase? kaOtherAssetsDatabase,
            PaginationInfo paginationInfo,
            SearchViewModel searchViewModel
        )
        {
            _aeDatabase = aeDatabase;
            _kaAvatarsDatabase = kaAvatarsDatabase;
            _kaWearablesDatabase = kaWearablesDatabase;
            _kaWorldObjectsDatabase = kaWorldObjectsDatabase;
            _kaOtherAssetsDatabase = kaOtherAssetsDatabase;
            _paginationInfo = paginationInfo;
            _searchViewModel = searchViewModel;
            _currentSortMethod = SortMethod.CreatedDateDesc; // デフォルト値を設定
        }

        /// <summary>
        /// 初期化処理を行う
        /// </summary>
        public void Initialize()
        {
            LoadSortMethod();
        }

        public enum SortMethod
        {
            /// <summary>追加順（新しい順）</summary>
            CreatedDateDesc,

            /// <summary>追加順（古い順）</summary>
            CreatedDateAsc,

            /// <summary>アセット名（A-Z順）</summary>
            TitleAsc,

            /// <summary>アセット名（Z-A順）</summary>
            TitleDesc,

            /// <summary>ショップ名（A-Z順）</summary>
            AuthorAsc,

            /// <summary>ショップ名（Z-A順）</summary>
            AuthorDesc,

            /// <summary>BOOTHID順（新しい順、大→小）</summary>
            BoothIdDesc,

            /// <summary>BOOTHID順（古い順、小→大）</summary>
            BoothIdAsc,
        }

        /// <summary>
        /// フィルターされたアバターリストを取得
        /// </summary>
        /// <returns>フィルターされたアバターリスト</returns>
        public List<object> GetFilteredAvatars()
        {
            var items = new List<object>();

            // AEのアバターを追加
            if (_aeDatabase?.Items != null)
            {
                items.AddRange(
                    _aeDatabase.Items.Where(item =>
                    {
                        var category = item.GetAECategoryName();
                        var key = "UnityEditorAssetBrowser_CategoryAssetType_" + category;

                        // アセットタイプが0（アバター）のアイテムのみを表示
                        if (EditorPrefs.HasKey(key))
                        {
                            return EditorPrefs.GetInt(key) == AssetTypeConstants.AVATAR;
                        }
                        return item.Type == "0"; // キーが存在しない場合は従来の判定
                    })
                );
            }

            // KAのアバターを追加
            if (_kaAvatarsDatabase?.data != null)
            {
                items.AddRange(_kaAvatarsDatabase.data);
            }

            return SortItems(items.Where(_searchViewModel.IsItemMatchSearch).ToList());
        }

        /// <summary>
        /// フィルターされたアイテムリストを取得
        /// </summary>
        /// <returns>フィルターされたアイテムリスト</returns>
        public List<object> GetFilteredItems()
        {
            var items = new List<object>();
            if (_aeDatabase != null)
            {
                items.AddRange(
                    _aeDatabase.Items.Where(item =>
                    {
                        var category = item.GetAECategoryName();
                        var key = "UnityEditorAssetBrowser_CategoryAssetType_" + category;

                        // アセットタイプが1（アバター関連アセット）のアイテムのみを表示
                        if (EditorPrefs.HasKey(key))
                        {
                            return EditorPrefs.GetInt(key) == AssetTypeConstants.AVATAR_RELATED;
                        }
                        // キーが存在しない場合は従来の判定
                        return item.Type != "0"
                            && !item.CustomCategory.Contains(
                                "ワールド",
                                StringComparison.OrdinalIgnoreCase
                            )
                            && !item.CustomCategory.Contains(
                                "world",
                                StringComparison.OrdinalIgnoreCase
                            );
                    })
                );
            }
            if (_kaWearablesDatabase != null)
            {
                items.AddRange(_kaWearablesDatabase.data);
            }

            return SortItems(items.Where(_searchViewModel.IsItemMatchSearch).ToList());
        }

        /// <summary>
        /// フィルターされたワールドオブジェクトリストを取得
        /// </summary>
        /// <returns>フィルターされたワールドオブジェクトリスト</returns>
        public List<object> GetFilteredWorldObjects()
        {
            var items = new List<object>();

            // AEのワールドアイテムを追加
            if (_aeDatabase?.Items != null)
            {
                items.AddRange(
                    _aeDatabase.Items.Where(item =>
                    {
                        var category = item.GetAECategoryName();
                        var key = "UnityEditorAssetBrowser_CategoryAssetType_" + category;

                        // アセットタイプが2（ワールドオブジェクト）のアイテムのみを表示
                        if (EditorPrefs.HasKey(key))
                        {
                            return EditorPrefs.GetInt(key) == AssetTypeConstants.WORLD;
                        }
                        // キーが存在しない場合は従来の判定
                        return item.Type != "0"
                            && (
                                item.CustomCategory.Contains(
                                    "ワールド",
                                    StringComparison.OrdinalIgnoreCase
                                )
                                || item.CustomCategory.Contains(
                                    "world",
                                    StringComparison.OrdinalIgnoreCase
                                )
                            );
                    })
                );
            }

            // KAのワールドオブジェクトを追加
            if (_kaWorldObjectsDatabase?.data != null)
            {
                items.AddRange(_kaWorldObjectsDatabase.data);
            }

            return SortItems(items.Where(_searchViewModel.IsItemMatchSearch).ToList());
        }

        /// <summary>
        /// その他のアセットをフィルタリングして取得
        /// </summary>
        /// <returns>フィルタリングされたその他のアセットのリスト</returns>
        public List<object> GetFilteredOthers()
        {
            var items = new List<object>();

            // AEデータベースのアイテムをフィルタリング
            if (_aeDatabase != null)
            {
                foreach (var item in _aeDatabase.Items)
                {
                    var category = item.GetAECategoryName();
                    var key = "UnityEditorAssetBrowser_CategoryAssetType_" + category;

                    // 設定されたアセットタイプに基づいてフィルタリング
                    if (EditorPrefs.HasKey(key))
                    {
                        var assetType = EditorPrefs.GetInt(key);
                        if (assetType == AssetTypeConstants.OTHER) // その他
                        {
                            items.Add(item);
                        }
                    }
                }
            }

            // KAのその他アセットを追加
            if (_kaOtherAssetsDatabase?.data != null)
            {
                items.AddRange(_kaOtherAssetsDatabase.data);
            }

            return SortItems(items.Where(_searchViewModel.IsItemMatchSearch).ToList());
        }

        /// <summary>
        /// アイテムをソートする
        /// </summary>
        /// <param name="items">ソートするアイテムリスト</param>
        /// <returns>ソートされたアイテムリスト</returns>
        public List<object> SortItems(List<object> items)
        {
            switch (_currentSortMethod)
            {
                case SortMethod.CreatedDateDesc:
                    return items.OrderByDescending(item => GetCreatedDate(item)).ToList();
                case SortMethod.CreatedDateAsc:
                    return items.OrderBy(item => GetCreatedDate(item)).ToList();
                case SortMethod.TitleAsc:
                    return items.OrderBy(item => GetTitle(item)).ToList();
                case SortMethod.TitleDesc:
                    return items.OrderByDescending(item => GetTitle(item)).ToList();
                case SortMethod.AuthorAsc:
                    return items.OrderBy(item => GetAuthor(item)).ToList();
                case SortMethod.AuthorDesc:
                    return items.OrderByDescending(item => GetAuthor(item)).ToList();
                case SortMethod.BoothIdDesc:
                    return items
                        .OrderByDescending(item => _assetItemHelper.GetBoothItemId(item))
                        .ToList();
                case SortMethod.BoothIdAsc:
                    return items.OrderBy(item => _assetItemHelper.GetBoothItemId(item)).ToList();
                default:
                    return items;
            }
        }

        /// <summary>
        /// ソート方法を設定する
        /// </summary>
        /// <param name="method">設定するソート方法</param>
        public void SetSortMethod(SortMethod method)
        {
            if (_currentSortMethod != method)
            {
                _currentSortMethod = method;
                SaveSortMethod();
                SortMethodChanged?.Invoke();
            }
        }

        /// <summary>
        /// 現在のソート方法を保存する
        /// </summary>
        private void SaveSortMethod()
        {
            EditorPrefs.SetInt(
                $"AssetBrowser_SortMethod_{_paginationInfo.SelectedTab}",
                (int)_currentSortMethod
            );
        }

        /// <summary>
        /// 保存されたソート方法を読み込む
        /// </summary>
        private void LoadSortMethod()
        {
            _currentSortMethod = (SortMethod)
                EditorPrefs.GetInt(
                    $"AssetBrowser_SortMethod_{_paginationInfo.SelectedTab}",
                    (int)SortMethod.CreatedDateDesc
                );
        }

        /// <summary>
        /// エラーを処理する
        /// </summary>
        /// <param name="message">エラーメッセージ</param>
        private void HandleError(string message)
        {
            _lastError = message;
            ErrorOccurred?.Invoke(message);
            Debug.LogError(message);
        }

        /// <summary>
        /// 画像キャッシュを再取得する（非推奨 - UpdateVisibleImagesを使用）
        /// 新しい実装では表示中のアイテムのみキャッシュするため、このメソッドは使用しない
        /// </summary>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        [System.Obsolete("RefreshImageCache is deprecated. Use ImageServices.Instance.UpdateVisibleImages instead.")]
        public void RefreshImageCache(string aeDatabasePath, string kaDatabasePath)
        {
            // 新しい実装ではキャッシュクリアのみ実行
            // 実際の画像読み込みは表示時に UpdateVisibleImages で行う
            ImageServices.Instance.ClearCache();
        }

        #region Helper Methods for Sorting
        private DateTime? GetCreatedDate(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.CreatedDate;
            }
            else if (item is KonoAssetAvatarItem kaAvatarItem)
            {
                return kaAvatarItem.description.createdAt > 0
                    ? DateTimeOffset
                        .FromUnixTimeMilliseconds(kaAvatarItem.description.createdAt)
                        .DateTime
                    : (DateTime?)null;
            }
            else if (item is KonoAssetWearableItem kaWearableItem)
            {
                return kaWearableItem.description.createdAt > 0
                    ? DateTimeOffset
                        .FromUnixTimeMilliseconds(kaWearableItem.description.createdAt)
                        .DateTime
                    : (DateTime?)null;
            }
            else if (item is KonoAssetWorldObjectItem kaWorldObjectItem)
            {
                return kaWorldObjectItem.description.createdAt > 0
                    ? DateTimeOffset
                        .FromUnixTimeMilliseconds(kaWorldObjectItem.description.createdAt)
                        .DateTime
                    : (DateTime?)null;
            }
            else if (item is KonoAssetOtherAssetItem kaOtherAssetItem)
            {
                return kaOtherAssetItem.description.createdAt > 0
                    ? DateTimeOffset
                        .FromUnixTimeMilliseconds(kaOtherAssetItem.description.createdAt)
                        .DateTime
                    : (DateTime?)null;
            }
            return null;
        }

        private string GetTitle(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.Title ?? "";
            }
            else if (item is KonoAssetAvatarItem kaAvatarItem)
            {
                return kaAvatarItem.description.name ?? "";
            }
            else if (item is KonoAssetWearableItem kaWearableItem)
            {
                return kaWearableItem.description.name ?? "";
            }
            else if (item is KonoAssetWorldObjectItem kaWorldObjectItem)
            {
                return kaWorldObjectItem.description.name ?? "";
            }
            else if (item is KonoAssetOtherAssetItem kaOtherAssetItem)
            {
                return kaOtherAssetItem.description.name ?? "";
            }
            return "";
        }

        private string GetAuthor(object item)
        {
            if (item is AvatarExplorerItem aeItem)
            {
                return aeItem.AuthorName ?? "";
            }
            else if (item is KonoAssetAvatarItem kaAvatarItem)
            {
                return kaAvatarItem.description.creator ?? "";
            }
            else if (item is KonoAssetWearableItem kaWearableItem)
            {
                return kaWearableItem.description.creator ?? "";
            }
            else if (item is KonoAssetWorldObjectItem kaWorldObjectItem)
            {
                return kaWorldObjectItem.description.creator ?? "";
            }
            else if (item is KonoAssetOtherAssetItem kaOtherAssetItem)
            {
                return kaOtherAssetItem.description.creator ?? "";
            }
            return "";
        }
        #endregion

        /// <summary>
        /// 画像のフルパスを取得
        /// </summary>
        /// <param name="imagePath">画像の相対パス</param>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        /// <returns>画像のフルパス</returns>
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

        /// <summary>
        /// テクスチャを読み込む
        /// </summary>
        /// <param name="imagePath">画像の相対パス</param>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        /// <returns>読み込まれたテクスチャ、読み込みに失敗した場合はnull</returns>
        private Texture2D? LoadTexture(
            string imagePath,
            string aeDatabasePath,
            string kaDatabasePath
        )
        {
            string fullImagePath = GetFullImagePath(imagePath, aeDatabasePath, kaDatabasePath);
            return File.Exists(fullImagePath)
                ? ImageServices.Instance.LoadTexture(fullImagePath)
                : null;
        }

        /// <summary>
        /// 現在のタブのアイテムを取得
        /// </summary>
        /// <param name="selectedTab">選択中のタブ（0: アバター, 1: アイテム, 2: ワールドオブジェクト）</param>
        /// <returns>現在のタブのアイテムリスト</returns>
        public List<object> GetCurrentTabItems(int selectedTab) =>
            selectedTab switch
            {
                0 => GetFilteredAvatars(),
                1 => GetFilteredItems(),
                2 => GetFilteredWorldObjects(),
                _ => new List<object>(),
            };

        /// <summary>
        /// データベースを更新する
        /// </summary>
        public void UpdateDatabases(
            AvatarExplorerDatabase? aeDatabase,
            KonoAssetAvatarsDatabase? kaAvatarsDatabase,
            KonoAssetWearablesDatabase? kaWearablesDatabase,
            KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase,
            KonoAssetOtherAssetsDatabase? kaOtherAssetsDatabase
        )
        {
            // データベースがnullの場合は、即座に更新を完了
            if (
                aeDatabase == null
                && kaAvatarsDatabase == null
                && kaWearablesDatabase == null
                && kaWorldObjectsDatabase == null
                && kaOtherAssetsDatabase == null
            )
            {
                _aeDatabase = null;
                _kaAvatarsDatabase = null;
                _kaWearablesDatabase = null;
                _kaWorldObjectsDatabase = null;
                _kaOtherAssetsDatabase = null;
                return;
            }

            _aeDatabase = aeDatabase;
            _kaAvatarsDatabase = kaAvatarsDatabase;
            _kaWearablesDatabase = kaWearablesDatabase;
            _kaWorldObjectsDatabase = kaWorldObjectsDatabase;
            _kaOtherAssetsDatabase = kaOtherAssetsDatabase;
        }
    }
}
