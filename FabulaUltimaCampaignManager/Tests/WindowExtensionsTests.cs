using GdUnit4;
using static GdUnit4.Assertions;

namespace FabulaUltimaGMTool.Tests
{
    [TestSuite]
    public class WindowExtensionsTests
    {
        // macOS windows are point-sized: platform factor is always 1 regardless of retina scale or dpi
        [TestCase("macOS", 254, 2.0f, 1.0f, 1.0f)]
        [TestCase("macOS", 96, 1.0f, 1.0f, 1.0f)]
        // Windows: dpi/96 is the OS scale
        [TestCase("Windows", 96, 1.0f, 1.0f, 1.0f)]
        [TestCase("Windows", 120, 1.0f, 1.0f, 1.25f)]
        [TestCase("Windows", 144, 1.0f, 1.0f, 1.5f)]
        [TestCase("Windows", 192, 1.0f, 1.0f, 2.0f)]
        // X11 reports dpi; Wayland reports a compositor scale
        [TestCase("X11", 96, 1.0f, 1.0f, 1.0f)]
        [TestCase("X11", 144, 1.0f, 1.0f, 1.5f)]
        [TestCase("Wayland", 96, 2.0f, 1.0f, 2.0f)]
        // headless (e.g. --import) must not scale
        [TestCase("headless", 72, 1.0f, 1.0f, 1.0f)]
        // never shrink below 1, cap at 3, snap to quarter steps
        [TestCase("Windows", 48, 1.0f, 1.0f, 1.0f)]
        [TestCase("Windows", 960, 1.0f, 1.0f, 3.0f)]
        [TestCase("Windows", 134, 1.0f, 1.0f, 1.5f)]
        // the app's own UI Scale setting multiplies on top, on every platform
        [TestCase("macOS", 254, 2.0f, 1.5f, 1.5f)]
        [TestCase("macOS", 96, 1.0f, 1.25f, 1.25f)]
        [TestCase("Windows", 144, 1.0f, 1.25f, 2.0f)]
        [TestCase("Windows", 192, 1.0f, 2.0f, 3.0f)]
        // configs from before the setting existed load as 0: treated as 1
        [TestCase("macOS", 96, 1.0f, 0.0f, 1.0f)]
        public void ComputeDisplayScale_MatchesPlatformRules(string displayServer, int dpi, float reportedScale, float userScale, float expected)
        {
            AssertFloat(WindowExtensions.ComputeDisplayScale(displayServer, dpi, reportedScale, userScale)).IsEqualApprox(expected, 0.001f);
        }
    }
}
