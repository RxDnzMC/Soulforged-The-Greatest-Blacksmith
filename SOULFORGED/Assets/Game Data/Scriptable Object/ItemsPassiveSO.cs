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

[System.Serializable] // Agar muncul di Inspector Unity
public struct ItemLevelData
    {
        public int level;            // Angka level (1, 2, 3...)
        public float modifierValue;  // Nilai kekuatannya (misal: +10 Damage)
        public string levelDescription; // Teks penjelasan (misal: "Menambah 1 peluru")
    }

[CreateAssetMenu(fileName = "New Passive Item", menuName = "ScriptableObjects/Items/Passive")]
public class ItemsPassiveSO : ScriptableObject
{
    // Ini yang akan jadi Dropdown/Combo Box di Inspector
    
    public string itemName;
    public Sprite itemIcon;
    
    [Header("Settings")]
    public PassiveBuffType buffType;
    public List<ItemLevelData> levels;

    // FUNGSI BARU: Item ini sekarang bisa mengurus efeknya sendiri
    public void ApplyEffect(PlayerData data, int level)
    {
        ItemLevelData levelData = GetLevelData(level);
        float value = levelData.modifierValue;

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
                data.health += value; // Bonus: Langsung nambah HP saat ini
                break;
        }
    }

    public ItemLevelData GetLevelData(int currentLevel)
    {
        int index = Mathf.Clamp(currentLevel - 1, 0, levels.Count - 1);
        return levels[index];
    }
}