using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Random AoE Weapon", menuName = "ScriptableObjects/Weapons/Random AoE")]
public class RandomAoESO : ItemsSO
{
    protected override void SpawnProjectiles(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime, float spreadAngle)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        List<GameObject> validEnemies = new List<GameObject>();
        
        foreach (var enemy in allEnemies)
        {
            if (enemy != null && enemy.activeInHierarchy)
            {
                if (enemy.TryGetComponent(out EnemiesBase enemyBase) && enemyBase._health <= 0) continue; 

                Vector3 viewPos = mainCam.WorldToViewportPoint(enemy.transform.position);
                if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0)
                {
                    validEnemies.Add(enemy);
                }
            }
        }

        ShuffleList(validEnemies);

        for (int i = 0; i < count; i++)
        {
            Vector3 targetPos;
            Transform chosenTarget = null; // ✅ Menyimpan data musuh yang terpilih

            if (validEnemies.Count > 0)
            {
                int index = i % validEnemies.Count;
                chosenTarget = validEnemies[index].transform; // ✅ Ambil Transform musuh
                targetPos = chosenTarget.position;
            }
            else
            {
                Vector2 randomCircle = Random.insideUnitCircle * 5f; 
                targetPos = spawnPoint.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            }

            targetPos.y = 0.1f; 

            GameObject go = Instantiate(projectilePrefab, targetPos, Quaternion.identity);

            if (go.TryGetComponent(out IProjectile projectile))
            {
                projectile.Setup(speed, damage, lifetime, 0f, 0f, 0f, CurrentCooldown, size);
            }

            // ✅ KIRIM TARGET MUSUH KE SCRIPT AREA SIHIR AGAR BISA DIIKUTI
            if (go.TryGetComponent(out RandomAoEProjectile aoeScript))
            {
                aoeScript.SetTarget(chosenTarget);
            }

            if (SoundManager.Instance != null && !string.IsNullOrEmpty(shootSound)) 
            {
                SoundManager.Instance.PlaySound3D(shootSound, targetPos);
            }
        }
    }

    private void ShuffleList(List<GameObject> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            GameObject temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}