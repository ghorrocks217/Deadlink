using UnityEngine;
using System.Collections.Generic;

public class UpgradeManager : MonoBehaviour
{
    private List<UpgradeSO> activeUpgrades = new List<UpgradeSO>();

    public void AddUpgrade(UpgradeSO upgrade)
    {
        upgrade.Apply(gameObject);
        activeUpgrades.Add(upgrade);
    }

    public void RemoveUpgrade(UpgradeSO upgrade)
    {
        if (activeUpgrades.Contains(upgrade))
        {
            upgrade.Remove(gameObject);
            activeUpgrades.Remove(upgrade);
        }
    }

    public void RemoveAllUpgrades()
    {
        foreach (var upgrade in activeUpgrades)
        {
            upgrade.Remove(gameObject);
        }
        activeUpgrades.Clear();
    }
}
