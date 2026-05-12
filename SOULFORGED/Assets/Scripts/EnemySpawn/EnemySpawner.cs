using System.Collections.Generic;
using UnityEngine;

public partial class EnemySpawner : MonoBehaviour
{
    [Header("Data References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private List<EnemiesData> enemyTypes;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 20f;
    [SerializeField] private float spawnInterval = 1.5f;
    
    [Header("Stage Limits")]
    // Seret GameObject Lantai/Ground ke sini di Inspector
    [SerializeField] private Collider stageCollider; 
    // Margin agar musuh tidak spawn tepat di ujung tembok (opsional)
    [SerializeField] private float stageMargin = 1f;

    [Header("Limits")]
    [SerializeField] private int maxActiveEnemies = 3;
    [SerializeField] private string enemyTag = "Enemy";

    private float timer;

    void Update()
    {
        if (playerData == null || stageCollider == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
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
        float originalY = selectedEnemyData.enemyPrefab.transform.position.y;

        // 1. Hitung posisi acak di sekitar player seperti biasa
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float spawnX = playerData.playerPosition.x + (Mathf.Cos(randomAngle) * spawnRadius);
        float spawnZ = playerData.playerPosition.z + (Mathf.Sin(randomAngle) * spawnRadius);

        // 2. BATASI posisi tersebut agar tetap di dalam batas StageCollider
        Bounds bounds = stageCollider.bounds;

        // Kita batasi X dan Z agar tidak melewati batas min dan max dari collider lantai
        float clampedX = Mathf.Clamp(spawnX, bounds.min.x + stageMargin, bounds.max.x - stageMargin);
        float clampedZ = Mathf.Clamp(spawnZ, bounds.min.z + stageMargin, bounds.max.z - stageMargin);

        Vector3 spawnPosition = new Vector3(clampedX, originalY, clampedZ);

        // 3. Spawn musuh
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
    }

    public void UpdateWaveSettings(int newMax, float newInterval, List<EnemiesData> newPool)
    {
        maxActiveEnemies = newMax;
        spawnInterval = newInterval;
        enemyTypes = newPool; // Ganti list musuh sesuai wave sekarang
        Debug.Log("Spawner Updated for New Wave!");
    }   
}