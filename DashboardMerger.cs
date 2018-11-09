using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using DevExpress.DashboardCommon;

namespace DashboardMerger {
    public class DashboardMerger {
        TabContainerDashboardItem tabContainer;
        Dictionary<string, string> dataSourceNamesMap = new Dictionary<string, string>();
        Dictionary<string, string> groupNamesMap = new Dictionary<string, string>();
        Dictionary<string, string> dashboardItemsNamesMap = new Dictionary<string, string>();
        IList<DashboardItem> newItems = new List<DashboardItem>();

        Dashboard OriginalDashboard { get; }
        IEnumerable<DashboardItem> ItemsAndGroups {
            get {
                return OriginalDashboard.Items.Union(OriginalDashboard.Groups).Where(item => !(item is TabContainerDashboardItem));
            }
        }

        public DashboardMerger(Dashboard originalDashboard) {
            OriginalDashboard = originalDashboard;
        }

        public void MergeDashboard(Dashboard dashboard) {
            string errorMessage = String.Empty;
            if(!CheckDashboard(dashboard, out errorMessage)) {
                MessageBox.Show(errorMessage);
                return;
            }
            UpdateTabContainer();
            MergeParameters(dashboard.Parameters, OriginalDashboard.Parameters);
            MergeDataSources(dashboard.DataSources, OriginalDashboard.DataSources);
            MergeGroups(dashboard.Groups, OriginalDashboard.Groups);
            MergeItems(dashboard.Items, OriginalDashboard.Items);
            MergeLayout(dashboard.LayoutRoot, dashboard.Title.Text);
        }

        bool CheckDashboard(Dashboard dashboard, out string errorMessage) {
            errorMessage = String.Empty;
            if(dashboard.Items.Any(item => item is TabContainerDashboardItem)) {
                errorMessage = "Provided dashboard already has Tab Container";
                return false;
            }
            return true;
        }
        IDashboardDataSource CreateDataSourceCopy(IDashboardDataSource dataSourceToCopy) {
            DashboardEFDataSource efDataSource = dataSourceToCopy as DashboardEFDataSource;
            if(efDataSource != null) {
                XElement element = efDataSource.SaveToXml();
                DashboardEFDataSource newDataSource = new DashboardEFDataSource();
                newDataSource.LoadFromXml(element);
                newDataSource.Fill();
                return newDataSource;
            }

            DashboardExcelDataSource excelDataSource = dataSourceToCopy as DashboardExcelDataSource;
            if(excelDataSource != null) {
                XElement element = excelDataSource.SaveToXml();
                DashboardExcelDataSource newDataSource = new DashboardExcelDataSource();
                newDataSource.LoadFromXml(element);
                newDataSource.Fill();
                return newDataSource;
            }

            DashboardExtractDataSource extractDataSource = dataSourceToCopy as DashboardExtractDataSource;
            if(extractDataSource != null) {
                XElement element = extractDataSource.SaveToXml();
                DashboardExtractDataSource newDataSource = new DashboardExtractDataSource();
                newDataSource.LoadFromXml(element);
                return newDataSource;
            }

            DashboardObjectDataSource objectDataSource = dataSourceToCopy as DashboardObjectDataSource;
            if(objectDataSource != null) {
                XElement element = objectDataSource.SaveToXml();
                DashboardObjectDataSource newDataSource = new DashboardObjectDataSource();
                newDataSource.LoadFromXml(element);
                newDataSource.Fill();
                return newDataSource;
            }

            DashboardOlapDataSource olapDataSource = dataSourceToCopy as DashboardOlapDataSource;
            if(olapDataSource != null) {
                XElement element = olapDataSource.SaveToXml();
                DashboardOlapDataSource newDataSource = new DashboardOlapDataSource();
                newDataSource.LoadFromXml(element);
                newDataSource.Fill();
                return newDataSource;
            }

            DashboardSqlDataSource sqlDataSource = dataSourceToCopy as DashboardSqlDataSource;
            if(sqlDataSource != null) {
                XElement element = sqlDataSource.SaveToXml();
                DashboardSqlDataSource newDataSource = new DashboardSqlDataSource();
                newDataSource.LoadFromXml(element);
                newDataSource.Fill();
                return newDataSource;
            }
            return null;
        }
        DashboardItemGroup CreateGroupCopy(DashboardItemGroup group) {
            DashboardItemGroup groupCopy = new DashboardItemGroup();
            groupCopy.InteractivityOptions.IgnoreMasterFilters = group.InteractivityOptions.IgnoreMasterFilters;
            groupCopy.InteractivityOptions.IsMasterFilter = group.InteractivityOptions.IsMasterFilter;
            groupCopy.Name = group.Name;
            groupCopy.ShowCaption = group.ShowCaption;
            return groupCopy;
        }
        string GenerateName(string name, int index, IEnumerable<string> occupiedNames) {
            string result = String.Format("{0}_{1}", name, index);
            if(occupiedNames.Contains(result))
                return GenerateName(name, ++index, occupiedNames);
            return result;
        }
        void MergeDataSources(DataSourceCollection fromDataSources, DataSourceCollection toDataSources) {
            foreach(IDashboardDataSource dataSource in fromDataSources) {
                IDashboardDataSource dataSourceCopy = CreateDataSourceCopy(dataSource);
                if(dataSourceCopy != null) {
                    if(toDataSources.Any(d => d.ComponentName == dataSourceCopy.ComponentName)) {
                        string newName = GenerateName(dataSourceCopy.ComponentName, 1, toDataSources.Select(ds => ds.ComponentName));
                        dataSourceNamesMap.Add(dataSourceCopy.ComponentName, newName);
                        dataSourceCopy.ComponentName = newName;
                    }
                    toDataSources.Add(dataSourceCopy);
                }
            }
        }
        void MergeGroups(DashboardItemGroupCollection fromGroups, DashboardItemGroupCollection toGroups) {
            foreach(DashboardItemGroup group in fromGroups) {
                DashboardItemGroup groupCopy = CreateGroupCopy(group);
                if(toGroups.Any(g => g.ComponentName == group.ComponentName)) {
                    string newName = GenerateName(group.ComponentName, 1, toGroups.Select(g => g.ComponentName));
                    groupNamesMap.Add(group.ComponentName, newName);
                    groupCopy.ComponentName = newName;
                } else {
                    groupCopy.ComponentName = group.ComponentName;
                }
                toGroups.Add(groupCopy);
                newItems.Add(groupCopy);
            }
        }
        void MergeItems(DashboardItemCollection fromItems, DashboardItemCollection toItems) {
            foreach(DashboardItem dashboardItem in fromItems) {
                DashboardItem dashboardItemCopy = dashboardItem.CreateCopy();
                if(toItems.Any(item => item.ComponentName == dashboardItem.ComponentName)) {
                    string newName = GenerateName(dashboardItem.ComponentName, 1, toItems.Select(item => item.ComponentName));
                    dashboardItemsNamesMap.Add(dashboardItem.ComponentName, newName);
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
                    dataDashboardItem.DataSource = OriginalDashboard.DataSources[newDataSourceName];
                } else {
                    toItems.Add(dashboardItemCopy);
                }
                newItems.Add(dashboardItemCopy);
            }
        }
        void MergeLayout(DashboardLayoutGroup layoutRoot, string newPageName) {
            DashboardTabPage newTabPage = tabContainer.CreateTabPage();
            DashboardLayoutTabPage layoutPage = new DashboardLayoutTabPage(newTabPage);
            foreach(DashboardLayoutNode node in layoutRoot.GetNodesRecursive()) {
                if(node.DashboardItem != null) {
                    DashboardItemGroup group = node.DashboardItem as DashboardItemGroup;
                    if(group != null) {
                        string groupComponentName = group.ComponentName;
                        string newGroupComponentName = String.Empty;
                        if(!groupNamesMap.TryGetValue(group.ComponentName, out newGroupComponentName)) {
                            newGroupComponentName = group.ComponentName;
                        }
                        node.DashboardItem = newItems.Single(itm => itm.ComponentName == newGroupComponentName);
                    } else {
                        DashboardItem item = node.DashboardItem;
                        string newItemName = String.Empty;
                        if(!dashboardItemsNamesMap.TryGetValue(item.ComponentName, out newItemName)) {
                            newItemName = item.ComponentName;
                        }
                        node.DashboardItem = newItems.Single(itm => itm.ComponentName == newItemName);
                    }
                }
            }
            layoutPage.ChildNodes.Add(layoutRoot);
            foreach(DashboardItem item in newItems) {
                if(item.ParentContainer == null) {
                    item.ParentContainer = newTabPage;
                } else {
                    IDashboardItemContainer container = item.ParentContainer;
                    if(container is DashboardItemGroup) {
                        string newGroupName = String.Empty;
                        if(!groupNamesMap.TryGetValue(container.ComponentName, out newGroupName)) {
                            newGroupName = container.ComponentName;
                        }
                        item.ParentContainer = OriginalDashboard.Groups[newGroupName];
                    } else {
                        item.ParentContainer = newTabPage;
                    }
                }
            }
            DashboardLayoutTabContainer layoutTabContainer = OriginalDashboard.LayoutRoot.FindRecursive(tabContainer);
            layoutTabContainer.ChildNodes.Add(layoutPage);
            newTabPage.Name = newPageName;
        }
        void MergeParameters(DashboardParameterCollection fromParameters, DashboardParameterCollection toParameters) {
            foreach(DashboardParameter parameter in fromParameters) {
                if(toParameters.Any(p => p.Name == parameter.Name)) {
                    // resolve
                } else {
                    toParameters.Add((DashboardParameter)parameter.Clone());
                }
            }
        }
        void CreateTabContainer() {
            tabContainer = new TabContainerDashboardItem();
            OriginalDashboard.Items.Add(tabContainer);
        }
        void SetParentContainer(IDashboardItemContainer container) {
            foreach(DashboardItem item in ItemsAndGroups) {
                if(item.ParentContainer == null) {
                    if(!(item is TabContainerDashboardItem))
                        item.ParentContainer = container;
                }
            }
        }
        void MoveRootToTabPage(DashboardLayoutTabPage layoutPage) {
            DashboardLayoutGroup rootGroup = OriginalDashboard.LayoutRoot;
            OriginalDashboard.LayoutRoot = null;
            layoutPage.ChildNodes.Add(rootGroup);
        }
        void UpdateTabContainer() {
            tabContainer = OriginalDashboard.Items.FirstOrDefault(item => item is TabContainerDashboardItem) as TabContainerDashboardItem;
            if(tabContainer == null) {
                CreateTabContainer();
                DashboardLayoutTabContainer layoutTabContainer = new DashboardLayoutTabContainer(tabContainer, 1);
                if(ItemsAndGroups.Count() > 0) {
                    DashboardTabPage tabPage = tabContainer.CreateTabPage();
                    tabPage.Name = OriginalDashboard.Title.Text;
                    DashboardLayoutTabPage layoutPage = new DashboardLayoutTabPage(tabPage);
                    layoutTabContainer.ChildNodes.Add(layoutPage);
                    MoveRootToTabPage(layoutPage);
                    SetParentContainer(tabPage);
                }
                OriginalDashboard.LayoutRoot = new DashboardLayoutGroup();
                OriginalDashboard.LayoutRoot.ChildNodes.Add(layoutTabContainer);
            }
        }
    }
}
