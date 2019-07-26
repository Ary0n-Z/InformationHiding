using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using DIH_core.Src;
using DIH_core.Src.Methods;
using DIH_core.Src.MethodsInterface;
using DIH_core.Src.WAV;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DIH_core.ViewModels
{
    class MethodViewModel : NavigationViewModelBase
    {
        [AlsoNotifyFor("MethodProperties", "MethodDesc")]
        public IHidingMethod SelectedMethod { get; set; }
        public List<IHidingMethod> HidingMethods { get; set; }
        public string MethodDesc { get
            {
                if (SelectedMethod == null)
                    return "";
                else
                    return SelectedMethod.Description();
            } }
        private Dictionary<string, MethodConstrProp[]> methodsParamsDict;
        public MethodConstrProp[] MethodProperties
        {
            get
            {
                if (SelectedMethod != null)
                    return methodsParamsDict[SelectedMethod.ToString()];
                return null;
            }
        }
        
        public MethodViewModel(NavigationCodes navigationCodes)
        {
            SelectedMethod = null;
            HidingMethods = new List<IHidingMethod>(4);
            HidingMethods.Add(new LSBMethod());
            HidingMethods.Add(new PhaseCoding());
            HidingMethods.Add(new SpreadSpectrum());
            HidingMethods.Add(new ParityCoding());

            var attrType = typeof(HidingMethodParamAttribute);
            methodsParamsDict = new Dictionary<string, MethodConstrProp[]>(HidingMethods.Count);
            if (navigationCodes == NavigationCodes.Embed)
                foreach (var method in HidingMethods)
                {
                    var props_types = method.GetType().GetProperties();
                    var props = (from prop in props_types
                                 where Attribute.IsDefined(prop, attrType)
                                 where !((HidingMethodParamAttribute)prop.GetCustomAttribute(attrType)).NotInEmbed
                                 select new MethodConstrProp(method, prop)).ToArray();
                    methodsParamsDict.Add(method.ToString(), props);
                }
            else
            {
                foreach (var method in HidingMethods)
                {
                    var props_types = method.GetType().GetProperties();
                    var props = (from prop in props_types
                                 where Attribute.IsDefined(prop, typeof(HidingMethodParamAttribute))
                                 where !((HidingMethodParamAttribute)prop.GetCustomAttribute(attrType)).NotInExtract
                                 select new MethodConstrProp(method, prop)).ToArray();
                    methodsParamsDict.Add(method.ToString(), props);
                }
            }
        }
        public bool CanProgressOn()
        {
            return SelectedMethod != null;
        }
        [Command(CanExecuteMethodName = "CanProgressOn")]
        public void NextStep(){
            Navigate(new InterViewModelPackage(NavigationCodes.None, SelectedMethod));
        }
    }
        
}

