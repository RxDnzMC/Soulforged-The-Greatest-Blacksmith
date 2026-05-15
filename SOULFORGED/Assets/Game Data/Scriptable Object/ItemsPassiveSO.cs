using UnityEngine;
using System.Collections.Generic;

public enum PassiveBuffType
{
    ProjectileDamage,
    ProjectileSpeed,
    ProjectileLifetime,
    CooldownReduction,
    ProjectileCount,
    Defense,
    MaxHealth
}

[System.Serializable]
public struct ItemLevelData
{
    public int level;
    public float modifierValue;
    public string levelDescription;
}

[CreateAssetMenu(fileName = "New Passive Item", menuName = "ScriptableObjects/Items/Passive")]
public class ItemsPassiveSO : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    
    [Header("Settings")]
    public PassiveBuffType buffType;
    public List<ItemLevelData> levels;

    // REFERENCE KE PLAYERDATA
    [HideInInspector] public PlayerData playerData;
    
    private float lastAppliedValue = 0f;
    private int lastAppliedLevel = 0;

    // ==========================================
    // BARU: Level System (Pure In-Game)
    // ==========================================
    public int CurrentLevel
    {
        get
        {
            if (playerData != null)
                return playerData.GetPassiveItemLevel(this);
            return 1;
        }
    }
    
    public int MaxLevel
    {
        get { return levels.Count; }
    }
    
    public bool IsMaxLevel
    {
        get { return CurrentLevel >= MaxLevel; }
    }
    
    public string CurrentLevelName
    {
        get
        {
            ItemLevelData data = GetLevelData(CurrentLevel);
            return $"Lv.{data.level} - {data.levelDescription}";
        }
    }
    // ==========================================
    
    // Upgrade level (otomatis ngurangin efek lama)
    public void UpgradeLevel(PlayerData data, int newLevel)
    {
        // 1. Hapus efek lama
        RemoveLastEffect(data);
        
        // 2. Apply efek baru
        ApplyEffect(data, newLevel);
    }
    
    void RemoveLastEffect(PlayerData data)
    {
        if (lastAppliedLevel <= 0) return;
        
        switch (buffType)
        {
            case PassiveBuffType.ProjectileDamage:
                data.projectileDamageMultiplier -= lastAppliedValue;
                break;
            case PassiveBuffType.ProjectileSpeed:
                data.projectileSpeedMultiplier -= lastAppliedValue;
                break;
            case PassiveBuffType.ProjectileLifetime:
                data.projectileLifetimeMultiplier -= lastAppliedValue;
                break;
            case PassiveBuffType.CooldownReduction:
                data.cooldownReductionMultiplier += lastAppliedValue;
                break;
            case PassiveBuffType.ProjectileCount:
                data.projectileCountMultiplier -= lastAppliedValue;
                break;
            case PassiveBuffType.Defense:
                data.defense -= lastAppliedValue;
                break;
            case PassiveBuffType.MaxHealth:
                data.maxHealth -= lastAppliedValue;
                data.health -= lastAppliedValue;
                break;
        }
        
        lastAppliedValue = 0f;
        lastAppliedLevel = 0;
    }
    
    public void ApplyEffect(PlayerData data, int level)
    {
        ItemLevelData levelData = GetLevelData(level);
        float value = levelData.modifierValue;
        
        // Simpan value terakhir
        lastAppliedValue = value;
        lastAppliedLevel = level;

        switch (buffType)
        {
            case PassiveBuffType.ProjectileDamage:
                data.projectileDamageMultiplier += value;
                break;
            case PassiveBuffType.ProjectileSpeed:
                data.projectileSpeedMultiplier += value;
                break;
            case PassiveBuffType.ProjectileLifetime:
                data.projectileLifetimeMultiplier += value;
                break;
            case PassiveBuffType.CooldownReduction:
                data.cooldownReductionMultiplier -= value; 
                break;
            case PassiveBuffType.ProjectileCount:
                data.projectileCountMultiplier += value;
                break;
            case PassiveBuffType.Defense:
                data.defense += value;
                break;
            case PassiveBuffType.MaxHealth:
                data.maxHealth += value;
                data.health += value;
                break;
        }
    }

    public ItemLevelData GetLevelData(int currentLevel)
    {
        int index = Mathf.Clamp(currentLevel - 1, 0, levels.Count - 1);
        return levels[index];
    }
    
    // Reset tracking (dipanggil pas game over)
    public void ResetTracking()
    {
        lastAppliedValue = 0f;
        lastAppliedLevel = 0;
    }
}