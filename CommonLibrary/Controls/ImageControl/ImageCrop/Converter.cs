using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace CommonLibrary
{
    /// <summary>
    /// Value converter that translates true to <see cref="Visibility.Visible"/> and false to
    /// <see cref="Visibility.Collapsed"/>.
    /// </summary>
    public sealed class VisibilityConverter : IValueConverter
    {
        /// <summary>
        /// If true - converts from Visibility to Boolean.
        /// </summary>
        public bool IsReversed { get; set; }

        /// <summary>
        /// If true - converts true to Collapsed and false to Visible.
        /// </summary>
        public bool IsInversed { get; set; }

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <param name="value">The source data being passed to the target.</param>
        /// <param name="targetType">The type of the target property, specified by a helper structure that wraps the type name.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>The value to be passed to the target dependency property.</returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (IsReversed)
            {
                return (value is Visibility && (Visibility)value == Visibility.Visible) ^ IsInversed;
            }

            return (value is bool && (bool)value) ^ IsInversed ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object. This method is called only in <c>TwoWay</c> bindings. 
        /// </summary>
        /// <param name="value">The target data being passed to the source..</param>
        /// <param name="targetType">The type of the target property, specified by a helper structure that wraps the type name.</param>
        /// <param name="parameter">An optional parameter to be used in the converter logic.</param>
        /// <param name="language">The language of the conversion.</param>
        /// <returns>The value to be passed to the source object.</returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (IsReversed)
            {
                return (value is bool && (bool)value) ^ IsInversed ? Visibility.Visible : Visibility.Collapsed;
            }

            return (value is Visibility && (Visibility)value == Visibility.Visible) ^ IsInversed;
        }
    }


    /// <summary>
    /// Get the middle of two number
    /// 4 and 6 is 5
    /// </summary>
    public sealed class CropImageControlLineCoordinateConverter : DependencyObject, IValueConverter
    {


        public double No1
        {
            get { return (double)GetValue(No1Property); }
            set { SetValue(No1Property, value); }
        }

        // Using a DependencyProperty as the backing store for No1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty No1Property =
            DependencyProperty.Register("No1", typeof(double), typeof(CropImageControlLineCoordinateConverter), new PropertyMetadata(0.0));



        public double No2
        {
            get { return (double)GetValue(No2Property); }
            set { SetValue(No2Property, value); }
        }

        // Using a DependencyProperty as the backing store for No2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty No2Property =
            DependencyProperty.Register("No2", typeof(double), typeof(CropImageControlLineCoordinateConverter), new PropertyMetadata(0.0));




        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value==null || parameter==null)
            {
                return 0.0;
            }
            double no1 = (double)value;
            double no2 = double.Parse(parameter.ToString());
            return (no2+ no1)/2;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public sealed class DateTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var datetime = (DateTime)value;
            var format = (string)parameter;
            if (datetime!=null)
            {
                return datetime.ToString(format);
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
