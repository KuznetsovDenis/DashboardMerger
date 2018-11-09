using System;
using System.Linq;
using DevExpress.DashboardCommon;

namespace DashboardMerger {
    public static class ParametersMerger {
        public static void MergeParameters(DashboardParameterCollection fromParameters, DashboardMerger dashboardMerger) {
            DashboardParameterCollection toParameters = dashboardMerger.OriginalDashboard.Parameters;

            foreach(DashboardParameter parameter in fromParameters) {
                AddParamterCopy(parameter, dashboardMerger, (parameterCopy) => {
                    toParameters.Add(parameterCopy);
                });
            }
        }
        static void AddParamterCopy(DashboardParameter originalParamter, DashboardMerger dashboardMerger, Action<DashboardParameter> addParameterDelegate) {
            DashboardParameter parameterCopy = (DashboardParameter)originalParamter.Clone();
            DashboardParameterCollection toParameters = dashboardMerger.OriginalDashboard.Parameters;
            if(toParameters.Any(p => p.Name == parameterCopy.Name)) {
                if(ResolveParamterNamesConflict(parameterCopy))
                    addParameterDelegate(parameterCopy);
            } else {
                addParameterDelegate(parameterCopy);
            }
        }
        static bool ResolveParamterNamesConflict(DashboardParameter paramenterCopy) {
            
            // Provide your parameter name confilict resolution logic here

            return false;
        }
    }
}
