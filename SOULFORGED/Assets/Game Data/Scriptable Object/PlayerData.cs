using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
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