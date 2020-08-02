# Camera Eye

Camera Eye is a .NetCore WPF Windows application for viewing and manipulating IP cameras. It uses a plugin framework to support various cameras.

## How to Build

1. Download / clone the repository
2. Make sure the environment variable **NUGET_PACKAGES** is established and pointing to the global directory where packages are stored. See [this Microsoft article](https://docs.microsoft.com/en-us/nuget/reference/cli-reference/cli-ref-environment-variables) for more information. The post build process uses this environment var to copy files to the output directory.
3. Applications uses FFMPEG dlls. You need to download them (they're not included here because I'm not sure about licensing / distribution) and place them into the **Ffmpeg** directory where they will then get copied into the build. More info in the [readme file](https://github.com/victor-david/camera-eye/tree/master/src/Camera.App/Ffmpeg) of the **Ffmpeg** directory of this repository.
4. Load the project in Visual Studio 2019 and build.
