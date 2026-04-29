using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Items", order = 1)]
public class ItemsSO : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public float baseDamage;
    public float cooldown;
    public float speed;
    public float lifetime;
    public GameObject projectilePrefab;
    
    // Tambahkan ini untuk nyimpen waktu terakhir item ini dipake
    private float lastUsedTime = -999f;

    public virtual void Use(Transform spawnPoint)
    {
        
        if (projectilePrefab == null) return;

        // Update waktu terakhir dipake
        lastUsedTime = Time.time;

        // Spawn prefab-nya
        GameObject go = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
        
        // Kasih datanya ke script yang nempel di prefab
        if (go.TryGetComponent(out FireballProjectile projectile))
        {
            projectile.Setup(speed, baseDamage, lifetime);
        }
    }
}