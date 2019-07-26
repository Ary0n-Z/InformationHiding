using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DIH_core.Src;
using DIH_core.Src.MethodsInterface;
using DIH_core.Src.WAV;
using PropertyChanged;
using System;
using System.Text;

namespace DIH_core.ViewModels
{
    class CodingViewModel:NavigationViewModelBase
    {
        NavigationCodes action;
        public string Message { get; set; }
       
        [DependsOn("Message")]
        public long ConteinerCapacity
        {
            get {
                return
                  (conteinerCapacity - Message.Length);
                }
        }
        [DoNotNotify]
        public bool ViewCapacity { get {return action == NavigationCodes.Embed;} }
        [DoNotNotify]
        public string CodingAction { get; set; }
        public bool GetStatistics { get; set; } = false;
        private long conteinerCapacity;
        private IHidingMethod method;
        private DigitalAudio cover;
        protected IDocumentManagerService DocumentManagerService { get { return this.GetService<IDocumentManagerService>(); } }

        protected ISaveFileDialogService SaveFileDialogService { get { return this.GetService<ISaveFileDialogService>(); } }
        public CodingViewModel(NavigationCodes code,IHidingMethod hidingMethod,DigitalAudio audio)
        {
            Message = "";
            cover = audio;
            method = hidingMethod;
            conteinerCapacity = hidingMethod.GetAvailableSpace(audio);
            action = code;
            switch (code)
            {
                case NavigationCodes.Extract:
                    CodingAction = "Extract";
                    break;
                case NavigationCodes.Embed:
                    CodingAction = "Embed";
                    break;
                default:
                    CodingAction = "Undefined";
                    break;
            }
        }
        public bool CanEmbed()
        {
            if(action == NavigationCodes.Embed)
                if(Message.Length==0)
                    return false;
            return true;
        }
        [Command(CanExecuteMethodName ="CanEmbed")]
        public void Code()
        {
            OutputLogs logs = OutputLogs.Instance();
            switch (action)
            {
                case NavigationCodes.Extract:
                    OutputLogs.Instance().AddLog(new Message(MessageType.Info, "Extracting started...", method.ToString()));
                    try
                    {
                        var bytes = method.Extract(cover);
                        if (bytes == null)
                            throw new ApplicationException("No embedded message found.");
                        Message = Encoding.ASCII.GetString(bytes);
                        string goodMsg = "Embedded message successfuly extracted.";
                        logs.AddLog(new Message(MessageType.Info, goodMsg, method.ToString()));
                    }
                    catch (ApplicationException ex)
                    {
                        logs.AddLog(new Message(MessageType.Error, ex.Message, method.ToString()));
                    }
                    break;
                case NavigationCodes.Embed:
                    logs.AddLog(new Message(MessageType.Info, "Embedding started...", method.ToString()));
                    try
                    {
                        int[] signal = null;
                        if (GetStatistics)
                        {
                            signal = (int[])cover.GetSignal().Clone();
                        }
                        var data = Encoding.ASCII.GetBytes(Message);
                        method.Embed(cover, data);
                        logs.AddLog(new Message(MessageType.Info, "Message successfuly embeded.", method.ToString()));
                        SaveFileDialogService.DefaultExt = "wav";
                        SaveFileDialogService.DefaultFileName = "StegoAudio";
                        if (SaveFileDialogService.ShowDialog())
                        {
                            using (var stream = new WavWriter(SaveFileDialogService.OpenFile()))
                                stream.WriteWavDefault(cover);
                            logs.AddLog(new Message(MessageType.Info, "File was succesfuly saved.", "WavWriter"));
                        }
                        if (GetStatistics)
                        {
                            var stegosignal = cover.GetSignal();
                            var service = DocumentManagerService;
                            var doc = service.CreateDocument("Statistics", new StatisticsViewModel(signal,stegosignal));
                            doc.Show();
                        }
                    }
                    catch (ApplicationException ex)
                    {
                        logs.AddLog(new Message(MessageType.Error, ex.Message, method.ToString()));
                    }
                    break;
                default:
                    logs.AddLog(new Message(MessageType.Error, "Internal error", method.ToString()));
                    break;
            }
        }
    }
}
