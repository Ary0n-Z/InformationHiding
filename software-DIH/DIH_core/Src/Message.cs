using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH_core.Src
{
    public enum MessageType { Error,Info, Warrning }
    class Message
    {
        public string Who { get; private set; }
        public MessageType Type {get;private set;}
        public string Text { get; private set; }
        public Message(MessageType type, string text,string who)
        {
            Type = type;
            Text = text;
            Who = who;
        }

    }
}
