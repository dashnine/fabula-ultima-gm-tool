using Godot;

// every Window is a real OS window (embed_subwindows is off) and none inherit the
// root's ContentScaleFactor, so scale each one as it enters the tree
public partial class WindowScaler : Node
{
    public override void _Ready()
    {
        // growSize false: the root window is already showing the boot splash
        GetWindow().ApplyDisplayScale(growSize: false);
        GetTree().NodeAdded += HandleNodeAdded;
    }

    private void HandleNodeAdded(Node node)
    {
        if (node is Window window) window.ApplyDisplayScale();
    }

    // re-applies after the UI Scale setting changes; content rescales in place,
    // no open window changes size
    public void RescaleAll()
    {
        GetWindow().ApplyDisplayScale(growSize: false);
        foreach (var window in GetTree().Root.FindChildren("*", "Window", true, false))
        {
            (window as Window).ApplyDisplayScale(growSize: false);
        }
    }
}
