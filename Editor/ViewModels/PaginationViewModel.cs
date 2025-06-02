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
    /// UIとPaginationInfoモデルの間の橋渡し役として機能し、ページネーションの制御と表示を管理する
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
        /// 選択中のタブ（0: アバター, 1: アイテム, 2: ワールドオブジェクト）
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
        /// <returns>総ページ数（アイテム数が0の場合は1）</returns>
        public int GetTotalPages(List<object> items) => _paginationInfo.GetTotalPages(items);

        /// <summary>
        /// 現在のページのアイテムを取得
        /// </summary>
        /// <param name="items">アイテムリスト</param>
        /// <returns>現在のページに表示するアイテム</returns>
        public IEnumerable<object> GetCurrentPageItems(List<object> items) =>
            _paginationInfo.GetCurrentPageItems(items);

        /// <summary>
        /// ページをリセット（1ページ目に戻す）
        /// </summary>
        public void ResetPage() => _paginationInfo.ResetPage();

        /// <summary>
        /// 次のページに移動
        /// </summary>
        /// <param name="totalPages">総ページ数</param>
        /// <returns>移動が成功したかどうか（現在のページが最後のページの場合はfalse）</returns>
        public bool MoveToNextPage(int totalPages) => _paginationInfo.MoveToNextPage(totalPages);

        /// <summary>
        /// 前のページに移動
        /// </summary>
        /// <returns>移動が成功したかどうか（現在のページが1ページ目の場合はfalse）</returns>
        public bool MoveToPreviousPage() => _paginationInfo.MoveToPreviousPage();

        /// <summary>
        /// 指定したページに移動
        /// </summary>
        /// <param name="page">移動先のページ番号（1以上）</param>
        /// <param name="totalPages">総ページ数</param>
        /// <returns>移動が成功したかどうか（ページ番号が無効な場合はfalse）</returns>
        public bool MoveToPage(int page, int totalPages) =>
            _paginationInfo.MoveToPage(page, totalPages);

        /// <summary>
        /// 現在のタブのアイテム数を取得
        /// </summary>
        /// <param name="getFilteredAvatars">フィルターされたアバターを取得する関数</param>
        /// <param name="getFilteredItems">フィルターされたアイテムを取得する関数</param>
        /// <param name="getFilteredWorldObjects">フィルターされたワールドオブジェクトを取得する関数</param>
        /// <param name="getFilteredOthers">フィルターされたその他のアイテムを取得する関数</param>
        /// <returns>現在のタブのアイテム数</returns>
        public int GetCurrentTabItemCount(
            Func<List<object>> getFilteredAvatars,
            Func<List<object>> getFilteredItems,
            Func<List<object>> getFilteredWorldObjects,
            Func<List<object>> getFilteredOthers
        ) =>
            GetCurrentTabItems(
                getFilteredAvatars,
                getFilteredItems,
                getFilteredWorldObjects,
                getFilteredOthers
            ).Count;

        /// <summary>
        /// 現在のタブのアイテムを取得
        /// </summary>
        /// <param name="getFilteredAvatars">フィルターされたアバターを取得する関数</param>
        /// <param name="getFilteredItems">フィルターされたアイテムを取得する関数</param>
        /// <param name="getFilteredWorldObjects">フィルターされたワールドオブジェクトを取得する関数</param>
        /// <param name="getFilteredOthers">フィルターされたその他のアイテムを取得する関数</param>
        /// <returns>現在のタブのアイテムリスト</returns>
        public List<object> GetCurrentTabItems(
            Func<List<object>> getFilteredAvatars,
            Func<List<object>> getFilteredItems,
            Func<List<object>> getFilteredWorldObjects,
            Func<List<object>> getFilteredOthers
        ) =>
            _paginationInfo.SelectedTab switch
            {
                0 => getFilteredAvatars(),
                1 => getFilteredItems(),
                2 => getFilteredWorldObjects(),
                3 => getFilteredOthers(),
                _ => new List<object>(),
            };
    }
}
