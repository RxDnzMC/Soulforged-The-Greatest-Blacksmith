using UnityEngine;

[CreateAssetMenu(fileName = "New Light Hole", menuName = "ScriptableObjects/Weapons/Light Hole")]
public class LightHoleSO : ItemsSO 
{
    protected override void SpawnProjectiles(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime, float spreadAngle)
    {
        // Cari posisi pusat Player
        PlayerAttack playerRoot = spawnPoint.GetComponentInParent<PlayerAttack>();
        Transform playerTransform = playerRoot != null ? playerRoot.transform : spawnPoint;

        // Cari apakah Manager Light Hole sudah ada di player
        LightHoleMechanic existingAura = null;
        LightHoleMechanic[] activeAuras = playerTransform.GetComponentsInChildren<LightHoleMechanic>();

        foreach (var aura in activeAuras)
        {
            if (aura.gameObject.name == "LightHoleManager")
            {
                existingAura = aura;
                break;
            }
        }

        // Jika sudah ada, cukup update stat dan jumlahnya
        if (existingAura != null)
        {
            existingAura.UpdateStats(speed, damage, size, count);
        }
        else
        {
            // Jika belum ada, buat Pivot perputaran di tengah tubuh Player
            GameObject pivot = new GameObject("LightHoleManager");
            pivot.transform.SetParent(playerTransform);
            pivot.transform.localPosition = new Vector3(0f, 1f, 0f); // Naikkan sedikit agar tidak tenggelam
            
            // Pasang skrip mekanik
            LightHoleMechanic mechanic = pivot.AddComponent<LightHoleMechanic>();
            
            // Masukkan Prefab dan Jumlah bola awal
            mechanic.InitializeManager(count, projectilePrefab);
            
            // Panggil Setup standar wajib dari IProjectile
            mechanic.Setup(speed, damage, lifetime, 0f, 0f, 0f, CurrentCooldown, size);
        }
    }
}