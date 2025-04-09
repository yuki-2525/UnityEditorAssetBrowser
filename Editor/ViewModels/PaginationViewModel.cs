// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorAssetBrowser.Models;

namespace UnityEditorAssetBrowser.ViewModels
{
    /// <summary>
    /// ページネーションのビューモデル
    /// UIとPaginationInfoモデルの間の橋渡し役として機能する
    /// </summary>
    public class PaginationViewModel
    {
        /// <summary>ページネーション情報</summary>
        private readonly PaginationInfo _paginationInfo;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="paginationInfo">ページネーション情報</param>
        public PaginationViewModel(PaginationInfo paginationInfo)
        {
            _paginationInfo = paginationInfo;
        }

        /// <summary>
        /// 現在のページ番号
        /// </summary>
        public int CurrentPage => _paginationInfo.CurrentPage;

        /// <summary>
        /// 選択中のタブ
        /// </summary>
        public int SelectedTab
        {
            get => _paginationInfo.SelectedTab;
            set => _paginationInfo.SelectedTab = value;
        }

        /// <summary>
        /// 1ページあたりのアイテム数
        /// </summary>
        public int ItemsPerPage
        {
            get => _paginationInfo.ItemsPerPage;
            set => _paginationInfo.ItemsPerPage = value;
        }

        /// <summary>
        /// 総ページ数を取得
        /// </summary>
        /// <param name="items">アイテムリスト</param>
        /// <returns>総ページ数</returns>
        public int GetTotalPages(List<object> items)
        {
            return _paginationInfo.GetTotalPages(items);
        }

        /// <summary>
        /// 現在のページのアイテムを取得
        /// </summary>
        /// <param name="items">アイテムリスト</param>
        /// <returns>現在のページのアイテム</returns>
        public IEnumerable<object> GetCurrentPageItems(List<object> items)
        {
            return _paginationInfo.GetCurrentPageItems(items);
        }

        /// <summary>
        /// ページをリセット
        /// </summary>
        public void ResetPage()
        {
            _paginationInfo.ResetPage();
        }

        /// <summary>
        /// 次のページに移動
        /// </summary>
        /// <param name="totalPages">総ページ数</param>
        /// <returns>移動が成功したかどうか</returns>
        public bool MoveToNextPage(int totalPages)
        {
            return _paginationInfo.MoveToNextPage(totalPages);
        }

        /// <summary>
        /// 前のページに移動
        /// </summary>
        /// <returns>移動が成功したかどうか</returns>
        public bool MoveToPreviousPage()
        {
            return _paginationInfo.MoveToPreviousPage();
        }

        /// <summary>
        /// 指定したページに移動
        /// </summary>
        /// <param name="page">移動先のページ番号</param>
        /// <param name="totalPages">総ページ数</param>
        /// <returns>移動が成功したかどうか</returns>
        public bool MoveToPage(int page, int totalPages)
        {
            return _paginationInfo.MoveToPage(page, totalPages);
        }

        /// <summary>
        /// 現在のタブのアイテム数を取得
        /// </summary>
        /// <param name="getFilteredAvatars">フィルターされたアバターを取得する関数</param>
        /// <param name="getFilteredItems">フィルターされたアイテムを取得する関数</param>
        /// <param name="getFilteredWorldObjects">フィルターされたワールドオブジェクトを取得する関数</param>
        /// <returns>アイテム数</returns>
        public int GetCurrentTabItemCount(
            Func<List<object>> getFilteredAvatars,
            Func<List<object>> getFilteredItems,
            Func<List<object>> getFilteredWorldObjects
        )
        {
            switch (_paginationInfo.SelectedTab)
            {
                case 0:
                    return getFilteredAvatars().Count;
                case 1:
                    return getFilteredItems().Count;
                case 2:
                    return getFilteredWorldObjects().Count;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// 現在のタブのアイテムを取得
        /// </summary>
        /// <param name="getFilteredAvatars">フィルターされたアバターを取得する関数</param>
        /// <param name="getFilteredItems">フィルターされたアイテムを取得する関数</param>
        /// <param name="getFilteredWorldObjects">フィルターされたワールドオブジェクトを取得する関数</param>
        /// <returns>現在のタブのアイテムリスト</returns>
        public List<object> GetCurrentTabItems(
            Func<List<object>> getFilteredAvatars,
            Func<List<object>> getFilteredItems,
            Func<List<object>> getFilteredWorldObjects
        )
        {
            switch (_paginationInfo.SelectedTab)
            {
                case 0:
                    return getFilteredAvatars();
                case 1:
                    return getFilteredItems();
                case 2:
                    return getFilteredWorldObjects();
                default:
                    return new List<object>();
            }
        }
    }
}
