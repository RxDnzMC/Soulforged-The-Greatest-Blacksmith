using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Active Item", menuName = "ScriptableObjects/Items/Active", order = 1)]
public class ItemsSO : ScriptableObject, IAttackItem
{
    public string itemName;
    public Sprite itemIcon;
    public float baseDamage;
    public float cooldown;
    public float speed;
    public float lifetime;
    public GameObject projectilePrefab;
    
    [Header("Homing Swarm Settings")]
    public int projectileCount = 5;       // Jumlah peluru yang keluar
    public float turnSpeed = 15f;         // Seberapa cepat belok mengejar musuh
    public float homingDelay = 0.3f;      // Jeda lurus sebelum mulai mengejar
    public float scatterAngle = 45f;      // Seberapa menyebar pelurunya di awal

    // Tambahkan ini untuk nyimpen waktu terakhir item ini dipake
    private float _lastUsedTime = -999f;
    public bool isReady 
    {
        get 
        {
            // Logika: Apakah waktu sekarang sudah melewati waktu terakhir pakai + cooldown?
            return Time.time >= _lastUsedTime + cooldown;
        }
    }

    public virtual void Use(Transform spawnPoint)
    {
        
        if (projectilePrefab == null) return;

        // Update waktu terakhir dipake
        _lastUsedTime = Time.time;

        // Spawn prefab-nya
        GameObject go = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Kasih datanya ke script yang nempel di prefab
        if (go.TryGetComponent(out IProjectile projectile))
        {
            projectile.Setup(speed, baseDamage, lifetime, turnSpeed, homingDelay, scatterAngle);
        }

    }

    private void OnEnable()
    {
        // Reset cooldown saat game mulai atau saat item ini di-enable
        _lastUsedTime = -999f;
    }
}

