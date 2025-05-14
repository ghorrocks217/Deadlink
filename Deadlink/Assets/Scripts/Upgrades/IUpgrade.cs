using UnityEngine;

public interface IUpgrade
{
    void Apply(GameObject target);
    void Remove(GameObject target);
}
