using UnityEngine;

public interface IProjectile 
{
    void Setup(float speed, float damage, float lifetime);
}

public interface IAttackItem
{
    bool isReady { get; }
    void Use(Transform spawnPoint);
}
