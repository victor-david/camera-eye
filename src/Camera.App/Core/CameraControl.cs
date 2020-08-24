using Restless.App.Database.Core;
using Restless.App.Database.Tables;
using Restless.Camera.Contracts;
using Restless.Camera.Contracts.RawFrames.Video;
using Restless.Tools.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [TemplatePart(Name = PartStatus, Type = typeof(Controls.Border))]
    [TemplatePart(Name = PartError, Type = typeof(Controls.Border))]
    [TemplatePart(Name = PartController, Type = typeof(Controls.Border))]
    public class CameraControl : Controls.Control
    {
        #region Private
        private const string PartImage = "PART_Image";
        private const string PartStatus = "PART_Status";
        private const string PartError = "PART_Error";
        private const string PartController = "PART_Controller";

        private Controls.Image imageControl;
        private Controls.Border statusControl;
        private Controls.Border errorControl;
        private Controls.Border controllerControl;

        /* height in xaml is zero, gets animated up when mouse within activation zone */
        private const double ControllerHeight = 75.0;
        private const double ControllerActivationY = ControllerHeight + 10.0;
        private double controllerActivationX;
        private bool isControllerEnabled;

        private double imageScaledWidth;
        private double imageScaledHeight;
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
        private readonly TransformGroup transformGroup;
        private TranslateTransform MoveTransform => transformGroup.Children[0] as TranslateTransform;
        private ScaleTransform ZoomTransform => transformGroup.Children[1] as ScaleTransform;
        private ScaleTransform FlipTransform => transformGroup.Children[2] as ScaleTransform;
        private readonly TranslateTransform BannerTranslate;
        
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
            transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform()); 
            transformGroup.Children.Add(new ScaleTransform(1.0, 1.0));
            transformGroup.Children.Add(new ScaleTransform(1.0, 1.0));

            BannerTranslate = new TranslateTransform();
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

        #region Plugin (general / motion)
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
            (d as CameraControl)?.StartStopVideo();
        }
        #endregion

        /************************************************************************/

        #region Plugin (preset)
        /// <summary>
        /// Gets a boolean value that specifies whether the plugin supports ICameraPreset
        /// </summary>
        public bool IsPluginPreset
        {
            get => (bool)GetValue(IsPluginPresetProperty);
            private set => SetValue(IsPluginPresetPropertyKey, value);
        }

        private static readonly DependencyPropertyKey IsPluginPresetPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(IsPluginPreset), typeof(bool), typeof(CameraControl), new PropertyMetadata(false)
            );

        /// <summary>
        /// Identifies the <see cref="IsPluginPreset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsPluginPresetProperty = IsPluginPresetPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets an IEnumerable of of int for the presets
        /// </summary>
        public IEnumerable<int> PresetList
        {
            get => (IEnumerable<int>)GetValue(PresetListProperty);
            private set => SetValue(PresetListPropertyKey, value);
        }

        private static readonly DependencyPropertyKey PresetListPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(PresetList), typeof(IEnumerable<int>), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="PresetList"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PresetListProperty = PresetListPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the selected preset
        /// </summary>
        public int SelectedPreset
        {
            get => (int)GetValue(SelectedPresetProperty);
            set => SetValue(SelectedPresetProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="SelectedPreset"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectedPresetProperty = DependencyProperty.Register
            (
                nameof(SelectedPreset), typeof(int), typeof(CameraControl), new FrameworkPropertyMetadata()
                {
                    DefaultValue = 1,
                    BindsTwoWayByDefault = true,
                }
            );

        /// <summary>
        /// Gets the command to go to a preset.
        /// </summary>
        public ICommand GoPresetCommand
        {
            get => (ICommand)GetValue(GoPresetCommandProperty);
            private set => SetValue(GoPresetCommandPropertyKey, value);
        }

        private static readonly DependencyPropertyKey GoPresetCommandPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(GoPresetCommand), typeof(ICommand), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="GoPresetCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GoPresetCommandProperty = GoPresetCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the command to set a preset.
        /// </summary>
        public ICommand SetPresetCommand
        {
            get => (ICommand)GetValue(SetPresetCommandProperty);
            private set => SetValue(SetPresetCommandPropertyKey, value);
        }

        private static readonly DependencyPropertyKey SetPresetCommandPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(SetPresetCommand), typeof(ICommand), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="SetPresetCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SetPresetCommandProperty = SetPresetCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the command to clear a preset.
        /// </summary>
        public ICommand ClearPresetCommand
        {
            get => (ICommand)GetValue(ClearPresetCommandProperty);
            private set => SetValue(ClearPresetCommandPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ClearPresetCommandPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(ClearPresetCommand), typeof(ICommand), typeof(CameraControl), new PropertyMetadata(null)
            );

        /// <summary>
        /// Identifies the <see cref="ClearPresetCommand"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ClearPresetCommandProperty = ClearPresetCommandPropertyKey.DependencyProperty;
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
        /// Gets the image max height.
        /// </summary>
        public double ImageMaxHeight
        {
            get => (double)GetValue(ImageMaxHeightProperty);
            private set => SetValue(ImageMaxHeightPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ImageMaxHeightPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(ImageMaxHeight), typeof(double), typeof(CameraControl), new PropertyMetadata(0.0)
            );

        /// <summary>
        /// Identifies the <see cref="ImageMaxHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ImageMaxHeightProperty = ImageMaxHeightPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets a boolean value that determines if the video image is flipped vertically.
        /// </summary>
        public bool IsFlipped
        {
            get => (bool)GetValue(IsFlippedProperty);
            set => SetValue(IsFlippedProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="IsFlipped"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsFlippedProperty = DependencyProperty.Register
            (
                nameof(IsFlipped), typeof(bool), typeof(CameraControl), new PropertyMetadata(false, OnIsOrientationChanged)
            );

        /// <summary>
        /// Gets or sets a boolean value that determines if the video image is mirrored horizontally.
        /// </summary>
        public bool IsMirrored
        {
            get => (bool)GetValue(IsMirroredProperty);
            set => SetValue(IsMirroredProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="IsMirrored"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsMirroredProperty = DependencyProperty.Register
            (
                nameof(IsMirrored), typeof(bool), typeof(CameraControl), new PropertyMetadata(false, OnIsOrientationChanged)
            );

        private static void OnIsOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CameraControl)?.SetOrientation();
        }
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
        /// Gets the height of the status banner.
        /// </summary>
        public double StatusHeight
        {
            get => (double)GetValue(StatusHeightProperty);
            private set => SetValue(StatusHeightPropertyKey, value);
        }

        private static readonly DependencyPropertyKey StatusHeightPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(StatusHeight), typeof(double), typeof(CameraControl), new PropertyMetadata(24.0)
            );

        /// <summary>
        /// Identifies the <see cref="StatusHeight"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusHeightProperty = StatusHeightPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets or sets the status banner placement.
        /// </summary>
        public StatusPlacement StatusPlacement
        {
            get => (StatusPlacement)GetValue(StatusPlacementProperty);
            set => SetValue(StatusPlacementProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="StatusPlacement"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty StatusPlacementProperty = DependencyProperty.Register
            (
                nameof(StatusPlacement), typeof(StatusPlacement), typeof(CameraControl), new PropertyMetadata(StatusPlacement.None, OnStatusPlacementChanged)
            );

        private static void OnStatusPlacementChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as CameraControl)?.SyncStatusBannerToSize();
        }

        /// <summary>
        /// Gets a boolean value that determines if the camera name displays in the status banner.
        /// </summary>
        public bool IsStatusCameraName
        {
            get => (bool)GetValue(IsStatusCameraNameProperty);
            private set => SetValue(IsStatusCameraNamePropertyKey, value);
        }

        private static readonly DependencyPropertyKey IsStatusCameraNamePropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(IsStatusCameraName), typeof(bool), typeof(CameraControl), new PropertyMetadata(true)
            );

        /// <summary>
        /// Identifies the <see cref="IsStatusCameraName"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsStatusCameraNameProperty = IsStatusCameraNamePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a boolean value that determines if the current date/time displays in the status banner.
        /// </summary>
        public bool IsStatusDateTime
        {
            get => (bool)GetValue(IsStatusDateTimeProperty);
            private set => SetValue(IsStatusDateTimePropertyKey, value);
        }

        private static readonly DependencyPropertyKey IsStatusDateTimePropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(IsStatusDateTime), typeof(bool), typeof(CameraControl), new PropertyMetadata(true)
            );

        /// <summary>
        /// Identifies the <see cref="IsStatusDateTime"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsStatusDateTimeProperty = IsStatusDateTimePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a boolean value that determines if the frame count displays in the status banner.
        /// </summary>
        public bool IsStatusFrameCount
        {
            get => (bool)GetValue(IsStatusFrameCountProperty);
            private set => SetValue(IsStatusFrameCountPropertyKey, value);
        }

        private static readonly DependencyPropertyKey IsStatusFrameCountPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(IsStatusFrameCount), typeof(bool), typeof(CameraControl), new PropertyMetadata(true)
            );

        /// <summary>
        /// Identifies the <see cref="IsStatusFrameCount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsStatusFrameCountProperty = IsStatusFrameCountPropertyKey.DependencyProperty;

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
        #endregion

        /************************************************************************/

        #region Controls
        /// <summary>
        /// Gets or sets the background for the camera controls.
        /// </summary>
        public Brush ControlsBackground
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

        /// <summary>
        /// Gets the width of the popup controller area.
        /// </summary>
        public double ControllerWidth
        {
            get => (double)GetValue(ControllerWidthProperty);
            private set => SetValue(ControllerWidthPropertyKey, value);
        }

        private static readonly DependencyPropertyKey ControllerWidthPropertyKey = DependencyProperty.RegisterReadOnly
            (
                nameof(ControllerWidth), typeof(double), typeof(CameraControl), new PropertyMetadata(0.0, OnControllerWidthChanged)
            );

        /// <summary>
        /// Identifies the <see cref="ControllerWidth"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ControllerWidthProperty = ControllerWidthPropertyKey.DependencyProperty;

        private static void OnControllerWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CameraControl control)
            {
                control.controllerActivationX = (double)e.NewValue + 10.0;
            }
        }
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
            statusControl = GetTemplateChild(PartStatus) as Controls.Border;
            errorControl = GetTemplateChild(PartError) as Controls.Border;
            controllerControl = GetTemplateChild(PartController) as Controls.Border;

            if (imageControl == null || statusControl == null || errorControl == null || controllerControl == null)
            {
                throw new NotImplementedException("CameraControl template not implemented correctly");
            }

            Storyboard.SetTarget(ShowController, controllerControl);
            Storyboard.SetTargetProperty(ShowController, new PropertyPath(HeightProperty));

            Storyboard.SetTarget(HideController, controllerControl);
            Storyboard.SetTargetProperty(HideController, new PropertyPath(HeightProperty));

            Storyboard.SetTarget(HideErrorControl, errorControl);
            Storyboard.SetTargetProperty(HideErrorControl, new PropertyPath(OpacityProperty));

            imageControl.MouseLeftButtonDown += ImageControlMouseLeftButtonDown;
            imageControl.MouseMove += ImageControlMouseMove;
            imageControl.MouseLeave += ImageControlMouseLeave;
            imageControl.MouseUp += ImageControlMouseUp;
            imageControl.MouseWheel += ImageControlMouseWheel;
            imageControl.SizeChanged += ImageControlSizeChanged;
            imageControl.RenderTransform = transformGroup;

            statusControl.RenderTransform = BannerTranslate;

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
        
        /// <summary>
        /// Updates the status control according to the current flags of <see cref="Camera"/>.
        /// </summary>
        public void UpdateStatusControl()
        {
            if (Camera != null)
            {
                if (Camera.Flags.HasFlag(CameraFlags.StatusTop))
                {
                    StatusPlacement = StatusPlacement.Top;
                }
                else if (Camera.Flags.HasFlag(CameraFlags.StatusBottom))
                {
                    StatusPlacement = StatusPlacement.Bottom;
                }
                else
                {
                    StatusPlacement = StatusPlacement.None;
                }

                IsStatusCameraName = Camera.Flags.HasFlag(CameraFlags.StatusCameraName);
                IsStatusDateTime = Camera.Flags.HasFlag(CameraFlags.StatusDateTime);
                IsStatusFrameCount = Camera.Flags.HasFlag(CameraFlags.StatusFrameCount);

                /* if none of the flags are present, turn off the banner */
                if (!IsStatusCameraName && !IsStatusDateTime && !IsStatusFrameCount)
                {
                    StatusPlacement = StatusPlacement.None;
                }
            }
        }
        #endregion

        /************************************************************************/

        #region Protected methods
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!isControllerEnabled || controllerStatus == ControllerStatus.Showing || controllerStatus == ControllerStatus.Hiding)
            {
                return;
            }

            Point point = e.GetPosition(this);

            if (controllerStatus == ControllerStatus.Hidden && point.Y > ActualHeight - ControllerActivationY &&  point.X < controllerActivationX)
            {
                controllerStatus = ControllerStatus.Showing;
                ShowController.Begin();
                return;
            }

            if (controllerStatus == ControllerStatus.Shown && (point.Y < ActualHeight - ControllerActivationY ||  point.X > controllerActivationX))
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
            SyncStatusBannerToSize();
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

            Point point = e.GetPosition(imageControl);
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
            e.Handled = true;
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
            MoveTransform.X += deltaX;
            MoveTransform.Y += deltaY;
            SyncStatusBannerToSize();
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
            SyncStatusBannerToSize();
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

                /* remove handlers if needed */
                if (Plugin != null)
                {
                    Plugin.VideoFrameReceived -= PluginVideoFrameReceived;
                    Plugin.PluginException -= PluginPluginException;
                }

                /* throws if no plugin or plugin missing */
                Plugin = PluginFactory.Create(Camera);

                ControllerWidth = 0.0;

                IsPluginMotion = Plugin is ICameraMotion;
                if (IsPluginMotion)
                {
                    MotionCommand = RelayCommand.Create(RunMotionCommand);
                    ControllerWidth = 75.0;
                }

                if (Plugin is ICameraPreset preset)
                {
                    IsPluginPreset = true;
                    GoPresetCommand = RelayCommand.Create(RunGoPresetCommand);
                    SetPresetCommand = RelayCommand.Create(RunSetPresetCommand);
                    ClearPresetCommand = RelayCommand.Create(RunClearPresetCommand);
                    ControllerWidth += 258;
                    PresetList = Enumerable.Range(1, Math.Min((int)Camera.MaxPreset, preset.MaxPreset));
                }

                isControllerEnabled = IsPluginMotion || IsPluginPreset;

                IsFlipped = Camera.Flags.HasFlag(CameraFlags.Flip);
                IsMirrored = Camera.Flags.HasFlag(CameraFlags.Mirror);

                UpdateStatusControl();

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
            StatusTimeText = DateTime.Now.ToString(StatusTimeFormat);

            if (writeable == null)
            {
                InitializeWritableBitmap(decodedFrame.FrameParms);
                dirtyRect = new Int32Rect(0, 0, decodedFrame.FrameParms.Width, decodedFrame.FrameParms.Height);
                ImageMaxWidth = imageScaledWidth = decodedFrame.FrameParms.Width;
                ImageMaxHeight = imageScaledHeight = decodedFrame.FrameParms.Height;
                SyncStatusBannerToSize();
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

            Tuple<double, double> dpi = GetDpi();
            writeable = new WriteableBitmap
                (
                    parms.Width, parms.Height, dpi.Item1, dpi.Item2, pixelFormat, null
                );

            RenderOptions.SetBitmapScalingMode(writeable, BitmapScalingMode.NearestNeighbor);
            RenderOptions.SetEdgeMode(writeable, EdgeMode.Aliased);

            ImageSource = writeable;
        }

        /// <summary>
        /// Gets the DPI.
        /// </summary>
        /// <returns>A Tuple. Item1 is X Dpi, Item2 is Y Dpi</returns>
        private Tuple<double, double> GetDpi()
        {
            var source = PresentationSource.FromVisual(this);
            if (source != null)
            {
                Matrix matrix = source.CompositionTarget.TransformToDevice;
                return new Tuple<double, double>(matrix.M11 * 96.0, matrix.M22 * 96.0);
            }
            return new Tuple<double, double>(96.0, 96.0);
        }

        private void RunMotionCommand(object parm)
        {
            if (parm is CameraMotion motion)
            {
                (Plugin as ICameraMotion)?.Move(TranslatedMotion(motion));
            }
        }

        private CameraMotion TranslatedMotion(CameraMotion motion)
        {
            if (motion.IsX() && Camera.Flags.HasFlag(CameraFlags.TranslateX))
            {
                return motion.TranslatedX();
            }

            if (motion.IsY() && Camera.Flags.HasFlag(CameraFlags.TranslateY))
            {
                return motion.TranslatedY();
            }

            return motion;
        }

        private void RunGoPresetCommand(object parm)
        {
            (Plugin as ICameraPreset)?.MoveToPreset(SelectedPreset);
        }

        private void RunSetPresetCommand(object parm)
        {
            (Plugin as ICameraPreset)?.SetPreset(SelectedPreset);
        }

        private void RunClearPresetCommand(object parm)
        {
            (Plugin as ICameraPreset)?.ClearPreset(SelectedPreset);
        }

        private void RunFullScreenCommand(object parm)
        {
            // placeholder
        }

        private void ReturnFromFullScreen()
        {
            // placeholder
        }
        #endregion

        /************************************************************************/

        #region Private methods (transforms)
        private void ZoomImage(int factor)
        {
            double value = ZoomTransform.ScaleX;
            if (factor > 0)
            {
                value = Math.Round(Math.Min(value + 0.1, 5.0), 1);
            }
            else if (factor < 0)
            {
                value = Math.Round(Math.Max(value - 0.1, 0.5), 1);
            }
            else
            {
                value = 1.0;
            }

            ZoomTransform.ScaleX = ZoomTransform.ScaleY = value;

            SyncStatusBannerToSize();
        }

        private void SetOrientation()
        {
            FlipTransform.ScaleX = IsMirrored ? -1.0 : 1.0;
            FlipTransform.ScaleY = IsFlipped ? -1.0 : 1.0;
            SyncStatusBannerToSize();
        }

        private void ResetImageTransforms()
        {
            MoveTransform.X = MoveTransform.Y = 0;
            ZoomTransform.ScaleX = ZoomTransform.ScaleY = 1.0;
            BannerTranslate.X = BannerTranslate.Y = 0;
            SyncStatusBannerToSize();
        }

        private void SyncStatusBannerToSize()
        {
            if (imageControl == null || StatusPlacement == StatusPlacement.None) return;

            imageScaledWidth = imageControl.ActualWidth * ZoomTransform.ScaleX;
            imageScaledHeight = imageControl.ActualHeight * ZoomTransform.ScaleY;

            double xAdjust = (((ActualWidth - imageScaledWidth) / 2.0) + (MoveTransform.X * ZoomTransform.ScaleX * FlipTransform.ScaleX));
            double yAdjust = ((ActualHeight - imageScaledHeight) / 2.0) + (MoveTransform.Y * ZoomTransform.ScaleY * FlipTransform.ScaleY);

            if (StatusPlacement == StatusPlacement.Bottom) yAdjust += imageScaledHeight - StatusHeight;

            BannerTranslate.X = Math.Max(xAdjust, 0);
            BannerTranslate.Y = Math.Min(Math.Max(yAdjust, 0), ActualHeight - StatusHeight);

            double statusWidth = imageScaledWidth + (xAdjust < 0 ? xAdjust : 0.0);
            /* below a certain width, make it disappear */
            if (statusWidth < 150.0) statusWidth = 0.0;
            StatusWidth = statusWidth;
        }
        #endregion

        /************************************************************************/

        #region Private methods (storyboards)
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