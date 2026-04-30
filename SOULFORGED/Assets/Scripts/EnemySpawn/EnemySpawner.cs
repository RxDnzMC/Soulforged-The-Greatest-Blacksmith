using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private List<EnemiesData> enemyTypes;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnInterval = 1.5f;
    [Header("Limits")]
    [SerializeField] private int maxActiveEnemies = 3; // Batas maksimal musuh di layar  
    [SerializeField] private string enemyTag = "Enemy"; // Pastikan Prefab musuh punya Tag "Enemy"

    private float timer;
    private int totalSpawnedSoFar = 0; // Menghitung sudah berapa banyak yang di-spawn

    void Update()
    {
        if (playerData == null) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            // 2. CEK BATAS AKTIF: Hitung berapa musuh yang masih hidup di layar
            int currentEnemyCount = GameObject.FindGameObjectsWithTag(enemyTag).Length;

            if (currentEnemyCount < maxActiveEnemies)
            {
                SpawnEnemyOffScreen();
                timer = 0f;
            }
        }
    }

    private void SpawnEnemyOffScreen()
    {
        if (enemyTypes.Count == 0) return;

        EnemiesData selectedEnemyData = enemyTypes[Random.Range(0, enemyTypes.Count)];

        //Ambil posisi Y asli dari Prefab musuh yang dipilih
        float originalY = selectedEnemyData.enemyPrefab.transform.position.y;

        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float spawnX = playerData.playerPosition.x + (Mathf.Cos(randomAngle) * spawnRadius);
        float spawnZ = playerData.playerPosition.z + (Mathf.Sin(randomAngle) * spawnRadius);

        Vector3 spawnPosition = new Vector3(spawnX, originalY, spawnZ);

        GameObject enemyObject = Instantiate(selectedEnemyData.enemyPrefab, spawnPosition, Quaternion.identity);

        if (enemyObject.TryGetComponent(out EnemiesBase enemyScript))
        {
            enemyScript.Setup(
                selectedEnemyData.health, 
                selectedEnemyData.speed, 
                selectedEnemyData.baseDamage, 
                selectedEnemyData.attackInterval, 
                selectedEnemyData.expReward
            );
        }

        // Tambahkan hitungan setiap kali berhasil spawn
        totalSpawnedSoFar++;
    }
}