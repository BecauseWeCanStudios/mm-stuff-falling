using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;

namespace stuff_falling
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Update() {}

        private void DoubleTBPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            string decimalSeparator = ci.NumberFormat.CurrencyDecimalSeparator;
            if (decimalSeparator == ".")
            {
                decimalSeparator = "\\" + decimalSeparator;
            }
            var textBox = sender as TextBox;
            var pos = textBox.CaretIndex;
            e.Handled = !Regex.IsMatch(textBox.Text.Substring(0, pos) + e.Text + textBox.Text.Substring(pos), @"^[-+]?[0-9]*" + decimalSeparator + @"?[0-9]*$");
        }

        private void IntTBPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            var pos = textBox.CaretIndex;
            e.Handled = !Regex.IsMatch(textBox.Text.Substring(0, pos) + e.Text + textBox.Text.Substring(pos), @"^[-+]?[0-9]*$");
        }

        private void TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                Update();
            }
        }

        private void TB_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = e.Key == Key.Space;
        }
    }

    [ValueConversion(typeof(object), typeof(string))]
    public class StringFormatConverter : IValueConverter, IMultiValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(new object[] { value }, targetType, parameter, culture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
            return DependencyProperty.UnsetValue;
        }

        public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                string format = parameter?.ToString();
                if (String.IsNullOrEmpty(format))
                {
                    System.Text.StringBuilder builder = new System.Text.StringBuilder();
                    for (int index = 0; index < values.Length; ++index)
                    {
                        builder.Append("{" + index + "}");
                    }
                    format = builder.ToString();
                }
                return String.Format(/*culture,*/ format, values);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("StringFormatConverter({0}): {1}", parameter, ex.Message);
                return DependencyProperty.UnsetValue;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
            return null;
        }
    }

    [ValueConversion(typeof(object), typeof(string))]
    public class RadiusToVolumeConverter : StringFormatConverter
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double r;
            try
            {
                r = Double.Parse(value.ToString());
            }
            catch (FormatException e)
            {
                r = 1;
            }
            double v = Math.Pow(r, 3) * Math.PI * 4.0 / 3.0;
            return base.Convert(new object[] { v }, targetType, parameter, culture);
        }
    }

    [ValueConversion(typeof(object), typeof(string))]
    public class RadiusMassToDensityConverter : StringFormatConverter
    {
        public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            double r;
            try
            {
                r = Double.Parse(values[0].ToString());
            }
            catch (FormatException e)
            {
                r = 1;
            }
            double v = Math.Pow(r, 3) * Math.PI * 4.0 / 3.0;
            double m;
            try
            {
                m = Double.Parse(values[1].ToString());
            }
            catch (FormatException e)
            {
                m = 1;
            }
            return base.Convert(new object[] { m / v }, targetType, parameter, culture);
        }
    }

}
