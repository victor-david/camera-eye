using Restless.Tools.Mvvm;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Represents a custom window
    /// </summary>
    public class AppWindow : Window
    {
        #region Constructors
        public AppWindow()
        {
            MinimizeCommand = RelayCommand.Create((p) => WindowState = WindowState.Minimized);
            ChangeStateCommand = RelayCommand.Create(RunChangeStateCommand);
            CloseCommand = RelayCommand.Create((p) => Close());
            /* see comments in this method */
            SetMaxHeight();
            /* see comments in this method */
            SystemParameters.StaticPropertyChanged += SystemParametersStaticPropertyChanged;
            Loaded += (s, e) => OnLoaded(s, e);
            UseLayoutRounding = true;
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        static AppWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AppWindow), new FrameworkPropertyMetadata(typeof(AppWindow)));
        }
        #endregion

        /************************************************************************/

        #region Window properties
        /// <summary>
        /// Gets or sets the title bar menu
        /// </summary>
        public Menu Menu
        {
            get => (Menu)GetValue(MenuProperty);
            set => SetValue(MenuProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Menu"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MenuProperty = DependencyProperty.Register
            (
                nameof(Menu), typeof(Menu), typeof(AppWindow), new PropertyMetadata(null)
            );


        /// <summary>
        /// Gets or sets the brush used for the background of the title bar.
        /// </summary>
        public Brush TitleBarBackground
        {
            get => (Brush)GetValue(TitleBarBackgroundProperty);
            set => SetValue(TitleBarBackgroundProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TitleBarBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleBarBackgroundProperty = DependencyProperty.Register
            (
                nameof(TitleBarBackground), typeof(Brush), typeof(AppWindow), new PropertyMetadata(Brushes.Black)
            );


        /// <summary>
        /// Gets or sets the brush used for the foreground of the title bar
        /// </summary>
        public Brush TitleBarForeground
        {
            get => (Brush)GetValue(TitleBarForegroundProperty);
            set => SetValue(TitleBarForegroundProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TitleBarForeground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleBarForegroundProperty = DependencyProperty.Register
            (
                nameof(TitleBarForeground), typeof(Brush), typeof(AppWindow), new PropertyMetadata(Brushes.White)
            );

        /// <summary>
        /// Gets or sets the brush used for the buttons in the title bar.
        /// </summary>
        public Brush TitleBarButtonBrush
        {
            get => (Brush)GetValue(TitleBarButtonBrushProperty);
            set => SetValue(TitleBarButtonBrushProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TitleBarButtonBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleBarButtonBrushProperty = DependencyProperty.Register
            (
                nameof(TitleBarButtonBrush), typeof(Brush), typeof(AppWindow), new PropertyMetadata(Brushes.White)
            );

        /// <summary>
        /// Gets or sets the brush used for the bottom border of the title bar.
        /// </summary>
        public Brush TitleBarBorderBrush
        {
            get => (Brush)GetValue(TitleBarBorderBrushProperty);
            set => SetValue(TitleBarBorderBrushProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="TitleBarBorderBrush"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleBarBorderBrushProperty = DependencyProperty.Register
            (
                nameof(TitleBarBorderBrush), typeof(Brush), typeof(AppWindow), new PropertyMetadata(Brushes.Black)
            );
        #endregion

        /************************************************************************/

        #region Commands
        /// <summary>
        /// Gets the minimize command.
        /// </summary>
        public ICommand MinimizeCommand
        {
            get => (ICommand)GetValue(MinimizeCommandProperty);
            private set => SetValue(MinimizeCommandPropertyKey, value);
        }

        private static readonly DependencyPropertyKey MinimizeCommandPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(MinimizeCommand), typeof(ICommand), typeof(AppWindow), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="MinimizeCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimizeCommandProperty = MinimizeCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the change state command.
        /// </summary>
        public ICommand ChangeStateCommand
        {
            get => (ICommand)GetValue(ChangeStateCommandProperty);
            private set => SetValue(ChangeStateCommandPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ChangeStateCommandPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(ChangeStateCommand), typeof(ICommand), typeof(AppWindow), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="ChangeStateCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ChangeStateCommandProperty = ChangeStateCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the close command.
        /// </summary>
        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            private set => SetValue(CloseCommandPropertyKey, value);
        }

        private static readonly DependencyPropertyKey CloseCommandPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(CloseCommand), typeof(ICommand), typeof(AppWindow), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="CloseCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CloseCommandProperty = CloseCommandPropertyKey.DependencyProperty;
        #endregion

        /************************************************************************/

        #region Protected methods
        /// <summary>
        /// Called when the window is loaded. Override if needed. The base implementation does nothing.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args</param>
        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {
        }
        #endregion

        /************************************************************************/

        #region Private methods
        private void RunChangeStateCommand(object parm)
        {
            WindowState = WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
        }

        private void SystemParametersStaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            /* System parameters can change while the app is running. User can set task bar to auto hide.
             * This increases the maxium height available. Or vice-versa if user removes task bar auto hide.
             */
            if (e.PropertyName == nameof(SystemParameters.MaximizedPrimaryScreenHeight))
            {
                SetMaxHeight();
            }
        }

        private void SetMaxHeight()
        {
            /* Prevents window from extending beneath task bar when maximized, but could be problem with multiple monitors.
             * Why -2? Otherwise, the window is still two pixels under the task bar. Don't know why.
             */
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight - 2;
        }
        #endregion
    }
}