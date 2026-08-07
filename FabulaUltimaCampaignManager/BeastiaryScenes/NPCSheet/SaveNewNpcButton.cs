using FabulaUltimaNpc;
using FirstProject.Npc;
using Godot;
using System;
using System.Collections.Generic;

public partial class SaveNewNpcButton : Button, IBeastAttribute
{
    private int? _errorCount;
    private int? _warningCount;
    private AcceptDialog _errorDialog;

    public Action<ISet<BeastEntryNode.Action>> BeastTemplateAction { get; set; }

    public override void _Ready()
    {
        _errorDialog = new AcceptDialog { Title = "Can't Save" };
        AddChild(_errorDialog);
    }

    public void HandleBeastChanged(IBeastTemplate beastTemplate)
    {
        var npcModel = beastTemplate.Model as NpcModel;
        if (npcModel != null)
        {
            if (npcModel.Species == null) this.Text = "Add Copy To Scene";
            else this.Text = "Update";
        }
    }

    public void HandleErrorsAndWarnings(int errorCount, int warningCount)
	{
		_errorCount = errorCount;
		_warningCount = warningCount;
	}

	public void HandlePressed()
    {
        // validate first so the save decision uses this click's results;
        // the old order gated on the previous click's counts, so the first
        // click (or the first after a fix) silently did nothing
        BeastTemplateAction?.Invoke(new HashSet<BeastEntryNode.Action> { BeastEntryNode.Action.TRIGGER });
        if (_errorCount == 0)
        {
            BeastTemplateAction?.Invoke(new HashSet<BeastEntryNode.Action> { BeastEntryNode.Action.SAVE });
            return;
        }
        if (_errorCount == null)
        {
            _errorDialog.DialogText = "Couldn't validate the NPC. Close and reopen the sheet, then try again.";
        }
        else
        {
            _errorDialog.DialogText = _errorCount == 1
                ? "Fix the error listed under Errors and Warnings, then save again."
                : $"Fix the {_errorCount} errors listed under Errors and Warnings, then save again.";
        }
        _errorDialog.PopupCentered();
	}
}
