# FFMPEG

These folders need to contain the FFMPEG Windows binaries. Only .dlls, not the .exes

Application has been tested with FFMPEG version:4.3.1. The .dlls are not part of the
respository, but you can download them at:

https://ffmpeg.zeranoe.com/builds/

Download the shared versions (both x86 and x64), and extract the .dlls (don't need the .exes)
into the corresponding x86 / x64 directory.

**libffmpeghelper.dll** - This file *is* included in the respository. It's not part of FFMPEG,
but comes from:

https://github.com/BogdanovKirill/RtspClientSharp/tree/master/Examples/libffmpeghelper

If you prefer, you can build the **RtspClientSharp** project and grab the **libffmpeghelper.dll**
from the build.
