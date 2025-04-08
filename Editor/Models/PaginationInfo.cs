// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditorAssetBrowser.Models
{
    /// <summary>
    /// ページネーション情報を管理するクラス
    /// </summary>
    public class PaginationInfo
    {
        /// <summary>1ページあたりのアイテム表示数</summary>
        public const int ITEMS_PER_PAGE = 10;

        /// <summary>現在のページ番号</summary>
        public int CurrentPage { get; private set; } = 0;

        /// <summary>選択中のタブ</summary>
        public int SelectedTab { get; set; } = 0;

        private int _itemsPerPage = ITEMS_PER_PAGE;

        /// <summary>
        /// 1ページあたりのアイテム数
        /// </summary>
        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set => _itemsPerPage = Math.Max(1, value);
        }

        /// <summary>
        /// 総ページ数を取得
        /// </summary>
        /// <param name="items">アイテムリスト</param>
        /// <returns>総ページ数</returns>
        public int GetTotalPages(List<object> items)
        {
            int totalItems = items.Count;
            return Mathf.Max(1, Mathf.CeilToInt((float)totalItems / ItemsPerPage));
        }

        /// <summary>
        /// 現在のページのアイテムを取得
        /// </summary>
        /// <param name="items">アイテムリスト</param>
        /// <returns>現在のページのアイテム</returns>
        public IEnumerable<object> GetCurrentPageItems(List<object> items)
        {
            int startIndex = CurrentPage * ItemsPerPage;
            int endIndex = Mathf.Min(startIndex + ItemsPerPage, items.Count);
            return items.Skip(startIndex).Take(ItemsPerPage);
        }

        /// <summary>
        /// ページをリセット
        /// </summary>
        public void ResetPage()
        {
            CurrentPage = 0;
        }

        /// <summary>
        /// 次のページに移動
        /// </summary>
        /// <param name="totalPages">総ページ数</param>
        /// <returns>移動が成功したかどうか</returns>
        public bool MoveToNextPage(int totalPages)
        {
            if (CurrentPage < totalPages - 1)
            {
                CurrentPage++;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 前のページに移動
        /// </summary>
        /// <returns>移動が成功したかどうか</returns>
        public bool MoveToPreviousPage()
        {
            if (CurrentPage > 0)
            {
                CurrentPage--;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 指定したページに移動
        /// </summary>
        /// <param name="page">移動先のページ番号</param>
        /// <param name="totalPages">総ページ数</param>
        /// <returns>移動が成功したかどうか</returns>
        public bool MoveToPage(int page, int totalPages)
        {
            if (page >= 0 && page < totalPages)
            {
                CurrentPage = page;
                return true;
            }
            return false;
        }
    }
}
