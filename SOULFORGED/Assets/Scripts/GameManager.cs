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
    [Tooltip("Interval spawn boss dalam DETIK (30 = 30 detik, 180 = 3 menit, 300 = 5 menit)")]
    [SerializeField] private float bossSpawnIntervalInSeconds = 5f; // ✅ TAMBAHAN BARU   
    [Tooltip("Tambahan darah bos setiap bos baru muncul (Misal: 0.5 berarti Bos kedua HP-nya 1.5x)")]
    [SerializeField] private float bossHealthIncreasePerSpawn = 0.5f;
    [Tooltip("Tambahan damage bos setiap bos baru muncul")]
    [SerializeField] private float bossDamageIncreasePerSpawn = 0.2f;
    
    private GameObject currentBoss = null;
    private bool isWaitingForBossDeath = false;
    private int bossSpawnCount = 0; // Menghitung sudah berapa kali bos muncul
    private float nextBossSpawnTime = 0f; // ✅ TAMBAHAN BARU

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
    

    // MULTIPLIER (DIAMBIL DARI DIFFICULTY MANAGER)
        private float goldMultiplier = 1f;
        private float soulMultiplier = 1f;
        private float monsterHealthMultiplier = 1f;
        private float monsterDamageMultiplier = 1f;
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
        
        // ✅ TAMBAHKAN INI: Apply difficulty settings
        ApplyDifficultySettings();
        
        ItemList = listItemActive.activeItems;
        passiveItems = listItemPassive.passiveItems;
        
        foreach (var item in ItemList) if (item != null) item.playerData = playerData;
        foreach (var item in passiveItems) if (item != null) item.playerData = playerData;
        if (playerData.DefaultItem != null) playerData.DefaultItem.playerData = playerData;
        
        defaultItem();
        nextBossSpawnTime = bossSpawnIntervalInSeconds;
    }


    // ✅ TAMBAHKAN METHOD INI
    void ApplyDifficultySettings()
    {
        if (DifficultyManager.Instance == null) return;
        
        DifficultySettings difficulty = DifficultyManager.Instance.GetCurrentDifficulty();
        
        // Simpan multiplier
        goldMultiplier = difficulty.goldMultiplier;
        soulMultiplier = difficulty.soulMultiplier;
        monsterHealthMultiplier = difficulty.monsterHealthMultiplier;
        monsterDamageMultiplier = difficulty.monsterDamageMultiplier;
        
        // ✅ APPLY KE ENEMY SPAWNER
        if (enemySpawner != null)
        {
            enemySpawner.SetDifficultyMultipliers(monsterHealthMultiplier, monsterDamageMultiplier);
        }
        
        Debug.Log($"Difficulty Applied: {difficulty.difficultyName} | Gold: {goldMultiplier}x | Soul: {soulMultiplier}x | HP: {monsterHealthMultiplier}x | DMG: {monsterDamageMultiplier}x");
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
        CheckBossSpawn();
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
        // ✅ TAMBAHKAN INI: Reset music effect saat player mati
        MusicManager.Instance?.SetPauseEffect(false);
        
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

        // ✅ BAGIAN SPAWN BOSS DIHAPUS DARI SINI
        // Sekarang spawn boss ditangani oleh CheckBossSpawn() di Update()
    }

    void CheckBossSpawn()
    {
        if (elapsedTime >= nextBossSpawnTime)
        {
            SpawnBoss();
            nextBossSpawnTime = elapsedTime + bossSpawnIntervalInSeconds;
        }
    }

    // ==========================================
    // SISTEM BOSS SCALING TERPISAH
    // ==========================================
    void SpawnBoss()
    {
        if (isWaitingForBossDeath && currentBoss != null) return;

        isWaitingForBossDeath = true;

        // Hitung Multiplier KHUSUS BOSS
        float bossHealthMult = 1f + (bossSpawnCount * bossHealthIncreasePerSpawn);
        float bossDamageMult = 1f + (bossSpawnCount * bossDamageIncreasePerSpawn);
        
        // ✅ TAMBAHKAN: Gabungkan dengan difficulty multiplier
        float finalHealthMult = bossHealthMult * monsterHealthMultiplier;
        float finalDamageMult = bossDamageMult * monsterDamageMultiplier;

        // Menentukan bos mana yang muncul (Ganti-gantian 1 dan 2)
        GameObject bossPrefab = (bossSpawnCount % 2 != 0) ? Boss2 : Boss1;
        string bossMusic = (bossSpawnCount % 2 != 0) ? "Boss (30 Minute)" : "Boss (15 Minute)";

        MusicManager.Instance?.PlayTrack(bossMusic);
        
        currentBoss = Instantiate(bossPrefab, new Vector3(0, 0, 10), Quaternion.identity);
        ApplyMultiplierToBoss(currentBoss, finalDamageMult, finalHealthMult);

        if (enemySpawner != null) enemySpawner.enabled = false;
        
        Debug.Log($"Boss Muncul di Detik {elapsedTime}! Ini Bos Ke-{bossSpawnCount + 1}. HP: {finalHealthMult}x, DMG: {finalDamageMult}x");
        
        bossSpawnCount++; 
    }

    void ApplyMultiplierToBoss(GameObject boss, float damageMultiplier, float healthMultiplier)
    {
        if (boss == null) return;
        if (boss.TryGetComponent(out EnemiesBase bossStats))
        {
            // Di sini kamu bisa menerapkan multiplier ke script boss (jika boss pakai EnemiesBase)
            // Contoh (uncomment jika EnemiesBase kamu mendukung public variabel ini):
            bossStats._health *= healthMultiplier;
            bossStats._damage *= damageMultiplier;
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

            // Cek apakah masih ada upgrade yang tersedia
            if (HasAvailableUpgrades())
            {
                ShowUpgradeChoices(playerData.level);
            }
            else
            {
                // Tidak ada upgrade tersedia, langsung lanjut game tanpa pause
                Debug.Log("Tidak ada upgrade tersedia, melanjutkan game...");
            }
        }
    }

    void ShowUpgradeChoices(int level)
    {
        if (isUpgradeChoosing) return;
        
        List<object> choices = GetRandomUpgradeChoices();
        
        // Cek jika tidak ada pilihan upgrade
        if (choices.Count == 0)
        {
            Debug.Log("Tidak ada pilihan upgrade, melanjutkan game...");
            return; // Tidak pause game
        }
        
        isUpgradeChoosing = true;
        Time.timeScale = 0f;
        // ✅ TAMBAHKAN INI: Pause effect pada musik
        MusicManager.Instance?.SetPauseEffect(true);
        
        if (upgradeUI != null) 
        {
            upgradeUI.ShowUpgradeChoices(choices);
        }
        else 
        { 
            isUpgradeChoosing = false; 
            Time.timeScale = 1f; 
        }
    }
    
    List<object> GetRandomUpgradeChoices()
    {
        List<object> validItems = new List<object>();
        int currentActiveCount = playerData.ActiveItems.Count(x => x != null);
        int currentPassiveCount = playerData.PassivesItems.Count(x => x != null);
        
        foreach (var item in ItemList)
        {
            if (item == null) continue;
            if (playerData.ActiveItems.Contains(item)) 
            { 
                if (!item.IsMaxLevel) validItems.Add(item); 
            }
            else if (currentActiveCount < maxActiveSlots) 
            {
                validItems.Add(item);
            }
        }
        
        foreach (var item in passiveItems)
        {
            if (item == null) continue;
            if (playerData.PassivesItems.Contains(item)) 
            { 
                if (!item.IsMaxLevel) validItems.Add(item); 
            }
            else if (currentPassiveCount < maxPassiveSlots) 
            {
                validItems.Add(item);
            }
        }
        
        // Jika tidak ada item valid, return list kosong
        if (validItems.Count == 0)
        {
            Debug.LogWarning("Tidak ada upgrade yang tersedia!");
            return new List<object>();
        }
        
        List<object> shuffled = validItems.OrderBy(x => UnityEngine.Random.value).ToList();
        List<object> choices = new List<object>();
        for (int i = 0; i < Mathf.Min(choicesPerLevel, shuffled.Count); i++) 
        {
            choices.Add(shuffled[i]);
        }
        return choices;
    }

    bool HasAvailableUpgrades()
    {
        int currentActiveCount = playerData.ActiveItems.Count(x => x != null);
        int currentPassiveCount = playerData.PassivesItems.Count(x => x != null);
        
        // Cek item aktif
        foreach (var item in ItemList)
        {
            if (item == null) continue;
            
            if (playerData.ActiveItems.Contains(item))
            {
                // Item sudah dimiliki, cek apakah masih bisa di-upgrade
                if (!item.IsMaxLevel) return true;
            }
            else if (currentActiveCount < maxActiveSlots)
            {
                // Slot masih tersedia untuk item baru
                return true;
            }
        }
        
        // Cek item pasif
        foreach (var item in passiveItems)
        {
            if (item == null) continue;
            
            if (playerData.PassivesItems.Contains(item))
            {
                // Item sudah dimiliki, cek apakah masih bisa di-upgrade
                if (!item.IsMaxLevel) return true;
            }
            else if (currentPassiveCount < maxPassiveSlots)
            {
                // Slot masih tersedia untuk item baru
                return true;
            }
        }
        
        return false;
    }
    public void OnUpgradeSelected(object selectedItem)
    {
        isUpgradeChoosing = false;
        Time.timeScale = 1f;
        // ✅ TAMBAHKAN INI: Resume effect musik setelah memilih upgrade
        MusicManager.Instance?.SetPauseEffect(false);
        
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
        if (gameOverPanel != null) 
        {
            MusicManager.Instance?.SetPauseEffect(false);
            gameOverPanel.ShowGameOver();
        }
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