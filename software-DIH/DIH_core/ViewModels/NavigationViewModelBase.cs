using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DIH_core.ViewModels
{
    class NavigationViewModelBase:ViewModelBase
    {
        public enum NavigationCodes { Extract, Embed, Start, None }

        protected event Action<InterViewModelPackage> StepFinished;
        protected override void OnParameterChanged(object parameter)
        {
            base.OnParameterChanged(parameter);
            StepFinished += (Action<InterViewModelPackage>)parameter;
        }
        protected void Navigate(InterViewModelPackage package)
        {
            if (StepFinished != null)
                StepFinished.Invoke(package);
            StepFinished = null;
        }
    }
}
