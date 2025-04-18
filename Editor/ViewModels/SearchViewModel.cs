// Copyright (c) 2025 yuki-2525

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditorAssetBrowser.Models;
using UnityEditorAssetBrowser.Services;

namespace UnityEditorAssetBrowser.ViewModels
{
    public class SearchViewModel
    {
        private readonly ItemSearchService _itemSearchService;
        private readonly Dictionary<int, SearchCriteria> _tabSearchCriteria =
            new Dictionary<int, SearchCriteria>();
        private int _currentTab;

        public SearchCriteria SearchCriteria { get; private set; } = new SearchCriteria();

        public SearchViewModel(
            AvatarExplorerDatabase? aeDatabase,
            KonoAssetAvatarsDatabase? kaAvatarsDatabase,
            KonoAssetWearablesDatabase? kaWearablesDatabase,
            KonoAssetWorldObjectsDatabase? kaWorldObjectsDatabase
        )
        {
            _itemSearchService = new ItemSearchService(aeDatabase);
        }

        public void SetCurrentTab(int tab)
        {
            if (!_tabSearchCriteria.ContainsKey(_currentTab))
            {
                _tabSearchCriteria[_currentTab] = new SearchCriteria();
            }
            _tabSearchCriteria[_currentTab] = SearchCriteria.Clone();

            _currentTab = tab;
            if (_tabSearchCriteria.TryGetValue(tab, out var criteria))
            {
                SearchCriteria = criteria.Clone();
            }
            else
            {
                SearchCriteria = new SearchCriteria();
                _tabSearchCriteria[tab] = SearchCriteria.Clone();
            }
        }

        public void ClearSearchCriteria()
        {
            SearchCriteria = new SearchCriteria();
            _tabSearchCriteria[_currentTab] = SearchCriteria.Clone();
        }

        public bool IsItemMatchSearch(object item)
        {
            return _itemSearchService.IsItemMatchSearch(item, SearchCriteria);
        }
    }
}
