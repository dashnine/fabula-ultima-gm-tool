using Godot;
using System;

public static class WindowExtensions
{
    public static void ResizeForResolution(this Window window)
    {
         // figure out window size
        
        var screenSize = DisplayServer.ScreenGetSize();
        var yMultiplier = screenSize.Y / 1080;
        if (yMultiplier < 1) yMultiplier = 1; // int division hits 0 below 1080p, which would zero the window
        window.ContentScaleFactor = Math.Clamp( yMultiplier, 0.5f, 8);
        window.Size = new Vector2I( window.Size.X * yMultiplier, window.Size.Y * yMultiplier);

        // keep the whole window on screen so bottom buttons stay reachable;
        // the internal scroll containers absorb any lost height
        var usable = DisplayServer.ScreenGetUsableRect(window.CurrentScreen);
        window.Size = new Vector2I(Math.Min(window.Size.X, usable.Size.X), Math.Min(window.Size.Y, usable.Size.Y));
        window.Position = new Vector2I(
            Math.Clamp(window.Position.X, usable.Position.X, Math.Max(usable.Position.X, usable.End.X - window.Size.X)),
            Math.Clamp(window.Position.Y, usable.Position.Y, Math.Max(usable.Position.Y, usable.End.Y - window.Size.Y)));
    }
}
