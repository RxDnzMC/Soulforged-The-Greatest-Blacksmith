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
    // BARU: Stat Upgrade Levels
    // ==========================================
    [Header("Stat Upgrade Levels")]
    public int maxHealthLevel = 1;
    public int defenseLevel = 1;
    public int moveSpeedLevel = 1;
    public int projectileDamageLevel = 1;
    public int cooldownReductionLevel = 1;
    
    //Temp Currency, resets every start/end of game
    public int gold; 
    public int souls;

    public ItemsSO DefaultItem;
    [Header("Default Slot untuk Item Aktif (Jangan DIISI)")]
    public List<ItemsSO> DefaultSlot = new List<ItemsSO>(4);
    public List<ItemsPassiveSO> DefaultPassiveSlot = new List<ItemsPassiveSO>(4);

    [Header("List Item Aktif dan Pasif")]
    public List<ItemsSO> ActiveItems = new List<ItemsSO>();
    public List<ItemsPassiveSO> PassivesItems = new List<ItemsPassiveSO>();

    // ==========================================
    // DUAL LEVEL SYSTEM
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
    // DEFENSE SYSTEM - DAMAGE CALCULATION
    // ==========================================
    
    /// <summary>
    /// Hitung damage setelah dikurangi defense
    /// Defense bisa berupa flat reduction atau persen
    /// </summary>
    public float CalculateDamage(float incomingDamage)
    {
        float reducedDamage = incomingDamage;
        
        // Defense flat reduction
        reducedDamage -= defense;
        
        // Minimal damage 1 biar tetap kerasa
        if (reducedDamage < 1f)
            reducedDamage = 1f;
        
        return reducedDamage;
    }
    
    /// <summary>
    /// Method untuk mengambil damage dengan perhitungan defense
    /// </summary>
    public void TakeDamage(float damage)
    {
        float finalDamage = CalculateDamage(damage);
        health -= finalDamage;
        
        Debug.Log($"[Damage] Incoming: {damage} → After Defense ({defense}): {finalDamage} → Health: {health}/{maxHealth}");
        
        // Trigger death event jika health <= 0
        if (health <= 0)
        {
            OnPlayerDeath();
        }
    }
    
    /// <summary>
    /// Event saat player mati (bisa di-override atau panggil event)
    /// </summary>
    private void OnPlayerDeath()
    {
        Debug.Log("PLAYER HAS DIED!");
        // Nanti panggil GameManager untuk handle game over
        // GameManager.Instance?.HandlePlayerDeath();
    }
    
    /// <summary>
    /// Heal player
    /// </summary>
    public void Heal(float amount)
    {
        health += amount;
        if (health > maxHealth)
            health = maxHealth;
        
        Debug.Log($"[Heal] +{amount} HP → Health: {health}/{maxHealth}");
    }
    
    /// <summary>
    /// Upgrade defense (dipanggil dari item atau level up)
    /// </summary>
    public void UpgradeDefense(float additionalDefense)
    {
        defense += additionalDefense;
        Debug.Log($"[Defense Up] Defense now: {defense}");
    }
    
    // ==========================================
    
    // Ambil total level item aktif (permanent + in-game)
    public int GetItemLevel(ItemsSO item)
    {
        if (item == null) return 1;
        
        int permLevel = 1;
        int gameLevel = 0;
        
        ItemLevelPair permPair = permanentItemLevels.Find(x => x.activeItem == item);
        if (permPair != null) permLevel = permPair.level;
        
        ItemLevelPair gamePair = inGameItemLevels.Find(x => x.activeItem == item);
        if (gamePair != null) gameLevel = gamePair.level;
        
        return permLevel + gameLevel;
    }
    
    // ==========================================
    // BARU: Ambil in-game level doang (tanpa permanent)
    // ==========================================
    public int GetInGameLevel(ItemsSO item)
    {
        if (item == null) return 0;
        
        ItemLevelPair gamePair = inGameItemLevels.Find(x => x.activeItem == item);
        return gamePair != null ? gamePair.level : 0;
    }
    
    public int GetInGamePassiveLevel(ItemsPassiveSO item)
    {
        if (item == null) return 0;
        
        ItemLevelPair gamePair = inGameItemLevels.Find(x => x.passiveItem == item);
        return gamePair != null ? gamePair.level : 0;
    }
    
    public int GetPermanentLevel(ItemsSO item)
    {
        if (item == null) return 1;
        
        ItemLevelPair permPair = permanentItemLevels.Find(x => x.activeItem == item);
        return permPair != null ? permPair.level : 1;
    }
    // ==========================================
    
    // Set level permanent (dari scene upgrade)
    public void SetPermanentLevel(ItemsSO item, int level)
    {
        if (item == null) return;
        
        ItemLevelPair pair = permanentItemLevels.Find(x => x.activeItem == item);
        if (pair != null)
            pair.level = level;
        else
            permanentItemLevels.Add(new ItemLevelPair { activeItem = item, level = level });
    }
    
    // Set level in-game (reset tiap game)
    public void SetInGameLevel(ItemsSO item, int level)
    {
        if (item == null) return;
        
        ItemLevelPair pair = inGameItemLevels.Find(x => x.activeItem == item);
        if (pair != null)
            pair.level = level;
        else
            inGameItemLevels.Add(new ItemLevelPair { activeItem = item, level = level });
    }
    
    // Set level permanent (pasif)
    public void SetPermanentPassiveLevel(ItemsPassiveSO item, int level)
    {
        if (item == null) return;
        
        ItemLevelPair pair = permanentItemLevels.Find(x => x.passiveItem == item);
        if (pair != null)
            pair.level = level;
        else
            permanentItemLevels.Add(new ItemLevelPair { passiveItem = item, level = level });
    }
    
    // Set level in-game (pasif)
    public void SetInGamePassiveLevel(ItemsPassiveSO item, int level)
    {
        if (item == null) return;
        
        ItemLevelPair pair = inGameItemLevels.Find(x => x.passiveItem == item);
        if (pair != null)
            pair.level = level;
        else
            inGameItemLevels.Add(new ItemLevelPair { passiveItem = item, level = level });
    }

    // Ambil level item pasif (pure in-game)
    public int GetPassiveItemLevel(ItemsPassiveSO item)
    {
        if (item == null) return 1;
        
        ItemLevelPair gamePair = inGameItemLevels.Find(x => x.passiveItem == item);
        return gamePair != null ? gamePair.level : 1;
    }
    
    // Reset in-game level aja (permanent tetep)
    public void ResetInGameLevels()
    {
        inGameItemLevels.Clear();
    }
    
    // Reset semua (development)
    public void ResetAllLevels()
    {
        permanentItemLevels.Clear();
        inGameItemLevels.Clear();
    }
    // ==========================================

    public void ResetData()
    {
        // Reset stats
        health = maxHealth;
        defense = 0; // Reset defense juga
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