using FabulaUltimaNpc;
using FirstProject.Encounters;
using FirstProject.Npc;
using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class AffinityEditPopup : Window, INpcReader, INpcStatusReader
{
    private BattleStatus _status;
    private IBeastTemplate _template;
    private readonly List<AffinityEdit> _rows = new List<AffinityEdit>();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        this.CloseRequested += Hide;
        foreach (var child in this.FindChildren("*")
            .Where(c => c is AffinityEdit))
        {
            var row = child as AffinityEdit;
            row.BeastTemplateAction += HandleTemplateAction;
            _rows.Add(row);
        }
    }

    public void HandleNpcChanged(NpcInstance npc)
    {
        // edits target this npc's own model clone, never the bestiary template
        _template = npc.Template;
    }

    public void HandleStatusSet(BattleStatus status)
    {
        _status = status;
    }

    public void HandleOpen()
    {
        if (_template == null) return;
        RefreshRows();
        PopupCentered();
    }

    private void RefreshRows()
    {
        foreach (var row in _rows)
        {
            row.HandleBeastChanged(_template);
        }
    }

    private void HandleTemplateAction(ISet<BeastEntryNode.Action> actions)
    {
        // a selection swapped an affinity skill on the model; the rest of the
        // battle finds out through the shared status
        RefreshRows();
        _status?.NotifyAffinityChanged();
    }
}
