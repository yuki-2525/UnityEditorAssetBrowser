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

            tabSearchCriteria[currentTabIndex] = CurrentSearchCriteria.Clone();
        }

        private void LoadTabCriteria()
        {
            if (!tabSearchCriteria.ContainsKey(currentTabIndex))
            {
                tabSearchCriteria[currentTabIndex] = new SearchCriteria();
            }

            CurrentSearchCriteria = tabSearchCriteria[currentTabIndex].Clone();
            CurrentSearchCriteria.ClearTabSpecificCriteria(currentTabIndex);
        }
    }
}
