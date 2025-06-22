// Copyright (c) 2025 yuki-2525

#nullable enable

namespace UnityEditorAssetBrowser.Models
{
    /// <summary>
    /// アセットタイプの定数定義クラス
    /// EditorPrefsで使用するアセットタイプの値を統一管理する
    /// </summary>
    public static class AssetTypeConstants
    {
        /// <summary>アバタータイプ</summary>
        public const int AVATAR = 0;
        
        /// <summary>アバター関連タイプ</summary>
        public const int AVATAR_RELATED = 1;
        
        /// <summary>ワールドタイプ</summary>
        public const int WORLD = 2;
        
        /// <summary>その他タイプ</summary>
        public const int OTHER = 3;
    }
}