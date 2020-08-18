## Camera Eye

**Camera Eye** is a .Net Core 3.1 WPF Windows application for viewing and manipulating multiple IP cameras. It uses a plugin framework to support various cameras.

### Features
- Change the number of slots on the video wall to suit your need.
- Drag a camera from the camera list and drop it into the slot you want.
- Supports RTSP and MJPEG.
- Pan and tilt the camera*.
- Set, clear, and go to preset positions*.
- Pan and zoom the video image digitally.
- Flip and mirror the camera output*.
- Change the video brightness, contrast, hue, and saturation*.
- Establish a banner over the video (top, bottom, or off) that displays camera name, current time, and frame count.
- Everything saved for next run. Window size, position, slots, which cameras are where, etc.

For features marked with an asterick to be enabled, both the camera and its associated plugin need to support them.
A plugin implements one required interface and zero or more optional interfaces, depending on its target camera
and its intended use.

### Projects
The solution consists of several projects:

1. Camera.App - This is the main application and user interface.
2. Restless.App.Database - This project provides database support. It uses Sqlite to store information about the plugins, the configuration of cameras, and
application settings.
3. Restless.Camera.Contracts - This projects provides interfaces that a camera plugin must implement (some interfaces are optional, depending on the functionality
the plugin provides), and classes that are used by the plugins to perform their operations.
4. Restless.Plugin.Framework - This project provides abstract classes that implement parts of the required interfaces that a camera plugin uses. It's not necessary that a plugin
derive from these classes, but it can be helpful as they provide some common functionality such as an Http client and Rtsp handling.
5. Restless.Plugin.Foscam - A plugin that handles older Foscam SD cameras. It implements **ICameraMotion** to provide the ability to pan and tilt the camera; 
**ICameraPreset** to set and go to preset camera positions; and other interfaces to provide access to other camera settings.
6. Restless.Plugin.Axis - A plugin that handles Axis cameras. It (currently) provides only the video stream, no ability to move the camera.
7. Restless.Plugin.Amcrest - A plugin that handles newer Amcrest cameras. It implements **ICameraMotion** to provide the ability to pan and tilt the camera; 
**ICameraPreset** to provide the ability to set, clear, and go to preset camera positions; and other interfaces to provide access to other camera settings.

### How to Build

1. Download / clone the repository
2. Make sure the environment variable **NUGET_PACKAGES** is established and pointing to the global directory where packages are stored. See [this Microsoft article](https://docs.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-environment-variables) for more information. The post build process uses this environment var to copy files to the output directory.
3. Applications uses FFMPEG dlls. You need to download them (they're not included here because I'm not sure about licensing / distribution) and place them into the **Ffmpeg** directory where they will then get copied into the build. More info in the [readme file](https://github.com/victor-david/camera-eye/tree/master/src/Camera.App/Ffmpeg) of the **Ffmpeg** directory of this repository.
4. Load the project in Visual Studio 2019 and build.
