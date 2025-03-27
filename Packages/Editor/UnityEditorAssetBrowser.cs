using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Helper;
using Newtonsoft.Json;

namespace UnityEditorAssetBrowser
{
public class UnityEditorAssetBrowser : EditorWindow
{
    private AvatarExplorerDatabase? aeDatabase;
        private KonoAssetAvatarsDatabase? kaAvatarsDatabase;
        private KonoAssetWearablesDatabase? kaWearablesDatabase;
        private KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase;
    private Vector2 scrollPosition;
    private string aeDatabasePath = "";
    private string kaDatabasePath = "";
    private string searchQuery = "";
    private int selectedTab = 0;
        private string[] tabs = { "アバター", "アバター関連", "ワールド" };
        private bool showDebugInfo = false;
        private Dictionary<string, bool> foldouts = new Dictionary<string, bool>();
        private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
        private GUIStyle titleStyle;
        private GUIStyle boxStyle;
        private const int ItemsPerPage = 10;
        private int currentPage = 0;
        private Dictionary<string, bool> unityPackageFoldouts = new Dictionary<string, bool>();
        private string lastSearchQuery = "";

    [MenuItem("Window/Unity Editor Asset Browser")]
    public static void ShowWindow()
    {
        GetWindow<UnityEditorAssetBrowser>("Unity Editor Asset Browser");
    }

    private void OnEnable()
    {
        LoadSettings();
            InitializeStyles();
        }

        private void InitializeStyles()
        {
            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                margin = new RectOffset(4, 4, 4, 4)
            };

            boxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(10, 10, 10, 10),
                margin = new RectOffset(0, 0, 5, 5)
            };
    }

    private void OnGUI()
    {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);

        // データベースパスの設定
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("AE Database Path:", GUILayout.Width(120));
            aeDatabasePath = EditorGUILayout.TextField(aeDatabasePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
                var path = EditorUtility.OpenFolderPanel("Select AE Database Directory", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                aeDatabasePath = path;
                LoadAEDatabase();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("KA Database Path:", GUILayout.Width(120));
            kaDatabasePath = EditorGUILayout.TextField(kaDatabasePath);
        if (GUILayout.Button("Browse", GUILayout.Width(60)))
        {
                var path = EditorUtility.OpenFolderPanel("Select KA Database Directory", "", "");
            if (!string.IsNullOrEmpty(path))
            {
                kaDatabasePath = path;
                LoadKADatabase();
            }
        }
        EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // タブ切り替え
            var newTab = GUILayout.Toolbar(selectedTab, tabs);
            if (newTab != selectedTab)
            {
                selectedTab = newTab;
                currentPage = 0; // タブ切り替え時にページをリセット
            }

            EditorGUILayout.Space(10);

            // 検索フィールド
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Search:", GUILayout.Width(60));
            var newSearchQuery = EditorGUILayout.TextField(searchQuery);
            if (newSearchQuery != searchQuery)
            {
                searchQuery = newSearchQuery;
                currentPage = 0; // 検索値が変更されたらページをリセット
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // スクロールビューとページネーションボタンのコンテナ
            GUILayout.BeginVertical();

        // スクロールビュー
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        switch (selectedTab)
        {
            case 0:
                    ShowAvatarsContent();
                break;
            case 1:
                    ShowItemsContent();
                    break;
                case 2:
                    ShowWorldObjectsContent();
                break;
        }
            EditorGUILayout.EndScrollView();

            // ページネーションボタン（スクロールビューの外）
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("前へ", GUILayout.Width(100)) && currentPage > 0)
            {
                currentPage--;
            }
            int totalPages = GetTotalPages();
            GUILayout.Label($"ページ {currentPage + 1} / {totalPages}");
            if (GUILayout.Button("次へ", GUILayout.Width(100)) && currentPage < totalPages - 1)
            {
                currentPage++;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
            EditorGUILayout.EndVertical();

        if (GUI.changed)
        {
            SaveSettings();
        }
    }

        private void ShowAvatarsContent()
        {
            var allItems = new List<object>();

            // AEのアバター（type=0）を追加
            if (aeDatabase?.Items != null)
            {
                allItems.AddRange(aeDatabase.Items.Where(item => item.Type == "0"));
            }

            // KAのアバターを追加
            if (kaAvatarsDatabase?.data != null)
            {
                allItems.AddRange(kaAvatarsDatabase.data);
            }

            // 検索フィルター適用
            var filteredItems = allItems.Where(item =>
            {
                if (string.IsNullOrEmpty(searchQuery)) return true;
                
                if (item is AvatarExplorerItem aeItem)
                    return aeItem.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
                
                if (item is KonoAssetAvatarItem kaItem)
                    return kaItem.description.name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
                
                return false;
            }).ToList();

            // ページネーション
            int startIndex = currentPage * ItemsPerPage;
            int endIndex = Mathf.Min(startIndex + ItemsPerPage, filteredItems.Count);
            var pageItems = filteredItems.Skip(startIndex).Take(ItemsPerPage);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    ShowAvatarItem(aeItem);
                }
                else if (item is KonoAssetAvatarItem kaItem)
                {
                    ShowKonoAssetItem(kaItem);
                }
            }
        }

        private void ShowItemsContent()
        {
            var allItems = new List<object>();

            // AEのアイテム（type≠0）を追加
            if (aeDatabase?.Items != null)
            {
                allItems.AddRange(aeDatabase.Items.Where(item => item.Type != "0"));
            }

            // KAのウェアラブルを追加
            if (kaWearablesDatabase?.data != null)
            {
                allItems.AddRange(kaWearablesDatabase.data);
            }

            // 検索フィルター適用
            var filteredItems = allItems.Where(item =>
            {
                if (string.IsNullOrEmpty(searchQuery)) return true;
                
                if (item is AvatarExplorerItem aeItem)
                    return aeItem.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
                
                if (item is KonoAssetWearableItem kaItem)
                    return kaItem.description.name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase);
                
                return false;
            }).ToList();

            // ページネーション
            int startIndex = currentPage * ItemsPerPage;
            int endIndex = Mathf.Min(startIndex + ItemsPerPage, filteredItems.Count);
            var pageItems = filteredItems.Skip(startIndex).Take(ItemsPerPage);

            foreach (var item in pageItems)
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    ShowAvatarItem(aeItem);
                }
                else if (item is KonoAssetWearableItem kaItem)
                {
                    ShowKonoAssetWearableItem(kaItem);
                }
            }
        }

        private void ShowWorldObjectsContent()
        {
            if (kaWorldObjectsDatabase?.data == null) return;

            // 検索フィルター適用
            var filteredItems = kaWorldObjectsDatabase.data.Where(item =>
                string.IsNullOrEmpty(searchQuery) || 
                item.description.name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)
            ).ToList();

            // ページネーション
            int startIndex = currentPage * ItemsPerPage;
            int endIndex = Mathf.Min(startIndex + ItemsPerPage, filteredItems.Count);
            var pageItems = filteredItems.Skip(startIndex).Take(ItemsPerPage);

            foreach (var item in pageItems)
            {
                if (item is KonoAssetWorldObjectItem worldItem)
                {
                    ShowKonoAssetWorldObjectItem(worldItem);
                }
            }
        }

        private int GetTotalPages()
        {
            int totalItems = 0;
            switch (selectedTab)
            {
                case 0:
                    totalItems = GetFilteredAvatars().Count;
                    break;
                case 1:
                    totalItems = GetFilteredItems().Count;
                    break;
                case 2:
                    totalItems = GetFilteredWorldObjects().Count;
                    break;
            }
            return Mathf.Max(1, Mathf.CeilToInt((float)totalItems / ItemsPerPage));
        }

        private List<AvatarExplorerItem> GetFilteredAvatars()
        {
            var items = aeDatabase?.Items?.ToList() ?? new List<AvatarExplorerItem>();
            if (string.IsNullOrEmpty(searchQuery))
                return items;

            return items.Where(item => 
                item.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                item.AuthorName.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();
        }

        private List<object> GetFilteredItems()
        {
            var items = new List<object>();
            if (aeDatabase != null)
            {
                items.AddRange(aeDatabase.Items);
            }
            if (kaWearablesDatabase != null)
            {
                items.AddRange(kaWearablesDatabase.data);
            }

            if (string.IsNullOrEmpty(searchQuery))
                return items;

            return items.Where(item =>
            {
                if (item is AvatarExplorerItem aeItem)
                {
                    return aeItem.Title.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                           aeItem.AuthorName.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                }
                else if (item is KonoAssetWearableItem kaItem)
                {
                    return kaItem.description.name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                           kaItem.description.creator.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0;
                }
                return false;
            }).ToList();
        }

        private List<KonoAssetWorldObjectItem> GetFilteredWorldObjects()
        {
            var items = kaWorldObjectsDatabase?.data ?? new KonoAssetWorldObjectItem[0];
            if (string.IsNullOrEmpty(searchQuery))
                return items.ToList();

            return items.Where(item =>
                item.description.name.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0 ||
                item.description.creator.IndexOf(searchQuery, StringComparison.OrdinalIgnoreCase) >= 0
            ).ToList();
        }

        private void ShowAvatarItem(AvatarExplorerItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            // タイトルと画像
            GUILayout.BeginHorizontal();
            if (!string.IsNullOrEmpty(item.ImagePath))
            {
                string imagePath;
                if (item.ImagePath.StartsWith("Datas"))
                {
                    imagePath = Path.Combine(aeDatabasePath, item.ImagePath.Replace("Datas\\", ""));
                }
                else
                {
                    imagePath = item.ImagePath;
                }

                if (File.Exists(imagePath))
                {
                    var texture = LoadTexture(imagePath);
                    if (texture != null)
                    {
                        GUILayout.Label(texture, GUILayout.Width(100), GUILayout.Height(100));
                    }
                }
            }
            GUILayout.BeginVertical();
            GUILayout.Label(item.Title, EditorStyles.boldLabel);
            GUILayout.Label($"作者: {item.AuthorName}");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // UnityPackageのトグル
            if (!unityPackageFoldouts.ContainsKey(item.Title))
            {
                unityPackageFoldouts[item.Title] = false;
            }
            unityPackageFoldouts[item.Title] = EditorGUILayout.Foldout(unityPackageFoldouts[item.Title], "UnityPackage");

            if (unityPackageFoldouts[item.Title])
            {
                var unityPackages = DatabaseHelper.FindUnityPackages(item.ItemPath);
                foreach (var package in unityPackages)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Path.GetFileName(package));
                    if (GUILayout.Button("インポート", GUILayout.Width(100)))
                    {
                        AssetDatabase.ImportPackage(package, true);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }

        private void ShowKonoAssetItem(KonoAssetAvatarItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            // タイトルと画像
            GUILayout.BeginHorizontal();
            if (item.description.imageFilename != null)
            {
                var imagePath = Path.Combine(kaDatabasePath, "images", item.description.imageFilename);
                if (File.Exists(imagePath))
                {
                    var texture = LoadTexture(imagePath);
                    if (texture != null)
                    {
                        GUILayout.Label(texture, GUILayout.Width(100), GUILayout.Height(100));
                    }
                }
            }
            GUILayout.BeginVertical();
            GUILayout.Label(item.description.name, EditorStyles.boldLabel);
            GUILayout.Label($"作者: {item.description.creator}");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // UnityPackageのトグル
            if (!unityPackageFoldouts.ContainsKey(item.description.name))
            {
                unityPackageFoldouts[item.description.name] = false;
            }
            unityPackageFoldouts[item.description.name] = EditorGUILayout.Foldout(unityPackageFoldouts[item.description.name], "UnityPackage");

            if (unityPackageFoldouts[item.description.name])
            {
                var dataPath = Path.Combine(kaDatabasePath, "data", item.id);
                var unityPackages = DatabaseHelper.FindUnityPackages(dataPath);
                foreach (var package in unityPackages)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Path.GetFileName(package));
                    if (GUILayout.Button("インポート", GUILayout.Width(100)))
                    {
                        AssetDatabase.ImportPackage(package, true);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }

        private void ShowKonoAssetWearableItem(KonoAssetWearableItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            // タイトルと画像
            GUILayout.BeginHorizontal();
            if (item.description.imageFilename != null)
            {
                var imagePath = Path.Combine(kaDatabasePath, "images", item.description.imageFilename);
                if (File.Exists(imagePath))
                {
                    var texture = LoadTexture(imagePath);
                    if (texture != null)
                    {
                        GUILayout.Label(texture, GUILayout.Width(100), GUILayout.Height(100));
                    }
                }
            }
            GUILayout.BeginVertical();
            GUILayout.Label(item.description.name, EditorStyles.boldLabel);
            GUILayout.Label($"作者: {item.description.creator}");
            GUILayout.Label($"カテゴリー: {item.category}");
            if (item.supportedAvatars != null && item.supportedAvatars.Length > 0)
            {
                GUILayout.Label($"対応アバター: {string.Join(", ", item.supportedAvatars)}");
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // UnityPackageのトグル
            if (!unityPackageFoldouts.ContainsKey(item.description.name))
            {
                unityPackageFoldouts[item.description.name] = false;
            }
            unityPackageFoldouts[item.description.name] = EditorGUILayout.Foldout(unityPackageFoldouts[item.description.name], "UnityPackage");

            if (unityPackageFoldouts[item.description.name])
            {
                var dataPath = Path.Combine(kaDatabasePath, "data", item.id);
                var unityPackages = DatabaseHelper.FindUnityPackages(dataPath);
                foreach (var package in unityPackages)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Path.GetFileName(package));
                    if (GUILayout.Button("インポート", GUILayout.Width(100)))
                    {
                        AssetDatabase.ImportPackage(package, true);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }

        private void ShowKonoAssetWorldObjectItem(KonoAssetWorldObjectItem item)
        {
            GUILayout.BeginVertical(EditorStyles.helpBox);
            
            // タイトルと画像
            GUILayout.BeginHorizontal();
            if (item.description.imageFilename != null)
            {
                var imagePath = Path.Combine(kaDatabasePath, "images", item.description.imageFilename);
                if (File.Exists(imagePath))
                {
                    var texture = LoadTexture(imagePath);
                    if (texture != null)
                    {
                        GUILayout.Label(texture, GUILayout.Width(100), GUILayout.Height(100));
                    }
                }
            }
            GUILayout.BeginVertical();
            GUILayout.Label(item.description.name, EditorStyles.boldLabel);
            GUILayout.Label($"作者: {item.description.creator}");
            GUILayout.Label($"カテゴリー: {item.category}");
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            // UnityPackageのトグル
            if (!unityPackageFoldouts.ContainsKey(item.description.name))
            {
                unityPackageFoldouts[item.description.name] = false;
            }
            unityPackageFoldouts[item.description.name] = EditorGUILayout.Foldout(unityPackageFoldouts[item.description.name], "UnityPackage");

            if (unityPackageFoldouts[item.description.name])
            {
                var dataPath = Path.Combine(kaDatabasePath, "data", item.id);
                var unityPackages = DatabaseHelper.FindUnityPackages(dataPath);
                foreach (var package in unityPackages)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(Path.GetFileName(package));
                    if (GUILayout.Button("インポート", GUILayout.Width(100)))
                    {
                        AssetDatabase.ImportPackage(package, true);
                    }
                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }

        private Texture2D LoadTexture(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            if (imageCache.TryGetValue(path, out var cachedTexture))
            {
                return cachedTexture;
            }

            if (File.Exists(path))
            {
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
                    Debug.LogError($"Error loading texture from {path}: {ex.Message}");
                }
            }

            return null;
    }

    private void LoadAEDatabase()
    {
        if (!string.IsNullOrEmpty(aeDatabasePath))
        {
                Debug.Log($"Loading AE database from: {aeDatabasePath}");
            aeDatabase = DatabaseHelper.LoadAEDatabase(aeDatabasePath);
                if (aeDatabase != null)
                {
                    Debug.Log($"AE database loaded successfully. Items count: {aeDatabase.Items.Count}");
                }
            }
    }

    private void LoadKADatabase()
    {
        if (!string.IsNullOrEmpty(kaDatabasePath))
        {
                Debug.Log($"Loading KA database from: {kaDatabasePath}");
                var metadataPath = Path.Combine(kaDatabasePath, "metadata");
                
                if (!Directory.Exists(metadataPath))
                {
                    Debug.LogError($"Metadata directory not found at: {metadataPath}");
                    return;
                }

                // アバターの読み込み
                var avatarsPath = Path.Combine(metadataPath, "avatars.json");
                if (File.Exists(avatarsPath))
                {
                    var baseDb = DatabaseHelper.LoadKADatabase(avatarsPath);
                    if (baseDb != null)
                    {
                        kaAvatarsDatabase = new KonoAssetAvatarsDatabase
                        {
                            version = baseDb.version,
                            data = baseDb.data.Select(item => JsonConvert.DeserializeObject<KonoAssetAvatarItem>(JsonConvert.SerializeObject(item))).ToArray()
                        };
                        Debug.Log($"KA avatars loaded successfully. Items count: {kaAvatarsDatabase.data.Length}");
                    }
                }

                // ウェアラブルの読み込み
                var wearablesPath = Path.Combine(metadataPath, "avatarWearables.json");
                if (File.Exists(wearablesPath))
                {
                    var baseDb = DatabaseHelper.LoadKADatabase(wearablesPath);
                    if (baseDb != null)
                    {
                        kaWearablesDatabase = new KonoAssetWearablesDatabase
                        {
                            version = baseDb.version,
                            data = baseDb.data.Select(item => JsonConvert.DeserializeObject<KonoAssetWearableItem>(JsonConvert.SerializeObject(item))).ToArray()
                        };
                        Debug.Log($"KA wearables loaded successfully. Items count: {kaWearablesDatabase.data.Length}");
                    }
                }

                // ワールドオブジェクトの読み込み
                var worldObjectsPath = Path.Combine(metadataPath, "worldObjects.json");
                if (File.Exists(worldObjectsPath))
                {
                    var baseDb = DatabaseHelper.LoadKADatabase(worldObjectsPath);
                    if (baseDb != null)
                    {
                        kaWorldObjectsDatabase = new KonoAssetWorldObjectsDatabase
                        {
                            version = baseDb.version,
                            data = baseDb.data.Select(item => JsonConvert.DeserializeObject<KonoAssetWorldObjectItem>(JsonConvert.SerializeObject(item))).ToArray()
                        };
                        Debug.Log($"KA world objects loaded successfully. Items count: {kaWorldObjectsDatabase.data.Length}");
                    }
                }
        }
    }

    private void LoadSettings()
    {
        aeDatabasePath = EditorPrefs.GetString("UnityEditorAssetBrowser_AEDatabasePath", "");
        kaDatabasePath = EditorPrefs.GetString("UnityEditorAssetBrowser_KADatabasePath", "");
            if (!string.IsNullOrEmpty(aeDatabasePath)) LoadAEDatabase();
            if (!string.IsNullOrEmpty(kaDatabasePath)) LoadKADatabase();
    }

    private void SaveSettings()
    {
        EditorPrefs.SetString("UnityEditorAssetBrowser_AEDatabasePath", aeDatabasePath);
        EditorPrefs.SetString("UnityEditorAssetBrowser_KADatabasePath", kaDatabasePath);
        }
    }
} 