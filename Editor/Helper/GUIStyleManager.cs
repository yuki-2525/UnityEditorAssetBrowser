#nullable enable

using UnityEditor;
using UnityEngine;

namespace UnityEditorAssetBrowser.Helper
{
    /// <summary>
    /// Unity EditorのGUIスタイルを管理するクラス
    /// 共通で使用するスタイルをキャッシュし、効率的に提供する
    /// </summary>
    public static class GUIStyleManager
    {
        private static GUIStyle? _titleStyle;
        private static GUIStyle? _boxStyle;

        /// <summary>
        /// タイトル用のスタイル
        /// 太字で14ptのフォントサイズを使用し、適切なマージンを設定
        /// </summary>
        public static GUIStyle TitleStyle
        {
            get
            {
                if (_titleStyle == null)
                {
                    _titleStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 14,
                        margin = new RectOffset(4, 4, 4, 4),
                    };
                }
                return _titleStyle;
            }
        }

        /// <summary>
        /// ボックス用のスタイル
        /// ヘルプボックスをベースに、適切なパディングとマージンを設定
        /// </summary>
        public static GUIStyle BoxStyle
        {
            get
            {
                if (_boxStyle == null)
                {
                    _boxStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(10, 10, 10, 10),
                        margin = new RectOffset(0, 0, 5, 5),
                    };
                }
                return _boxStyle;
            }
        }
    }
}
