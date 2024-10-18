using System;

namespace Twm.Core.Messages
{
    public class MessageEventArgs : EventArgs
    {
        public MessageEventArgs(string message, string subMessage)
        {
            Message = message;
            SubMessage = subMessage;
        }

        public MessageEventArgs(string message, string subMessage, string status)
        {
            Message = message;
            SubMessage = subMessage;
            Status = status;
        }


        public string Tag { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public string SubMessage { get; set; }
    }
}