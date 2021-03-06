# FFMPEG

These folders need to contain the FFMPEG Windows binaries. Only .dlls, not the .exes

Application has been tested with FFMPEG version:4.3.1. The .dlls are not part of the
respository, but you can download them at:

https://ffmpeg.zeranoe.com/builds/

Download the shared versions (both x86 and x64), and extract the .dlls (don't need the .exes)
into the corresponding x86 / x64 directory. There's a total of eight .dlls from FFMPEG, plus:

**UPDATE (06-MAR-2021):** The web site listed above is no longer in operation. You can find the
Windows .dlls at the following location:

https://github.com/BtbN/FFmpeg-Builds/releases

Download a shared version (for instance, ffmpeg-XXXXXX-win64-gpl-shared.zip) and extract
the .dlls. Note that these builds do **not** contain 32 bit versions. If you want to compile CameraEye
in 32 bit mode, you'll need to build the 32 bit FFMPEG .dlls yourself. Build instructions are in the BtBn
repository. Otherwise, just stick with CameraEye in a 64 bit build and you'll be good to go.

**libffmpeghelper.dll** - This file *is* included in the respository. It's not part of FFMPEG,
but comes from:

https://github.com/BogdanovKirill/RtspClientSharp/tree/master/Examples/libffmpeghelper

If you prefer, you can build the **RtspClientSharp** project and grab the **libffmpeghelper.dll**
from the build.
