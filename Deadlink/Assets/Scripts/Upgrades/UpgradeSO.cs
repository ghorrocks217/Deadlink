using UnityEngine;

public abstract class UpgradeSO : ScriptableObject, IUpgrade
{
    public string upgradeName;

    public abstract void Apply(GameObject target);
    public abstract void Remove(GameObject target);
}
