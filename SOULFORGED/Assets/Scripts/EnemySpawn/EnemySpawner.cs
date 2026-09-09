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
    [SerializeField] private Collider stageCollider; 
    [SerializeField] private float stageMargin = 1f;

    [Header("Limits")]
    [SerializeField] private int maxActiveEnemies = 3;
    [SerializeField] private string enemyTag = "Enemy";

    // ✅ VARIABEL UNTUK MENYIMPAN MULTIPLIER CURRENT WAVE
    private float currentDamageMultiplier = 1f;
    private float currentHealthMultiplier = 1f;
    private float currentSpeedMultiplier = 1f;
    
    // ✅ TAMBAHAN BARU: Variable untuk difficulty multiplier
    private float difficultyHealthMultiplier = 1f;
    private float difficultyDamageMultiplier = 1f;

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

        float clampedX = Mathf.Clamp(spawnX, bounds.min.x + stageMargin, bounds.max.x - stageMargin);
        float clampedZ = Mathf.Clamp(spawnZ, bounds.min.z + stageMargin, bounds.max.z - stageMargin);

        Vector3 spawnPosition = new Vector3(clampedX, originalY, clampedZ);

        // 3. Spawn musuh
        GameObject enemyObject = Instantiate(selectedEnemyData.enemyPrefab, spawnPosition, Quaternion.identity);

        if (enemyObject.TryGetComponent(out EnemiesBase enemyScript))
        {
            // ✅ APPLY MULTIPLIER KE STATS ENEMY
            // Gabungkan wave multiplier dengan difficulty multiplier
            float finalHealth = selectedEnemyData.health * currentHealthMultiplier * difficultyHealthMultiplier;
            float finalSpeed = selectedEnemyData.speed * currentSpeedMultiplier;
            float finalDamage = selectedEnemyData.baseDamage * currentDamageMultiplier * difficultyDamageMultiplier;
            
            enemyScript.Setup(
                finalHealth,           // Health dengan wave & difficulty multiplier
                finalSpeed,           // Speed dengan wave multiplier
                finalDamage,          // Damage dengan wave & difficulty multiplier
                selectedEnemyData.attackInterval, 
                selectedEnemyData.expReward
            );
            
            // Debug log untuk testing
            Debug.Log($"Spawn Enemy: HP {finalHealth} (Wave: {currentHealthMultiplier}x, Difficulty: {difficultyHealthMultiplier}x), " +
                     $"DMG {finalDamage} (Wave: {currentDamageMultiplier}x, Difficulty: {difficultyDamageMultiplier}x)");
        }
    }

    /// <summary>
    /// Update wave settings dengan multiplier untuk monster
    /// </summary>
    public void UpdateWaveSettings(int newMax, float newInterval, List<EnemiesData> newPool)
    {
        maxActiveEnemies = newMax;
        spawnInterval = newInterval;
        enemyTypes = newPool;
        Debug.Log($"Spawner Updated: MaxEnemy={newMax}, Interval={newInterval}, EnemyTypes={newPool.Count}");
    }
    
    /// <summary>
    /// Update wave settings LENGKAP dengan multiplier
    /// </summary>
    public void UpdateWaveSettings(int newMax, float newInterval, List<EnemiesData> newPool, 
        float damageMultiplier, float healthMultiplier, float speedMultiplier)
    {
        maxActiveEnemies = newMax;
        spawnInterval = newInterval;
        enemyTypes = newPool;
        
        // ✅ SIMPAN MULTIPLIER
        currentDamageMultiplier = damageMultiplier;
        currentHealthMultiplier = healthMultiplier;
        currentSpeedMultiplier = speedMultiplier;
        
        Debug.Log($"Spawner Updated: Wave Multiplier - Damage: {damageMultiplier}x, Health: {healthMultiplier}x, Speed: {speedMultiplier}x");
    }
    
    // ✅ TAMBAHAN BARU: Method untuk set difficulty multiplier
    /// <summary>
    /// Set difficulty multiplier dari DifficultyManager
    /// </summary>
    public void SetDifficultyMultipliers(float healthMultiplier, float damageMultiplier)
    {
        difficultyHealthMultiplier = healthMultiplier;
        difficultyDamageMultiplier = damageMultiplier;
        
        Debug.Log($"Difficulty Multiplier Applied - Health: {healthMultiplier}x, Damage: {damageMultiplier}x");
    }
    
    /// <summary>
    /// Reset multiplier ke default (opsional)
    /// </summary>
    public void ResetMultipliers()
    {
        currentDamageMultiplier = 1f;
        currentHealthMultiplier = 1f;
        currentSpeedMultiplier = 1f;
        
        // ✅ TAMBAHAN: Reset difficulty multiplier juga
        difficultyHealthMultiplier = 1f;
        difficultyDamageMultiplier = 1f;
    }
}