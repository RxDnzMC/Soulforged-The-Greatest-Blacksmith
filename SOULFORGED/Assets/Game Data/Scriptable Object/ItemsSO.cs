using UnityEngine;
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
    public float spreadAngle; // <-- Ditambahkan ke sini agar per-level
    public string levelDescription;
}

public class ItemsSO : ScriptableObject, IAttackItem 
{
    public string itemName;
    public Sprite itemIcon;
    public GameObject projectilePrefab;
    
    [Header("Audio")]
    public string shootSound = "Fireball Shoot";

    // Homing settings dihapus dari sini agar Inspector rapi

    [Header("Level Data (In-Game)")]
    public List<ActiveItemLevelData> levels = new List<ActiveItemLevelData>();

    [HideInInspector] public PlayerData playerData;
    
    protected float _lastUsedTime = -999f;

    // ==========================================
    // LEVEL IN-GAME & PERMANENT
    // ==========================================
    public int CurrentLevel => playerData != null ? playerData.GetInGameLevel(this) : 1;
    public int PermanentLevel => playerData != null ? playerData.GetPermanentLevel(this) : 0;
    
    [Header("Out-Game (Permanent) Settings")]
    public int MaxPermanentLevel = 5; 
    public float permanentDamageBonusPercent = 10f; 
    public int permanentGoldBaseCost = 100;
    public int permanentSoulsBaseCost = 10;

    public bool IsMaxPermanentLevel => PermanentLevel >= MaxPermanentLevel;
    public int MaxLevel => levels.Count;
    public bool IsMaxLevel => CurrentLevel >= MaxLevel;
    
    public string CurrentLevelName
    {
        get
        {
            ActiveItemLevelData data = GetCurrentLevelData();
            return $"Lv.{data.level} - {data.levelDescription}";
        }
    }
    
    public ActiveItemLevelData GetCurrentLevelData()
    {
        int level = CurrentLevel;
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        if (levels.Count == 0) return new ActiveItemLevelData();
        return levels[index];
    }
    
    public ActiveItemLevelData GetLevelData(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, levels.Count - 1);
        if (levels.Count == 0) return new ActiveItemLevelData();
        return levels[index];
    }
    
    public float CurrentCooldown 
    { 
        get 
        { 
            float baseCooldown = GetCurrentLevelData().cooldown;
            if (playerData != null && playerData.cooldownReductionMultiplier > 0)
            {
                float reduction = playerData.cooldownReductionMultiplier;
                float finalCooldown = baseCooldown * (1f - reduction);
                return Mathf.Max(0.2f, finalCooldown);
            }
            return baseCooldown;
        }
    }
    
    public bool isReady => Time.time >= _lastUsedTime + CurrentCooldown;

    public int GetCurrentLevel() => CurrentLevel;

    public void OnEquipped()
    {
        if (playerData != null)
        {
            int savedLevel = playerData.GetInGameLevel(this);
            if (savedLevel <= 1) playerData.SetInGameLevel(this, 1);
        }
    }

    public virtual void Use(Transform spawnPoint)
    {
        if (projectilePrefab == null) return;

        _lastUsedTime = Time.time;

        ActiveItemLevelData levelData = GetCurrentLevelData();
        
        float currentDamage = levelData.damage;
        float currentSize = levelData.sizeMultiplier;
        float currentSpeed = levelData.speed;
        float currentLifetime = levelData.lifetime;
        int currentCount = levelData.projectileCount;
        float currentSpreadAngle = levelData.spreadAngle; // Ambil nilai angle dari level
        
        float permDamageBonus = 1f + (PermanentLevel * (permanentDamageBonusPercent / 100f));
        currentDamage *= permDamageBonus;

        if (playerData != null)
        {
            if (playerData.projectileDamageMultiplier > 0) currentDamage *= playerData.projectileDamageMultiplier;
            if (playerData.projectileSpeedMultiplier > 0) currentSpeed *= playerData.projectileSpeedMultiplier;
            if (playerData.projectileLifetimeMultiplier > 0) currentLifetime *= playerData.projectileLifetimeMultiplier;
            if (playerData.projectileCountMultiplier > 0)
            {
                currentCount = Mathf.RoundToInt(currentCount * playerData.projectileCountMultiplier);
                currentCount = Mathf.Max(1, currentCount);
            }
        }

        SpawnProjectiles(spawnPoint, currentCount, currentDamage, currentSize, currentSpeed, currentLifetime, currentSpreadAngle);
    }

    // Parameter 'spreadAngle' ditambahkan agar diteruskan ke child class
    protected virtual void SpawnProjectiles(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime, float spreadAngle)
    {
        for (int i = 0; i < count; i++)
        {
            FireSingle(spawnPoint, damage, size, speed, lifetime, 0f);
        }
    }

    protected void FireSingle(Transform spawnPoint, float damage, float size, float speed, float lifetime, float angleOffset)
    {
        Quaternion rot = spawnPoint.rotation * Quaternion.Euler(0, angleOffset, 0);
        GameObject go = Instantiate(projectilePrefab, spawnPoint.position, rot);
        
        if (go.TryGetComponent(out IProjectile projectile))
            // Parameter Homing diisi 0f sementara agar tidak error di interface IProjectile
            projectile.Setup(speed, damage, lifetime, 0f, 0f, 0f, CurrentCooldown, size);
            
        if (SoundManager.Instance != null && !string.IsNullOrEmpty(shootSound)) 
            SoundManager.Instance.PlaySound3D(shootSound, spawnPoint.position);
    }

    protected virtual void OnEnable()
    {
        _lastUsedTime = -999f;
    }
}