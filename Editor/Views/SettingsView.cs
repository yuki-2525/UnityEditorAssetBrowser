using System;
using UnityEditor;
using UnityEditorAssetBrowser.Services;
using UnityEngine;

namespace UnityEditorAssetBrowser.Views
{
    public class SettingsView
    {
        private readonly Action<string> _onAEDatabasePathChanged;
        private readonly Action<string> _onKADatabasePathChanged;

        public SettingsView(
            Action<string> onAEDatabasePathChanged,
            Action<string> onKADatabasePathChanged
        )
        {
            _onAEDatabasePathChanged = onAEDatabasePathChanged;
            _onKADatabasePathChanged = onKADatabasePathChanged;
        }

        public void Draw()
        {
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
        }

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

            // Browseボタン
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
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
