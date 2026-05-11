using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObjects/Items", order = 1)]
public class HomingMissle : ScriptableObject
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

    private float lastUsedTime = -999f;

    // Fungsi utama saat senjata digunakan
    public virtual void Use(Transform spawnPoint)
    {
        if (projectilePrefab == null) return;

        lastUsedTime = Time.time;

        // Looping untuk memunculkan banyak peluru (Swarm)
        for (int i = 0; i < projectileCount; i++)
        {
            GameObject go = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            
            if (go.TryGetComponent(out Homing projectile))
            {
                // Kirim semua data ke Setup script peluru
                projectile.Setup(speed, baseDamage, lifetime, turnSpeed, homingDelay, scatterAngle);
            }
        }
    }
}