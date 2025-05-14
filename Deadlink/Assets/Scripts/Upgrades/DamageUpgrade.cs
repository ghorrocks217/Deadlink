using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Damage Upgrade")]
public class DamageUpgrade : UpgradeSO
{
    public float additionalDamage;

    public override void Apply(GameObject target)
    {
        var stats = target.GetComponent<CharacterStats>();
        if (stats != null)
        {
            stats.damage += additionalDamage;
        }
    }

    public override void Remove(GameObject target)
    {
        var stats = target.GetComponent<CharacterStats>();
        if (stats != null)
        {
            stats.damage -= additionalDamage;
        }
    }
}
