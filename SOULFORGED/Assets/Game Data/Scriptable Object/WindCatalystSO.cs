using UnityEngine;

[CreateAssetMenu(fileName = "New Wind Catalyst", menuName = "ScriptableObjects/Weapons/Wind Catalyst")]
public class WindCatalystSO : ItemsSO 
{
    protected override void SpawnProjectiles(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime, float spreadAngle)
    {
        // CARA PRO: Cari script PlayerAttack yang ada di induk paling atas (Pusat Player)
        // Ini memastikan kita mendapatkan posisi asli badan Player, BUKAN Weapon Pivot yang bergeser
        PlayerAttack playerRoot = spawnPoint.GetComponentInParent<PlayerAttack>();
        Transform playerTransform = playerRoot != null ? playerRoot.transform : spawnPoint;

        // Cari apakah aura sudah ada di pusat tubuh Player
        WindCatalystMechanic existingAura = playerTransform.GetComponentInChildren<WindCatalystMechanic>();

        if (existingAura != null)
        {
            // Jika sudah ada, cukup update level stat-nya
            existingAura.UpdateStats(damage, size);
        }
        else
        {
            // Jika belum ada, spawn tepat di posisi tengah Player Utama
            GameObject go = Instantiate(projectilePrefab, playerTransform.position, Quaternion.Euler(90f, 0f, 0f));
            
            // KUNCI UTAMA: Jadikan dia child dari PLAYER UTAMA (bukan dari spawnPoint/pivot yang bergerak)
            go.transform.SetParent(playerTransform); 
            
            // PAKSA posisi lokalnya menjadi 0,0,0 agar benar-benar simetris di tengah kaki Player
            go.transform.localPosition = Vector3.zero;
            
            if (go.TryGetComponent(out WindCatalystMechanic mechanic))
            {
                mechanic.Setup(speed, damage, 0f, 0f, 0f, 0f, CurrentCooldown, size);
            }
        }
    }

    
}

