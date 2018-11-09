using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using DevExpress.DashboardCommon;
using DevExpress.DashboardWin;
using DevExpress.XtraEditors;

namespace DashboardMerger {
    public partial class DesignerForm : XtraForm {
        public DesignerForm() {
            InitializeComponent();
            AppDomain.CurrentDomain.SetData("DataDirectory", @"c:\Sources\vcs\2018.2\Demos.Dashboard\MVCxDashboard\CSHTML\App_Data");
            dashboardDesigner.CreateRibbon();
            dashboardDesigner.UpdateDashboardTitle();
        }

        void dashboardDesignerCustomizeDashboardTitle(object sender, CustomizeDashboardTitleEventArgs e) {
            DashboardToolbarItem mergeItem = new DashboardToolbarItem("Open Dashboard to merge", MergeDashboard);
            e.Items.Add(mergeItem);
        }
        void MergeDashboard(DashboardToolbarItemClickEventArgs args) {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Dashboard files (*.xml)|*.xml";
            openFileDialog.Multiselect = true;
            if(openFileDialog.ShowDialog() == DialogResult.OK) {
                dashboardDesigner.Dashboard.BeginUpdate();
                try {
                    List<string> rejectedDashboard = new List<string>();
                    foreach(string fileName in openFileDialog.FileNames) {
                        using(Dashboard dashboard = new Dashboard()) {
                            dashboard.LoadFromXml(fileName);
                            DashboardMerger dashboardMerger = new DashboardMerger(dashboardDesigner.Dashboard);
                            if(!dashboardMerger.MergeDashboard(dashboard)) {
                                rejectedDashboard.Add(Path.GetFileName(fileName));
                            }
                        }
                    }
                    if(rejectedDashboard.Count > 0)
                        MessageBox.Show(String.Format("The following dashboard has not been merged{0}{1}", Environment.NewLine, String.Join(Environment.NewLine, rejectedDashboard)));
                } finally {
                    dashboardDesigner.Dashboard.EndUpdate();
                    dashboardDesigner.ReloadData();
                }
            }
        }

        void dashboardDesignerDashboardChanged(object sender, EventArgs e) {
        }

        void dashboardDesignerDataLoading(object sender, DataLoadingEventArgs e) {

        }
    }
}
