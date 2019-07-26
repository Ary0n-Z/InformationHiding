using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DIH_core.Src;
using DIH_core.Src.MethodsInterface;
using DIH_core.Src.WAV;
using System;
using System.Windows.Controls;
using static DIH_core.ViewModels.NavigationViewModelBase;

namespace DIH_core.ViewModels
{
    class WindowViewModel : ViewModelBase {
        public bool ViewBack { get; set; }
        public bool ViewHome { get; set; }
        private NavigationCodes navigationLine;
        public enum State { AudioSelecting, AlgorithmSelecting, MessageCoding }
        private State state;
        private DigitalAudio Audio = null;
        private IHidingMethod hidingMethod;
        private readonly Page startPage;
        private Page algorithmPage;

        public OutputLogs OutputLogs
        {
            get { return OutputLogs.Instance(); }
        }
        public Page CurrentPage{ get; set; }

        public WindowViewModel()
        {
            state = State.AudioSelecting;
            ViewHome = false;
            ViewBack = false;
            algorithmPage = null;
            navigationLine = NavigationCodes.Start;
            startPage = new Pages.MainPage();
            CurrentPage = startPage;
            ((ISupportParameter)CurrentPage.DataContext).Parameter = (Action<InterViewModelPackage>)Navigate;
        }
        [Command]
        public void GoHome() {
            ViewBack = false;
            state = State.AudioSelecting;
            CurrentPage = startPage;
            ((ISupportParameter)CurrentPage.DataContext).Parameter = (Action<InterViewModelPackage>)Navigate;
            Audio = null;
            hidingMethod = null;
            ViewHome = false;
        }
        [Command]
        public void GoBack()
        {
            ViewBack = false;
            state = State.AlgorithmSelecting;
            CurrentPage = algorithmPage;
            ((ISupportParameter)CurrentPage.DataContext).Parameter = (Action<InterViewModelPackage>)Navigate;
            hidingMethod = null;
        }
        private Page GetAlgorithmPage()
        {
            if(algorithmPage == null)
            {
                algorithmPage = new Pages.MethodPage
                {
                    DataContext = new MethodViewModel(navigationLine)
                };
            }
            else
                algorithmPage.DataContext = new MethodViewModel(navigationLine);
            ((ISupportParameter)algorithmPage.DataContext).Parameter = (Action<InterViewModelPackage>)Navigate;
            return algorithmPage;
        }
        public void Navigate(InterViewModelPackage package)
        {
            if (package.Code == NavigationCodes.Start)
                GoHome();
            else
            {
                ViewHome = true;
                switch (state)
                {
                    case State.AudioSelecting:
                        state = State.AlgorithmSelecting;
                        Audio = package.Data as DigitalAudio;
                        navigationLine = package.Code;
                        CurrentPage = GetAlgorithmPage();
                        break;
                    case State.AlgorithmSelecting:
                        state = State.MessageCoding;
                        ViewBack = true;
                        hidingMethod = package.Data as IHidingMethod;
                        CurrentPage = new Pages.CodingPage
                        {
                            DataContext = new CodingViewModel(navigationLine, hidingMethod, Audio)
                        };
                        ((ISupportParameter)CurrentPage.DataContext).Parameter = (Action<InterViewModelPackage>)Navigate;
                        break;
                    case State.MessageCoding:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
