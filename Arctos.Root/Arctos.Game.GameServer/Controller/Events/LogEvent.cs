using System;
using ArctosGameServer.Domain;

namespace ArctosGameServer.Controller.Events
{
    public delegate void LogEventHandler(object sender, LogEventArgs e);

    /// <summary>
    /// Event Args containing a log
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        public string Log { get; set; }
    }
}
