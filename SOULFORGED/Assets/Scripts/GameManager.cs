using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("References")]
    [SerializeField] private PlayerData playerData; // data yang akan diubah didalam game dan direset menggunakan TemporaryPlayerData
    [SerializeField] private EnemySpawner enemySpawner; // Referensi ke spawner
    [SerializeField] private UIController UIGame; // Assign di Inspector
    private PlayerData TemporaryPlayerData; // Untuk Menyimpan Data Awal, jadi bisa ngereset playerData ke kondisi awal saat game over atau restart
    [Header("Wave Settings")]
    [SerializeField] private List<WaveConfig> waves;
    
    public GameObject Boss1; 
    public GameObject Boss2; 
    private bool isBoss1Spawned = false;
    private bool isBoss2Spawned = false;
    private int currentWaveIndex = 0;
    private int localMaxHP = 1000; // Nilai max HP lokal untuk perhitungan UI, bisa disesuaikan dengan kebutuhan


    [Header("Timer Display")]
    public string timerString; // Ini yang nanti dikirim ke UI Text
    private float elapsedTime = 0f;
    [SerializeField] ListItemActive listItemActive;
    [SerializeField] ListItemPassive listItemPassive;
    [SerializeField] PlayerAttack playerAttack;
    private List<ItemsSO> ItemList = new List<ItemsSO>();
    private List<ItemsPassiveSO> passiveItems = new List<ItemsPassiveSO>();
    private List<ItemsSO> CurrentItem = new List<ItemsSO>();
    bool isPlayerDead = false;
    bool isItem1Added = false;
    bool isNewMusicAdded = false;
    bool isNewMusicAdded2 = false;

    [System.Serializable]
    public struct WaveConfig {
        public string waveName;
        public float startTime; // Detik ke berapa wave ini mulai
        public int maxActiveEnemies;
        public float spawnInterval;
        public List<EnemiesData> enemyPool; // Musuh yang muncul di wave ini
        public bool isBossWave;
        public bool isBossWave2;
    }

    void Awake()
    {
        // Set nilai dasar secara manual agar 'TemporaryPlayerData' menangkap nilai 1000
        localMaxHP = 1000; // Atur nilai max HP lokal sesuai kebutuhan
        playerData.maxHealth = localMaxHP;
        playerData.health = playerData.maxHealth;
        
        // Baru lakukan backup
        TemporaryPlayerData = Instantiate(playerData); 
    }
    void UpdateTimer()
    {
        elapsedTime += Time.deltaTime;

        // Formula merubah detik ke 00:00
        int minutes = Mathf.FloorToInt(elapsedTime / 60f);
        int seconds = Mathf.FloorToInt(elapsedTime % 60f);
        timerString = string.Format("{0:00}:{1:00}", minutes, seconds);
        UIGame.UpdateUITimer(timerString); // Kirim ke UIController untuk update teks timer
    }

    void CheckWaveProgression()
    {
        // Cek apakah ada wave berikutnya dan apakah waktunya sudah sampai
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
        
        // Update setting di EnemySpawner secara realtime
        enemySpawner.UpdateWaveSettings(
            config.maxActiveEnemies, 
            config.spawnInterval, 
            config.enemyPool
        );

        if (config.isBossWave && !isBoss1Spawned)
        {
            Instantiate(Boss1, new Vector3(0, 0, 10), Quaternion.identity); // Contoh posisi spawn Boss
            isBoss1Spawned = true;
            // MusicManager.Instance.PlayTrack("Boss (15 Minute)");
        }

        if (config.isBossWave2 && !isBoss2Spawned)
        {
            Instantiate(Boss2, new Vector3(0, 0, 10), Quaternion.identity); // Contoh posisi spawn Boss
            isBoss2Spawned = true;
            // MusicManager.Instance.PlayTrack("Boss (30 Minute)");
        }
    }

    void HandleLevelUp() {
    if (playerData.exp >= playerData.expToNextLevel) {
        playerData.level += 1;
        playerData.exp -= playerData.expToNextLevel;
        playerData.expToNextLevel *= 1.3f;

        // --- CEK EVENT LEVEL DI SINI ---
        CheckLevelEvents(playerData.level);

        Debug.Log($"Level Up! Sekarang level {playerData.level}");
        }
    }

    void ItemPassiveLevelUp(int index, List<ItemsPassiveSO> ownedPassives, PlayerData playerData, int NewLevel) 
    {
        // 1. Validasi index agar tidak IndexOutOfRange
        if (index < 0 || index >= ownedPassives.Count) return;

        ItemsPassiveSO item = ownedPassives[index];

        // 2. PERBAIKAN KRITIS: ItemLevelData adalah struct, bukan List.
        // Kita ambil data level yang baru ke variabel lokal untuk referensi
        ItemLevelData stats = item.GetLevelData(NewLevel); 

        // 3. Jalankan efeknya ke PlayerData
        // Fungsi ApplyEffect ini akan mengambil modifierValue berdasarkan NewLevel
        item.ApplyEffect(playerData, NewLevel); 

        Debug.Log($"Item {item.itemName} naik ke level {NewLevel}. Deskripsi: {stats.levelDescription}");
    }
    void CheckLevelEvents(int level) {
    if (level == 2 && !isItem1Added) {
        // 1. Masukkan item ke slot aktif player
        playerData.ActiveItems[1] = ItemList[1]; 
        
        // 2. Masukkan item pasif ke slot pasif player
        ItemsPassiveSO itemBaru = passiveItems[0];
        playerData.PassivesItems[1] = itemBaru;

        // 3. SEKARANG APPLY EFEKNYA!
        // Kita asumsikan saat baru dapat, item-nya Level 1
        itemBaru.ApplyEffect(playerData, 1); 

        isItem1Added = true;
        Debug.Log($"Item {itemBaru.itemName} ditambahkan dan buff diterapkan!");
    }
    
    if (level == 5 && !isNewMusicAdded) {
        MusicManager.Instance?.PlayTrack("Boss (15 Minute)");
        isNewMusicAdded = true;
        ItemPassiveLevelUp(1, playerData.PassivesItems, playerData, 2); // Naikin level item pasif di slot 1 ke level 2
        }
    }
    void Start()
    {
        ItemList = listItemActive.activeItems; // DIPAKAI BUAT GACHA DI GAME MANAGER INI, INI LIST ITEM YANG UDH DIBUAT
        passiveItems = listItemPassive.passiveItems;
        defaultItem();
        Debug.Log($"[DEBUG] Health Saat Start: {playerData.health}");
        Debug.Log($"[DEBUG] Max Health Saat Start: {playerData.maxHealth}");
        Debug.Log($"[DEBUG] Level Saat Start: {playerData.level}");
    }

    // void ZeroHealth() {
    //     if (playerData.health <= 0) {
    //         Debug.Log("Player mati jir");
    //         isPlayerDead = true;
    //     }
    //     else {
    //        playerData.health -= 1;
    //     }
    // }
    // Update is called once per frame
    void defaultItem() {
        if (playerData.level == 1) {
            CurrentItem =  new List<ItemsSO>(playerData.DefaultSlot); // Ambil list dari PlayerData
            CurrentItem[0] = playerData.DefaultItem; // Set item ke slot 0
            playerData.ActiveItems = CurrentItem; // Simpan kembali ke PlayerData
            playerAttack.equippedActiveItems = playerData.ActiveItems; // Set juga ke PlayerAttack
            playerAttack.equippedPassiveItems = playerData.PassivesItems; // Set juga ke PlayerAttack
        }
    }

    
    void Update()
    {
        // if (isPlayerDead == false) ZeroHealth(); return;
        UpdateTimer();
        CheckWaveProgression();
        // Logika level up tetap di sini (seperti kodemu sebelumnya)
        HandleLevelUp();
        UIGame.UpdateHealthUI(playerData.health, playerData.maxHealth); // Update UI Health setiap frame (asumsi max health 1000)
        UIGame.UpdateXPUI(playerData.exp, playerData.expToNextLevel);
    }

    void OnDestroy()
    {
        ResetGame(); // Reset data ke kondisi awal saat game over atau restart
        playerData.ResetData(); // Reset semua data ketiga meninggalkan game
        
    }
    public void ResetGame()
{
    if (TemporaryPlayerData == null) return;

    string defaultValues = JsonUtility.ToJson(TemporaryPlayerData);
    JsonUtility.FromJsonOverwrite(defaultValues, playerData);

    // Cek dulu apakah UI masih ada sebelum diupdate
    if (UIGame != null) {
        // Update UI atau panggil fungsi reset UI
        }
    }

    void OnApplicationQuit()
    {
        // PENTING DI EDITOR:
        // Kembalikan nilai saat keluar dari game agar file .asset kamu 
        // di folder Project tidak tersimpan dalam keadaan game over/level tinggi.
        if (TemporaryPlayerData != null)
        {
            ResetGame();
        }
    }
}
