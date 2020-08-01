using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Restless.App.Camera.Core
{
    public class ReleaseButton : Button
    {
        #region Constructors
        public ReleaseButton()
        {
        }

        static ReleaseButton()
        {
            PaddingProperty.OverrideMetadata(typeof(ReleaseButton), new FrameworkPropertyMetadata(OnPaddingChanged));
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets or sets the command to execute when the mouse left button goes down.
        /// </summary>
        public ICommand MouseDownCommand
        {
            get => (ICommand)GetValue(MouseDownCommandProperty);
            set => SetValue(MouseDownCommandProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="MouseDownCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MouseDownCommandProperty = DependencyProperty.Register
            (
                nameof(MouseDownCommand), typeof(ICommand), typeof(ReleaseButton), new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets the parameter for <see cref="MouseDownCommand"/>.
        /// </summary>
        public object MouseDownCommandParameter
        {
            get => (object)GetValue(MouseDownCommandParameterProperty);
            set => SetValue(MouseDownCommandParameterProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="MouseDownCommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MouseDownCommandParameterProperty = DependencyProperty.Register
            (
                nameof(MouseDownCommandParameter), typeof(object), typeof(ReleaseButton), new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets the command to execute when the mouse left button goes up.
        /// </summary>
        public ICommand MouseUpCommand
        {
            get => (ICommand)GetValue(MouseUpCommandProperty);
            set => SetValue(MouseUpCommandProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="MouseUpCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MouseUpCommandProperty = DependencyProperty.Register
            (
                nameof(MouseUpCommand), typeof(ICommand), typeof(ReleaseButton), new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets or sets the parameter for <see cref="MouseUpCommand"/>.
        /// </summary>
        public object MouseUpCommandParameter
        {
            get => (object)GetValue(MouseUpCommandParameterProperty);
            set => SetValue(MouseUpCommandParameterProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="MouseUpCommandParameter"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MouseUpCommandParameterProperty = DependencyProperty.Register
            (
                nameof(MouseUpCommandParameter), typeof(object), typeof(ReleaseButton), new PropertyMetadata(null)
            );

        #endregion

        #region Pressed
        /// <summary>
        /// Gets or sets the movement direction when pressed.
        /// </summary>
        public Direction PressedDirection
        {
            get => (Direction)GetValue(PressedDirectionProperty);
            set => SetValue(PressedDirectionProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="PressedDirection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PressedDirectionProperty = DependencyProperty.Register
            (
                nameof(PressedDirection), typeof(Direction), typeof(ReleaseButton), new PropertyMetadata(Direction.None)
            );

        private static void OnPaddingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as ReleaseButton)?.InitializePressedPadding();
        }

        /// <summary>
        /// Gets the thickness to use when the button is pressed
        /// </summary>
        public Thickness PressedPadding
        {
            get => (Thickness)GetValue(PressedPaddingProperty);
            private set => SetValue(PressedPaddingPropertyKey, value);
        }

        private static readonly DependencyPropertyKey PressedPaddingPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(PressedPadding), typeof(Thickness), typeof(ReleaseButton), new PropertyMetadata(new Thickness(0))
            );

        /// <summary>
        /// Identifies the <see cref="PressedPadding"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PressedPaddingProperty = PressedPaddingPropertyKey.DependencyProperty;
        #endregion

        #region Methods
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            SetPressedPadding();
            if (MouseDownCommand != null && MouseDownCommand.CanExecute(MouseDownCommandParameter))
            {
                /* don't mark as handled or the pressed trigger doesn't fire */
                MouseDownCommand.Execute(MouseDownCommandParameter);
            }
        }

        protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonUp(e);
            InitializePressedPadding();
            if (MouseUpCommand != null && MouseUpCommand.CanExecute(MouseUpCommandParameter))
            {
                /* don't mark as handled or the pressed trigger doesn't fire */
                MouseUpCommand.Execute(MouseUpCommandParameter);
            }
        }
        #endregion

        #region Private methods
        private void InitializePressedPadding()
        {
            PressedPadding = new Thickness(Padding.Left, Padding.Top, Padding.Right, Padding.Bottom);
        }

        private void SetPressedPadding()
        {
            int change = 1;
            switch (PressedDirection)
            {
                case Direction.None:
                    PressedPadding = new Thickness(Padding.Left, Padding.Top, Padding.Right, Padding.Bottom);
                    break;
                case Direction.Left:
                    PressedPadding = new Thickness(Padding.Left - change, Padding.Top, Padding.Right + change, Padding.Bottom);
                    break;
                case Direction.Right:
                    PressedPadding = new Thickness(Padding.Left + change, Padding.Top, Padding.Right - change, Padding.Bottom);
                    break;
                case Direction.Up:
                    PressedPadding = new Thickness(Padding.Left, Padding.Top - change, Padding.Right, Padding.Bottom + change);
                    break;
                case Direction.Down:
                    PressedPadding = new Thickness(Padding.Left, Padding.Top + change, Padding.Right, Padding.Bottom - change);
                    break;
            }
        }
        #endregion
    }
}