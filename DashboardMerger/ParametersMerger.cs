using System.Linq;
using DevExpress.DashboardCommon;

namespace DashboardMerger {
    public static class ParametersMerger {
        public static void MergeParameters(DashboardParameterCollection fromParameters, DashboardMerger dashboardMerger) {
            DashboardParameterCollection toParameters = dashboardMerger.OriginalDashboard.Parameters;

            foreach(DashboardParameter parameter in fromParameters) {
                if(toParameters.Any(p => p.Name == parameter.Name)) {
                    // resolve
                } else {
                    toParameters.Add((DashboardParameter)parameter.Clone());
                }
            }
        }
    }
}
