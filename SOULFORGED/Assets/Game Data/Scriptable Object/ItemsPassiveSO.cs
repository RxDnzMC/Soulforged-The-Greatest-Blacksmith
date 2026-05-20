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
    
    [Header("Settings (In-Game)")]
    public PassiveBuffType buffType;
    public List<ItemLevelData> levels;

    [HideInInspector] public PlayerData playerData;
    
    private float lastAppliedValue = 0f;
    private int lastAppliedLevel = 0;

    // ==========================================
    // IN-GAME LEVELS
    // ==========================================
    public int CurrentLevel => playerData != null ? playerData.GetInGamePassiveLevel(this) : 1;
    public int MaxLevel => levels.Count;
    public bool IsMaxLevel => CurrentLevel >= MaxLevel;
    
    // ==========================================
    // OUT-GAME (PERMANENT) LEVELS
    // ==========================================
    public int PermanentLevel => playerData != null ? playerData.GetPermanentPassiveLevel(this) : 0;
    
    [Header("Out-Game (Permanent) Settings")]
    public int MaxPermanentLevel = 5;
    [Tooltip("Bonus stat per level permanen (Isi angka bulat, misal 10 untuk +10% Damage atau +10 HP)")]
    public float permanentBonusPerLevel = 5f;
    public int permanentGoldBaseCost = 100;
    public int permanentSoulsBaseCost = 10;
    
    public bool IsMaxPermanentLevel => PermanentLevel >= MaxPermanentLevel;

    public void UpgradeLevel(PlayerData data, int newLevel)
    {
        RemoveLastEffect(data);
        ApplyEffect(data, newLevel);
    }
    
    void RemoveLastEffect(PlayerData data)
    {
        if (lastAppliedLevel <= 0) return;
        
        switch (buffType)
        {
            case PassiveBuffType.ProjectileDamage: data.projectileDamageMultiplier -= lastAppliedValue; break;
            case PassiveBuffType.ProjectileSpeed: data.projectileSpeedMultiplier -= lastAppliedValue; break;
            case PassiveBuffType.ProjectileLifetime: data.projectileLifetimeMultiplier -= lastAppliedValue; break;
            case PassiveBuffType.CooldownReduction: data.cooldownReductionMultiplier -= lastAppliedValue; break;
            case PassiveBuffType.ProjectileCount: data.projectileCountMultiplier -= lastAppliedValue; break;
            case PassiveBuffType.Defense: data.defense -= lastAppliedValue; break;
            case PassiveBuffType.MaxHealth: 
                data.maxHealth -= lastAppliedValue; 
                if (data.health > data.maxHealth) data.health = data.maxHealth; 
                break;
        }
        
        lastAppliedValue = 0f;
        lastAppliedLevel = 0;
    }
    
    public void ApplyEffect(PlayerData data, int level)
    {
        ItemLevelData levelData = GetLevelData(level);
        float value = levelData.modifierValue;
        
        lastAppliedValue = value;
        lastAppliedLevel = level;

        switch (buffType)
        {
            case PassiveBuffType.ProjectileDamage: data.projectileDamageMultiplier += value; break;
            case PassiveBuffType.ProjectileSpeed: data.projectileSpeedMultiplier += value; break;
            case PassiveBuffType.ProjectileLifetime: data.projectileLifetimeMultiplier += value; break;
            case PassiveBuffType.CooldownReduction: 
                data.cooldownReductionMultiplier += value; 
                if (data.cooldownReductionMultiplier > 0.75f) data.cooldownReductionMultiplier = 0.75f;
                break;
            case PassiveBuffType.ProjectileCount: data.projectileCountMultiplier += value; break;
            case PassiveBuffType.Defense: data.defense += value; break;
            case PassiveBuffType.MaxHealth: data.maxHealth += value; data.health += value; break;
        }
    }

    public ItemLevelData GetLevelData(int currentLevel)
    {
        int index = Mathf.Clamp(currentLevel - 1, 0, levels.Count - 1);
        if (levels.Count == 0) return new ItemLevelData();
        return levels[index];
    }
    
    public void ResetTracking()
    {
        lastAppliedValue = 0f;
        lastAppliedLevel = 0;
    }
}