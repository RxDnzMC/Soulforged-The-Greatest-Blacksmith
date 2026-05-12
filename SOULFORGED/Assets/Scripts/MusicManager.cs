using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioReverbFilter reverbFilter; // Tarik komponen filter ke sini
    [SerializeField] private AudioLowPassFilter lowPassFilter; // Tarik komponen lowpass ke sini

    private float originalVolume = 0.5f; // Simpan volume asli player
    private Coroutine fadeCoroutine;

    public void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        else { Instance = this; DontDestroyOnLoad(gameObject); }
        
        // Pastikan reverb mati di awal
        if (lowPassFilter != null) lowPassFilter.enabled = false;
    }

    // FUNGSI BARU UNTUK PAUSE EFFECT
    public void SetPauseEffect(bool isPaused)
    {
        if (isPaused)
        {
            musicSource.volume = originalVolume * 0.3f; // Kecilkan ke 30%
            if (lowPassFilter != null) lowPassFilter.enabled = true; // Nyalakan gema
        }
        else
        {
            musicSource.volume = originalVolume; // Kembalikan ke volume asli
            if (lowPassFilter != null) lowPassFilter.enabled = false; // Matikan gema
        }
    }

    public void PlayTrack(string trackName, float FadeDuration = 0.5f)
    {
        AudioClip nextClip = musicLibrary.GetTrack(trackName);
        if (nextClip != null)
        {
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(AnimateMusicCrossfade(nextClip, FadeDuration));
        }
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float FadeDuration)
    {
        float percent = 0;
        float startVolume = musicSource.volume;

        while (percent < 1)
        {
            // Gunakan unscaledDeltaTime supaya fade tetap jalan walau game lagi PAUSE
            percent += Time.unscaledDeltaTime / FadeDuration; 
            musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
            yield return null;
        }

        musicSource.clip = nextTrack;
        musicSource.Play();

        percent = 0;
        while (percent < 1)
        {
            percent += Time.unscaledDeltaTime / FadeDuration;
            musicSource.volume = Mathf.Lerp(0, originalVolume, percent);
            yield return null;
        }
    }
    public void PlayTrackSimple(string trackName)
    {
        PlayTrack(trackName, 0.5f); // Memanggil fungsi asli dengan duration default
    }
}