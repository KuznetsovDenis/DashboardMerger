using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.DashboardCommon;

namespace DashboardMerger {
    public static class ItemsMerger {
        public static void MergeGroups(DashboardItemGroupCollection fromGroups, DashboardMerger dashboardMerger) {
            DashboardItemGroupCollection toGroups = dashboardMerger.OriginalDashboard.Groups;
            IDictionary<string, string> groupNamesMap = dashboardMerger.GroupNamesMap;
            IList<DashboardItem> newItems = dashboardMerger.NewItems;
            foreach(DashboardItemGroup group in fromGroups) {
                DashboardItemGroup groupCopy = CreateGroupCopy(group);
                if(toGroups.Any(g => g.ComponentName == group.ComponentName)) {
                    string newName = NamesGenerator.GenerateName(group.ComponentName, 1, toGroups.Select(g => g.ComponentName));
                    groupNamesMap.Add(group.ComponentName, newName);
                    groupCopy.ComponentName = newName;
                } else {
                    groupCopy.ComponentName = group.ComponentName;
                }
                toGroups.Add(groupCopy);
                newItems.Add(groupCopy);
            }
        }
        public static void MergeItems(DashboardItemCollection fromItems, DashboardMerger dashboardMerger) {
            DashboardItemCollection toItems = dashboardMerger.OriginalDashboard.Items;
            IDictionary<string, string> dashboardItemNamesMap = dashboardMerger.DashboardItemNamesMap;
            IDictionary<string, string> dataSourceNamesMap = dashboardMerger.DataSourceNamesMap;
            DataSourceCollection existingDataSources = dashboardMerger.OriginalDashboard.DataSources;
            IList<DashboardItem> newItems = dashboardMerger.NewItems;

            foreach(DashboardItem dashboardItem in fromItems) {
                DashboardItem dashboardItemCopy = dashboardItem.CreateCopy();
                if(toItems.Any(item => item.ComponentName == dashboardItem.ComponentName)) {
                    string newName = NamesGenerator.GenerateName(dashboardItem.ComponentName, 1, toItems.Select(item => item.ComponentName));
                    dashboardItemNamesMap.Add(dashboardItem.ComponentName, newName);
                    dashboardItemCopy.ComponentName = newName;
                } else {
                    dashboardItemCopy.ComponentName = dashboardItem.ComponentName;
                }
                DataDashboardItem dataDashboardItem = dashboardItemCopy as DataDashboardItem;
                if(dataDashboardItem != null && dataDashboardItem.DataSource != null) {
                    string newDataSourceName = String.Empty;
                    if(dataSourceNamesMap.Keys.Any(name => name == dataDashboardItem.DataSource.ComponentName)) {
                        newDataSourceName = dataSourceNamesMap[dataDashboardItem.DataSource.ComponentName];
                    } else {
                        newDataSourceName = dataDashboardItem.DataSource.ComponentName;
                    }
                    dataDashboardItem.DataSource = null;
                    toItems.Add(dashboardItemCopy);
                    dataDashboardItem.DataSource = existingDataSources[newDataSourceName];
                } else {
                    toItems.Add(dashboardItemCopy);
                }
                newItems.Add(dashboardItemCopy);
            }
        }
        static DashboardItemGroup CreateGroupCopy(DashboardItemGroup group) {
            DashboardItemGroup groupCopy = new DashboardItemGroup();
            groupCopy.InteractivityOptions.IgnoreMasterFilters = group.InteractivityOptions.IgnoreMasterFilters;
            groupCopy.InteractivityOptions.IsMasterFilter = group.InteractivityOptions.IsMasterFilter;
            groupCopy.Name = group.Name;
            groupCopy.ShowCaption = group.ShowCaption;
            return groupCopy;
        }
    }
}
