using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

public class AdventureGameSkin : ModuleSkin
{
    public override string ModuleId { get { return "spwizAdventureGame"; } }
    public override string Name { get { return "RPG"; } }

    private Component _comp;
    private AdventureGamePrefab _prefab;

    private static Func<Component, int> s_enemyIndex;
    private static readonly string[] s_enemies = new[] { "Dragon", "Demon", "Eagle", "Goblin", "Troll", "Wizard", "Golem", "Lizard" };
    private string Enemy { get { return s_enemies[s_enemyIndex(_comp)]; } }

    private static Func<Component, int, string> s_getStatDisplay;
    private string GetStats()
    {
        return Enumerable.Range(0, 7).Select(i => s_getStatDisplay(_comp, i)).Join("\n");
    }

    private static Func<Component, IList> s_inventoryIndices;
    private static readonly string[] s_items = new[] {
        "Broadsword", "Caber", "Nasty Knife", "Longbow", "Magic Orb", "Grimoire",
        "Balloon", "Battery", "Bellows", "Cheat Code", "Crystal Ball", "Feather",
        "Hard Drive", "Lamp", "Moonstone", "Potion", "Small Dog", "Stepladder",
        "Sunstone", "Symbol", "Ticket", "Trophy"
    };
    private int ForeignInventoryCount { get { return s_inventoryIndices(_comp).Count; } }
    private IEnumerable<string> ForeignInventory { get { return s_inventoryIndices(_comp).Cast<int>().Select(i => s_items[i]); } }
    private int _inventoryCount;
    private List<string> Inventory { get { var inventory = ForeignInventory.ToList(); _inventoryCount = inventory.Count; return inventory; } }

    private static Action<Component, int> s_setSelectedItem;
    private static Action<Component> s_use;
    private void Use(int foreignIndex)
    {
        s_setSelectedItem(_comp, foreignIndex);
        s_use(_comp);
    }

    protected override void Initialize()
    {
        var t = GetComponent("AdventureGameModule").GetType();

        var param = Expression.Parameter(typeof(Component), "comp");
        var param_t = Expression.Convert(param, t);
        var enemy = Expression.Field(param_t, t.GetField("SelectedEnemy", BindingFlags.Instance | BindingFlags.NonPublic));
        s_enemyIndex = Expression.Lambda<Func<Component, int>>(Expression.Convert(enemy, typeof(int)), param).Compile();

        var i_param = Expression.Parameter(typeof(int), "i");
        var call = Expression.Call(param_t, t.Method("GetStatDisplay"), i_param);
        s_getStatDisplay = Expression.Lambda<Func<Component, int, string>>(call, param, i_param).Compile();

        var inventory = Expression.Field(param_t, t.GetField("InvValues", BindingFlags.Instance | BindingFlags.NonPublic));
        s_inventoryIndices = Expression.Lambda<Func<Component, IList>>(Expression.Convert(inventory, typeof(IList)), param).Compile();

        var selected = t.GetField("SelectedItem", BindingFlags.NonPublic | BindingFlags.Instance);
        s_setSelectedItem = (c, i) => selected.SetValue(c, i);

        var use = Expression.Call(param_t, t.Method("HandlePress"), Expression.Constant(4, typeof(int)));
        s_use = Expression.Lambda<Action<Component>>(use, param).Compile();
    }

    protected override void OnActivate()
    {
        _comp = GetComponent("AdventureGameModule");

        transform.GetChild(0).gameObject.SetActive(false);
        _prefab = AddPrefab().GetComponent<AdventureGamePrefab>();

        _prefab.Enemy = Enemy;
        _prefab.Items = Inventory;
        ShowStats();

        AssignButtons();
    }

    private void AssignButtons()
    {
        var buttons = _prefab.GetSelectables();
        ReplaceSelectableChildren(1, buttons);

        for (int i = 0; i < buttons.Length; i++)
        {
            int j = i;

            buttons[j].OnInteract = () =>
            {
                Use(j);
                if (ForeignInventoryCount != _inventoryCount)
                {
                    _inventoryCount = ForeignInventoryCount;
                    _prefab.Remove(j);
                    ShowStats();
                    AssignButtons();
                    Audio.PlaySoundAtTransform("spwizAdventureGame_Use" + UnityEngine.Random.Range(0, 3), transform);
                }

                return false;
            };
        }
    }

    private void ShowStats() { _prefab.Stats = GetStats(); }

    protected override void OnSolve()
    {
        Audio.PlaySoundAtTransform("spwizAdventureGame_Solve", transform);
        _prefab.Solve();
    }
}
