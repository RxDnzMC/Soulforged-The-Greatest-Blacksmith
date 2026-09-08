using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private UIController UIGame;
    [SerializeField] private UpgradeSelectionUI upgradeUI;
    private PlayerData TemporaryPlayerData;
    
    [Header("Wave Settings (Monster Pools)")]
    [Tooltip("Isi list ini untuk mengatur pool monster. Jika menit melebihi list, game akan memakai list terakhir.")]
    [SerializeField] private List<WaveConfig> waves;
    
    [Header("Kroco Auto Scaling Settings")]
    [Tooltip("Tambahan darah kroco setiap 1 menit")]
    [SerializeField] private float healthIncreasePerMinute = 0.5f;
    [Tooltip("Tambahan damage kroco setiap 1 menit")]
    [SerializeField] private float damageIncreasePerMinute = 0.2f;
    [Tooltip("Tambahan speed kroco setiap 1 menit")]
    [SerializeField] private float speedIncreasePerMinute = 0.05f;

    [Header("Boss Setup & Scaling")]
    public GameObject Boss1; 
    public GameObject Boss2; 
    [Tooltip("Tambahan darah bos setiap bos baru muncul (Misal: 0.5 berarti Bos kedua HP-nya 1.5x)")]
    [SerializeField] private float bossHealthIncreasePerSpawn = 0.5f;
    [Tooltip("Tambahan damage bos setiap bos baru muncul")]
    [SerializeField] private float bossDamageIncreasePerSpawn = 0.2f;
    
    private GameObject currentBoss = null;
    private bool isWaitingForBossDeath = false;
    private int bossSpawnCount = 0; // Menghitung sudah berapa kali bos muncul

    [Header("Timer Display")]
    public string timerString;
    private float elapsedTime = 0f;
    private int currentMinute = -1; // Timer tracking
    
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
    
    [Header("Level Progression Settings")]
    [SerializeField] private float baseExpRequirement = 100f;
    [SerializeField] private float expGrowthRate = 1.15f; 
    [SerializeField] private float maxExpRequirement = 5000f;
    [SerializeField] private int softCapLevel = 15; 
    [SerializeField] private float softCapGrowthRate = 1.08f; 
    
    bool isPlayerDead = false;
    bool isUpgradeChoosing = false;

    [System.Serializable]
    public struct WaveConfig {
        public string waveName;
        public int maxActiveEnemies;
        public float spawnInterval;
        public List<EnemiesData> enemyPool;
        
        [Header("Base Stats (Biarkan 1)")]
        public float baseDamageMultiplier;
        public float baseHealthMultiplier;
        public float baseSpeedMultiplier;
    }

    void Awake()
    {
        playerData.health = playerData.maxHealth;
        playerData.expToNextLevel = GetExpRequirement(playerData.level);
        TemporaryPlayerData = Instantiate(playerData); 
    }

    void Start()
    {
        Application.targetFrameRate = 60;

        ItemList = listItemActive.activeItems;
        passiveItems = listItemPassive.passiveItems;
        
        foreach (var item in ItemList) if (item != null) item.playerData = playerData;
        foreach (var item in passiveItems) if (item != null) item.playerData = playerData;
        if (playerData.DefaultItem != null) playerData.DefaultItem.playerData = playerData;
        
        defaultItem();
    }

    float GetExpRequirement(int level)
    {
        float requirement;
        if (level <= softCapLevel)
            requirement = baseExpRequirement * Mathf.Pow(expGrowthRate, level - 1);
        else
            requirement = (baseExpRequirement * Mathf.Pow(expGrowthRate, softCapLevel - 1)) * Mathf.Pow(softCapGrowthRate, level - softCapLevel);
        
        return Mathf.Min(requirement, maxExpRequirement);
    }

    void defaultItem() 
    {
        if (playerData.level == 1) 
        {
            CurrentItem = new List<ItemsSO>(playerData.DefaultSlot);
            if (playerData.DefaultItem != null)
            {
                playerData.DefaultItem.playerData = playerData;
                if (playerData.GetInGameLevel(playerData.DefaultItem) <= 1)
                    playerData.SetInGameLevel(playerData.DefaultItem, 1); 
                CurrentItem[0] = playerData.DefaultItem;
            }
            playerData.ActiveItems = CurrentItem;
            playerAttack.equippedActiveItems = playerData.ActiveItems;
            playerAttack.equippedPassiveItems = playerData.PassivesItems;
        }
    }

    void Update()
    {
        UpdateTimer();
        CheckMinuteProgression();
        CheckBossStatus();
        HandleLevelUp();
        CheckPlayerDeath();
        
        UIGame.UpdateHealthUI(playerData.health, playerData.maxHealth);
        UIGame.UpdateXPUI(playerData.exp, playerData.expToNextLevel);
    }

    void CheckPlayerDeath()
    {
        if (!isPlayerDead && playerData.health <= 0)
        {
            isPlayerDead = true;
            HandlePlayerDeath();
        }
    }

    void HandlePlayerDeath()
    {
        Debug.Log("PLAYER DIED! Game Over...");
        if (enemySpawner != null) enemySpawner.enabled = false;
        Time.timeScale = 0f;
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            if (player.TryGetComponent(out PlayerAttack playerAttackComp)) playerAttackComp.enabled = false;
            foreach (var renderer in player.GetComponentsInChildren<SpriteRenderer>()) renderer.enabled = false;
        }
        
        if (gameOverPanel != null) gameOverPanel.ShowGameOver();
        else TriggerGameOver();
    }

    void UpdateTimer()
    {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
        UIGame.UpdateUITimer(timerString);
    }

    // ==========================================
    // SISTEM AUTO SCALING
    // ==========================================
    void CheckMinuteProgression()
    {
        int passedMinutes = Mathf.FloorToInt(elapsedTime / 60f);

        if (passedMinutes > currentMinute)
        {
            currentMinute = passedMinutes;
            ApplyMinuteSettings(currentMinute);
        }
    }

    void ApplyMinuteSettings(int minute)
    {
        if (waves.Count == 0) return;

        WaveConfig config = waves[Mathf.Min(minute, waves.Count - 1)];

        // INI HANYA UNTUK KROCO
        float finalHealthMult = config.baseHealthMultiplier + (minute * healthIncreasePerMinute);
        float finalDamageMult = config.baseDamageMultiplier + (minute * damageIncreasePerMinute);
        float finalSpeedMult = config.baseSpeedMultiplier + (minute * speedIncreasePerMinute);

        enemySpawner.UpdateWaveSettings(
            config.maxActiveEnemies, 
            config.spawnInterval, 
            config.enemyPool,
            finalDamageMult,
            finalHealthMult,
            finalSpeedMult
        );

        // CEK SPAWN BOSS (Setiap KELIPATAN 3 MENIT)
        if (minute > 0 && minute % 3 == 0)
        {
            SpawnBoss(minute);
        }
    }

    // ==========================================
    // SISTEM BOSS SCALING TERPISAH
    // ==========================================
    void SpawnBoss(int minute)
    {
        if (isWaitingForBossDeath && currentBoss != null) return;

        isWaitingForBossDeath = true;

        // Hitung Multiplier KHUSUS BOSS
        // Bos pertama (bossSpawnCount = 0) akan bernilai 1x (Normal)
        // Bos kedua (bossSpawnCount = 1) akan bernilai 1.5x, dst.
        float bossHealthMult = 1f + (bossSpawnCount * bossHealthIncreasePerSpawn);
        float bossDamageMult = 1f + (bossSpawnCount * bossDamageIncreasePerSpawn);

        // Menentukan bos mana yang muncul (Ganti-gantian 1 dan 2)
        GameObject bossPrefab = (bossSpawnCount % 2 != 0) ? Boss2 : Boss1;
        string bossMusic = (bossSpawnCount % 2 != 0) ? "Boss (30 Minute)" : "Boss (15 Minute)";

        MusicManager.Instance?.PlayTrack(bossMusic);
        
        currentBoss = Instantiate(bossPrefab, new Vector3(0, 0, 10), Quaternion.identity);
        ApplyMultiplierToBoss(currentBoss, bossDamageMult, bossHealthMult);

        if (enemySpawner != null) enemySpawner.enabled = false;
        
        Debug.Log($"Boss Muncul di Menit {minute}! Ini Bos Ke-{bossSpawnCount + 1}. HP: {bossHealthMult}x");
        
        // Tambahkan hitungan bos untuk bos berikutnya
        bossSpawnCount++; 
    }

    void ApplyMultiplierToBoss(GameObject boss, float damageMultiplier, float healthMultiplier)
    {
        if (boss == null) return;
        if (boss.TryGetComponent(out EnemiesBase bossStats))
        {
            // Di sini kamu bisa menerapkan multiplier ke script boss (jika boss pakai EnemiesBase)
            // Contoh (uncomment jika EnemiesBase kamu mendukung public variabel ini):
            // bossStats.maxHealth *= healthMultiplier;
            // bossStats.damage *= damageMultiplier;
        }
    }
    
    void CheckBossStatus()
    {
        if (!isWaitingForBossDeath) return;
        
        if (currentBoss != null && currentBoss.activeInHierarchy) return; 
        
        isWaitingForBossDeath = false;
        MusicManager.Instance?.PlayTrack("Stage 1");
        
        if (enemySpawner != null)
            enemySpawner.enabled = true;
        
        Debug.Log("BOSS MATI! Kroco muncul lagi...");
    }

    // ==========================================
    // LEVELING & UPGRADE SYSTEM
    // ==========================================
    void HandleLevelUp() 
    {
        // PENTING: Jangan proses level up jika panel upgrade masih terbuka!
        if (isUpgradeChoosing) return; 

        if (playerData.exp >= playerData.expToNextLevel) 
        {
            playerData.level += 1;
            playerData.exp -= playerData.expToNextLevel;
            playerData.expToNextLevel = GetExpRequirement(playerData.level);

            ShowUpgradeChoices(playerData.level);
        }
    }

    void ShowUpgradeChoices(int level)
    {
        if (isUpgradeChoosing) return;
        isUpgradeChoosing = true;
        Time.timeScale = 0f;
        
        List<object> choices = GetRandomUpgradeChoices();
        if (upgradeUI != null) upgradeUI.ShowUpgradeChoices(choices);
        else { isUpgradeChoosing = false; Time.timeScale = 1f; }
    }
    
    List<object> GetRandomUpgradeChoices()
    {
        List<object> validItems = new List<object>();
        int currentActiveCount = playerData.ActiveItems.Count(x => x != null);
        int currentPassiveCount = playerData.PassivesItems.Count(x => x != null);
        
        foreach (var item in ItemList)
        {
            if (item == null) continue;
            if (playerData.ActiveItems.Contains(item)) { if (!item.IsMaxLevel) validItems.Add(item); }
            else if (currentActiveCount < maxActiveSlots) validItems.Add(item);
        }
        
        foreach (var item in passiveItems)
        {
            if (item == null) continue;
            if (playerData.PassivesItems.Contains(item)) { if (!item.IsMaxLevel) validItems.Add(item); }
            else if (currentPassiveCount < maxPassiveSlots) validItems.Add(item);
        }
        
        List<object> shuffled = validItems.OrderBy(x => UnityEngine.Random.value).ToList();
        List<object> choices = new List<object>();
        for (int i = 0; i < Mathf.Min(choicesPerLevel, shuffled.Count); i++) choices.Add(shuffled[i]);
        return choices;
    }
    
    public void OnUpgradeSelected(object selectedItem)
    {
        isUpgradeChoosing = false;
        Time.timeScale = 1f;
        
        if (selectedItem is ItemsSO activeItem) ApplyActiveItemUpgrade(activeItem);
        else if (selectedItem is ItemsPassiveSO passiveItem) ApplyPassiveItemUpgrade(passiveItem);
        
        if (upgradeUI != null) upgradeUI.OnUpgradeComplete();
    }
    
    void ApplyActiveItemUpgrade(ItemsSO item)
    {
        if (item == null) return;
        item.playerData = playerData;
        
        int existingIndex = playerData.ActiveItems.IndexOf(item);
        if (existingIndex >= 0)
        {
            if (item.IsMaxLevel) return;
            playerData.SetInGameLevel(item, playerData.GetInGameLevel(item) + 1);
        }
        else
        {
            int emptySlot = -1;
            for (int i = 0; i < maxActiveSlots; i++) if (i >= playerData.ActiveItems.Count || playerData.ActiveItems[i] == null) { emptySlot = i; break; }
            if (emptySlot >= 0)
            {
                while (playerData.ActiveItems.Count <= emptySlot) playerData.ActiveItems.Add(null);
                playerData.ActiveItems[emptySlot] = item;
                playerData.SetInGameLevel(item, 1); 
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
            if (item.IsMaxLevel) return;
            int newLevel = item.CurrentLevel + 1;
            playerData.SetInGamePassiveLevel(item, newLevel);
            item.UpgradeLevel(playerData, newLevel);
        }
        else
        {
            int emptySlot = -1;
            for (int i = 0; i < maxPassiveSlots; i++) if (i >= playerData.PassivesItems.Count || playerData.PassivesItems[i] == null) { emptySlot = i; break; }
            if (emptySlot >= 0)
            {
                while (playerData.PassivesItems.Count <= emptySlot) playerData.PassivesItems.Add(null);
                playerData.PassivesItems[emptySlot] = item;
                playerData.SetInGamePassiveLevel(item, 1);
                item.ApplyEffect(playerData, 1);
            }
        }
        playerAttack.equippedPassiveItems = playerData.PassivesItems;
    }

    void OnDestroy()
    {
        ResetGame();
        playerData.ResetData();
    }

    public void ResetGame()
    {
        if (TemporaryPlayerData == null) return;
        int savedGlobalGold = playerData.Globalgold;
        int savedGlobalSouls = playerData.Globalsouls;
        string defaultValues = JsonUtility.ToJson(TemporaryPlayerData);
        JsonUtility.FromJsonOverwrite(defaultValues, playerData);
        playerData.Globalgold = savedGlobalGold;
        playerData.Globalsouls = savedGlobalSouls;
        playerData.ResetInGameLevels();
        Time.timeScale = 1f;
        isUpgradeChoosing = false;
    }

    void OnApplicationQuit() { if (TemporaryPlayerData != null) ResetGame(); }
    
    [Header("Game Over")]
    [SerializeField] GameOverPanel gameOverPanel;

    public void TriggerGameOver()
    {
        if (gameOverPanel != null) gameOverPanel.ShowGameOver();
        else
        {
            playerData.Globalgold += playerData.gold;
            playerData.Globalsouls += playerData.souls;
            playerData.gold = 0;
            playerData.souls = 0;
            Time.timeScale = 1f;
            MusicManager.Instance?.PlayTrack("Main Menu");
            MusicManager.Instance?.SetPauseEffect(false);
            SceneManager.LoadSceneAsync(0);
        }
    }
}