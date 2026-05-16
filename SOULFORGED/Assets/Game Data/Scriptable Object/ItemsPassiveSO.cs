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
    
    // Optional: Tambahkan cost untuk upgrade
}

[CreateAssetMenu(fileName = "New Passive Item", menuName = "ScriptableObjects/Items/Passive")]
public class ItemsPassiveSO : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    
    [Header("Settings")]
    public PassiveBuffType buffType;
    public List<ItemLevelData> levels;

    [HideInInspector] public PlayerData playerData;
    
    private float lastAppliedValue = 0f;
    private int lastAppliedLevel = 0;

    public int CurrentLevel
    {
        get
        {
            if (playerData != null)
                return playerData.GetPassiveItemLevel(this);
            return 1;
        }
    }
    
    public int MaxLevel => levels.Count;
    public bool IsMaxLevel => CurrentLevel >= MaxLevel;
    
    public string CurrentLevelName
    {
        get
        {
            ItemLevelData data = GetLevelData(CurrentLevel);
            return $"Lv.{data.level} - {data.levelDescription}";
        }
    }
    
    // ✅ TAMBAH: Ambil biaya upgrade
    
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
                // ✅ PERBAIKI: Cooldown reduction pakai + karena efeknya mengurangi cooldown
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
                if (data.health > data.maxHealth)
                    data.health = data.maxHealth;
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
            case PassiveBuffType.ProjectileDamage:
                data.projectileDamageMultiplier += value;
                Debug.Log($"[{itemName}] +{value} Projectile Damage → Total: {data.projectileDamageMultiplier}");
                break;
            case PassiveBuffType.ProjectileSpeed:
                data.projectileSpeedMultiplier += value;
                Debug.Log($"[{itemName}] +{value} Projectile Speed → Total: {data.projectileSpeedMultiplier}");
                break;
            case PassiveBuffType.ProjectileLifetime:
                data.projectileLifetimeMultiplier += value;
                Debug.Log($"[{itemName}] +{value} Projectile Lifetime → Total: {data.projectileLifetimeMultiplier}");
                break;
            case PassiveBuffType.CooldownReduction:
                // Nilai modifierValue adalah persen (0.1 = 10% reduction)
                data.cooldownReductionMultiplier += value;
                
                // Clamp ke max 75% biar gak OP
                if (data.cooldownReductionMultiplier > 0.75f)
                {
                    data.cooldownReductionMultiplier = 0.75f;
                }
                Debug.Log($"[{itemName}] CD Reduction +{value * 100}% → Total: {data.cooldownReductionMultiplier * 100}%");
                break;
            case PassiveBuffType.ProjectileCount:
                data.projectileCountMultiplier += value;
                Debug.Log($"[{itemName}] +{value} Projectile Count → Total: {data.projectileCountMultiplier}");
                break;
            case PassiveBuffType.Defense:
                data.defense += value;
                Debug.Log($"[{itemName}] +{value} Defense → Total: {data.defense}");
                break;
            case PassiveBuffType.MaxHealth:
                data.maxHealth += value;
                data.health += value;
                Debug.Log($"[{itemName}] +{value} Max Health → Total: {data.maxHealth}");
                break;
        }
    }

    public ItemLevelData GetLevelData(int currentLevel)
    {
        int index = Mathf.Clamp(currentLevel - 1, 0, levels.Count - 1);
        return levels[index];
    }
    
    public void ResetTracking()
    {
        lastAppliedValue = 0f;
        lastAppliedLevel = 0;
    }
}