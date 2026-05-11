using UnityEngine;

public class EnemiesBase : MonoBehaviour
{
    [SerializeField] public PlayerData playerData;

    public float _health;
    public float _speed;
    public float _damage;
    public float _attackInterval;
    public float expReward;
    
    private bool isDead = false;

    // Fungsi ini dipanggil oleh Spawner untuk memberi nilai stats
    public void Setup(float health, float speed, float baseDamage, float attackInterval, float expReward)
    {
        _health = health;
        _speed = speed;
        _damage = baseDamage;
        _attackInterval = attackInterval;
        this.expReward = expReward;
    }

    // Hanya urus saat musuh kena peluru
    void OnTriggerEnter(Collider other)
{   
    if (isDead) return; 
    
    // 1. Pastikan yang menabrak adalah objek pemberi damage
    if (other.CompareTag("ProjectileDamage")) 
    {
        // 2. Cek apakah benda ini punya "Lisensi" IProjectile
        if (other.TryGetComponent(out IProjectile projectile)) 
        {
            // 3. Ambil damage-nya lewat interface (Sangat Simpel!)
            _health -= projectile.Damage;
            
            Debug.Log($"Musuh Kena Serangan! Sisa Health: {_health}");
            
            // 4. Cek kematian
            if (_health <= 0) 
            {
                EnemyDead();
            }
        }
    }
}

    void EnemyDead() 
    {
        if (isDead) return;
        isDead = true;

        //MATIKAN COLLIDER
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        // MATIKAN RIGIDBODY (biar gak bisa didorong)
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Biar gak dipengaruhi fisika
            // Atau: rb.constraints = RigidbodyConstraints.FreezeAll;
        }
            
        // Tambah EXP
        if (playerData != null) playerData.exp += expReward;

        Animator anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (anim != null) 
        {
            anim.SetTrigger("Die");
            
            // --- CARA MENCARI DURASI ANIMASI MATI ---
            float animDuration = 0.1f;
            
            // Ambil semua daftar animasi yang ada di dalam Animator musuh ini
            RuntimeAnimatorController ac = anim.runtimeAnimatorController;
            if (ac != null)
            {
                foreach (AnimationClip clip in ac.animationClips)
                {
                    // Cari klip animasi yang namanya ada kata "Die" atau "Dead"
                    if (clip.name.Contains("Die") || clip.name.Contains("Dead"))
                    {
                        animDuration = clip.length; // Ketemu! Ambil durasi aslinya
                        break; // Berhenti mencari
                    }
                }
            }

            Debug.Log($"Musuh mati! Menunggu {animDuration} detik sebelum hancur.");
            Destroy(gameObject, animDuration); 
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}