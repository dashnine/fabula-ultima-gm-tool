using FabulaUltimaNpc;
using Godot;
using System;
using System.Collections.Generic;

public partial class DescriptionImage : TextureRect, IBeastAttribute
{
    private IBeastTemplate _beastTemplate = null;

    public Action<System.Collections.Generic.ISet<BeastEntryNode.Action>> BeastTemplateAction { get; set; }

    public void HandleBeastChanged(IBeastTemplate beastTemplate)
    {
        _beastTemplate = beastTemplate;
        if (string.IsNullOrWhiteSpace(_beastTemplate.ImageFile)) return;

        if (_beastTemplate.ImageFile.CopyToResourceFolder(out var newPath))
        {
            HandleImageSet(newPath);
            return;
        }
        var thumbnailDimension = ThumbnailDimension();
        if (thumbnailDimension > 0)
        {
            this.Texture = BeastImageCache.GetThumbnail(_beastTemplate.ImageFile, thumbnailDimension) ?? this.Texture;
            return;
        }
        Texture2D texture =
            FirstProject.ResourceExtensions.Load<Texture2D>(_beastTemplate.ImageFile) ??
            LoadFromFile(_beastTemplate.ImageFile);
		this.Texture = texture;
    }

    // the bestiary list flags its entries for thumbnail-size decoding; every other
    // host of this script (edit sheet, stat window) stays full resolution
    private int ThumbnailDimension()
    {
        for (var node = GetParent(); node != null; node = node.GetParent())
        {
            if (node is BeastEntryNode entry)
            {
                return entry.UseThumbnails ? entry.ThumbnailDimension : 0;
            }
        }
        return 0;
    }

    private static ImageTexture LoadFromFile(string filePath)
    {
        var image = Image.LoadFromFile(filePath);        
		return ImageTexture.CreateFromImage(image);
    }

	public void HandleImageSet(string imageFileName)
	{
        if (string.IsNullOrWhiteSpace(imageFileName)) return;
        imageFileName.CopyToResourceFolder(out var newPath);
        // the copy overwrites same-named files, so any cached thumbnail of the
        // target path is stale before the CHANGED fan-out reloads it
        BeastImageCache.Invalidate(newPath);
        _beastTemplate.ImageFile = newPath;
        BeastTemplateAction.Invoke(new HashSet<BeastEntryNode.Action> { BeastEntryNode.Action.CHANGED });
    }
}