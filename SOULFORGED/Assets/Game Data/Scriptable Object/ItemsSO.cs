using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct ActiveItemLevelData
{
    public int level;
    
    [Header("Stats di Level Ini")]
    public float damage;
    public float cooldown;
    public float speed;
    public float lifetime;
    public float sizeMultiplier;
    public int projectileCount;
    
    public string levelDescription;
    
    [Header("Cost to Upgrade TO This Level")]
    public int goldCost;
    public int soulsCost;
}

[CreateAssetMenu(fileName = "New Active Item", menuName = "ScriptableObjects/Items/Active", order = 1)]
public class ItemsSO : ScriptableObject, IAttackItem
{
    public string itemName;
    public Sprite itemIcon;
    public GameObject projectilePrefab;
    
    [Header("Homing Swarm Settings (Tetap)")]
    public float turnSpeed = 15f;
    public float homingDelay = 0.3f;
    public float scatterAngle = 45f;
    
    [Header("Multi Shot Settings")]
    public float multiShotDelay = 0.15f;

    [Header("Level Data")]
    public List<ActiveItemLevelData> levels = new List<ActiveItemLevelData>();

    [HideInInspector] public PlayerData playerData;
    [HideInInspector] public MonoBehaviour coroutineRunner;
    
    private float _lastUsedTime = -999f;
    private bool _isBursting = false;

    public int CurrentLevel
    {
        get
        {
            if (playerData != null)
                return playerData.GetItemLevel(this);
            return 1;
        }
    }
    
    public string CurrentLevelName
    {
        get
        {
            ActiveItemLevelData data = GetCurrentLevelData();
            return $"Lv.{data.level} - {data.levelDescription}";
        }
    }
    
    public int MaxLevel => levels.Count;
    public bool IsMaxLevel => CurrentLevel >= MaxLevel;
    
    public ActiveItemLevelData GetCurrentLevelData()
    {
        int level = CurrentLevel;
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        
        if (levels.Count == 0)
        {
            Debug.LogError($"Item {itemName} gak punya level data!");
            return new ActiveItemLevelData();
        }
        
        return levels[index];
    }
    
    public ActiveItemLevelData GetLevelData(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        if (levels.Count == 0) return new ActiveItemLevelData();
        return levels[index];
    }
    
    public int GetUpgradeGoldCost()
    {
        if (IsMaxLevel) return 0;
        return GetLevelData(CurrentLevel + 1).goldCost;
    }
    
    public int GetUpgradeSoulsCost()
    {
        if (IsMaxLevel) return 0;
        return GetLevelData(CurrentLevel + 1).soulsCost;
    }
    
    public float CurrentCooldown 
    { 
        get 
        { 
            float baseCooldown = GetCurrentLevelData().cooldown;
            
            if (playerData != null && playerData.cooldownReductionMultiplier > 0)
            {
                // ✅ PAKE PERKALIAN (bukan pembagian)
                float reduction = playerData.cooldownReductionMultiplier;
                float finalCooldown = baseCooldown * (1f - reduction);
                
                // Minimal cooldown 0.2 detik
                return Mathf.Max(0.2f, finalCooldown);
            }
            return baseCooldown;
        }
    }
    
    public bool isReady => Time.time >= _lastUsedTime + CurrentCooldown && !_isBursting;

    public int GetCurrentLevel() => CurrentLevel;

    public void OnEquipped()
    {
        if (playerData != null)
        {
            int savedLevel = playerData.GetItemLevel(this);
            if (savedLevel <= 0)
                playerData.SetPermanentLevel(this, 1);
            
            Debug.Log($"Item {itemName} equipped! Level: {CurrentLevel}/{MaxLevel}");
        }
    }

    public virtual void Use(Transform spawnPoint)
    {
        if (projectilePrefab == null) return;
        if (_isBursting) return;

        _lastUsedTime = Time.time;

        ActiveItemLevelData levelData = GetCurrentLevelData();
        
        // ✅ AMBIL BASE STATS DARI LEVEL DATA
        float currentDamage = levelData.damage;
        float currentSize = levelData.sizeMultiplier;
        float currentSpeed = levelData.speed;
        float currentLifetime = levelData.lifetime;
        int currentCount = levelData.projectileCount;
        
        // ✅ TERAPKAN SEMUA MULTIPLIER DARI PLAYERDATA (PASSIVE ITEMS)
        if (playerData != null)
        {
            // Damage Multiplier
            if (playerData.projectileDamageMultiplier > 0)
                currentDamage *= playerData.projectileDamageMultiplier;
            
            // Speed Multiplier
            if (playerData.projectileSpeedMultiplier > 0)
                currentSpeed *= playerData.projectileSpeedMultiplier;
            
            // Lifetime Multiplier
            if (playerData.projectileLifetimeMultiplier > 0)
                currentLifetime *= playerData.projectileLifetimeMultiplier;
            
            // Count Multiplier
            if (playerData.projectileCountMultiplier > 0)
            {
                currentCount = Mathf.RoundToInt(currentCount * playerData.projectileCountMultiplier);
                currentCount = Mathf.Max(1, currentCount);
            }
        }

        // Burst fire jika count > 1
        if (currentCount > 1 && coroutineRunner != null)
        {
            coroutineRunner.StartCoroutine(BurstFire(spawnPoint, currentCount, currentDamage, currentSize, currentSpeed, currentLifetime));
        }
        else
        {
            FireSingle(spawnPoint, currentDamage, currentSize, currentSpeed, currentLifetime);
        }
    }

    IEnumerator BurstFire(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime)
    {
        _isBursting = true;
        
        for (int i = 0; i < count; i++)
        {
            float angle = i == 0 ? 0 : UnityEngine.Random.Range(-scatterAngle / 3, scatterAngle / 3);
            Quaternion rot = spawnPoint.rotation * Quaternion.Euler(0, angle, 0);
            
            GameObject go = Instantiate(projectilePrefab, spawnPoint.position, rot);
            
            if (go.TryGetComponent(out IProjectile projectile))
            {
                projectile.Setup(speed, damage, lifetime, 
                            turnSpeed, homingDelay, scatterAngle, 
                            CurrentCooldown, size);
            }
            
            if (i < count - 1)
                yield return new WaitForSeconds(multiShotDelay);
        }
        
        _isBursting = false;
        
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound3D("Fireball Shoot", spawnPoint.position);
    }

    void FireSingle(Transform spawnPoint, float damage, float size, float speed, float lifetime)
    {
        GameObject go = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
        
        if (go.TryGetComponent(out IProjectile projectile))
        {
            projectile.Setup(speed, damage, lifetime, 
                        turnSpeed, homingDelay, scatterAngle, 
                        CurrentCooldown, size);
        }
    }

    private void OnEnable()
    {
        _lastUsedTime = -999f;
        _isBursting = false;
    }
}