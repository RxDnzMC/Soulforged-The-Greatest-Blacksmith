using UnityEngine;

public interface IProjectile 
{   
    float Damage { get; }
    void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle);
}

public interface IAttackItem
{
    bool isReady { get; }
    void Use(Transform spawnPoint);
}
