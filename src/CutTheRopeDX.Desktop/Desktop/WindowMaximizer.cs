using System;
using System.Runtime.InteropServices;

namespace CutTheRopeDX.Desktop
{
    /// <summary>
    /// Maximizes the game window through SDL2, which MonoGame's DesktopGL backend uses for windowing.
    /// Best-effort: any failure (missing library, different backend) leaves the window as it was.
    /// </summary>
    internal static class WindowMaximizer
    {
        private static readonly string[] SdlLibraryNames =
        [
            "SDL2", "SDL2.dll", "libSDL2-2.0.so.0", "libSDL2-2.0.0.dylib", "libSDL2.dylib",
        ];

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void SdlMaximizeWindow(IntPtr window);

        public static bool TryMaximize(IntPtr windowHandle)
        {
            if (windowHandle == IntPtr.Zero)
            {
                return false;
            }

            foreach (string name in SdlLibraryNames)
            {
                try
                {
                    if (!NativeLibrary.TryLoad(name, out IntPtr lib)
                        || !NativeLibrary.TryGetExport(lib, "SDL_MaximizeWindow", out IntPtr fn))
                    {
                        continue;
                    }

                    Marshal.GetDelegateForFunctionPointer<SdlMaximizeWindow>(fn)(windowHandle);
                    return true;
                }
                catch (Exception)
                {
                    // Try the next candidate name.
                }
            }

            return false;
        }
    }
}