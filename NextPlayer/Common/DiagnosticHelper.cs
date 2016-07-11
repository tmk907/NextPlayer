using System;

namespace NextPlayer.Common
{
    public class DiagnosticHelper
    {
        public static void TrackEvent(string name)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackEvent(name);
        }

        public static void TrackTrace(string trace, Microsoft.HockeyApp.SeverityLevel severityLevel)
        {
            Microsoft.HockeyApp.HockeyClient.Current.TrackTrace(trace, severityLevel);
        }
    }
}
