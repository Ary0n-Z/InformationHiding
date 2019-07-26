using DIH_core.Src;
using DIH_core.Src.MethodsInterface;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace DIH_core.TemplateSelectors
{
    class MethodParamTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var prop = item as MethodConstrProp;
            DataTemplate dataTemplate = null;
            switch (prop.Attr.Type)
            {
                case HidingMethodParamAttribute.ParamType.Set:
                    dataTemplate = Application.Current.FindResource("MePa_Set") as DataTemplate;
                    break;
                case HidingMethodParamAttribute.ParamType.ConstrValue:
                    dataTemplate = Application.Current.FindResource("MePa_Constr") as DataTemplate;
                    break;
                case HidingMethodParamAttribute.ParamType.Range:
                    dataTemplate = Application.Current.FindResource("MePa_SetRange") as DataTemplate;
                    break;
                case HidingMethodParamAttribute.ParamType.IntValue:
                    dataTemplate = Application.Current.FindResource("MePa_SetInt") as DataTemplate;
                    break;
                case HidingMethodParamAttribute.ParamType.FilePath:
                    dataTemplate = Application.Current.FindResource("MePa_Filepath") as DataTemplate;
                    break;
                default:
                    break;
            }
            return dataTemplate;
        }
    }
}