using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

        /// <summary>
        /// 初期化が完了したかどうかを示すフラグ
        /// </summary>
        private bool _isInitialized = false;

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
            InitializeCategoryAssetTypes();
        }

        /// <summary>
        /// カテゴリごとのアセットタイプ設定を初期化
        /// EditorPrefsから値を読み込むか、デフォルト値を設定する
        /// </summary>
        private void InitializeCategoryAssetTypes()
        {
            if (_isInitialized)
            {
                return;
            }
            _isInitialized = true;

            // 初期値の設定
            var defaultTypes = new Dictionary<string, int>
            {
                { "アバター", 0 }, // アバター
                { "衣装", 1 }, // アバター関連アセット
                { "テクスチャ", 1 }, // アバター関連アセット
                { "ギミック", 1 }, // アバター関連アセット
                { "アクセサリー", 1 }, // アバター関連アセット
                { "髪型", 1 }, // アバター関連アセット
                { "アニメーション", 1 }, // アバター関連アセット
                { "ツール", 3 }, // その他
                { "シェーダー", 3 }, // その他
            };

            // 指定された順序のカテゴリの初期化
            foreach (var category in _orderedCategories)
            {
                var key = PREFS_KEY_PREFIX + category;
                if (EditorPrefs.HasKey(key))
                {
                    var value = EditorPrefs.GetInt(key);
                    _categoryAssetTypes[category] = value;
                }
                else if (defaultTypes.ContainsKey(category))
                {
                    _categoryAssetTypes[category] = defaultTypes[category];
                    EditorPrefs.SetInt(key, defaultTypes[category]);
                }
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
                    if (EditorPrefs.HasKey(key))
                    {
                        var value = EditorPrefs.GetInt(key);
                        _categoryAssetTypes[category] = value;
                    }
                    else
                    {
                        var defaultValue = GetDefaultAssetTypeForCategory(category);
                        _categoryAssetTypes[category] = defaultValue;
                        EditorPrefs.SetInt(key, defaultValue);
                    }
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
        /// 設定画面のUIを描画
        /// </summary>
        public void Draw()
        {
            // データベース設定セクション
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("データベース設定", EditorStyles.boldLabel);

            DrawDatabasePathField(
                "AE Database Path:",
                DatabaseService.GetAEDatabasePath(),
                _onAEDatabasePathChanged
            );
            DrawDatabasePathField(
                "KA Database Path:",
                DatabaseService.GetKADatabasePath(),
                _onKADatabasePathChanged
            );

            EditorGUILayout.EndVertical();

            // AEのカテゴリ一覧セクション
            var aeDatabase = DatabaseService.GetAEDatabase();
            if (aeDatabase != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("AvatarExplorer カテゴリ設定", EditorStyles.boldLabel);

                _categoryScrollPosition = EditorGUILayout.BeginScrollView(_categoryScrollPosition);

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
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
