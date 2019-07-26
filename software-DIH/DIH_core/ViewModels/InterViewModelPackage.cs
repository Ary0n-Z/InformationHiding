using DIH_core.Src.WAV;
using static DIH_core.ViewModels.NavigationViewModelBase;

namespace DIH_core.ViewModels
{
    class InterViewModelPackage
    {
        public InterViewModelPackage(NavigationCodes code, object data)
        {
            Code = code;
            Data = data;
        }
        public NavigationCodes Code { get; set; }
        public object Data {get;set;}
    }
}
