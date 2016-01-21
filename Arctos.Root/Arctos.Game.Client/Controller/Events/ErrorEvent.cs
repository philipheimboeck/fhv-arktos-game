using System;

namespace Arctos.Controller.Events
{
    public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);

    public class ErrorEventArgs : EventArgs
    {
        public string Message { get; set; }

        public ErrorEventArgs(string message)
        {
            this.Message = message;
        }
    }
}