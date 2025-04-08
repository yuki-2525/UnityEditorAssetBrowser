using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorAssetBrowser.Models;

namespace UnityEditorAssetBrowser.ViewModels
{
    public class SearchCriteriaManager
    {
        private readonly Dictionary<int, SearchCriteria> tabSearchCriteria =
            new Dictionary<int, SearchCriteria>();
        private int currentTabIndex = 0;

        public SearchCriteria CurrentSearchCriteria { get; private set; } = new SearchCriteria();

        public void SetCurrentTab(int tabIndex)
        {
            if (currentTabIndex != tabIndex)
            {
                SaveCurrentTabCriteria();
                currentTabIndex = tabIndex;
                LoadTabCriteria();
            }
        }

        private void SaveCurrentTabCriteria()
        {
            if (!tabSearchCriteria.ContainsKey(currentTabIndex))
            {
                tabSearchCriteria[currentTabIndex] = new SearchCriteria();
            }

            var criteria = tabSearchCriteria[currentTabIndex];
            criteria.SearchQuery = CurrentSearchCriteria.SearchQuery;
            criteria.ShowAdvancedSearch = CurrentSearchCriteria.ShowAdvancedSearch;
            criteria.TitleSearch = CurrentSearchCriteria.TitleSearch;
            criteria.AuthorSearch = CurrentSearchCriteria.AuthorSearch;
            criteria.CategorySearch = CurrentSearchCriteria.CategorySearch;
            criteria.SupportedAvatarsSearch = CurrentSearchCriteria.SupportedAvatarsSearch;
            criteria.MemoSearch = CurrentSearchCriteria.MemoSearch;
            criteria.TagsSearch = CurrentSearchCriteria.TagsSearch;
        }

        private void LoadTabCriteria()
        {
            if (!tabSearchCriteria.ContainsKey(currentTabIndex))
            {
                tabSearchCriteria[currentTabIndex] = new SearchCriteria();
            }

            var criteria = tabSearchCriteria[currentTabIndex];
            CurrentSearchCriteria.SearchQuery = criteria.SearchQuery;
            CurrentSearchCriteria.ShowAdvancedSearch = criteria.ShowAdvancedSearch;
            CurrentSearchCriteria.TitleSearch = criteria.TitleSearch;
            CurrentSearchCriteria.AuthorSearch = criteria.AuthorSearch;
            CurrentSearchCriteria.CategorySearch = criteria.CategorySearch;
            CurrentSearchCriteria.SupportedAvatarsSearch = criteria.SupportedAvatarsSearch;
            CurrentSearchCriteria.MemoSearch = criteria.MemoSearch;
            CurrentSearchCriteria.TagsSearch = criteria.TagsSearch;

            // タブ固有の検索条件をクリア
            if (currentTabIndex == 0) // アバタータブ
            {
                CurrentSearchCriteria.CategorySearch = "";
                CurrentSearchCriteria.SupportedAvatarsSearch = "";
            }
            else if (currentTabIndex == 2) // ワールドタブ
            {
                CurrentSearchCriteria.SupportedAvatarsSearch = "";
            }
        }
    }
}
