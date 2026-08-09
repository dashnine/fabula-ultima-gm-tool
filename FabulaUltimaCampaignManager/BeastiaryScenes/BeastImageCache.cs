using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

// the bestiary list shows every beast's portrait in a small box and rebuilds on
// every filter keystroke; decode at thumbnail size once per path instead of
// holding full-resolution art for the whole list
public static class BeastImageCache
{
    // 2x the on-screen box so thumbs stay crisp under ContentScaleFactor <= 2 and macOS retina
    public const int CollapsedMaxDimension = 220;
    public const int ExpandedMaxDimension = 440;

    private static readonly Dictionary<string, Texture2D> _cache = new();

    public static Texture2D GetThumbnail(string imagePath, int maxDimension)
    {
        if (string.IsNullOrWhiteSpace(imagePath)) return null;
        var key = $"{imagePath}@{maxDimension}";
        if (_cache.TryGetValue(key, out var cached)) return cached;
        var thumbnail = CreateThumbnail(imagePath, maxDimension);
        if (thumbnail != null) _cache[key] = thumbnail;
        return thumbnail;
    }

    // drop every cached size for a path (the copy step overwrites same-named files,
    // so a re-imported image can change content behind an unchanged path)
    public static void Invalidate(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath)) return;
        var prefix = $"{imagePath}@";
        foreach (var key in _cache.Keys.Where(k => k.StartsWith(prefix)).ToList())
        {
            _cache.Remove(key);
        }
    }

    private static Texture2D CreateThumbnail(string imagePath, int maxDimension)
    {
        Image image;
        if (ResourceLoader.Exists(imagePath))
        {
            // CacheMode.Ignore returns a private instance — never the shared wrapper a
            // full-resolution consumer may hold — so disposing it reclaims the
            // full-size pixels now instead of holding them for the session
            var full = ResourceLoader.Load<Texture2D>(imagePath, null, ResourceLoader.CacheMode.Ignore);
            image = full?.GetImage();
            full?.Dispose();
        }
        else
        {
            image = Image.LoadFromFile(imagePath);
        }
        if (image == null || image.IsEmpty()) return null;

        var largest = Math.Max(image.GetWidth(), image.GetHeight());
        if (largest > maxDimension)
        {
            var scale = (float)maxDimension / largest;
            image.Resize(
                Math.Max(1, (int)(image.GetWidth() * scale)),
                Math.Max(1, (int)(image.GetHeight() * scale)),
                Image.Interpolation.Lanczos);
        }
        var texture = ImageTexture.CreateFromImage(image);
        image.Dispose();
        return texture;
    }
}
