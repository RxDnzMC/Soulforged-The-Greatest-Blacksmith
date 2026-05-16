using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider MusicSlider;
    public Slider SoundSlider;
    
    private const string MUSIC_VOL_KEY = "MusicVolume";
    private const string SFX_VOL_KEY = "SFXVolume";
    
    void Start()
    {
        // Load volume yang udah di-save
        LoadVolume();
        
        // Setup slider listener (auto-save pas diubah)
        if (MusicSlider != null)
        {
            MusicSlider.onValueChanged.AddListener(delegate { OnMusicSliderChanged(); });
        }
        
        if (SoundSlider != null)
        {
            SoundSlider.onValueChanged.AddListener(delegate { OnSFXSliderChanged(); });
        }
        
        MusicManager.Instance.PlayTrack("Main Menu");
    }
    
    public void PlayGame() 
    {
        SaveVolume(); // Save sebelum pindah scene
        SceneManager.LoadSceneAsync(1);
        MusicManager.Instance.PlayTrack("Stage 1");
    }

    public void QuitGame() 
    {
        SaveVolume(); // Save sebelum keluar
        Application.Quit();
    }

    // ==========================================
    // VOLUME CONTROL
    // ==========================================
    
    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat(MUSIC_VOL_KEY, volume);
        PlayerPrefs.SetFloat(MUSIC_VOL_KEY, volume); // Auto-save
        PlayerPrefs.Save();
    }
 
    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat(SFX_VOL_KEY, volume);
        PlayerPrefs.SetFloat(SFX_VOL_KEY, volume); // Auto-save
        PlayerPrefs.Save();
    }
    
    // Auto-save pas slider digeser
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
        
        PlayerPrefs.Save(); // ✅ WAJIB: Biar langsung ke-save ke disk
        Debug.Log($"Volume Saved! Music: {musicVolume}, SFX: {sfxVolume}");
    }
 
    public void LoadVolume()
    {
        // Default value = 0 (volume penuh)
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOL_KEY, 0);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOL_KEY, 0);
        
        // Set ke AudioMixer
        audioMixer.SetFloat(MUSIC_VOL_KEY, musicVolume);
        audioMixer.SetFloat(SFX_VOL_KEY, sfxVolume);
        
        // Set slider position (biar gak loncat)
        if (MusicSlider != null) MusicSlider.value = musicVolume;
        if (SoundSlider != null) SoundSlider.value = sfxVolume;
        
        Debug.Log($"Volume Loaded! Music: {musicVolume}, SFX: {sfxVolume}");
    }
    
    // ==========================================
    // AUTO-SAVE SAAT KELUAR APLIKASI
    // ==========================================
    
    void OnApplicationQuit()
    {
        SaveVolume();
    }
    
    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveVolume(); // Save pas aplikasi di-pause (misal buka menu HP)
        }
    }
}