using UnityEngine;
using System.Collections.Generic; // Tambahkan ini untuk menggunakan Dictionary
 
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
 
    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2DSource;
 
    // ==========================================
    // SISTEM ANTI-SPAM (COOLDOWN SUARA)
    // ==========================================
    private Dictionary<string, float> soundTimers = new Dictionary<string, float>();
    private float antiSpamThreshold = 0.05f; // Jeda 0.05 detik (Bisa dibesarkan kalau masih bocor)

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
 
    // Fungsi pengecekan: Boleh mainkan suara atau tidak?
    private bool CanPlaySound(string soundName)
    {
        if (soundTimers.TryGetValue(soundName, out float lastPlayedTime))
        {
            // Jika belum melewati batas waktu threshold, block suaranya!
            if (Time.time - lastPlayedTime < antiSpamThreshold)
            {
                return false; 
            }
        }
        
        // Catat waktu terbaru suara ini dimainkan
        soundTimers[soundName] = Time.time;
        return true;
    }

    public void PlaySound3D(string soundName, Vector3 pos)
    {
        // 1. CEK ANTI SPAM
        if (!CanPlaySound(soundName)) return;

        AudioClip clip = sfxLibrary.GetClipFromName(soundName);
        if (clip == null) return;

        // Buat objek sementara secara manual
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = pos;
        
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        
        // PENTING: Hubungkan ke Mixer Group SFX kamu!
        aSource.outputAudioMixerGroup = sfx2DSource.outputAudioMixerGroup; 
        
        aSource.clip = clip;
        aSource.spatialBlend = 1f; // Set jadi murni 3D
        aSource.Play();
        
        // Hancurkan setelah durasi lagu habis
        Destroy(tempGO, clip.length);
    }
    
    public void PlaySound2D(string soundName)
    {
        // 1. CEK ANTI SPAM
        if (!CanPlaySound(soundName)) return;

        AudioClip clip = sfxLibrary.GetClipFromName(soundName);
        if (clip != null)
        {
            sfx2DSource.PlayOneShot(clip);
        }
    }
}