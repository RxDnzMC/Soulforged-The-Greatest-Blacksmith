using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider MusicSlider;
    public Slider SoundSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void PlayGame() {
        SceneManager.LoadSceneAsync(1);
        MusicManager.Instance.PlayTrack("Stage 1");
    }

    public void QuitGame() {
        Application.Quit();
    }

    public void Start()
    {
        MusicManager.Instance.PlayTrack("Main Menu");
    }

    public void UpdateMusicVolume(float volume)
    {
        audioMixer.SetFloat("MusicVolume", volume);
    }
 
    public void UpdateSoundVolume(float volume)
    {
        audioMixer.SetFloat("SFXVolume", volume);
    }
 
    public void SaveVolume()
    {
        audioMixer.GetFloat("MusicVolume", out float musicVolume);
        PlayerPrefs.SetFloat("MusicVolume", musicVolume);
 
        audioMixer.GetFloat("SFXVolume", out float sfxVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
    }
 
    public void LoadVolume()
    {
        MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        SoundSlider.value = PlayerPrefs.GetFloat("SFXVolume");
    }
}
