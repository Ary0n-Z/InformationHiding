using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using System.Linq;
using PropertyChanged;
using DIH_core.Src.WAV;
using System.IO;
using System;
using DIH_core.Src;

namespace DIH_core.ViewModels
{
    class MainViewModel : NavigationViewModelBase
    {
        public bool CanProgressOn()
        {
            return Audio != null;
        }


        #region Audio data
        [DoNotNotify]
        public DigitalAudio Audio { get; set; }
        #endregion

        protected IOpenFileDialogService OpenFileDialogService { get { return GetService<IOpenFileDialogService>(); } }
        public string AudioFileName { get; set; }
        public MainViewModel()
        {
            Audio = null;
        }
        [Command(CanExecuteMethodName = "CanProgressOn")]
        public void ExtractStep()
        {
            Navigate(new InterViewModelPackage(NavigationCodes.Extract, Audio));
            AudioFileName = "";
            Audio = null;
        }
        [Command(CanExecuteMethodName = "CanProgressOn")]
        public void EmbedStep()
        {
            Navigate(new InterViewModelPackage(NavigationCodes.Embed, Audio));
            AudioFileName = "";
            Audio = null;
        }
        [Command]
        public void OpenWAV()
        {
            OutputLogs logs = OutputLogs.Instance();
            OpenFileDialogService.Filter = "Wav Files (.wav)|*.wav|All Files (*.*)|*.*";
            OpenFileDialogService.FilterIndex = 1;
            OpenFileDialogService.Title = "Open target audio";
            if (OpenFileDialogService.ShowDialog())
            {
                IFileInfo file = OpenFileDialogService.Files.First();
                WavReader reader = null;
                try
                {
                    reader = new WavReader(File.Open(file.GetFullName(), FileMode.Open, FileAccess.Read, FileShare.None));
                    Audio = reader.ReadAudio();
                    AudioFileName = file.Name;
                    string msg = string.Concat("Succesfully loaded file - ", file.GetFullName());
                    logs.AddLog(new Message(MessageType.Info, msg, "WavReader"));
                }
                catch (ApplicationException ex)
                {
                    logs.AddLog(new Message(MessageType.Error, ex.Message,ex.Source));
                }
                finally
                {
                    if(reader!=null)
                        reader.Dispose();
                }
            }
        }
    }
}
