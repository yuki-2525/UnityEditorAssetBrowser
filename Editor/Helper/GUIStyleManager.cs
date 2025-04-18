#nullable enable

using UnityEditor;
using UnityEngine;

namespace UnityEditorAssetBrowser.Helper
{
    public static class GUIStyleManager
    {
        private static GUIStyle? _titleStyle;
        private static GUIStyle? _boxStyle;

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
