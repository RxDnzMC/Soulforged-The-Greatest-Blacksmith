using UnityEngine;

[CreateAssetMenu(fileName = "New Tornado", menuName = "ScriptableObjects/Weapons/Tornado")]
public class TornadoSO : ItemsSO 
{
    protected override void SpawnProjectiles(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime, float spreadAngle)
    {
        // Mencari posisi asli Player seperti pada WindCatalyst[cite: 1]
        PlayerAttack playerRoot = spawnPoint.GetComponentInParent<PlayerAttack>();
        Transform playerTransform = playerRoot != null ? playerRoot.transform : spawnPoint;

        // Loop untuk memunculkan tornado sebanyak 'count' (Bertambah seiring level)
        for (int i = 0; i < count; i++)
        {
            // Spawn di posisi Player
            GameObject go = Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);
            
            // PENTING: Jangan di-SetParent ke player agar tornado bisa bergerak bebas lepas dari player
            
            if (go.TryGetComponent(out TornadoMechanic mechanic))
            {
                // Inisialisasi stat dari level saat ini (damage, ukuran, kecepatan, waktu hidup)
                mechanic.Setup(speed, damage, 0f, 0f, lifetime, 0f, CurrentCooldown, size);
            }
        }
    }
}