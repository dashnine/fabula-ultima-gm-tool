using Godot;

public partial class node_2d : Control
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetWindow().Title = "Fabula Ultima GM Tool";
		// stretch is disabled, so guard against shrinking the window below the layout minimum
		GetWindow().MinSize = new Vector2I(1100, 640);
		// the tab title comes from the node name, which code paths depend on;
		// override just the visible text to the correct spelling
		GetNode<TabContainer>("HSplitContainer/CampaignManager").SetTabTitle(1, "Bestiary");
	}
}
