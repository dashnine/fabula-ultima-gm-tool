using FabulaUltimaGMTool;
using Godot;
using System;

public static class WindowExtensions
{
    // pure so tests can table-check it without a display.
    // macOS windows are point-sized (the OS handles retina), so scaling there would double up;
    // Windows sizes are physical pixels, so dpi/96 is the OS scale the app should match.
    // userScale is the app's own UI Scale setting, multiplied on top for every platform
    public static float ComputeDisplayScale(string displayServer, int screenDpi, float reportedScale, float userScale)
    {
        float scale;
        switch (displayServer)
        {
            case "Windows":
            case "X11":
                scale = screenDpi / 96f;
                break;
            case "Wayland":
                scale = reportedScale;
                break;
            default: // macOS, headless
                scale = 1f;
                break;
        }
        if (userScale <= 0) userScale = 1f; // configs from before the setting existed
        return Math.Clamp(Mathf.Round(scale * userScale * 4f) / 4f, 1f, 3f); // quarter steps, never shrink
    }

    public static float GetDisplayScale(this Window window)
    {
        var configuration = window.GetNodeOrNull<UserConfigurationState>("/root/UserConfigurationState");
        return ComputeDisplayScale(
            DisplayServer.GetName(),
            DisplayServer.ScreenGetDpi(window.CurrentScreen),
            DisplayServer.ScreenGetScale(window.CurrentScreen),
            configuration?.UserConfigurationData?.UiScale ?? 1f);
    }

    // growSize: only windows being opened grow to a proportional size; applying to
    // an already-visible window (the root during the boot splash, or any window
    // when the UI Scale setting changes) must never jump the window around
    public static void ApplyDisplayScale(this Window window, bool growSize = true)
    {
        var scale = window.GetDisplayScale();
        var usable = DisplayServer.ScreenGetUsableRect(window.CurrentScreen);
        if (window.ContentScaleMode != Window.ContentScaleModeEnum.Disabled)
        {
            // stretch-mode windows (the player view) already zoom their canvas with
            // physical size; a factor on top would double-scale. Grow the window once.
            if (growSize && !window.HasMeta("display_scaled"))
            {
                window.SetMeta("display_scaled", true);
                window.Size = (Vector2I)((Vector2)window.Size * scale);
            }
        }
        else if (!Mathf.IsEqualApprox(window.ContentScaleFactor, scale))
        {
            // MinSize stays authored: forcing it up with the factor makes the OS
            // grow visible windows, which is exactly the jump this avoids
            var ratio = scale / window.ContentScaleFactor;
            window.ContentScaleFactor = scale;
            if (growSize)
            {
                window.Size = (Vector2I)((Vector2)window.Size * ratio);
            }
        }

        // keep the whole window on screen so bottom buttons stay reachable;
        // the internal scroll containers absorb any lost height
        window.Size = new Vector2I(Math.Min(window.Size.X, usable.Size.X), Math.Min(window.Size.Y, usable.Size.Y));
        window.Position = new Vector2I(
            Math.Clamp(window.Position.X, usable.Position.X, Math.Max(usable.Position.X, usable.End.X - window.Size.X)),
            Math.Clamp(window.Position.Y, usable.Position.Y, Math.Max(usable.Position.Y, usable.End.Y - window.Size.Y)));
    }
}
