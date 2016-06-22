using System;

namespace NextPlayer.Common
{
    public class DiagnosticHelper
    {
        public static void TrackEvent(string name)
        {
            //App.TelemetryClient.TrackEvent(name);
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(name);
        }

        public static void TrackTrace(string trace, Microsoft.ApplicationInsights.DataContracts.SeverityLevel severityLevel)
        {
            App.TelemetryClient.TrackTrace(trace, severityLevel);
        }
    }
}
