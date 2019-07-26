using DIH_core.Src;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DIH_core.Converters
{
    class MessageToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string imgPath = null;
            if (value is MessageType)
                switch ((MessageType)value)
                {
                    case MessageType.Error:
                        imgPath = "/Icons/MessageError.png";
                        break;
                    case MessageType.Info:
                        imgPath = "/Icons/MessageInfo.png";
                        break;
                    case MessageType.Warrning:
                        imgPath = "/Icons/MessageWarrning.png";
                        break;
                    default:
                        break;
                }
            return imgPath;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
