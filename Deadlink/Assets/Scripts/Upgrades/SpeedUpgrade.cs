using UnityEngine;

[CreateAssetMenu(fileName = "SpeedUpgrade", menuName = "Upgrades/Speed Upgrade")]
public class SpeedUpgrade : UpgradeSO
{
    public float additionalSpeed = 2f;

    public override void Apply(GameObject target)
    {
        var stats = target.GetComponent<CharacterStats>();
        if (stats != null)
        {
            stats.speed += additionalSpeed;
        }
    }

    public override void Remove(GameObject target)
    {
        var stats = target.GetComponent<CharacterStats>();
        if (stats != null)
        {
            stats.speed -= additionalSpeed;
        }
    }
}
