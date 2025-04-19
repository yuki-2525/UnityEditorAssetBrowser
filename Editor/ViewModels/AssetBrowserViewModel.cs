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
        public event Action? DatabaseUpdated;
        public event Action? SortMethodChanged;
        public event Action<string>? ErrorOccurred;

        private AvatarExplorerDatabase? _aeDatabase;
        private KonoAssetAvatarsDatabase? _kaAvatarsDatabase;
        private KonoAssetWearablesDatabase? _kaWearablesDatabase;
        private KonoAssetWorldObjectsDatabase? _kaWorldObjectsDatabase;
        private readonly PaginationInfo _paginationInfo;
        private SortMethod _currentSortMethod = SortMethod.CreatedDateDesc;
        private readonly SearchViewModel _searchViewModel;
        private string? _lastError;

        public string? LastError => _lastError;

        public SortMethod CurrentSortMethod => _currentSortMethod;

        public AssetBrowserViewModel(
            AvatarExplorerDatabase? aeDatabase,
            KonoAssetAvatarsDatabase? kaAvatarsDatabase,
            KonoAssetWearablesDatabase? kaWearablesDatabase,
            KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase,
            PaginationInfo paginationInfo,
            SearchViewModel searchViewModel
        )
        {
            _aeDatabase = aeDatabase;
            _kaAvatarsDatabase = kaAvatarsDatabase;
            _kaWearablesDatabase = kaWearablesDatabase;
            _kaWorldObjectsDatabase = kaWorldObjectsDatabase;
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
        }

        /// <summary>
        /// フィルターされたアバターリストを取得
        /// </summary>
        /// <returns>フィルターされたアバターリスト</returns>
        public List<object> GetFilteredAvatars()
        {
            var items = new List<object>();

            // AEのアバター（type=0）を追加
            if (_aeDatabase?.Items != null)
            {
                items.AddRange(_aeDatabase.Items.Where(item => item.Type == "0"));
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
                // Type!=0 かつ CustomCategoryに"ワールド"または"world"が含まれていないアイテムを追加
                items.AddRange(
                    _aeDatabase.Items.Where(item =>
                        item.Type != "0"
                        && !item.CustomCategory.Contains(
                            "ワールド",
                            StringComparison.OrdinalIgnoreCase
                        )
                        && !item.CustomCategory.Contains(
                            "world",
                            StringComparison.OrdinalIgnoreCase
                        )
                    )
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
                        item.Type != "0"
                        && (
                            item.CustomCategory.Contains(
                                "ワールド",
                                StringComparison.OrdinalIgnoreCase
                            )
                            || item.CustomCategory.Contains(
                                "world",
                                StringComparison.OrdinalIgnoreCase
                            )
                        )
                    )
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
                default:
                    return items;
            }
        }

        /// <summary>
        /// データベースを更新する
        /// </summary>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        /// <returns>更新が成功したかどうか</returns>
        public async Task<bool> UpdateDatabases(string aeDatabasePath, string kaDatabasePath)
        {
            try
            {
                await Task.Run(() =>
                {
                    var (newAeDb, newKaAvatarsDb, newKaWearablesDb, newKaWorldObjectsDb) =
                        RefreshDatabases(aeDatabasePath, kaDatabasePath);

                    // 内部状態の更新
                    _aeDatabase = newAeDb;
                    _kaAvatarsDatabase = newKaAvatarsDb;
                    _kaWearablesDatabase = newKaWearablesDb;
                    _kaWorldObjectsDatabase = newKaWorldObjectsDb;
                });

                // イベント通知
                DatabaseUpdated?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                HandleError($"データベースの更新に失敗しました: {ex.Message}");
                return false;
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
        /// データベースを再読み込みする
        /// </summary>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        /// <returns>再読み込みが成功したかどうか</returns>
        public (
            AvatarExplorerDatabase?,
            KonoAssetAvatarsDatabase?,
            KonoAssetWearablesDatabase?,
            KonoAssetWorldObjectsDatabase?
        ) RefreshDatabases(string aeDatabasePath, string kaDatabasePath)
        {
            AvatarExplorerDatabase? newAeDatabase = null;
            KonoAssetAvatarsDatabase? newKaAvatarsDatabase = null;
            KonoAssetWearablesDatabase? newKaWearablesDatabase = null;
            KonoAssetWorldObjectsDatabase? newKaWorldObjectsDatabase = null;

            if (
                !string.IsNullOrEmpty(aeDatabasePath)
                && File.Exists(Path.Combine(aeDatabasePath, "database.json"))
            )
            {
                try
                {
                    var json = File.ReadAllText(Path.Combine(aeDatabasePath, "database.json"));
                    newAeDatabase =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<AvatarExplorerDatabase>(json);
                }
                catch (Exception e)
                {
                    Debug.LogError($"AEデータベースの読み込みに失敗しました: {e.Message}");
                }
            }

            if (!string.IsNullOrEmpty(kaDatabasePath))
            {
                // Avatars
                var avatarsPath = Path.Combine(kaDatabasePath, "avatars.json");
                if (File.Exists(avatarsPath))
                {
                    try
                    {
                        var json = File.ReadAllText(avatarsPath);
                        newKaAvatarsDatabase =
                            Newtonsoft.Json.JsonConvert.DeserializeObject<KonoAssetAvatarsDatabase>(
                                json
                            );
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            $"KAアバターデータベースの読み込みに失敗しました: {e.Message}"
                        );
                    }
                }

                // Wearables
                var wearablesPath = Path.Combine(kaDatabasePath, "wearables.json");
                if (File.Exists(wearablesPath))
                {
                    try
                    {
                        var json = File.ReadAllText(wearablesPath);
                        newKaWearablesDatabase =
                            Newtonsoft.Json.JsonConvert.DeserializeObject<KonoAssetWearablesDatabase>(
                                json
                            );
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            $"KAウェアラブルデータベースの読み込みに失敗しました: {e.Message}"
                        );
                    }
                }

                // World Objects
                var worldObjectsPath = Path.Combine(kaDatabasePath, "world_objects.json");
                if (File.Exists(worldObjectsPath))
                {
                    try
                    {
                        var json = File.ReadAllText(worldObjectsPath);
                        newKaWorldObjectsDatabase =
                            Newtonsoft.Json.JsonConvert.DeserializeObject<KonoAssetWorldObjectsDatabase>(
                                json
                            );
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(
                            $"KAワールドオブジェクトデータベースの読み込みに失敗しました: {e.Message}"
                        );
                    }
                }
            }

            return (
                newAeDatabase,
                newKaAvatarsDatabase,
                newKaWearablesDatabase,
                newKaWorldObjectsDatabase
            );
        }

        /// <summary>
        /// 画像キャッシュを再取得する
        /// </summary>
        /// <param name="aeDatabasePath">AEデータベースのパス</param>
        /// <param name="kaDatabasePath">KAデータベースのパス</param>
        public void RefreshImageCache(string aeDatabasePath, string kaDatabasePath)
        {
            ImageServices.Instance.ClearCache();

            // AEの画像をキャッシュ
            if (_aeDatabase?.Items != null)
            {
                foreach (var item in _aeDatabase.Items)
                {
                    var imagePath = Path.Combine(aeDatabasePath, "images", item.ImagePath);
                    LoadTexture(imagePath, aeDatabasePath, kaDatabasePath);
                }
            }

            // KAの画像をキャッシュ
            if (_kaAvatarsDatabase?.data != null)
            {
                foreach (var item in _kaAvatarsDatabase.data)
                {
                    var imagePath = Path.Combine(
                        kaDatabasePath,
                        "images",
                        item.description.imageFilename
                    );
                    LoadTexture(imagePath, aeDatabasePath, kaDatabasePath);
                }
            }
            if (_kaWearablesDatabase?.data != null)
            {
                foreach (var item in _kaWearablesDatabase.data)
                {
                    if (string.IsNullOrEmpty(item.description.imageFilename))
                    {
                        continue;
                    }

                    var imagePath = Path.Combine(
                        kaDatabasePath,
                        "images",
                        item.description.imageFilename
                    );
                    LoadTexture(imagePath, aeDatabasePath, kaDatabasePath);
                }
            }
            if (_kaWorldObjectsDatabase?.data != null)
            {
                foreach (var item in _kaWorldObjectsDatabase.data)
                {
                    var imagePath = Path.Combine(
                        kaDatabasePath,
                        "images",
                        item.description.imageFilename
                    );
                    LoadTexture(imagePath, aeDatabasePath, kaDatabasePath);
                }
            }
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

        public void LoadAEDatabase(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                ClearAEDatabase();
                return;
            }

            DatabaseService.SetAEDatabasePath(path);
            DatabaseService.LoadAndUpdateAEDatabase();
            _aeDatabase = DatabaseService.GetAEDatabase();
            DatabaseUpdated?.Invoke();
        }

        public void LoadKADatabase(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                ClearKADatabase();
                return;
            }

            DatabaseService.SetKADatabasePath(path);
            DatabaseService.LoadAndUpdateKADatabase();
            _kaAvatarsDatabase = DatabaseService.GetKAAvatarsDatabase();
            _kaWearablesDatabase = DatabaseService.GetKAWearablesDatabase();
            _kaWorldObjectsDatabase = DatabaseService.GetKAWorldObjectsDatabase();
            DatabaseUpdated?.Invoke();
        }

        public void ClearAEDatabase()
        {
            _aeDatabase = null;
            DatabaseUpdated?.Invoke();
        }

        public void ClearKADatabase()
        {
            _kaAvatarsDatabase = null;
            _kaWearablesDatabase = null;
            _kaWorldObjectsDatabase = null;
            DatabaseUpdated?.Invoke();
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
    }
}
