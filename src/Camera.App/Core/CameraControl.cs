using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Camera.Contracts;
using Restless.Camera.Contracts.RawFrames.Video;
using Restless.Tools.Mvvm;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Controls = System.Windows.Controls;

namespace Restless.App.Camera.Core
{
    /// <summary>
    /// Represents a composite control to view and move (if supported) a camera
    /// </summary>
    [TemplatePart(Name = PartImage, Type = typeof(Controls.Image))]
    [TemplatePart(Name = PartError, Type = typeof(Controls.Border))]
    [TemplatePart(Name = PartController, Type = typeof(Controls.Border))]
    public class CameraControl : Controls.Control
    {
        #region Private
        private const string PartImage = "PART_Image";
        private const string PartError = "PART_Error";
        private const string PartController = "PART_Controller";

        private Controls.Image imageControl;
        private Controls.Border errorControl;
        private Controls.Border controllerControl;

        private const double ControllerHeight = 75.0;
        private const double ControllerHeightCushion = 10.0;
        private double imageScaledWidth;
        private bool isMouseDown;
        private bool isCameraMotionStarted;
        private Point mouseDownPoint;
        private const int CameraMotionMovementThreshold = 10;
        private string pendingError;
        private WriteableBitmap writeable;
        private Int32Rect dirtyRect;
        private Storyboard ShowController; 
        private Storyboard HideController;
        private Storyboard HideErrorControl;
        private ControllerStatus controllerStatus;
        private Window hostWindow;
        private bool isLeftControlKeyDown;
        private readonly VideoDecoderManager videoDecoder;
        private TransformParameters transformParameters;
        #endregion

        /************************************************************************/

        #region Private helper enum
        private enum ControllerStatus
        {
            Hidden,
            Hiding,
            Showing,
            Shown,
        }
        #endregion

        /************************************************************************/

        #region Constructors
        public CameraControl()
        {
            videoDecoder = new VideoDecoderManager();
            Loaded += CameraControlLoaded;
            Unloaded += CameraControlUnloaded;
            InitializeStoryBoards();
            controllerStatus = ControllerStatus.Hidden;
        }

        static CameraControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CameraControl), new FrameworkPropertyMetadata(typeof(CameraControl)));
        }
        #endregion

        /************************************************************************/

        #region Camera
        /// <summary>
        /// Gets or sets the camera
        /// </summary>
        public CameraRow Camera
        {
            get => (CameraRow)GetValue(CameraProperty);
            set => SetValue(CameraProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Camera"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraProperty = DependencyProperty.Register
            (
                nameof(Camera), typeof(CameraRow), typeof(CameraControl), new PropertyMetadata(null, OnCameraChanged)
            );

        private static void OnCameraChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CameraControl control && e.NewValue != null)
            {
                control.InitializeCamera();
            }
        }

        /// <summary>
        /// Gets the camera name.
        /// </summary>
        public string CameraName
        {
            get => (string)GetValue(CameraNameProperty);
            private set => SetValue(CameraNamePropertyKey, value);
        }

        private static readonly DependencyPropertyKey CameraNamePropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(CameraName), typeof(string), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="CameraName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CameraNameProperty = CameraNamePropertyKey.DependencyProperty;
        #endregion

        /************************************************************************/

        #region Plugin
        private ICameraPlugin Plugin { get; set; }
        private bool IsMouseCameraMotionAvailable;

        /// <summary>
        /// Gets a boolean value that indicates if we have a plugin
        /// </summary>
        public bool HavePlugin
        {
            get => (bool)GetValue(HavePluginProperty);
            private set => SetValue(HavePluginPropertyKey, value);
        }

        private static readonly DependencyPropertyKey HavePluginPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(HavePlugin), typeof(bool), typeof(CameraControl), new PropertyMetadata(false)
            );

        /// <summary>
        /// Identifies the <see cref="HavePlugin"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HavePluginProperty = HavePluginPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a boolean value that specifies whether the plugin supports ICameraMotion
        /// </summary>
        public bool IsPluginMotion
        {
            get => (bool)GetValue(IsPluginMotionProperty);
            private set => SetValue(IsPluginMotionPropertyKey, value);
        }

        private static readonly DependencyPropertyKey IsPluginMotionPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(IsPluginMotion), typeof(bool), typeof(CameraControl), new PropertyMetadata(false)
            );

        /// <summary>
        /// Identifies the <see cref="IsPluginMotion"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsPluginMotionProperty = IsPluginMotionPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the motion command.
        /// </summary>
        public ICommand MotionCommand
        {
            get => (ICommand)GetValue(MotionCommandProperty);
            private set => SetValue(MotionCommandPropertyKey, value);
        }

        private static readonly DependencyPropertyKey MotionCommandPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(MotionCommand), typeof(ICommand), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="MotionCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty MotionCommandProperty = MotionCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets a boolean that determines if moving the camera using the mouse is activated
        /// </summary>
        public bool IsMouseCameraMotion
        {
            get => (bool)GetValue(IsMouseCameraMotionProperty);
            set => SetValue(IsMouseCameraMotionProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="IsMouseCameraMotion"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsMouseCameraMotionProperty = DependencyProperty.Register
            (
                nameof(IsMouseCameraMotion), typeof(bool), typeof(CameraControl), new PropertyMetadata(false)
            );

        /// <summary>
        /// Gets or sets the command to stop video
        /// </summary>
        public bool IsVideoRunning
        {
            get => (bool)GetValue(IsVideoRunningProperty);
            set => SetValue(IsVideoRunningProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="IsVideoRunning"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsVideoRunningProperty = DependencyProperty.Register
            (
                nameof(IsVideoRunning), typeof(bool), typeof(CameraControl), new PropertyMetadata(false, OnIsVideoRunningChanged)
            );

        private static void OnIsVideoRunningChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CameraControl control)
            {
                control.StartStopVideo();
            }
        }
        #endregion

        /************************************************************************/

        #region Image
        /// <summary>
        /// Gets the image source.
        /// </summary>
        public ImageSource ImageSource
        {
            get => (ImageSource)GetValue(ImageSourceProperty);
            private set => SetValue(ImageSourcePropertyKey, value);
        }

        private static readonly DependencyPropertyKey ImageSourcePropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(ImageSource), typeof(ImageSource), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="ImageSource"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageSourceProperty = ImageSourcePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the image max width
        /// </summary>
        public double ImageMaxWidth
        {
            get => (double)GetValue(ImageMaxWidthProperty);
            private set => SetValue(ImageMaxWidthPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ImageMaxWidthPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(ImageMaxWidth), typeof(double), typeof(CameraControl), new PropertyMetadata(0.0)
            );

        /// <summary>
        /// Identifies the <see cref="ImageMaxWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageMaxWidthProperty = ImageMaxWidthPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the image min width
        /// </summary>
        public double ImageMinWidth
        {
            get => (double)GetValue(ImageMinWidthProperty);
            private set => SetValue(ImageMinWidthPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ImageMinWidthPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(ImageMinWidth), typeof(double), typeof(CameraControl), new PropertyMetadata(320.0)
            );

        /// <summary>
        /// Identifies the <see cref="ImageMinWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageMinWidthProperty = ImageMinWidthPropertyKey.DependencyProperty;
        #endregion

        /************************************************************************/

        #region Feed Status
        /// <summary>
        /// Gets the width of the status banner
        /// </summary>
        public double StatusWidth
        {
            get => (double)GetValue(StatusWidthProperty);
            private set => SetValue(StatusWidthPropertyKey, value);
        }

        private static readonly DependencyPropertyKey StatusWidthPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(StatusWidth), typeof(double), typeof(CameraControl), new PropertyMetadata(0.0)
            );

        /// <summary>
        /// Identifies the <see cref="StatusWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusWidthProperty = StatusWidthPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the command to change the status alignment
        /// </summary>
        public ICommand StatusAlignmentCommand
        {
            get => (ICommand)GetValue(StatusAlignmentCommandProperty);
            private set => SetValue(StatusAlignmentCommandPropertyKey, value);
        }

        private static readonly DependencyPropertyKey StatusAlignmentCommandPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(StatusAlignmentCommand), typeof(ICommand), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="StatusAlignmentCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusAlignmentCommandProperty = StatusAlignmentCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the vertical alignment for the status banner
        /// </summary>
        public VerticalAlignment StatusVerticalAlignment
        {
            get => (VerticalAlignment)GetValue(StatusVerticalAlignmentProperty);
            private set => SetValue(StatusVerticalAlignmentPropertyKey, value);
        }

        private static readonly DependencyPropertyKey StatusVerticalAlignmentPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(StatusVerticalAlignment), typeof(VerticalAlignment), typeof(CameraControl), new PropertyMetadata(VerticalAlignment.Top)
            );

        /// <summary>
        /// Identifies the <see cref="StatusVerticalAlignment"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusVerticalAlignmentProperty = StatusVerticalAlignmentPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the date/time format to use on the status banner. Default is "yyyy-MMM-dd hh:mm:ss tt".
        /// </summary>
        public string StatusTimeFormat
        {
            get => (string)GetValue(StatusTimeFormatProperty);
            set => SetValue(StatusTimeFormatProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="StatusTimeFormat"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusTimeFormatProperty = DependencyProperty.Register
            (
                nameof(StatusTimeFormat), typeof(string), typeof(CameraControl), new PropertyMetadata("yyyy-MMM-dd hh:mm:ss tt")
            );

        /// <summary>
        /// Gets the status time text
        /// </summary>
        public string StatusTimeText
        {
            get => (string)GetValue(StatusTimeTextProperty);
            private set => SetValue(StatusTimeTextPropertyKey, value);
        }

        private static readonly DependencyPropertyKey StatusTimeTextPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(StatusTimeText), typeof(string), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="StatusTimeText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusTimeTextProperty = StatusTimeTextPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the frame count.
        /// </summary>
        public int FrameCount
        {
            get => (int)GetValue(FrameCountProperty);
            private set => SetValue(FrameCountPropertyKey, value);
        }

        private static readonly DependencyPropertyKey FrameCountPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(FrameCount), typeof(int), typeof(CameraControl), new PropertyMetadata(0)
            );

        /// <summary>
        /// Identifies the <see cref="FrameCount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FrameCountProperty =  FrameCountPropertyKey.DependencyProperty;
        #endregion

        /************************************************************************/

        #region Feed Error
        /// <summary>
        /// Gets the error text.
        /// </summary>
        public string ErrorText
        {
            get => (string)GetValue(ErrorTextProperty);
            private set => SetValue(ErrorTextPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ErrorTextPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(ErrorText), typeof(string), typeof(CameraControl), new PropertyMetadata(null, OnErrorTextChanged)
            );

        /// <summary>
        /// Identifies the <see cref="ErrorText"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ErrorTextProperty = ErrorTextPropertyKey.DependencyProperty;

        private static void OnErrorTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CameraControl control)
            {
                if (control.errorControl != null)
                {
                    control.errorControl.Visibility = !string.IsNullOrEmpty(control.ErrorText) ? Visibility.Visible : Visibility.Collapsed;
                    if (control.errorControl.Visibility == Visibility.Visible)
                    {
                        control.errorControl.Opacity = 1.0;
                        control.HideErrorControl.Begin();
                    }
                }
                else
                {
                    control.pendingError = control.ErrorText;
                }
            }
        }

        /// <summary>
        /// Gets the width used for the error banner
        /// </summary>
        public double ErrorWidth
        {
            get => (double)GetValue(ErrorWidthProperty);
            private set => SetValue(ErrorWidthPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ErrorWidthPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(ErrorWidth), typeof(double), typeof(CameraControl), new PropertyMetadata(double.NaN)
            );

        /// <summary>
        /// Identifies the <see cref="ErrorWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ErrorWidthProperty = ErrorWidthPropertyKey.DependencyProperty;
        #endregion

        /************************************************************************/

        #region Controls
        /// <summary>
        /// Gets or sets the background for the camera controls.
        /// </summary>
        public System.Windows.Media.Brush ControlsBackground
        {
            get => (System.Windows.Media.Brush)GetValue(ControlsBackgroundProperty);
            set => SetValue(ControlsBackgroundProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="ControlsBackground"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ControlsBackgroundProperty = DependencyProperty.Register
            (
                nameof(ControlsBackground), typeof(System.Windows.Media.Brush), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Gets the command to go full screen.
        /// </summary>
        public ICommand FullScreenCommand
        {
            get => (ICommand)GetValue(FullScreenCommandProperty);
            private set => SetValue(FullScreenCommandPropertyKey, value);
        }

        private static readonly DependencyPropertyKey FullScreenCommandPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(FullScreenCommand), typeof(ICommand), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="FullScreenCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FullScreenCommandProperty = FullScreenCommandPropertyKey.DependencyProperty;
        #endregion

        /************************************************************************/

        #region Public methods
        /// <summary>
        /// Called when the control template is applied
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (imageControl != null)
            {
                imageControl.MouseLeftButtonDown -= ImageControlMouseLeftButtonDown;
                imageControl.MouseMove -= ImageControlMouseMove;
                imageControl.MouseLeave -= ImageControlMouseLeave;
                imageControl.MouseUp -= ImageControlMouseUp;
                imageControl.MouseWheel -= ImageControlMouseWheel;
                imageControl.SizeChanged -= ImageControlSizeChanged;
            }

            imageControl = GetTemplateChild(PartImage) as Controls.Image;
            errorControl = GetTemplateChild(PartError) as Controls.Border;
            controllerControl = GetTemplateChild(PartController) as Controls.Border;

            Storyboard.SetTarget(ShowController, controllerControl);
            Storyboard.SetTargetProperty(ShowController, new PropertyPath(HeightProperty));

            Storyboard.SetTarget(HideController, controllerControl);
            Storyboard.SetTargetProperty(HideController, new PropertyPath(HeightProperty));

            Storyboard.SetTarget(HideErrorControl, errorControl);
            Storyboard.SetTargetProperty(HideErrorControl, new PropertyPath(OpacityProperty));

            if (imageControl != null)
            {
                imageControl.MouseLeftButtonDown += ImageControlMouseLeftButtonDown;
                imageControl.MouseMove += ImageControlMouseMove;
                imageControl.MouseLeave += ImageControlMouseLeave;
                imageControl.MouseUp += ImageControlMouseUp;
                imageControl.MouseWheel += ImageControlMouseWheel;
                imageControl.SizeChanged += ImageControlSizeChanged;
                InitializeImageTransforms();
            }

            InitializeIsMouseCameraMotionAvailable();
            /*
             * A camera can attempt to initialize before OnAppyTemplate(), which gets
             * the error control from the template. If an error occurs during camera
             * initialization (no plugin, etc.) and the error control is not ready
             * at the time the error message is set, we save the pending error until
             * it can be displayed here. 
             */ 
            if (!string.IsNullOrEmpty(pendingError))
            {
                ErrorText = null;
                ErrorText = pendingError;
                pendingError = null;
            }
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (controllerStatus == ControllerStatus.Showing || controllerStatus == ControllerStatus.Hiding)
            {
                return;
            }

            Point point = e.GetPosition(this);

            if (point.Y > ActualHeight - ControllerHeight - ControllerHeightCushion && controllerStatus == ControllerStatus.Hidden)
            {
                controllerStatus = ControllerStatus.Showing;
                ShowController.Begin();
                return;
            }

            if (point.Y < ActualHeight - ControllerHeight * 2.0 && controllerStatus == ControllerStatus.Shown)
            {
                controllerStatus = ControllerStatus.Hiding;
                HideController.Begin();
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (controllerStatus == ControllerStatus.Showing || controllerStatus == ControllerStatus.Shown)
            {
                controllerStatus = ControllerStatus.Hiding;
                HideController.Begin();
            }
        }

        protected override Geometry GetLayoutClip(System.Windows.Size layoutSlotSize)
        {
            /* base.GetLayoutClip(..) returns null.
             * This allows the image (when scale transformed) to cover the title bar.
             * By returning a geometry that matches the layout slot, the image behaves
             * as expected, stays inside the client area. On a standard window, this isn't
             * neccessary, but we're using a custom window.
             */
            return new RectangleGeometry
                (
                    new Rect(0, 0, layoutSlotSize.Width, layoutSlotSize.Height),
                    0, 0
                );
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            if (sizeInfo.WidthChanged) SetWidths(sizeInfo.NewSize.Width);
        }
        #endregion

        /************************************************************************/

        #region Private methods (image control event handlers)
        private void ImageControlMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = true;
            mouseDownPoint = e.GetPosition(imageControl);
        }

        private void ImageControlMouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouseDown) return;
            if (isLeftControlKeyDown && !IsMouseCameraMotionAvailable) return;

            System.Windows.Point point = e.GetPosition(imageControl);
            double deltaX = point.X - mouseDownPoint.X;
            double deltaY = point.Y - mouseDownPoint.Y;

            if (!isLeftControlKeyDown)
            {
                PanImage(deltaX, deltaY);
            }
            else if (IsMouseCameraMotionAvailable && !isCameraMotionStarted)
            {
                MoveCamera(deltaX, deltaY);
            }
        }

        private void MoveCamera(double deltaX, double deltaY)
        {
            double absDeltaX = Math.Abs(deltaX);
            double absDeltaY = Math.Abs(deltaY);

            if (absDeltaX > CameraMotionMovementThreshold || absDeltaY > CameraMotionMovementThreshold)
            {
                if (absDeltaX > absDeltaY)
                {
                    // horz
                    MotionCommand?.Execute(deltaX > 0 ? CameraMotion.Left : CameraMotion.Right);
                }
                else
                {
                    // vert
                    MotionCommand?.Execute(deltaY > 0 ? CameraMotion.Up : CameraMotion.Down);
                }
                isCameraMotionStarted = true;
            }
        }

        private void PanImage(double deltaX, double deltaY)
        {
            if (GetImageTranslateTransform() is TranslateTransform translate)
            {
                translate.X += deltaX;
                translate.Y += deltaY;
            }
        }

        private void ImageControlMouseLeave(object sender, MouseEventArgs e)
        {
            isMouseDown = false;
            if (isCameraMotionStarted)
            {
                MotionCommand?.Execute(CameraMotion.Stop);
                isCameraMotionStarted = false;
            }
        }

        private void ImageControlMouseUp(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);
            isMouseDown = false;

            if (isCameraMotionStarted)
            {
                MotionCommand?.Execute(CameraMotion.Stop);
                isCameraMotionStarted = false;
            }
        }

        private void ImageControlMouseWheel(object sender, MouseWheelEventArgs e)
        {
            ZoomImage(e.Delta);
        }

        private void ImageControlSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.WidthChanged) SetWidths(e.NewSize.Width);
        }
        #endregion

        /************************************************************************/

        #region Private method (control hooks)
        private void CameraControlLoaded(object sender, RoutedEventArgs e)
        {
            hostWindow = Window.GetWindow(this);
            if (hostWindow != null)
            {
                hostWindow.KeyDown += CameraControlKeyDown;
                hostWindow.KeyUp += CameraControlKeyUp;
            }
        }

        private void CameraControlUnloaded(object sender, RoutedEventArgs e)
        {
            if (hostWindow != null)
            {
                hostWindow.KeyDown -= CameraControlKeyDown;
                hostWindow.KeyUp -= CameraControlKeyUp;
            }
        }

        private void CameraControlKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl) isLeftControlKeyDown = true;
            if (e.Key == Key.Escape)
            {
                ResetImageTransforms();
                ReturnFromFullScreen();
            }
        }

        private void CameraControlKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl) isLeftControlKeyDown = false;
        }
        #endregion

        /************************************************************************/

        #region Private methods
        /// <summary>
        /// Initializes the camera when <see cref="Camera"/> changes. Only called if Camera is non-null
        /// </summary>
        private void InitializeCamera()
        {
            try
            {
                CameraName = Camera.Name;

                if (Camera.PluginId == PluginTable.Defs.Values.NullPluginId)
                {
                    throw new InvalidPluginException("The selected camera does not have an associated plugin.");
                }

                ICameraPluginCreator creator = CompositionManager.Instance.GetCameraPluginCreator(Camera.PluginGuid);
                if (creator == null)
                {
                    throw new InvalidPluginException($"The plugin configured for the camera [{Camera.PluginName}, Id: {Camera.PluginGuid} cannot be found.");
                }

                if (Plugin != null)
                {
                    Plugin.VideoFrameReceived -= PluginVideoFrameReceived;
                    Plugin.PluginException -= PluginPluginException;
                }

                Plugin = creator.Create(new ConnectionParameters(Camera.IpAddress, Camera.Port, Camera.UserId, Camera.Password));
                IsPluginMotion = Plugin is ICameraMotion;
                if (IsPluginMotion)
                {
                    MotionCommand = RelayCommand.Create(RunMotionCommand);
                }

                /* establish status alignment (alignment top is default, don't need to check) */
                if (Camera.Flags.HasFlag(CameraFlags.StatusBottom))
                {
                    StatusVerticalAlignment = VerticalAlignment.Bottom;
                }

                if (Camera.Flags.HasFlag(CameraFlags.StatusOff))
                {
                    // center causes trigger to hide status banner
                    StatusVerticalAlignment = VerticalAlignment.Center;
                }

                StatusAlignmentCommand = RelayCommand.Create(RunStatusAlignmentCommand);
                FullScreenCommand = RelayCommand.Create(RunFullScreenCommand);

                Plugin.VideoFrameReceived += PluginVideoFrameReceived;
                Plugin.PluginException += PluginPluginException;
                InitializeIsMouseCameraMotionAvailable();
                HavePlugin = true;
            }
            catch (Exception ex)
            {
                ErrorText = ex.Message;
            }
        }

        private void InitializeIsMouseCameraMotionAvailable()
        {
            IsMouseCameraMotionAvailable =
                IsMouseCameraMotion && IsPluginMotion && imageControl != null;
        }

        private void StartStopVideo()
        {
            if (IsVideoRunning)
            {
                Plugin?.StartVideo();
            }
            else
            {
                Plugin?.StopVideoAsync();
            }
        }

        private void PluginPluginException(object sender, PluginException e)
        {
            Dispatcher.Invoke(() => ErrorText = e.Message, DispatcherPriority.Send);
        }

        private void PluginVideoFrameReceived(object sender, RawVideoFrame frame)
        {
            Dispatcher.Invoke(() => ProcessVideoFrame(frame), DispatcherPriority.Send);
        }

        private void ProcessVideoFrame(RawVideoFrame frame)
        {
            IDecodedVideoFrame decodedFrame = videoDecoder.Decode(frame);
            if (decodedFrame == null) return;
            FrameCount++;

            if (writeable == null)
            {
                InitializeWritableBitmap(decodedFrame.FrameParms);
                dirtyRect = new Int32Rect(0, 0, decodedFrame.FrameParms.Width, decodedFrame.FrameParms.Height);
                ImageMaxWidth = imageScaledWidth = decodedFrame.FrameParms.Width;
                SetWidths(ImageMaxWidth);
            }

            writeable.Lock();

            try
            {
                decodedFrame.TransformTo(writeable.BackBuffer, writeable.BackBufferStride, transformParameters);
                writeable.AddDirtyRect(dirtyRect);
            }
            finally
            {
                writeable.Unlock();
            }
        }

        private void InitializeWritableBitmap(DecodedVideoFrameParameters parms)
        {
            /* Pixel format for transform parms and for writeable bitmap must match.
             * Only PixelFormats.Pbgra32, Bgr24, and Gray8 are supported.
            */
            PixelFormat pixelFormat = PixelFormats.Pbgra32;
            transformParameters = new TransformParameters(new System.Drawing.Size(parms.Width, parms.Height), pixelFormat);

            // TODO ScreenInfo.DpiX, ScreenInfo.DpiY
            writeable = new WriteableBitmap
                (
                    parms.Width, parms.Height, 96.0, 96.0, pixelFormat, null
                );

            RenderOptions.SetBitmapScalingMode(writeable, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(writeable, EdgeMode.Aliased);

            ImageSource = writeable;
        }

        private void RunMotionCommand(object parm)
        {
            if (parm is CameraMotion motion)
            {
                (Plugin as ICameraMotion)?.Move(motion);
            }
        }

        private void RunStatusAlignmentCommand(object parm)
        {
            if (parm is VerticalAlignment alignment)
            {
                switch (alignment)
                {
                    case VerticalAlignment.Top:
                        Camera.ChangeFlags(CameraFlags.StatusTop, CameraFlags.StatusOff | CameraFlags.StatusBottom);
                        break;
                    case VerticalAlignment.Bottom:
                        Camera.ChangeFlags(CameraFlags.StatusBottom, CameraFlags.StatusOff | CameraFlags.StatusTop);
                        break;
                    default:
                        Camera.ChangeFlags(CameraFlags.StatusOff, CameraFlags.StatusTop | CameraFlags.StatusBottom);
                        break;
                }
                StatusVerticalAlignment = alignment;
            }
        }

        private void RunFullScreenCommand(object parm)
        {
            // placeholder
        }

        private void ReturnFromFullScreen()
        {
            // placeholder
        }

        private void ZoomImage(int factor)
        {
            if (GetImageScaleTransform() is ScaleTransform scale)
            {
                double value = scale.ScaleX;
                if (factor > 0)
                {
                    value = Math.Round(Math.Min(value + 0.1, 5.0), 1);
                }
                else if (factor < 0)
                {
                    value = Math.Round(Math.Max(value - 0.1, 0.4), 1);
                }
                else
                {
                    value = 1.0;
                }

                scale.ScaleX = scale.ScaleY = value;
                imageScaledWidth = imageControl.ActualWidth * value;
                SetWidths(imageControl.ActualWidth * value);
            }
        }

        private ScaleTransform GetImageScaleTransform()
        {
            if (imageControl?.RenderTransform is TransformGroup group && group.Children.Count > 1)
            {
                return group.Children[1] as ScaleTransform;
            }
            return null;
        }

        private TranslateTransform GetImageTranslateTransform()
        {
            if (imageControl?.RenderTransform is TransformGroup group && group.Children.Count > 0)
            {
                return group.Children[0] as TranslateTransform;
            }
            return null;
        }

        private double GetImageScaledWidth()
        {
            if (GetImageScaleTransform() is ScaleTransform scale)
            {
                return imageControl.ActualWidth * scale.ScaleX;
            }
            return 0.0;
        }

        private void SetWidths(double proposedWidth)
        {
            imageScaledWidth = GetImageScaledWidth();
            StatusWidth = Math.Min(Math.Min(Math.Max(proposedWidth, imageScaledWidth), imageScaledWidth), ActualWidth);
            /* ErrorWidth default = double.NaN. Do not set to zero or initial failure to connect won't show */
            if (StatusWidth > 0) ErrorWidth = StatusWidth;
        }

        // method called from OnApplyTemplate() only is imageControl != null
        private void InitializeImageTransforms()
        {
            TransformGroup group = new TransformGroup();
            group.Children.Add(new TranslateTransform());
            group.Children.Add(new ScaleTransform(1.0, 1.0));
            imageControl.RenderTransform = group;
        }

        private void ResetImageTransforms()
        {
            if (GetImageTranslateTransform() is TranslateTransform translate)
            {
                translate.X = translate.Y = 0;
            }
            if (GetImageScaleTransform() is ScaleTransform scale)
            {
                scale.ScaleX = scale.ScaleY = 1.0;
            }
            if (imageControl != null)
            {
                SetWidths(imageControl.ActualWidth);
            }
        }

        private void InitializeStoryBoards()
        {
            ShowController = new Storyboard() { FillBehavior = FillBehavior.HoldEnd };
            ShowController.Children.Add(new DoubleAnimation()
            {
                To = ControllerHeight,
                Duration = TimeSpan.FromMilliseconds(200),
            });

            ShowController.Completed += (s, e) => controllerStatus = ControllerStatus.Shown;

            HideController = new Storyboard() { FillBehavior = FillBehavior.HoldEnd };
            HideController.Children.Add(new DoubleAnimation()
            {
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(150),
            });
            HideController.Completed += (s, e) => controllerStatus = ControllerStatus.Hidden;

            HideErrorControl = new Storyboard();

            DoubleAnimationUsingKeyFrames keyFrames = new DoubleAnimationUsingKeyFrames()
            {
                Duration = TimeSpan.FromMilliseconds(6000)
            };

            keyFrames.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = 1.0, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(0)) });
            keyFrames.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = 1.0, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(5500)) });
            keyFrames.KeyFrames.Add(new LinearDoubleKeyFrame() { Value = 0.0, KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(6000)) });
            HideErrorControl.Children.Add(keyFrames);
            HideErrorControl.Completed += (s, e) => ErrorText = null;
        }
        #endregion
    }
}