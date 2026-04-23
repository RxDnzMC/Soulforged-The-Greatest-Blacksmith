using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public Transform spawnPoint;
    // Panggil SO yang sudah kamu buat, bukan ngetik variabel satu-satu lagi
    public EnemiesData dataMusuh; 
    private int spawnCount = 0;

    void Update()
    {
        if (spawnCount < 5) // Contoh: batasi spawn hanya 5 kali
        {
            SpawnEnemy();
            spawnCount++;
        }
        
    }

    void SpawnEnemy()
    {
        if (dataMusuh != null)
        {
            // Cukup panggil fungsi Use yang sudah kita bikin di SO tadi!
            // Fungsi ini sudah otomatis melakukan Instantiate DAN Setup.
            dataMusuh.Use(spawnPoint);
        }
        else
        {
            Debug.LogWarning("Waduh, dataMusuh di Spawner belum diisi!");
        }
    }
}