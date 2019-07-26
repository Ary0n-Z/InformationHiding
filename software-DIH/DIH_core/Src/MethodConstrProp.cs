using DIH_core.Src.MethodsInterface;
using DevExpress.Mvvm;
using PropertyChanged;
using System.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using DevExpress.Mvvm.DataAnnotations;
using Microsoft.Win32;

namespace DIH_core.Src
{
    class MethodConstrProp:BindableBase
    {
        public string PropName { get { return propertyInfo.Name; }}
        private object propHolder;
        private PropertyInfo propertyInfo;
        public HidingMethodParamAttribute Attr { get; set; }
        public MethodConstrProp(object holder,PropertyInfo prop)
        {
            propHolder = holder;
            propertyInfo = prop;
            Attr = prop.GetCustomAttribute(typeof(HidingMethodParamAttribute)) as HidingMethodParamAttribute;
            GetFilepathCommand = new DelegateCommand(()=> {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Wav Files (.wav)|*.wav|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.Title = "Open primal audio";
                if (openFileDialog.ShowDialog() == true)
                {
                    Filepath = openFileDialog.FileName;
                    Attr.Required = false;
                }
            });
        }
        #region Set property
        public int SetValue
        {
            get
            {
                return (int)propertyInfo.GetValue(propHolder);
            }
            set
            {
                try
                {
                    propertyInfo.SetValue(propHolder, value);
            }catch(ApplicationException ex)
                {
                    OutputLogs.Instance().AddLog(new Message(MessageType.Error, ex.Message, propHolder.GetType().Name));
                }
}
        }
        #endregion
        #region Set Constr Value
        public int SetConstrValue
        {
            get
            {
                return (int)propertyInfo.GetValue(propHolder);
            }
            set
            {
                try
                {
                    propertyInfo.SetValue(propHolder, value);
                }catch(ApplicationException ex)
                {
                    OutputLogs.Instance().AddLog(new Message(MessageType.Error, ex.Message, propHolder.GetType().Name));
                }
            }
        }
        #endregion
        #region Set Int Value
        public int SetIntValue
        {
            get
            {
                return (int)propertyInfo.GetValue(propHolder);
            }
            set
            {
                propertyInfo.SetValue(propHolder, value);
            }
        }
        #endregion
        #region Set Filepath
        public string Filepath
        {
            get
            {
                return (string)propertyInfo.GetValue(propHolder);
            }
            set
            {
                try
                {
                    propertyInfo.SetValue(propHolder, value);
                }
                catch (ApplicationException ex)
                {
                    OutputLogs.Instance().AddLog(new Message(MessageType.Error, ex.Message, propHolder.GetType().Name));
                }
            }
        }
        public DelegateCommand GetFilepathCommand { get; private set; }
        #endregion
        #region Set Range
        public double SetRange
        {
            get
            {
                return (double)propertyInfo.GetValue(propHolder);
            }
            set
            {
                if(value < Attr.MaxRange && value > Attr.MinRange)
                    propertyInfo.SetValue(propHolder, value);
                else
                    OutputLogs.Instance().AddLog(
                        new Message(
                            MessageType.Error,
                            string.Format("Value must be between {0} and {1}.",Attr.MinRange,Attr.MaxRange), 
                            PropName));
            }
        }
        #endregion
    }
}
