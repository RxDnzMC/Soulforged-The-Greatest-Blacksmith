using UnityEngine;

public class WindCatalystMechanic : MonoBehaviour, IProjectile
{
    [SerializeField] PlayerData playerData;
    
    private float cooldown;
    private float damage;
    private float lifetime;
    
    public float Damage => damage;
    public float Cooldown => cooldown;

    public void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle, float cooldown, float size)
    {
        this.cooldown = cooldown;
        this.lifetime = lifetime;
        this.damage = damage;
        transform.localScale *= size;
        
        // PAKSA POSISI DI KAKI PLAYER (abaikan spawnPoint dari PlayerAttack)
        transform.position = playerData.playerPosition;
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        
        if (lifetime > 0) Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Stay di kaki player
        transform.position = playerData.playerPosition;
    }
    
    void Awake()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound3D("Fireball Shoot", transform.position);
    }
}