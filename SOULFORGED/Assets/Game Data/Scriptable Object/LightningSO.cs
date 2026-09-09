using UnityEngine;

[CreateAssetMenu(fileName = "New Chain Lightning", menuName = "ScriptableObjects/Weapons/Lightning")]
public class LightningSO : ItemsSO 
{
    protected override void SpawnProjectiles(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime, float spreadAngle)
    {
        // Cari posisi pusat Player
        PlayerAttack playerRoot = spawnPoint.GetComponentInParent<PlayerAttack>();
        Transform playerTransform = playerRoot != null ? playerRoot.transform : spawnPoint;

        // Cari apakah Manager Lightning sudah ada di player
        ChainLightningMechanic existingLightning = null;
        ChainLightningMechanic[] activeMechanics = playerTransform.GetComponentsInChildren<ChainLightningMechanic>();

        foreach (var mechanic in activeMechanics)
        {
            if (mechanic.gameObject.name == "LightningManager")
            {
                existingLightning = mechanic;
                break;
            }
        }

        // Jika sudah ada, cukup update stat-nya
        if (existingLightning != null)
        {
            existingLightning.UpdateStats(speed, damage, size, count, spreadAngle);
        }
        else
        {
            // Jika belum ada, buat objek Manager baru di tengah tubuh Player
            GameObject managerObj = new GameObject("LightningManager");
            managerObj.transform.SetParent(playerTransform);
            managerObj.transform.localPosition = new Vector3(0f, 1f, 0f); 
            
            // Pasang skrip mekanik
            ChainLightningMechanic mechanic = managerObj.AddComponent<ChainLightningMechanic>();
            
            // Masukkan Prefab VFX Petir dari SO ke dalam Manager
            mechanic.InitializeManager(count, projectilePrefab);
            
            // Panggil Setup standar wajib dari IProjectile
            mechanic.Setup(speed, damage, lifetime, 0f, 0f, spreadAngle, CurrentCooldown, size);
        }
    }
}