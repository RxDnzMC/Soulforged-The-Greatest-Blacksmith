using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [SerializeField] private MusicLibrary musicLibrary;
    [SerializeField] private AudioSource musicSource;

    private Coroutine fadeCoroutine; // Simpan reference coroutine agar bisa dihentikan

    public void Awake()
    {
        if (Instance != null) { Destroy(gameObject); }
        else { Instance = this; DontDestroyOnLoad(gameObject); }
    }

    public void PlayTrack(string trackName, float FadeDuration = 0.5f)
    {
        AudioClip nextClip = musicLibrary.GetTrack(trackName);
        
        if (nextClip != null)
        {
            // Jika ada fade yang sedang berjalan, hentikan dulu supaya tidak tabrakan
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            
            fadeCoroutine = StartCoroutine(AnimateMusicCrossfade(nextClip, FadeDuration));
        }
    }

    // Tambahkan ini agar muncul di Button
    public void PlayTrackSimple(string trackName)
    {
        PlayTrack(trackName, 0.5f); // Memanggil fungsi asli dengan duration default
    }

    IEnumerator AnimateMusicCrossfade(AudioClip nextTrack, float FadeDuration)
    {
        float percent = 0;
        float startVolume = musicSource.volume; // Ambil volume saat ini (misal: 0.5)

        // FADE OUT
        while (percent < 1)
        {
            percent += Time.deltaTime / FadeDuration;
            musicSource.volume = Mathf.Lerp(startVolume, 0, percent);
            yield return null;
        }

        musicSource.clip = nextTrack;
        musicSource.Play();

        percent = 0;
        // FADE IN (Kembali ke volume awal pemain, bukan dipaksa ke 1.0)
        while (percent < 1)
        {
            percent += Time.deltaTime / FadeDuration;
            musicSource.volume = Mathf.Lerp(0, startVolume, percent);
            yield return null;
        }
    }
}