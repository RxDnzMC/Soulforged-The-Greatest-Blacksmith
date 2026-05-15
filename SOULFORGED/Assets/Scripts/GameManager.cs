using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private UIController UIGame;
    private PlayerData TemporaryPlayerData;
    
    [Header("Wave Settings")]
    [SerializeField] private List<WaveConfig> waves;
    
    public GameObject Boss1; 
    public GameObject Boss2; 
    private bool isBoss1Spawned = false;
    private bool isBoss2Spawned = false;
    private int currentWaveIndex = 0;
    private int localMaxHP = 1000;

    [Header("Timer Display")]
    public string timerString;
    private float elapsedTime = 0f;
    
    [Header("Item Lists")]
    [SerializeField] ListItemActive listItemActive;
    [SerializeField] ListItemPassive listItemPassive;
    [SerializeField] PlayerAttack playerAttack;
    
    private List<ItemsSO> ItemList = new List<ItemsSO>();
    private List<ItemsPassiveSO> passiveItems = new List<ItemsPassiveSO>();
    private List<ItemsSO> CurrentItem = new List<ItemsSO>();
    
    [Header("Roguelike Upgrade Settings")]
    [SerializeField] int choicesPerLevel = 3;
    [SerializeField] int maxActiveSlots = 4;
    [SerializeField] int maxPassiveSlots = 4;
    
    bool isPlayerDead = false;
    bool isUpgradeChoosing = false;
    bool isNewMusicAdded = false;

    [System.Serializable]
    public struct WaveConfig {
        public string waveName;
        public float startTime;
        public int maxActiveEnemies;
        public float spawnInterval;
        public List<EnemiesData> enemyPool;
        public bool isBossWave;
        public bool isBossWave2;
    }

    void Awake()
    {
        localMaxHP = 1000;
        playerData.maxHealth = localMaxHP;
        playerData.health = playerData.maxHealth;
        TemporaryPlayerData = Instantiate(playerData); 
    }

    void Start()
    {
        ItemList = listItemActive.activeItems;
        passiveItems = listItemPassive.passiveItems;
        
        foreach (var item in ItemList)
        {
            if (item != null) item.playerData = playerData;
        }
        
        if (playerData.DefaultItem != null)
            playerData.DefaultItem.playerData = playerData;
        
        defaultItem();
        
        if (playerData.DefaultItem != null)
            Debug.Log($"[DEBUG] Default Item Level: {playerData.DefaultItem.CurrentLevel}");
    }

    void defaultItem() 
    {
        if (playerData.level == 1) 
        {
            CurrentItem = new List<ItemsSO>(playerData.DefaultSlot);
            
            if (playerData.DefaultItem != null)
            {
                playerData.DefaultItem.playerData = playerData;
                
                int savedLevel = playerData.GetItemLevel(playerData.DefaultItem);
                if (savedLevel <= 0)
                    playerData.SetPermanentLevel(playerData.DefaultItem, 1);
                
                CurrentItem[0] = playerData.DefaultItem;
            }
            
            playerData.ActiveItems = CurrentItem;
            playerAttack.equippedActiveItems = playerData.ActiveItems;
            playerAttack.equippedPassiveItems = playerData.PassivesItems;
            
            if (playerData.DefaultItem != null)
                Debug.Log($"Default Item Equipped! Level: {playerData.DefaultItem.CurrentLevel}");
        }
    }

    void Update()
    {
        UpdateTimer();
        CheckWaveProgression();
        HandleLevelUp();
        UIGame.UpdateHealthUI(playerData.health, playerData.maxHealth);
        UIGame.UpdateXPUI(playerData.exp, playerData.expToNextLevel);
    }

    void UpdateTimer()
    {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
        UIGame.UpdateUITimer(timerString);
    }

    void CheckWaveProgression()
    {
        if (currentWaveIndex + 1 < waves.Count)
        {
            if (elapsedTime >= waves[currentWaveIndex + 1].startTime)
            {
                currentWaveIndex++;
                ApplyWaveSettings(waves[currentWaveIndex]);
            }
        }
    }

    void ApplyWaveSettings(WaveConfig config)
    {
        Debug.Log("Masuk Wave: " + config.waveName);
        enemySpawner.UpdateWaveSettings(config.maxActiveEnemies, config.spawnInterval, config.enemyPool);

        if (config.isBossWave && !isBoss1Spawned)
        {
            Instantiate(Boss1, new Vector3(0, 0, 10), Quaternion.identity);
            isBoss1Spawned = true;
        }

        if (config.isBossWave2 && !isBoss2Spawned)
        {
            Instantiate(Boss2, new Vector3(0, 0, 10), Quaternion.identity);
            isBoss2Spawned = true;
        }
    }

    void HandleLevelUp() 
    {
        if (playerData.exp >= playerData.expToNextLevel) 
        {
            playerData.level += 1;
            playerData.exp -= playerData.expToNextLevel;
            playerData.expToNextLevel *= 1.3f;

            CheckLevelEvents(playerData.level);
            ShowUpgradeChoices(playerData.level);

            Debug.Log($"Level Up! Sekarang level {playerData.level}");
        }
    }

    // ==========================================
    // SISTEM ROGUELIKE UPGRADE
    // ==========================================
    
    void ShowUpgradeChoices(int level)
    {
        if (isUpgradeChoosing) return;
        isUpgradeChoosing = true;
        
        Time.timeScale = 0f;
        
        List<object> choices = GetRandomUpgradeChoices();
        
        // TODO: Tampilin UI pilihan
        Debug.Log($"=== LEVEL {level} - PILIH UPGRADE ===");
        for (int i = 0; i < choices.Count; i++)
        {
            if (choices[i] is ItemsSO activeItem)
            {
                int currentLevel = playerData.GetItemLevel(activeItem);
                Debug.Log($"{i+1}. [AKTIF] {activeItem.itemName} (Lv.{currentLevel})");
            }
            else if (choices[i] is ItemsPassiveSO passiveItem)
            {
                int currentLevel = playerData.GetPassiveItemLevel(passiveItem);
                Debug.Log($"{i+1}. [PASIF] {passiveItem.itemName} (Lv.{currentLevel})");
            }
        }
    }
    
    List<object> GetRandomUpgradeChoices()
    {
        List<object> allItems = new List<object>();
        
        foreach (var item in ItemList)
        {
            if (item != null) allItems.Add(item);
        }
        foreach (var item in passiveItems)
        {
            if (item != null) allItems.Add(item);
        }
        
        List<object> shuffled = allItems.OrderBy(x => Random.value).ToList();
        List<object> choices = new List<object>();
        
        for (int i = 0; i < Mathf.Min(choicesPerLevel, shuffled.Count); i++)
        {
            choices.Add(shuffled[i]);
        }
        
        return choices;
    }
    
    public void OnUpgradeSelected(object selectedItem)
    {
        isUpgradeChoosing = false;
        Time.timeScale = 1f;
        
        if (selectedItem is ItemsSO activeItem)
            ApplyActiveItemUpgrade(activeItem);
        else if (selectedItem is ItemsPassiveSO passiveItem)
            ApplyPassiveItemUpgrade(passiveItem);
    }
    
    void ApplyActiveItemUpgrade(ItemsSO item)
    {
        if (item == null) return;
        item.playerData = playerData;
        
        // Cek apakah item udah ada di inventory
        int existingIndex = playerData.ActiveItems.IndexOf(item);
        
        if (existingIndex >= 0)
        {
            // SUDAH PUNYA → UPGRADE IN-GAME LEVEL
            int currentInGameLevel = playerData.GetItemLevel(item) - GetPermanentLevel(item);
            int newInGameLevel = currentInGameLevel + 1;
            
            playerData.SetInGameLevel(item, newInGameLevel);
            
            Debug.Log($"UPGRADE: {item.itemName} ke Level {playerData.GetItemLevel(item)}!");
        }
        else
        {
            // BELUM PUNYA → CARI SLOT KOSONG
            int emptySlot = -1;
            for (int i = 0; i < maxActiveSlots; i++)
            {
                if (i >= playerData.ActiveItems.Count || playerData.ActiveItems[i] == null)
                {
                    emptySlot = i;
                    break;
                }
            }
            
            if (emptySlot >= 0)
            {
                // Pastikan list cukup besar
                while (playerData.ActiveItems.Count <= emptySlot)
                    playerData.ActiveItems.Add(null);
                
                playerData.ActiveItems[emptySlot] = item;
                playerData.SetInGameLevel(item, 0);
                
                Debug.Log($"ITEM BARU: {item.itemName} di slot {emptySlot}! Level: {playerData.GetItemLevel(item)}");
            }
            else
            {
                Debug.LogWarning("Inventory aktif penuh!");
                // TODO: Kasih opsi replace item
            }
        }
        
        playerAttack.equippedActiveItems = playerData.ActiveItems;
    }
    
    void ApplyPassiveItemUpgrade(ItemsPassiveSO item)
    {
        if (item == null) return;
        item.playerData = playerData;
        
        // Cek apakah item udah ada di inventory
        int existingIndex = playerData.PassivesItems.IndexOf(item);
        
        if (existingIndex >= 0)
        {
            // SUDAH PUNYA → UPGRADE IN-GAME LEVEL
            if (item.IsMaxLevel)
            {
                Debug.Log($"{item.itemName} udah MAX LEVEL!");
                return;
            }
            
            int currentLevel = item.CurrentLevel;
            int newLevel = currentLevel + 1;
            
            playerData.SetInGamePassiveLevel(item, newLevel);
            item.UpgradeLevel(playerData, newLevel);
            
            Debug.Log($"UPGRADE PASSIVE: {item.itemName} Lv.{currentLevel} → Lv.{newLevel}!");
        }
        else
        {
            // BELUM PUNYA → CARI SLOT KOSONG
            int emptySlot = -1;
            for (int i = 0; i < maxPassiveSlots; i++)
            {
                if (i >= playerData.PassivesItems.Count || playerData.PassivesItems[i] == null)
                {
                    emptySlot = i;
                    break;
                }
            }
            
            if (emptySlot >= 0)
            {
                // Pastikan list cukup besar
                while (playerData.PassivesItems.Count <= emptySlot)
                    playerData.PassivesItems.Add(null);
                
                playerData.PassivesItems[emptySlot] = item;
                
                // Set level ke 1 (in-game)
                playerData.SetInGamePassiveLevel(item, 1);
                item.ApplyEffect(playerData, 1);
                
                Debug.Log($"ITEM PASSIVE BARU: {item.itemName} di slot {emptySlot}! Level: 1");
            }
            else
            {
                Debug.LogWarning("Inventory pasif penuh! (Maks 4)");
            }
        }
        
        playerAttack.equippedPassiveItems = playerData.PassivesItems;
}   
    
    int GetPermanentLevel(ItemsSO item)
    {
        PlayerData.ItemLevelPair pair = playerData.permanentItemLevels.Find(x => x.activeItem == item);
        return pair != null ? pair.level : 1;
    }
    // ==========================================

    void CheckLevelEvents(int level) 
    {
        if (level >= 5 && !isNewMusicAdded) 
        {
            MusicManager.Instance?.PlayTrack("Boss (15 Minute)");
            isNewMusicAdded = true;
        }
    }

    void OnDestroy()
    {
        ResetGame();
        playerData.ResetData();
    }

    public void ResetGame()
    {
        if (TemporaryPlayerData == null) return;
        
        string defaultValues = JsonUtility.ToJson(TemporaryPlayerData);
        JsonUtility.FromJsonOverwrite(defaultValues, playerData);
        
        // Reset in-game level
        playerData.ResetInGameLevels();
        
        // Reset tracking di semua item pasif
        foreach (var item in passiveItems)
        {
            if (item != null) item.ResetTracking();
        }
    }

    void OnApplicationQuit()
    {
        if (TemporaryPlayerData != null)
            ResetGame();
    }
}