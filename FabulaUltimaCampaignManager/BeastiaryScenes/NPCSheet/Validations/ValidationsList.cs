using FabulaUltimaGMTool.BeastiaryScenes;
using FabulaUltimaNpc;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ValidationsList : VBoxContainer, IBeastAttribute
{
    private NpcSheet _npcSheet;

    [Export]
    public string StartNodePath { get; set; }

    [Signal]
    public delegate void IsBeastValidEventHandler(int errorCount, int warningCount);

    public Action<ISet<BeastEntryNode.Action>> BeastTemplateAction { get; set; }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _npcSheet = GetNode(StartNodePath) as NpcSheet;
        // a broken path here would otherwise silently disable validation and block saving
        if (_npcSheet == null) GD.PushError($"ValidationsList couldn't find the NPC sheet at '{StartNodePath}'");
    }

    public void HandleBeastChanged(IBeastTemplate beastTemplate)
    {
        if (_npcSheet == null)
        {
            // can't validate without the sheet; fail open rather than gating saves forever
            EmitSignal(SignalName.IsBeastValid, 0, 0);
            return;
        }
        // clear prior validations
        var children = this.FindChildren("*", recursive: false);
        foreach (var child in children)
        {
            RemoveChild(child);
            child.QueueFree();
        }

        int errors = 0;
        int warnings = 0;
        foreach(var validatable in _npcSheet.FindChildren("*", owned: false).Where(c => c is IValidatable).Select(c => c as IValidatable))
        {
            foreach(var validation in validatable.Validate())
            {
                var validationLabel = new Label
                {
                    Text = $"{validation.Level}: {validatable.Name} - {validation.Message}"
                };
                AddChild(validationLabel);
                validationLabel.Owner = this;
                // color-code so failures stand out in the scrolling form
                if (validation.Level == ValidationLevel.ERROR)
                {
                    validationLabel.AddThemeColorOverride("font_color", new Color("e06a66"));
                    errors++;
                }
                if (validation.Level == ValidationLevel.WARNING)
                {
                    validationLabel.AddThemeColorOverride("font_color", new Color("d19a4a"));
                    warnings++;
                }
            }           
        }
        EmitSignal(SignalName.IsBeastValid, errors, warnings);
    }
}
