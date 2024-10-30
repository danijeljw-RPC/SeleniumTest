using System;

namespace SeleniumTest.Configuration
{
    public static class PlatformHelper
    {
        public static string GetPlatform()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return Environment.Is64BitOperatingSystem ? "win64" : "win32";
            if (Environment.OSVersion.Platform == PlatformID.Unix)
                return "linux64";
            if (Environment.OSVersion.Platform == PlatformID.MacOSX)
                return Environment.Is64BitProcess ? "mac-x64" : "mac-arm64";

            throw new NotSupportedException("Unsupported platform.");
        }
    }
}


