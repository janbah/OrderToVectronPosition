using System.Diagnostics;

namespace Order2VPos.Core.Common
{
    public class LogWriter
    {
        readonly EventLog appLog;

        public LogWriter()
        {
            //appLog = new EventLog(Constants.LogName, ".", Constants.Source);
        }

        public void WriteEntry(string message, EventLogEntryType type, int eventID) => appLog.WriteEntry($"{message}", type, eventID);
    }

}
