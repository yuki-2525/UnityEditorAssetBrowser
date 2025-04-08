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
        private readonly SearchCriteriaManager searchCriteriaManager;
        private readonly ItemSearchService itemSearchService;

        public SearchViewModel(AvatarExplorerDatabase? aeDatabase = null)
        {
            searchCriteriaManager = new SearchCriteriaManager();
            itemSearchService = new ItemSearchService(aeDatabase);
        }

        public SearchCriteria SearchCriteria => searchCriteriaManager.CurrentSearchCriteria;

        public void SetCurrentTab(int tabIndex)
        {
            searchCriteriaManager.SetCurrentTab(tabIndex);
        }

        public bool IsItemMatchSearch(object item)
        {
            return itemSearchService.IsItemMatchSearch(item, SearchCriteria);
        }
    }
}
