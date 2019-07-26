using DevExpress.Mvvm;
using System.Collections.ObjectModel;


namespace DIH_core.Src
{
    class OutputLogs : BindableBase
    {
        static private OutputLogs instance = null;
        private OutputLogs() {
        }
        static public OutputLogs Instance() {
            if (instance == null)
            {
                instance = new OutputLogs();
                instance.Messages = new ObservableCollection<Message>();
                instance.AddLog(new Message(MessageType.Warrning, "This app works with uncompressed wav 16 bit depth.", "App"));
            }
            return instance;
        }
        public ObservableCollection<Message> Messages { get; private set; }
        public void AddLog(Message msg)
        {
            Messages.Insert(0,msg);
        }
    }
}
