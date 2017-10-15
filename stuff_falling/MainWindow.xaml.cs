using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using LiveCharts;
using LiveCharts.Wpf;
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
            Model.CalculationCompleted += OnCalculationComplete;
            DataContext = this;
        }

        public List<string> Labels { get; set; } = new List<string>();

        public SeriesCollection Series { get; set; } = new SeriesCollection();

        private delegate void UpdateDelegate(Model.Result result);

        private void AddSeries(List<double> values, String title)
        {
            Series.Add(new LineSeries
            {
                Title = title,
                Values = new ChartValues<double>(values),
                LineSmoothness = 0,
                PointGeometry = null,
                Fill = new SolidColorBrush()
            });
        } 

        private void UpdateData(Model.Result result)
        {
            Series.Clear();
            AddSeries(result.Height, "Height");
            AddSeries(result.Speed, "Speed");
            AddSeries(result.Acceleration, "Acceleration");
            Labels.Clear();
            Labels.AddRange(result.Time.ConvertAll(new Converter<double, string>((double x) => { return x.ToString(); })));
        }

        private void OnCalculationComplete(object sender, Model.Result result)
        {
            Dispatcher.Invoke(new UpdateDelegate(UpdateData), result);
        }

        private void Update()
        {
            List<Model.Forces> forces = new List<Model.Forces>();
            if (Archimedes.IsChecked.Value)
                forces.Add(Model.Forces.Archimedes);
            if (LiquidFriction.IsChecked.Value)
                forces.Add(Model.Forces.Viscosity);
            if (GasDrag.IsChecked.Value)
                forces.Add(Model.Forces.Drag);
            Model.BeginCalculate(new Model.Parameters()
            {
                Forces = forces,
                Height = Convert.ToDouble(StartHeight.Text),
                Speed = Convert.ToDouble(StartSpeed.Text),
                EndTime = Convert.ToDouble(EndTime.Text),
                SegmentCount = Convert.ToDouble(PointNumber.Text),
                IsConstGravitationalAcceleration = GIsConst.IsChecked.Value,
                SphereRadius = Convert.ToDouble(BallRadius.Text),
                SphereMass = Convert.ToDouble(BallMass.Text),
                EnviromentDensity = Convert.ToDouble(EnvDensity.Text),
                EnviromentViscosity = Convert.ToDouble(EnvViscosity.Text)
            });
        }

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

        private void CheckboxOnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsLoaded)
            {
                Update();
            }
        }

        private void ListBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(ListBox, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item == null)
                return;
            var series = (LineSeries)item.Content;
            series.Visibility = series.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;
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

    public class OpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (Visibility)value == Visibility.Visible
                ? 1d
                : .2d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
