using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerData", menuName ="ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject 
{
    public float health;
    public float maxHealth;
    public float defense;
    public Vector3 playerPosition;
    public int level;
    public float exp;
    public float expToNextLevel;
    public float expMultiplier;
    public float projectileDamageMultiplier;
    public float projectileSpeedMultiplier;
    public float projectileLifetimeMultiplier;
    public float cooldownReductionMultiplier;
    public float projectileCountMultiplier;
    public int Globalgold;
    public int Globalsouls;
    
    // ==========================================
    // BASE STATS (Angka Murni Tanpa Upgrade Out-Game)
    // ==========================================
    [Header("Base Stats (Angka Awal)")]
    public float baseMaxHealth = 100f;
    public float baseDefense = 0f;
    public float baseMoveSpeed = 5f;
    public float baseDamageMultiplier = 1f;
    public float baseCooldownReduction = 0f;
    
    public int gold; 
    public int souls;

    public ItemsSO DefaultItem;
    [Header("Default Slot untuk Item Aktif")]
    public List<ItemsSO> DefaultSlot = new List<ItemsSO>(4);
    public List<ItemsPassiveSO> DefaultPassiveSlot = new List<ItemsPassiveSO>(4);

    [Header("List Item Aktif dan Pasif")]
    public List<ItemsSO> ActiveItems = new List<ItemsSO>();
    public List<ItemsPassiveSO> PassivesItems = new List<ItemsPassiveSO>();

    // ==========================================
    // DUAL LEVEL SYSTEM (WEAPON & PASIF JADI SATU)
    // ==========================================
    [Header("Permanent Levels (Gak Reset)")]
    public List<ItemLevelPair> permanentItemLevels = new List<ItemLevelPair>();
    
    [Header("In-Game Levels (Reset Tiap Game)")]
    public List<ItemLevelPair> inGameItemLevels = new List<ItemLevelPair>();
    
    [System.Serializable]
    public class ItemLevelPair
    {
        public ItemsSO activeItem;
        public ItemsPassiveSO passiveItem;
        public int level;
    }

    // ==========================================
    // MAGIC FUNCTION: MENGHITUNG OTOMATIS
    // ==========================================
    private void OnValidate()
    {
        RecalculatePermanentStats();
    }

    [ContextMenu("🚨 RESET ALL PERMANENT UPGRADES (OUT-GAME) 🚨")]
    public void ResetPermanentUpgrades()
    {
        permanentItemLevels.Clear();
        RecalculatePermanentStats();
        Debug.Log("<b>BERHASIL:</b> List Permanent dikosongkan dan Stat kembali ke semula!");
    }

    public void RecalculatePermanentStats()
    {
        // 1. Reset ke Base
        maxHealth = baseMaxHealth;
        defense = baseDefense;
        float moveSpd = baseMoveSpeed;
        projectileDamageMultiplier = baseDamageMultiplier;
        cooldownReductionMultiplier = baseCooldownReduction;

        // 2. Tambahkan Bonus dari List
        if (permanentItemLevels != null)
        {
            foreach (var pair in permanentItemLevels)
            {
                if (pair.passiveItem != null)
                {
                    // Level 1 = 1x bonus, Level 2 = 2x bonus
                    float bonusAmount = pair.level * pair.passiveItem.permanentBonusPerLevel;
                    
                    switch (pair.passiveItem.buffType)
                    {
                        case PassiveBuffType.MaxHealth: maxHealth += bonusAmount; break;
                        case PassiveBuffType.Defense: defense += bonusAmount; break;
                        case PassiveBuffType.ProjectileSpeed: moveSpd += bonusAmount; break;
                        case PassiveBuffType.ProjectileDamage: projectileDamageMultiplier += (bonusAmount / 100f); break;
                        case PassiveBuffType.CooldownReduction: cooldownReductionMultiplier += (bonusAmount / 100f); break;
                    }
                }
            }
        }

        PlayerPrefs.SetFloat("MoveSpeed", moveSpd);
    }
    
    public float CalculateDamage(float incomingDamage)
    {
        float reducedDamage = incomingDamage - defense;
        return reducedDamage < 1f ? 1f : reducedDamage;
    }
    
    public void TakeDamage(float damage)
    {
        health -= CalculateDamage(damage);
        if (health <= 0) Debug.Log("PLAYER HAS DIED!");
    }
    
    public void Heal(float amount)
    {
        health += amount;
        if (health > maxHealth) health = maxHealth;
    }
    
    // ==========================================
    // FUNGSI LEVELING & GETTER
    // ==========================================
    public int GetItemLevel(ItemsSO item) => GetInGameLevel(item);
    
    public int GetInGameLevel(ItemsSO item)
    {
        if (item == null) return 1; 
        ItemLevelPair gamePair = inGameItemLevels.Find(x => x.activeItem == item);
        return gamePair != null ? gamePair.level : 1;
    }
    
    public int GetPermanentLevel(ItemsSO item)
    {
        if (item == null) return 0; 
        ItemLevelPair permPair = permanentItemLevels.Find(x => x.activeItem == item);
        return permPair != null ? permPair.level : 0;
    }

    public int GetInGamePassiveLevel(ItemsPassiveSO item)
    {
        if (item == null) return 1;
        ItemLevelPair gamePair = inGameItemLevels.Find(x => x.passiveItem == item);
        return gamePair != null ? gamePair.level : 1;
    }

    public int GetPermanentPassiveLevel(ItemsPassiveSO item)
    {
        if (item == null) return 0;
        ItemLevelPair permPair = permanentItemLevels.Find(x => x.passiveItem == item);
        return permPair != null ? permPair.level : 0;
    }
    
    public int GetPassiveItemLevel(ItemsPassiveSO item) => GetInGamePassiveLevel(item);

    // ==========================================
    // FUNGSI SET LEVEL
    // ==========================================
    public void SetPermanentLevel(ItemsSO item, int level)
    {
        if (item == null) return;
        ItemLevelPair pair = permanentItemLevels.Find(x => x.activeItem == item);
        if (pair != null) pair.level = level;
        else permanentItemLevels.Add(new ItemLevelPair { activeItem = item, level = level });
    }
    
    public void SetInGameLevel(ItemsSO item, int level)
    {
        if (item == null) return;
        ItemLevelPair pair = inGameItemLevels.Find(x => x.activeItem == item);
        if (pair != null) pair.level = level;
        else inGameItemLevels.Add(new ItemLevelPair { activeItem = item, level = level });
    }
    
    public void SetPermanentPassiveLevel(ItemsPassiveSO item, int level)
    {
        if (item == null) return;
        ItemLevelPair pair = permanentItemLevels.Find(x => x.passiveItem == item);
        if (pair != null) pair.level = level;
        else permanentItemLevels.Add(new ItemLevelPair { passiveItem = item, level = level });
    }
    
    public void SetInGamePassiveLevel(ItemsPassiveSO item, int level)
    {
        if (item == null) return;
        ItemLevelPair pair = inGameItemLevels.Find(x => x.passiveItem == item);
        if (pair != null) pair.level = level;
        else inGameItemLevels.Add(new ItemLevelPair { passiveItem = item, level = level });
    }
    
    // ==========================================
    // RESET FUNGSI (IN-GAME)
    // ==========================================
    public void ResetInGameLevels()
    {
        inGameItemLevels.Clear();
    }
    
    public void ResetAllLevels()
    {
        permanentItemLevels.Clear();
        inGameItemLevels.Clear();
        RecalculatePermanentStats();
    }

    public void ResetData()
    {
        RecalculatePermanentStats(); 
        health = maxHealth; 
        gold = 0;
        souls = 0;
        ActiveItems.Clear();
        PassivesItems.Clear();
        ResetInGameLevels();
        ResetInventory();
    }

    public void ResetInventory()
    {
        ActiveItems = new List<ItemsSO>(DefaultSlot);
        PassivesItems = new List<ItemsPassiveSO>(DefaultPassiveSlot);
    }
}