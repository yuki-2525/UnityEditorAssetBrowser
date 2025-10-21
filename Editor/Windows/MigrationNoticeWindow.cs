#nullable enable

using UnityEditor;
using UnityEngine;

namespace UnityEditorAssetBrowser
{
    /// <summary>
    /// 移行案内を表示するエディタウィンドウ
    /// </summary>
    public class MigrationNoticeWindow : EditorWindow
    {
        private const string VPM_REPO_URL = "vcc://vpm/addRepo?url=https://vpm.sakurayuki.dev/vpm.json";
        private const string YOUTUBE_URL = "https://youtu.be/fYnEdZiTk8Y"; // 実際のURLに置き換え
        private const string BLUE_COLOR = "#4169E1";
        private const string RED_COLOR = "#FF0000";
        private Vector2 scrollPosition;

        /// <summary>
        /// ウィンドウを表示
        /// </summary>
        public static void ShowWindow()
        {
            var window = GetWindow<MigrationNoticeWindow>("Unity Editor Asset Browser 移行のお知らせ");
            window.minSize = new Vector2(450, 550);
            window.maxSize = new Vector2(800, 800);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.Space(10);
            
            var richStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                richText = true
            };

            // タイトル
            var titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            EditorGUILayout.LabelField("Unity Editor Asset Browser の移行のお知らせ", titleStyle);
            
            EditorGUILayout.Space(20);

            // 説明文
            EditorGUILayout.LabelField("2025年10月20日以降のアップデートを反映するためには、ツールの移行が必要です。", richStyle);
            EditorGUILayout.Space(10);

            var linkStyle = new GUIStyle(EditorStyles.label)
            {
                richText = true,
                wordWrap = true
            };
            linkStyle.normal.textColor = Color.white;
            var linkRect = GUILayoutUtility.GetRect(new GUIContent("1. こちらからsakurayukiのVPMリポジトリを登録"), linkStyle);
            var vpmText = $"1. <color={BLUE_COLOR}>こちら</color>から\"sakurayuki\"のVPMリポジトリを登録";
            GUI.Label(linkRect, vpmText, linkStyle);
            
            if (Event.current.type == EventType.MouseDown && linkRect.Contains(Event.current.mousePosition))
            {
                Application.OpenURL(VPM_REPO_URL);
                Event.current.Use();
            }
            
            // マウスオーバー時のカーソル変更
            EditorGUIUtility.AddCursorRect(linkRect, MouseCursor.Link);

            EditorGUILayout.LabelField("2. このプロジェクトのUnityを終了する", richStyle);
            EditorGUILayout.LabelField("3. VCCのプロジェクト管理ページより\"Unity Editor Asset Browser\"のパッケージを削除", richStyle);
            EditorGUILayout.LabelField("4. VCCのプロジェクト管理ページより\"UniAsset -Unity Editor Asset Browser-\"のパッケージを登録", richStyle);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("以上の手順で移行が完了します。", richStyle);
            
            EditorGUILayout.Space(10);
            
            // Youtube動画へのリンク
            var ytRect = GUILayoutUtility.GetRect(new GUIContent("詳細な手順については動画をご確認ください。"), linkStyle);
            var ytText = $"詳細な手順については<color={RED_COLOR}>動画</color>をご確認ください。";
            GUI.Label(ytRect, ytText, linkStyle);
            
            if (Event.current.type == EventType.MouseDown && ytRect.Contains(Event.current.mousePosition))
            {
                Application.OpenURL(YOUTUBE_URL);
                Event.current.Use();
            }
            
            // マウスオーバー時のカーソル変更
            EditorGUIUtility.AddCursorRect(ytRect, MouseCursor.Link);

            EditorGUILayout.Space(20);

            // 注意事項
            EditorGUILayout.LabelField("設定は引き継がれます。移行前と変わらずに使うことが可能です。", richStyle);

            EditorGUILayout.Space(20);

            // 閉じるボタン
            if (GUILayout.Button("このウィンドウを閉じる", GUILayout.Height(30)))
            {
                Close();
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.EndScrollView();
        }
    }
}