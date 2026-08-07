using FabulaUltimaGMTool.BeastiaryScenes;
using FabulaUltimaNpc;
using FirstProject.Npc;
using Godot;
using System;
using System.Collections.Generic;

public partial class InstanceNameEdit : LineEdit, IBeastAttribute, IValidatable
{
    public Action<ISet<BeastEntryNode.Action>> BeastTemplateAction { get; set; }

    string IValidatable.Name => "Instance Name";
    private string _currentName = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		this.Visible = false;
	}

    public void HandleBeastChanged(IBeastTemplate beastTemplate)
    {
        var npcModel = beastTemplate.Model as NpcModel;
        if (npcModel == null) return;
        this.Visible = true;
        // only bind on first show: this fires on every validation pass, and
        // the model's instance is a different object from the sheet's, so
        // rebinding stomped whatever the user had typed (and re-warned)
        if (_currentName == null) this.Text = _currentName = npcModel.Instance.InstanceName;
    }

    public void HandleTextChanged(string newText)
    {
        _currentName = newText;
    }

    public IEnumerable<TemplateValidation> Validate()
    {
        if (!this.Visible) yield break;
        // todo: fix this validation
        if (string.IsNullOrWhiteSpace(_currentName)) yield return new TemplateValidation { Level = ValidationLevel.WARNING, Message = "No name set - a random one will be assigned when added to the scene" };
    }
}
