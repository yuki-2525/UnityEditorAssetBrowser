using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorAssetBrowser.Models;

namespace UnityEditorAssetBrowser.ViewModels
{
    /// <summary>
    /// 検索条件を管理するクラス
    /// タブごとに検索条件を保持し、タブ切り替え時に適切な検索条件を復元する
    /// </summary>
    public class SearchCriteriaManager
    {
        /// <summary>タブごとの検索条件</summary>
        private readonly Dictionary<int, SearchCriteria> tabSearchCriteria = new();

        /// <summary>現在のタブインデックス</summary>
        private int currentTabIndex;

        /// <summary>
        /// 現在の検索条件
        /// タブ切り替え時に自動的に更新される
        /// </summary>
        public SearchCriteria CurrentSearchCriteria { get; private set; } = new();

        /// <summary>
        /// 現在のタブを設定し、検索条件を切り替える
        /// </summary>
        /// <param name="tabIndex">切り替え先のタブインデックス</param>
        public void SetCurrentTab(int tabIndex)
        {
            if (currentTabIndex != tabIndex)
            {
                SaveCurrentTabCriteria();
                currentTabIndex = tabIndex;
                LoadTabCriteria();
            }
        }

        /// <summary>
        /// 現在のタブの検索条件を保存
        /// </summary>
        private void SaveCurrentTabCriteria()
        {
            tabSearchCriteria[currentTabIndex] = CurrentSearchCriteria.Clone();
        }

        /// <summary>
        /// 現在のタブの検索条件を読み込み
        /// </summary>
        private void LoadTabCriteria()
        {
            if (!tabSearchCriteria.TryGetValue(currentTabIndex, out var criteria))
            {
                criteria = new SearchCriteria();
                tabSearchCriteria[currentTabIndex] = criteria;
            }

            CurrentSearchCriteria = criteria.Clone();
            CurrentSearchCriteria.ClearTabSpecificCriteria(currentTabIndex);
        }
    }
}
