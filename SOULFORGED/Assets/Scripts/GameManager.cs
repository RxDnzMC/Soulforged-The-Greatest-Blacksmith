using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private UIController UIGame;
    [SerializeField] private UpgradeSelectionUI upgradeUI;
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
        foreach (var item in passiveItems)
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
        
        // Manual update UI
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
        
        if (upgradeUI != null)
        {
            upgradeUI.ShowUpgradeChoices(choices);
        }
        else
        {
            // Fallback
            Debug.LogWarning("UpgradeSelectionUI belum di-assign!");
            Debug.Log($"=== LEVEL {level} - PILIH UPGRADE ===");
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i] is ItemsSO activeItem)
                    Debug.Log($"{i+1}. [AKTIF] {activeItem.itemName} (Lv.{playerData.GetItemLevel(activeItem)})");
                else if (choices[i] is ItemsPassiveSO passiveItem)
                    Debug.Log($"{i+1}. [PASIF] {passiveItem.itemName} (Lv.{playerData.GetPassiveItemLevel(passiveItem)})");
            }
            
            isUpgradeChoosing = false;
            Time.timeScale = 1f;
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
        
        // Gak perlu update UI inventory manual
    }
    
    void ApplyActiveItemUpgrade(ItemsSO item)
    {
        if (item == null) return;
        item.playerData = playerData;
        
        int existingIndex = playerData.ActiveItems.IndexOf(item);
        
        if (existingIndex >= 0)
        {
            if (item.IsMaxLevel)
            {
                Debug.Log($"{item.itemName} udah MAX LEVEL!");
                return;
            }
            
            int currentInGameLevel = playerData.GetInGameLevel(item);
            int newInGameLevel = currentInGameLevel + 1;
            
            playerData.SetInGameLevel(item, newInGameLevel);
            
            Debug.Log($"UPGRADE: {item.itemName} ke Level {playerData.GetItemLevel(item)}!");
        }
        else
        {
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
                while (playerData.ActiveItems.Count <= emptySlot)
                    playerData.ActiveItems.Add(null);
                
                playerData.ActiveItems[emptySlot] = item;
                playerData.SetInGameLevel(item, 0);
                
                Debug.Log($"ITEM BARU: {item.itemName} di slot {emptySlot}! Level: {playerData.GetItemLevel(item)}");
            }
            else
            {
                Debug.LogWarning("Inventory aktif penuh! (Maks 4)");
            }
        }
        
        playerAttack.equippedActiveItems = playerData.ActiveItems;
    }
    
    void ApplyPassiveItemUpgrade(ItemsPassiveSO item)
    {
        if (item == null) return;
        item.playerData = playerData;
        
        int existingIndex = playerData.PassivesItems.IndexOf(item);
        
        if (existingIndex >= 0)
        {
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
                while (playerData.PassivesItems.Count <= emptySlot)
                    playerData.PassivesItems.Add(null);
                
                playerData.PassivesItems[emptySlot] = item;
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
        
        playerData.ResetInGameLevels();
        
        foreach (var item in passiveItems)
        {
            if (item != null) item.ResetTracking();
        }
        
        Time.timeScale = 1f;
        isUpgradeChoosing = false;
    }

    void OnApplicationQuit()
    {
        if (TemporaryPlayerData != null)
            ResetGame();
    }
}