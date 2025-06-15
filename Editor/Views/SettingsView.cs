using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorAssetBrowser;
using UnityEditorAssetBrowser.Services;
using UnityEngine;

namespace UnityEditorAssetBrowser.Views
{
    /// <summary>
    /// 設定画面のUIを管理するクラス
    /// データベースパスの設定と、AvatarExplorerのカテゴリごとのアセットタイプ設定を提供
    /// </summary>
    public class SettingsView
    {
        private readonly Action<string> _onAEDatabasePathChanged;
        private readonly Action<string> _onKADatabasePathChanged;
        private Vector2 _categoryScrollPosition;
        private bool _showDatabaseSettings;
        private bool _showCategorySettings;
        private bool _showFolderThumbnailSettings = false;
        private List<string> _userExcludeFolders;
        private HashSet<string> _enabledDefaultExcludeFolders;
        private string _newExcludeFolder = "";
        private Vector2 _excludeFoldersScrollPosition;
        private bool _showDefaultExcludeFolders = false;

        /// <summary>
        /// 指定された順序で表示するカテゴリのリスト
        /// </summary>
        private readonly string[] _orderedCategories = new[]
        {
            "アバター",
            "衣装",
            "テクスチャ",
            "ギミック",
            "アクセサリー",
            "髪型",
            "アニメーション",
            "ツール",
            "シェーダー",
        };

        /// <summary>
        /// カテゴリに設定可能なアセットタイプのリスト
        /// </summary>
        private readonly string[] _assetTypes = new[]
        {
            "アバター",
            "アバター関連アセット",
            "ワールドアセット",
            "その他",
        };

        /// <summary>
        /// カテゴリごとのアセットタイプ設定を保持する辞書
        /// </summary>
        private Dictionary<string, int> _categoryAssetTypes = new Dictionary<string, int>();

        /// <summary>
        /// EditorPrefsに保存する際のキーのプレフィックス
        /// </summary>
        private const string PREFS_KEY_PREFIX = "UnityEditorAssetBrowser_CategoryAssetType_";

        // EditorPrefsキー
        private const string PREFS_KEY_SHOW_FOLDER_THUMBNAIL =
            "UnityEditorAssetBrowser_ShowFolderThumbnail";
        private const string PREFS_KEY_GENERATE_FOLDER_THUMBNAIL =
            "UnityEditorAssetBrowser_GenerateFolderThumbnail";
        private const string PREFS_KEY_EXCLUDE_FOLDERS = "UnityEditorAssetBrowser_ExcludeFolders";

        // 初期設定リスト（abc順）
        private static readonly List<string> _allDefaultExcludeFolders = ExcludeFolderService
            .GetAllDefaultExcludeFolders()
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToList();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="onAEDatabasePathChanged">AEデータベースのパスが変更された時のコールバック</param>
        /// <param name="onKADatabasePathChanged">KAデータベースのパスが変更された時のコールバック</param>
        public SettingsView(
            Action<string> onAEDatabasePathChanged,
            Action<string> onKADatabasePathChanged
        )
        {
            _onAEDatabasePathChanged = onAEDatabasePathChanged;
            _onKADatabasePathChanged = onKADatabasePathChanged;
            ExcludeFolderService.InitializeDefaultExcludeFolders();
            InitializeCategoryAssetTypes();
            InitializeSettingsVisibility();
            InitializeExcludeFolders();
        }

        /// <summary>
        /// 設定の表示状態を初期化
        /// </summary>
        private void InitializeSettingsVisibility()
        {
            var aePath = DatabaseService.GetAEDatabasePath();
            var kaPath = DatabaseService.GetKADatabasePath();

            // データベース設定の表示状態を初期化
            _showDatabaseSettings = string.IsNullOrEmpty(aePath) && string.IsNullOrEmpty(kaPath);

            // カテゴリ設定の表示状態を初期化
            _showCategorySettings = !string.IsNullOrEmpty(aePath);
        }

        /// <summary>
        /// カテゴリごとのアセットタイプ設定を初期化
        /// EditorPrefsから値を読み込むか、デフォルト値を設定する
        /// </summary>
        private void InitializeCategoryAssetTypes()
        {
            // 指定された順序のカテゴリの初期化
            foreach (var category in _orderedCategories)
            {
                var key = PREFS_KEY_PREFIX + category;
                var value = EditorPrefs.GetInt(key);
                _categoryAssetTypes[category] = value;
            }

            // その他のカテゴリの初期化
            var aeDatabase = DatabaseService.GetAEDatabase();
            if (aeDatabase != null)
            {
                var otherCategories = aeDatabase
                    .Items.Select(item => item.GetAECategoryName())
                    .Distinct()
                    .Where(category => !_orderedCategories.Contains(category))
                    .OrderBy(category => category);

                foreach (var category in otherCategories)
                {
                    var key = PREFS_KEY_PREFIX + category;
                    var value = EditorPrefs.GetInt(key);
                    _categoryAssetTypes[category] = value;
                }
            }
        }

        /// <summary>
        /// カテゴリのアセットタイプ設定をEditorPrefsに保存
        /// </summary>
        /// <param name="category">カテゴリ名</param>
        /// <param name="value">設定するアセットタイプのインデックス</param>
        private void SaveCategoryAssetType(string category, int value)
        {
            var key = PREFS_KEY_PREFIX + category;
            EditorPrefs.SetInt(key, value);
        }

        /// <summary>
        /// カテゴリのデフォルトアセットタイプを取得
        /// </summary>
        /// <param name="category">カテゴリ名</param>
        /// <returns>デフォルトのアセットタイプのインデックス</returns>
        private int GetDefaultAssetTypeForCategory(string category)
        {
            if (category.Contains("ワールド") || category.Contains("world"))
            {
                return 2; // ワールドアセット
            }
            return 3; // その他
        }

        /// <summary>
        /// 除外フォルダ設定を初期化
        /// </summary>
        private void InitializeExcludeFolders()
        {
            var prefs = ExcludeFolderService.LoadPrefs();
            _userExcludeFolders = prefs?.userFolders ?? new List<string>();
            _enabledDefaultExcludeFolders = new HashSet<string>(
                prefs?.enabledDefaults ?? ExcludeFolderService.GetAllDefaultExcludeFolders()
            );
        }

        /// <summary>
        /// 除外フォルダ設定を保存
        /// </summary>
        private void SaveExcludeFoldersAndCombined()
        {
            ExcludeFolderService.SaveExcludeFolders(
                _userExcludeFolders,
                _enabledDefaultExcludeFolders.ToList()
            );
            var combined = new List<string>(_userExcludeFolders);
            combined.AddRange(_enabledDefaultExcludeFolders);
            ExcludeFolderService.SaveCombinedExcludePatterns(combined);
        }

        [Serializable]
        private class ExcludeFoldersData
        {
            public List<string> folders = new List<string>();
        }

        /// <summary>
        /// 設定画面のUIを描画
        /// </summary>
        public void Draw()
        {
            // データベース設定セクション
            _showDatabaseSettings = EditorGUILayout.Foldout(
                _showDatabaseSettings,
                "データベース設定",
                true
            );
            if (_showDatabaseSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawDatabasePathField(
                    "AE Database Path:",
                    DatabaseService.GetAEDatabasePath(),
                    path =>
                    {
                        _onAEDatabasePathChanged(path);
                        // AEのパスが設定されたらカテゴリ設定を開く
                        if (!string.IsNullOrEmpty(path))
                        {
                            _showCategorySettings = true;
                        }
                    }
                );
                DrawDatabasePathField(
                    "KA Database Path:",
                    DatabaseService.GetKADatabasePath(),
                    _onKADatabasePathChanged
                );
                EditorGUILayout.EndVertical();
            }

            // AEのカテゴリ一覧セクション
            EditorGUILayout.Space(10);
            _showCategorySettings = EditorGUILayout.Foldout(
                _showCategorySettings,
                "AvatarExplorer カテゴリ設定",
                true
            );
            if (_showCategorySettings)
            {
                var aeDatabase = DatabaseService.GetAEDatabase();
                if (aeDatabase == null)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.HelpBox("これはAvatarExplorer用の設定です", MessageType.Info);
                    EditorGUILayout.EndVertical();
                }
                else
                {
                    _categoryScrollPosition = EditorGUILayout.BeginScrollView(
                        _categoryScrollPosition
                    );
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                    // 指定された順序のカテゴリを表示
                    foreach (var category in _orderedCategories)
                    {
                        var items = aeDatabase
                            .Items.Where(item => item.GetAECategoryName() == category)
                            .ToList();
                        if (items.Any())
                        {
                            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField(
                                category,
                                EditorStyles.boldLabel,
                                GUILayout.Width(200)
                            );
                            EditorGUILayout.LabelField($"{items.Count}個のアイテム");
                            EditorGUILayout.EndHorizontal();

                            // アセットタイプの選択
                            EditorGUILayout.BeginHorizontal();
                            EditorGUILayout.LabelField("アセットタイプ:", GUILayout.Width(100));
                            var newValue = EditorGUILayout.Popup(
                                _categoryAssetTypes[category],
                                _assetTypes,
                                GUILayout.Width(200)
                            );
                            if (newValue != _categoryAssetTypes[category])
                            {
                                _categoryAssetTypes[category] = newValue;
                                SaveCategoryAssetType(category, newValue);
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndVertical();
                        }
                    }

                    // その他のカテゴリを表示
                    var otherCategories = aeDatabase
                        .Items.Select(item => item.GetAECategoryName())
                        .Distinct()
                        .Where(category => !_orderedCategories.Contains(category))
                        .OrderBy(category => category);

                    foreach (var category in otherCategories)
                    {
                        var items = aeDatabase
                            .Items.Where(item => item.GetAECategoryName() == category)
                            .ToList();
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(
                            category,
                            EditorStyles.boldLabel,
                            GUILayout.Width(200)
                        );
                        EditorGUILayout.LabelField($"{items.Count}個のアイテム");
                        EditorGUILayout.EndHorizontal();

                        // アセットタイプの選択
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("アセットタイプ:", GUILayout.Width(100));
                        var newValue = EditorGUILayout.Popup(
                            _categoryAssetTypes[category],
                            _assetTypes,
                            GUILayout.Width(200)
                        );
                        if (newValue != _categoryAssetTypes[category])
                        {
                            _categoryAssetTypes[category] = newValue;
                            SaveCategoryAssetType(category, newValue);
                        }
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.EndVertical(); // helpBox
                    EditorGUILayout.EndScrollView();
                }
            }

            // フォルダサムネイル設定セクション
            EditorGUILayout.Space(10);
            _showFolderThumbnailSettings = EditorGUILayout.Foldout(
                _showFolderThumbnailSettings,
                "フォルダサムネイル設定",
                true
            );
            if (_showFolderThumbnailSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                // フォルダサムネイルを表示する
                bool showFolderThumbnail = EditorPrefs.GetBool(
                    PREFS_KEY_SHOW_FOLDER_THUMBNAIL,
                    true
                );
                bool newShowFolderThumbnail = EditorGUILayout.ToggleLeft(
                    "フォルダサムネイルを表示する",
                    showFolderThumbnail
                );
                bool newGenerateFolderThumbnail = false;
                if (newShowFolderThumbnail != showFolderThumbnail)
                {
                    EditorPrefs.SetBool(PREFS_KEY_SHOW_FOLDER_THUMBNAIL, newShowFolderThumbnail);
                    FolderIconDrawer.SetEnabled(newShowFolderThumbnail);
                    // 設定ウィンドウ内の変数で判定し、必要ならサムネイルもONに
                    if (newShowFolderThumbnail && !newGenerateFolderThumbnail)
                    {
                        newGenerateFolderThumbnail = true;
                        EditorPrefs.SetBool(PREFS_KEY_GENERATE_FOLDER_THUMBNAIL, true);
                    }
                }

                // フォルダサムネイルを生成する
                bool generateFolderThumbnail = EditorPrefs.GetBool(
                    PREFS_KEY_GENERATE_FOLDER_THUMBNAIL,
                    true
                );
                EditorGUI.BeginDisabledGroup(newShowFolderThumbnail); // ONの間はグレーアウト
                newGenerateFolderThumbnail = EditorGUILayout.ToggleLeft(
                    "フォルダサムネイルを生成する",
                    generateFolderThumbnail
                );
                EditorGUI.EndDisabledGroup();
                // ON→OFFにしようとしたときのみ警告ダイアログ
                if (
                    !newShowFolderThumbnail
                    && generateFolderThumbnail
                    && !newGenerateFolderThumbnail
                )
                {
                    bool confirm = EditorUtility.DisplayDialog(
                        "注意",
                        "この設定をオフにすると、\nオフの間にインポートしたアセットのサムネイルは\nオンに戻した後も表示されません。\n\nよろしいですか？",
                        "OK",
                        "キャンセル"
                    );
                    if (confirm)
                    {
                        EditorPrefs.SetBool(PREFS_KEY_GENERATE_FOLDER_THUMBNAIL, false);
                    }
                    else
                    {
                        newGenerateFolderThumbnail = true; // チェックを戻す
                    }
                }
                else if (
                    newGenerateFolderThumbnail != generateFolderThumbnail
                    && !newShowFolderThumbnail
                )
                {
                    EditorPrefs.SetBool(
                        PREFS_KEY_GENERATE_FOLDER_THUMBNAIL,
                        newGenerateFolderThumbnail
                    );
                }

                // 初期設定領域（トグル式、デフォルト閉じ）
                _showDefaultExcludeFolders = EditorGUILayout.Foldout(
                    _showDefaultExcludeFolders,
                    "初期設定除外フォルダ（ON/OFF）",
                    true
                );
                if (_showDefaultExcludeFolders)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    foreach (var def in _allDefaultExcludeFolders)
                    {
                        EditorGUILayout.BeginHorizontal();
                        bool isOn = _enabledDefaultExcludeFolders.Contains(def);
                        bool newIsOn = EditorGUILayout.ToggleLeft(def, isOn);
                        if (newIsOn != isOn)
                        {
                            if (newIsOn)
                                _enabledDefaultExcludeFolders.Add(def);
                            else
                                _enabledDefaultExcludeFolders.Remove(def);
                            SaveExcludeFoldersAndCombined();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndVertical();
                }

                // ユーザー追加領域
                EditorGUILayout.LabelField("ユーザー追加除外フォルダ", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal();
                GUI.SetNextControlName("NewExcludeFolderField");
                _newExcludeFolder = EditorGUILayout.TextField(
                    "新しい除外フォルダ",
                    _newExcludeFolder
                );
                bool shouldAdd = false;
                // エンターキー対応
                if (
                    Event.current.type == EventType.KeyDown
                    && Event.current.keyCode == KeyCode.Return
                    && GUI.GetNameOfFocusedControl() == "NewExcludeFolderField"
                )
                {
                    shouldAdd = true;
                    Event.current.Use();
                }
                if (GUILayout.Button("追加", GUILayout.Width(60)))
                {
                    shouldAdd = true;
                }
                if (shouldAdd)
                {
                    if (!string.IsNullOrEmpty(_newExcludeFolder))
                    {
                        if (
                            !_userExcludeFolders.Contains(_newExcludeFolder)
                            && !_allDefaultExcludeFolders.Contains(_newExcludeFolder)
                        )
                        {
                            _userExcludeFolders.Insert(0, _newExcludeFolder); // 先頭に追加
                            SaveExcludeFoldersAndCombined();
                        }
                    }
                    _newExcludeFolder = "";
                }
                EditorGUILayout.EndHorizontal();

                // ユーザー追加分リスト（上から順に）
                float userListMaxHeight = 300f;
                _excludeFoldersScrollPosition = EditorGUILayout.BeginScrollView(
                    _excludeFoldersScrollPosition,
                    GUILayout.Height(
                        Mathf.Min(_userExcludeFolders.Count * 28 + 10, userListMaxHeight)
                    )
                );
                for (int i = 0; i < _userExcludeFolders.Count; i++)
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(_userExcludeFolders[i]);
                    if (GUILayout.Button("削除", GUILayout.Width(60)))
                    {
                        _userExcludeFolders.RemoveAt(i);
                        SaveExcludeFoldersAndCombined();
                        i--;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.EndVertical();
            }
        }

        /// <summary>
        /// データベースパス設定フィールドを描画
        /// </summary>
        /// <param name="label">フィールドのラベル</param>
        /// <param name="path">現在のパス</param>
        /// <param name="onPathChanged">パスが変更された時のコールバック</param>
        private void DrawDatabasePathField(string label, string path, Action<string> onPathChanged)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, GUILayout.Width(120));

            // パスを編集不可のテキストフィールドとして表示
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(path);
            EditorGUI.EndDisabledGroup();

            // 削除ボタン
            if (!string.IsNullOrEmpty(path) && GUILayout.Button("削除", GUILayout.Width(60)))
            {
                onPathChanged("");
                if (label == "AE Database Path:")
                {
                    InitializeCategoryAssetTypes();
                }
            }

            // 参照ボタン
            if (GUILayout.Button("参照", GUILayout.Width(60)))
            {
                var selectedPath = EditorUtility.OpenFolderPanel(
                    $"Select {label} Directory",
                    "",
                    ""
                );
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    onPathChanged(selectedPath);
                    if (label == "AE Database Path:")
                    {
                        InitializeCategoryAssetTypes();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
