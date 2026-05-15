using UnityEngine;

public interface IProjectile
{
    float Damage { get; }
    float Cooldown { get; } // Tambah ini agar musuh bisa tahu cooldown senjata
    void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle, float cooldown, float size);
}

public interface IAttackItem
{
    bool isReady { get; }
    void Use(Transform spawnPoint);
}
