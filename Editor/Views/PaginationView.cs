// Copyright (c) 2025 yuki-2525

#nullable enable

using UnityEditor;
using UnityEditorAssetBrowser.ViewModels;
using UnityEngine;

namespace UnityEditorAssetBrowser.Views
{
    public class PaginationView
    {
        private readonly PaginationViewModel _paginationViewModel;
        private readonly AssetBrowserViewModel _assetBrowserViewModel;

        public PaginationView(
            PaginationViewModel paginationViewModel,
            AssetBrowserViewModel assetBrowserViewModel
        )
        {
            _paginationViewModel = paginationViewModel;
            _assetBrowserViewModel = assetBrowserViewModel;
        }

        public void DrawPaginationButtons()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("前へ", GUILayout.Width(100)))
            {
                _paginationViewModel.MoveToPreviousPage();
            }
            var currentItems = _assetBrowserViewModel.GetCurrentTabItems(
                _paginationViewModel.SelectedTab
            );
            int totalPages = _paginationViewModel.GetTotalPages(currentItems);
            GUILayout.Label($"ページ {_paginationViewModel.CurrentPage + 1} / {totalPages}");
            if (GUILayout.Button("次へ", GUILayout.Width(100)))
            {
                _paginationViewModel.MoveToNextPage(totalPages);
            }
            GUILayout.EndHorizontal();
        }
    }
}
