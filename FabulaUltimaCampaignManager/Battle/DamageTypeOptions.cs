using FirstProject;
using FirstProject.Beastiary;
using FirstProject.Encounters;
using FirstProject.Npc;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class DamageTypeOptions : OptionButton, INpcReader, INpcStatusReader
{
    [Signal]
    public delegate void DamageTypeAndAffinityChangedEventHandler(SignalWrapper<Affinity> affinitySignal, string damageType);


    private readonly IDictionary<int, string> _indexToDamageNameMap = new Dictionary<int, string>();
    private readonly IDictionary<int, string> _indexToItemText = new Dictionary<int, string>();
    private IReadOnlyDictionary<string, Affinity> _affinities;
    private NpcInstance _npc;
    public const string HEAL = "Heal";
    public const string MP_LOSS = "Lose MP";
    public const string MP_GAIN = "Gain MP";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        var dBAccess = GetNode<DbAccess>("/root/DbAccess");
		var repository = dBAccess.Repository;
        var damageTypeValues = repository.GetDamageTypes().Where(v => !string.Equals(v.Name, "no damage", StringComparison.InvariantCultureIgnoreCase ));
		var startIndex = this.ItemCount;
		foreach(var name in damageTypeValues)
		{
			this.AddItem(name.Name, startIndex);
			_indexToDamageNameMap[startIndex] = name.Name.ToLowerInvariant();
            _indexToItemText[startIndex] = name.Name;
            startIndex++;
		}
		
        _indexToDamageNameMap[startIndex] = HEAL;
        this.AddItem(HEAL, startIndex);
        startIndex++;
        _indexToDamageNameMap[startIndex] = MP_GAIN;
        this.AddItem(MP_GAIN, startIndex);        
        startIndex++;
        _indexToDamageNameMap[startIndex] = MP_LOSS;
        this.AddItem(MP_LOSS, startIndex);
    }

    public void HandleNpcChanged(NpcInstance npc)
    {
        _npc = npc;
		_affinities = npc.Resistances;
        var damageNameToIndexMap = _indexToDamageNameMap.ToDictionary(p => p.Value, p => p.Key);
        foreach(var affinity in _affinities)
        {
            var itemIndex = damageNameToIndexMap[affinity.Key];
            // annotate from the stored base text: this re-runs after mid-battle
            // affinity edits, and GetItemText would stack " - VU - RS"
            var itemText = _indexToItemText[itemIndex];
            var newText = affinity.Value != Affinity.NONE ? $"{itemText} - {affinity.Value}" : itemText;
            SetItemText(itemIndex, newText);
        }
        // the selected type's affinity may have changed; keep the damage preview in sync
        if (Selected >= 0) OnItemSelected(Selected);
    }

    public void HandleStatusSet(BattleStatus status)
    {
        status.AffinityChanged += _ => HandleNpcChanged(_npc);
    }

    public void OnItemSelected(int index)
    {
        if (!_indexToDamageNameMap.ContainsKey(index)) return;
        var damageType = _indexToDamageNameMap[index];        
        Affinity affinity;        

        switch(damageType)
        {
            case HEAL:
                affinity = Affinity.HEAL;
                break;
            case MP_LOSS:
                affinity = Affinity.MP_DAMAGE;
                break;
            case MP_GAIN:
                affinity = Affinity.MP_GAIN;
                break;
            default:
                affinity = _affinities[damageType];
                break;
        }        

        EmitSignal(SignalName.DamageTypeAndAffinityChanged, new SignalWrapper<Affinity> (affinity), damageType);
    }
}
