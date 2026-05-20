using UnityEngine;

[CreateAssetMenu(fileName = "New Homing Weapon", menuName = "ScriptableObjects/Weapons/Homing Weapon")]
public class HomingSO : ItemsSO
{
    [Header("Homing Projectile Settings")]
    [Tooltip("Kecepatan berbelok peluru untuk mengejar musuh (Makin besar, beloknya makin tajam)")]
    public float turnSpeed = 5f;
    
    [Tooltip("Waktu jeda sebelum peluru mulai aktif berbelok mengejar musuh (dalam detik)")]
    public float homingDelay = 0.2f;
    
    [Tooltip("Sudut acak sebaran awal peluru sesaat setelah keluar dari senjata")]
    public float scatterAngle = 15f;

    protected override void SpawnProjectiles(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime, float spreadAngle)
    {
        // Logika multi-shot sebaran kipas mirip FireballSO jika jumlah peluru lebih dari 1
        float startAngle = count <= 1 ? 0f : -spreadAngle / 2f;
        float angleStep = count <= 1 ? 0f : spreadAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float currentAngle = count <= 1 ? 0f : startAngle + (angleStep * i);
            
            // Menggabungkan rotasi spawn point dengan sebaran level
            Quaternion rot = spawnPoint.rotation * Quaternion.Euler(0, currentAngle, 0);
            GameObject go = Instantiate(projectilePrefab, spawnPoint.position, rot);
            
            if (go.TryGetComponent(out IProjectile projectile))
            {
                // Meneruskan variabel riil dari Inspector ScriptableObject ini ke script Peluru
                projectile.Setup(speed, damage, lifetime, turnSpeed, homingDelay, scatterAngle, CurrentCooldown, size);
            }
            
            if (SoundManager.Instance != null && !string.IsNullOrEmpty(shootSound)) 
                SoundManager.Instance.PlaySound3D(shootSound, spawnPoint.position);
        }
    }
}