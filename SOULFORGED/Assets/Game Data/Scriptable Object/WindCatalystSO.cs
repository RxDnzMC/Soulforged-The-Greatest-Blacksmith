using UnityEngine;

[CreateAssetMenu(fileName = "New Wind Catalyst", menuName = "ScriptableObjects/Weapons/Wind Catalyst")]
public class WindCatalystSO : ItemsSO 
{
    protected override void SpawnProjectiles(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime, float spreadAngle)
    {
        PlayerAttack playerRoot = spawnPoint.GetComponentInParent<PlayerAttack>();
        Transform playerTransform = playerRoot != null ? playerRoot.transform : spawnPoint;

        // 1. CARI SEMUA AURA, LALU FILTER NAMA PREFAB-NYA
        WindCatalystMechanic existingAura = null;
        WindCatalystMechanic[] activeAuras = playerTransform.GetComponentsInChildren<WindCatalystMechanic>();

        foreach (var aura in activeAuras)
        {
            // Cek apakah aura ini adalah Laser Beam atau Shockwave
            if (aura.gameObject.name.StartsWith(projectilePrefab.name))
            {
                existingAura = aura;
                break;
            }
        }

        // 2. SPAWN ATAU UPDATE
        if (existingAura != null)
        {
            existingAura.UpdateStats(damage, size);
        }
        else
        {
            GameObject go = Instantiate(projectilePrefab, playerTransform.position, Quaternion.Euler(90f, 0f, 0f));
            
            // 3. WAJIB: Hapus akhiran "(Clone)" bawaan Unity agar nama sama persis dengan prefab
            go.name = projectilePrefab.name; 
            
            go.transform.SetParent(playerTransform); 
            go.transform.localPosition = Vector3.zero;
            
            if (go.TryGetComponent(out WindCatalystMechanic mechanic))
            {
                mechanic.Setup(speed, damage, 0f, 0f, 0f, 0f, CurrentCooldown, size);
            }
        }
    }

    
}

