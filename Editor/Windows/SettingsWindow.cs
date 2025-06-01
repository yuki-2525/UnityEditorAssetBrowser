using UnityEditor;
using UnityEditorAssetBrowser.Services;
using UnityEditorAssetBrowser.ViewModels;
using UnityEditorAssetBrowser.Views;
using UnityEngine;

namespace UnityEditorAssetBrowser.Windows
{
    public class SettingsWindow : EditorWindow
    {
        private SettingsView _settingsView;
        private AssetBrowserViewModel _assetBrowserViewModel;
        private SearchViewModel _searchViewModel;
        private PaginationViewModel _paginationViewModel;

        public static void ShowWindow(
            AssetBrowserViewModel assetBrowserViewModel,
            SearchViewModel searchViewModel,
            PaginationViewModel paginationViewModel
        )
        {
            var window = GetWindow<SettingsWindow>("設定");
            window.minSize = new Vector2(400, 200);
            window._assetBrowserViewModel = assetBrowserViewModel;
            window._searchViewModel = searchViewModel;
            window._paginationViewModel = paginationViewModel;

            // DatabaseServiceにViewModelの参照を設定
            DatabaseService.SetViewModels(
                assetBrowserViewModel,
                searchViewModel,
                paginationViewModel
            );
        }

        private void OnEnable()
        {
            _settingsView = new SettingsView(
                DatabaseService.OnAEDatabasePathChanged,
                DatabaseService.OnKADatabasePathChanged
            );
        }

        private void OnGUI()
        {
            _settingsView.Draw();
        }
    }
}
