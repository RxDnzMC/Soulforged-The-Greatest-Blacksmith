using UnityEngine;
 
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
 
    [SerializeField]
    private SoundLibrary sfxLibrary;
    [SerializeField]
    private AudioSource sfx2DSource;
 
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
 
    public void PlaySound3D(string soundName, Vector3 pos)
    {
        AudioClip clip = sfxLibrary.GetClipFromName(soundName);
        if (clip == null) return;

        // Buat objek sementara secara manual
        GameObject tempGO = new GameObject("TempAudio");
        tempGO.transform.position = pos;
        
        AudioSource aSource = tempGO.AddComponent<AudioSource>();
        
        // PENTING: Hubungkan ke Mixer Group SFX kamu!
        // Pastikan kamu sudah punya variabel private AudioMixerGroup sfxGroup di SoundManager
        aSource.outputAudioMixerGroup = sfx2DSource.outputAudioMixerGroup; 
        
        aSource.clip = clip;
        aSource.spatialBlend = 1f; // Set jadi murni 3D
        aSource.Play();
        
        // Hancurkan setelah durasi lagu habis
        Destroy(tempGO, clip.length);
    }
    
    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(sfxLibrary.GetClipFromName(soundName));
    }
}