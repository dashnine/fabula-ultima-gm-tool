using FirstProject;
using FirstProject.Encounters;
using FirstProject.Npc;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CurrentEncounter : PanelContainer
{
	private Encounter Encounter { get; set; }
	private RunState RunState { get; set; }
	private AcceptDialog _addErrorDialog;

	private Func<string> GetNextName { get; set; }

    [Signal]
    public delegate void UpdateEncounterEventHandler(Encounter encounter);

	[Export]
	public int NPCLimit { get; set; } = 4;

    [Export]
    public Configuration _configuration { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		// todo: show last run encounter
        if (Encounter == null)
        {
            this.Visible = false;
        }
		RunState = GetNode<RunState>("/root/RunState");

        var nameQueue = new Queue<string>();
		GetNextName = new Func<string>(() =>
		{
			if (!nameQueue.Any())
			{
                if (_configuration?.InstanceNames == null || _configuration.InstanceNames.Length == 0)
                {
                    // survive a missing/unloaded configuration resource instead of NRE-ing
                    GD.PushError("CurrentEncounter has no instance-name configuration; using a generic name");
                    return $"NPC {Guid.NewGuid().ToString("N")[..4]}";
                }
                var nameList = new Godot.Collections.Array<string>(_configuration.InstanceNames);
                nameList.Shuffle();
				foreach(var name in nameList)
				{
					nameQueue.Enqueue(name);
				}
            }
			return nameQueue.Dequeue();
		});
    }
	
	private void ShowAddError(string message)
	{
		if (_addErrorDialog == null)
		{
			_addErrorDialog = new AcceptDialog { Title = "Can't Add NPC" };
			AddChild(_addErrorDialog);
		}
		_addErrorDialog.DialogText = message;
		_addErrorDialog.PopupCentered();
	}

	public void HandleUpdateEncounter(Encounter encounter)
	{
        Encounter = encounter;
		if(Encounter != null)
		{
			this.Visible = true;
            RunState.RunningEncounter = Encounter;
        }
		else
		{
			this.Visible = false;
		}
    }

	public void HideEncounterIfDeleted(Encounter encounter)
	{
		if (Encounter == null) return;
		if (encounter == null) return;
		var deletedEncounter = encounter;
		if (Encounter.Id != deletedEncounter.Id) return;
		this.Visible = false;
		Encounter = null;
	}

	public void AddNpcToEncounter(NpcInstance npc)
	{
		if (npc == null) return;
		// these were silent early-returns; tell the user why the add was dropped
		if (Encounter == null)
		{
			ShowAddError("No scene is loaded.\nLoad a scene from the Campaign tab first.");
			return;
		}
		//todo: enable adding more npcs
		if (Encounter.NpcCollection.Count() >= NPCLimit)
		{
			ShowAddError($"This scene already has {NPCLimit} NPCs.");
			return;
		}
		var clone = new NpcInstance(npc);
		if (string.IsNullOrWhiteSpace(clone.InstanceName))
		{
			clone.InstanceName = GetNextName();
		}
        Encounter.AddNpc(clone); // use this to ensure change is emitted		
		EmitSignal(SignalName.UpdateEncounter, Encounter);
    }

	public void RemoveNpcFromEncounter(NpcInstance npc)
	{
        if (Encounter == null) return;
        if (npc == null) return;
		Encounter.RemoveNpc(npc);
        EmitSignal(SignalName.UpdateEncounter, Encounter);
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (!(data.As<GodotObject>() is NpcInstance npc)) return false;
        return true;
    }

    public override void _DropData(Vector2 atPosition, Variant data)
    {
        var original = data.As<NpcInstance>();
		AddNpcToEncounter(original);
    }
}
