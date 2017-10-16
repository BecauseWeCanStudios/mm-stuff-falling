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
using LiveCharts.Charts;
using MahApps.Metro.Controls;
using LiveCharts.Wpf.Charts.Base;
using System.Windows.Media.Animation;

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
            Update();
        }

        public List<string> Labels { get; set; } = new List<string>();

        public SeriesCollection HeightSeries { get; set; } = new SeriesCollection();
        public SeriesCollection SpeedSeries { get; set; } = new SeriesCollection();
        public SeriesCollection AccelerationSeries { get; set; } = new SeriesCollection();

        private List<Ellipse> Ellipsies = new List<Ellipse>();
        private List<DoubleAnimationUsingKeyFrames> Animations = new List<DoubleAnimationUsingKeyFrames>();

        private bool AddEllipse = true;
        private bool UpdateAnimation = true;
        private int index = 0;

        private delegate void UpdateDelegate(Model.Result result);

        private void UpdateSeries(SeriesCollection series, List<double> values)
        {
            if (series.Count > 0)
                series.RemoveAt(series.Count - 1);
            series.Add(new LineSeries
            {
                Title = "Эксперимент №" + (series.Count + 1).ToString(),
                Values = new ChartValues<double>(values),
                LineSmoothness = 0,
                PointGeometry = null,
                Fill = new SolidColorBrush(),
                Stroke = new SolidColorBrush(Chart.Colors[(int)(index - Chart.Colors.Count * Math.Truncate(index / (double)Chart.Colors.Count))])
            });
        } 

        private void UpdateData(Model.Result result)
        {
            if (AddEllipse)
            {
                Ellipsies.Add(new Ellipse() { Width = 30, Height = 30 });
                Canvas.Children.Add(Ellipsies.Last());
                Canvas.SetLeft(Canvas.Children[Canvas.Children.Count - 1], 625);
                Canvas.SetTop(Canvas.Children[Canvas.Children.Count - 1], 115);
                Animations.Add(new DoubleAnimationUsingKeyFrames());
                AddEllipse = false;
            }
            UpdateSeries(HeightSeries, result.Height);
            UpdateSeries(SpeedSeries, result.Speed);
            UpdateSeries(AccelerationSeries, result.Acceleration);
            Labels.Clear();
            Labels.AddRange(result.Time.ConvertAll(new Converter<double, string>((double x) => { return x.ToString(); })));
            Ellipsies.Last().Fill = new SolidColorBrush(Chart.Colors[(int)(index - Chart.Colors.Count * Math.Truncate(index / (double)Chart.Colors.Count))]);
            UpdateAnimation = true;
        }

        private void OnCalculationComplete(object sender, Model.Result result)
        {
            Dispatcher.Invoke(new UpdateDelegate(UpdateData), result);
        }

        private String PassDefaultIfEmpty(String s)
        {
            if (String.IsNullOrEmpty(s))
                return "1";
            if (s == "-" || s == "+")
                return s + "1";
            return s;
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
                Height = Convert.ToDouble(PassDefaultIfEmpty(StartHeight.Text)),
                Speed = Convert.ToDouble(PassDefaultIfEmpty(StartSpeed.Text)),
                EndTime = Convert.ToDouble(PassDefaultIfEmpty(EndTime.Text)),
                SegmentCount = Convert.ToDouble(PassDefaultIfEmpty(PointNumber.Text)),
                IsConstGravitationalAcceleration = GIsConst.IsChecked.Value,
                SphereRadius = Convert.ToDouble(PassDefaultIfEmpty(BallRadius.Text)),
                SphereMass = Convert.ToDouble(PassDefaultIfEmpty(BallMass.Text)),
                EnviromentDensity = Convert.ToDouble(PassDefaultIfEmpty(EnvDensity.Text)),
                EnviromentViscosity = Convert.ToDouble(PassDefaultIfEmpty(EnvViscosity.Text))
            });
        }

        private void DoubleTBPreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            string decimalSeparator = ci.NumberFormat.NumberDecimalSeparator;
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

        private void CheckboxOnIsEnabledChanged(object sender, EventArgs e)
        {
            if (this.IsLoaded)
            {
                Update();
            }
        }

        private void ListBox_OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement((ItemsControl)sender, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item == null)
                return;
            var series = (LineSeries)item.Content;
            series.Visibility = series.Visibility == Visibility.Visible
                ? Visibility.Hidden
                : Visibility.Visible;
        }

        private void SaveExperimentButton_Click(object sender, RoutedEventArgs e)
        {
            HeightSeries.Insert(HeightSeries.Count - 1, HeightSeries.Last());
            SpeedSeries.Insert(SpeedSeries.Count - 1, SpeedSeries.Last());
            AccelerationSeries.Insert(AccelerationSeries.Count - 1, AccelerationSeries.Last());
            AddEllipse = true;
            ++index;
            Update();
        }

        private void StartAnimationButton_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateAnimation)
            {
                double height = -1;
                foreach (var it in HeightSeries)
                    foreach (double h in it.Values)
                        if (h > height)
                            height = h;
                for (int i = 0; i < Ellipsies.Count; ++i)
                {
                    DoubleAnimationUsingKeyFrames anim = new DoubleAnimationUsingKeyFrames();
                    foreach (double it in HeightSeries[i].Values)
                    {
                        anim.KeyFrames.Add(new LinearDoubleKeyFrame(575 - 455 * it / height));
                    }
                    anim.Duration = new Duration(new TimeSpan(0, 0, 5));
                    Animations[i] = anim;
                    Canvas.SetTop(Ellipsies[i], anim.KeyFrames[0].Value);
                }
            }
            for (int i = 0; i < Ellipsies.Count; ++i)
                Ellipsies[i].BeginAnimation(Canvas.TopProperty, Animations[i]);
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
