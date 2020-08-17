using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Provides attached properties
    /// </summary>
    public static class Property
    {
        #region RowHeights
        private const string RowHeightsPropertyName = "RowHeights";

        public static string GetRowHeights(DependencyObject obj)
        {
            return (string)obj.GetValue(RowHeightsProperty);
        }

        public static void SetRowHeights(DependencyObject obj, string value)
        {
            obj.SetValue(RowHeightsProperty, value);
        }

        public static readonly DependencyProperty RowHeightsProperty = DependencyProperty.RegisterAttached
            (
                RowHeightsPropertyName, typeof(string), typeof(Property), new PropertyMetadata()
                {
                    DefaultValue = string.Empty,
                    PropertyChangedCallback = OnRowHeightsChanged
                }
            );

        private static void OnRowHeightsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Grid grid)) return;

            grid.RowDefinitions.Clear();

            string definitions = e.NewValue.ToString();
            if (string.IsNullOrEmpty(definitions)) return;
            var heights = definitions.Split(',');

            foreach (string value in heights)
            {
                if (value.ToLower() == "auto")
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
                else if (value.EndsWith("*"))
                {
                    string value2 = value.Replace("*", "");
                    if (string.IsNullOrEmpty(value2)) value2 = "1";
                    var numHeight = int.Parse(value2);
                    grid.RowDefinitions.Add(new RowDefinition
                    {
                        Height = new GridLength(numHeight, GridUnitType.Star)
                    });
                }
                else
                {
                    var numHeight = int.Parse(value);
                    grid.RowDefinitions.Add(new RowDefinition
                    {
                        Height = new GridLength(numHeight, GridUnitType.Pixel)
                    });
                }
            }
        }
        #endregion

        /************************************************************************/

        #region ColumnWidths
        private const string ColumnWidthsPropertyName = "ColumnWidths";

        public static string GetColumnWidths(DependencyObject obj)
        {
            return (string)obj.GetValue(ColumnWidthsProperty);
        }

        public static void SetColumnWidths(DependencyObject obj, string value)
        {
            obj.SetValue(ColumnWidthsProperty, value);
        }

        public static readonly DependencyProperty ColumnWidthsProperty = DependencyProperty.RegisterAttached
            (
                ColumnWidthsPropertyName, typeof(string), typeof(Property), new PropertyMetadata()
                {
                    DefaultValue = string.Empty,
                    PropertyChangedCallback = OnColumnWidthsChanged
                }
            );

        private static void OnColumnWidthsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is Grid grid)) return;

            grid.ColumnDefinitions.Clear();

            var definitions = e.NewValue.ToString();
            if (string.IsNullOrEmpty(definitions)) return;
            var widths = definitions.Split(',');

            foreach (string value in widths)
            {
                if (value.ToLower() == "auto")
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }
                else if (value.EndsWith("*"))
                {
                    string value2 = value.Replace("*", "");
                    if (string.IsNullOrEmpty(value2)) value2 = "1";
                    var numWidth = int.Parse(value2);
                    grid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(numWidth, GridUnitType.Star)
                    });
                }
                else
                {
                    var numWidth = int.Parse(value);
                    grid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(numWidth, GridUnitType.Pixel)
                    });
                }
            }
        }
        #endregion

        /************************************************************************/

        #region IsVisible
        private const string IsVisiblePropertyName = "IsVisible";

        public static readonly DependencyProperty IsVisibleProperty = DependencyProperty.RegisterAttached
            (
                IsVisiblePropertyName, typeof(bool), typeof(Property), new PropertyMetadata(true, OnIsVisibleChanged)
            );

        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.Visibility = ((bool)e.NewValue) == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public static void SetIsVisible(DependencyObject element, bool value)
        {
            element.SetValue(IsVisibleProperty, value);
        }

        public static bool GetIsVisible(DependencyObject element)
        {
            return (bool)element.GetValue(IsVisibleProperty);
        }
        #endregion

        /************************************************************************/

        #region IsCollapsed
        private const string IsCollapsedPropertyName = "IsCollapsed";

        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.RegisterAttached
            (
                IsCollapsedPropertyName, typeof(bool), typeof(Property), new PropertyMetadata(false, OnIsCollapsedChanged)
            );

        private static void OnIsCollapsedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                element.Visibility = ((bool)e.NewValue) == true ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        public static void SetIsCollapsed(DependencyObject element, bool value)
        {
            element.SetValue(IsCollapsedProperty, value);
        }

        public static bool GetIsCollapsed(DependencyObject element)
        {
            return (bool)element.GetValue(IsCollapsedProperty);
        }
        #endregion

        /************************************************************************/

        #region RolloverBrush
        private const string RolloverBrushPropertyName = "RolloverBrush";

        public static readonly DependencyProperty RolloverBrushProperty = DependencyProperty.RegisterAttached
            (
                RolloverBrushPropertyName, typeof(Brush), typeof(Property), new PropertyMetadata(Brushes.Transparent)
            );

        public static void SetRolloverBrush(DependencyObject element, Brush value)
        {
            element.SetValue(RolloverBrushProperty, value);
        }

        public static Brush GetRolloverBrush(DependencyObject element)
        {
            return (Brush)element.GetValue(RolloverBrushProperty);
        }
        #endregion
    }
}