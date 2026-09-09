using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider MusicSlider;
    public Slider SoundSlider;
    
    [Header("Difficulty Panel")]
    [SerializeField] private GameObject difficultyPanel;
    
    [Header("Player Data")]
    [SerializeField] private PlayerData playerData; // ✅ Tambahkan reference PlayerData
    
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";
    
    void Start()
    {
        if (SettingsManager.Instance == null)
        {
            GameObject settingsObj = new GameObject("SettingsManager");
            settingsObj.AddComponent<SettingsManager>();
        }
        LoadVolume();
        
        if (MusicSlider != null)
            MusicSlider.onValueChanged.AddListener(delegate { OnMusicSliderChanged(); });
        
        if (SoundSlider != null)
            SoundSlider.onValueChanged.AddListener(delegate { OnSFXSliderChanged(); });
        
        SetupDamageNumberToggle();
        
        if (difficultyPanel != null)
            difficultyPanel.SetActive(false);
        
        MusicManager.Instance.PlayTrack("Main Menu");
    }
    
    // ==========================================
    // DIFFICULTY METHODS - Untuk OnClick() di Inspector
    // ==========================================
    
    public void PlayGame() 
    {
        SaveVolume();
        
        if (difficultyPanel != null)
        {
            difficultyPanel.SetActive(true);
        }
        else
        {
            SelectNormalDifficulty();
        }
    }
    
    public void SelectNormalDifficulty()
    {
        SelectDifficulty(0);
    }
    
    public void SelectHardDifficulty()
    {
        SelectDifficulty(1);
    }
    
    public void SelectExpertDifficulty()
    {
        SelectDifficulty(2);
    }
    
    public void SelectHellDifficulty()
    {
        SelectDifficulty(3);
    }
    
    void SelectDifficulty(int difficultyIndex)
    {
        if (DifficultyManager.Instance == null)
        {
            GameObject diffObj = new GameObject("DifficultyManager");
            diffObj.AddComponent<DifficultyManager>();
        }
        
        DifficultyManager.Instance.SelectDifficulty(difficultyIndex);
        
        if (difficultyPanel != null)
            difficultyPanel.SetActive(false);
        
        StartGame();
    }
    
    void StartGame()
    {
        SaveVolume();
        SceneManager.LoadSceneAsync(1);
        MusicManager.Instance.PlayTrack("Stage 1");
    }

    public void QuitGame() 
    {
        SaveVolume();
        Application.Quit();
    }
    
    // ==========================================
    // DEBUG/CHEAT BUTTONS - Untuk Testing
    // ==========================================
    
    /// <summary>
    /// Reset PlayerData ke nilai default
    /// </summary>
    public void ResetPlayerData()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData is null! Assign di Inspector!");
            return;
        }
        
        // Reset semua data
        playerData.ResetData();
        
        // Reset level permanent juga
        playerData.ResetAllLevels();
        
        // Reset gold dan soul ke 0
        playerData.Globalgold = 0;
        playerData.Globalsouls = 0;
        playerData.gold = 0;
        playerData.souls = 0;

        
        // Reset multiplier
        playerData.expMultiplier = 1f;
        playerData.projectileDamageMultiplier = 1f;
        playerData.projectileSpeedMultiplier = 1f;
        playerData.projectileLifetimeMultiplier = 1f;
        playerData.cooldownReductionMultiplier = 0f;
        playerData.projectileCountMultiplier = 1f;
        
        // Reset health
        playerData.health = playerData.baseMaxHealth;
        playerData.maxHealth = playerData.baseMaxHealth;
        playerData.defense = playerData.baseDefense;
        
        // Reset inventory
        playerData.ResetInventory();
        
        Debug.Log("✅ PLAYERDATA BERHASIL DI-RESET!");
    }
    
    /// <summary>
    /// Set gold dan soul ke 999 juta untuk testing
    /// </summary>
    public void SetMaxCurrency()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerData is null! Assign di Inspector!");
            return;
        }
        
        int maxValue = 999000000; // 999 juta
        
        playerData.Globalgold = maxValue;
        playerData.Globalsouls = maxValue;
        
        Debug.Log($"✅ CURRENCY DI-SET KE 999 JUTA! Gold: {playerData.Globalgold}, Souls: {playerData.Globalsouls}");
    }
    
    // ==========================================
    // VOLUME CONTROL
    // ==========================================
    
    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat(MUSIC_VOL_KEY, volume);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, volume);
        PlayerPrefs.Save();
    }
 
    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat(SFX_VOL_KEY, volume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, volume);
        PlayerPrefs.Save();
    }
    
    void OnMusicSliderChanged()
    {
        UpdateMusicVolume(MusicSlider.value);
    }
    
    void OnSFXSliderChanged()
    {
        UpdateSoundVolume(SoundSlider.value);
    }
 
    public void SaveVolume()
    {
        audioMixer.GetFloat(MUSIC_VOL_KEY, out float musicVolume);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, musicVolume);
 
        audioMixer.GetFloat(SFX_VOL_KEY, out float sfxVolume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, sfxVolume);
        
        PlayerPrefs.Save();
        Debug.Log($"Volume Saved! Music: {musicVolume}, SFX: {sfxVolume}");
    }
 
    public void LoadVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0);
        
        audioMixer.SetFloat(MUSIC_VOL_KEY, musicVolume);
        audioMixer.SetFloat(SFX_VOL_KEY, sfxVolume);
        
        if (MusicSlider != null) MusicSlider.value = musicVolume;
        if (SoundSlider != null) SoundSlider.value = sfxVolume;
        
        Debug.Log($"Volume Loaded! Music: {musicVolume}, SFX: {sfxVolume}");
    }
    
    [Header("Damage Number Toggle")]
    [SerializeField] private Toggle damageNumberToggle;
    private const string DAMAGE_NUMBER_KEY = "ShowDamageNumbers";
    
    void SetupDamageNumberToggle()
    {
        if (damageNumberToggle == null) return;
        
        bool showDamage = PlayerPrefs.GetInt(DAMAGE_NUMBER_KEY, 1) == 1;
        damageNumberToggle.isOn = showDamage;
        
        damageNumberToggle.onValueChanged.AddListener(OnDamageNumberToggleChanged);
    }
    
    void OnDamageNumberToggleChanged(bool isOn)
    {
        PlayerPrefs.SetInt(DAMAGE_NUMBER_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log($"Damage Numbers: {(isOn ? "ON" : "OFF")}");
    }
    
    void OnApplicationQuit()
    {
        SaveVolume();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveVolume();
        }
    }
}