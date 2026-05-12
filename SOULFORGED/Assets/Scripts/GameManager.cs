using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("References")]
    [SerializeField] private PlayerData playerData;
    [SerializeField] private EnemySpawner enemySpawner; // Referensi ke spawner
        [SerializeField] private UIController UIGame; // Assign di Inspector

    [Header("Wave Settings")]
    [SerializeField] private List<WaveConfig> waves;
    
    public GameObject Boss1; 
    public GameObject Boss2; 
    private bool isBoss1Spawned = false;
    private bool isBoss2Spawned = false;
    private int currentWaveIndex = 0;


    [Header("Timer Display")]
    public string timerString; // Ini yang nanti dikirim ke UI Text
    private float elapsedTime = 0f;
    [SerializeField] ListItemActive listItemActive;
    [SerializeField] PlayerAttack playerAttack;
    private List<ItemsSO> ItemList = new List<ItemsSO>();
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
        // ... (Logika level up yang lama pindahkan ke sini agar rapi)
        if (playerData.exp >= playerData.expToNextLevel) {
            playerData.level += 1; // Naikkan level
            playerData.exp -= playerData.expToNextLevel; // Reset ke sisa exp setelah naik level
            playerData.expToNextLevel *= 1.3f; // Next Level Requirement (naik 30% setiap level)
            Debug.Log($"Level Up! Sekarang level {playerData.level}");
        }
    }
    void Start()
    {
        ItemList = listItemActive.activeItems; // DIPAKAI BUAT GACHA DI GAME MANAGER INI, INI LIST ITEM YANG UDH DIBUAT
        
        playerData.health = 1000;
        playerData.exp = 0;
        playerData.level = 1;
        playerData.expToNextLevel = 100;
        defaultItem();
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
            playerAttack.equippedItems = playerData.ActiveItems; // Set juga ke PlayerAttack
            Debug.Log(playerData.ActiveItems + " ini item yang dipakai player");
            Debug.Log(playerAttack.equippedItems + " ini item yang dipakai player");
            Debug.Log("Item 0 ditambahkan ke Active Items!");
        }
    }
    void Update()
    {
        // if (isPlayerDead == false) ZeroHealth(); return;
        
        if (playerData.level == 2 && !isItem1Added) {
            CurrentItem = playerData.ActiveItems; // Ambil list dari PlayerData
            CurrentItem[1] = ItemList[1]; // Set item ke slot 1
            playerData.ActiveItems = CurrentItem; // Simpan kembali ke PlayerData
            Debug.Log("Item 1 ditambahkan ke Active Items!");
            isItem1Added = true;
        }
        
        if (playerData.level == 5 && !isNewMusicAdded) {
            if (MusicManager.Instance != null) {
                MusicManager.Instance.PlayTrack("Boss (15 Minute)");
                isNewMusicAdded = true;
            } else {
                Debug.LogWarning("MusicManager instance not found!");
            }
        }

        if (playerData.level == 12 && !isNewMusicAdded2) {
            if (MusicManager.Instance != null) {
                MusicManager.Instance.PlayTrack("Boss (30 Minute)");
                isNewMusicAdded2 = true;
            } else {
                Debug.LogWarning("MusicManager instance not found!");
            }
        }
        UpdateTimer();
        CheckWaveProgression();
        // Logika level up tetap di sini (seperti kodemu sebelumnya)
        HandleLevelUp();
    }

    void OnDestroy()
    {
        playerData.ResetData(); // Reset semua data ketiga meninggalkan game
    }
}
