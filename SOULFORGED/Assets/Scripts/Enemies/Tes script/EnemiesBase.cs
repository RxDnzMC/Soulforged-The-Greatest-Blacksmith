using UnityEngine;
using System.Collections;

public class EnemiesBase : MonoBehaviour
{
    [SerializeField] public PlayerData playerData;
    
    [Header("Damage Feedback")]
    [SerializeField] GameObject damageNumberPrefab;
    [SerializeField] float flashDuration = 0.1f;

    [Header("Loot Settings")]
    [SerializeField] GameObject coinPrefab;
    [Range(0, 100)] [SerializeField] float coinDropChance;
    
    [SerializeField] GameObject soulPrefab;
    [Range(0, 100)] [SerializeField] float soulDropChance;
    
    [Header("Heal Item Settings")]
    [SerializeField] GameObject healItemPrefab;
    [Range(0, 100)] [SerializeField] float healDropChance;
    
    [SerializeField] private float lootSpawnHeight = 1f; // Tinggi spawn loot di atas musuh

    public float _health;
    public float _speed;
    public float _damage;
    public float _attackInterval;
    public float expReward;
    
    private bool isDead = false;
    private Renderer[] renderers;
    private Material[] instanceMaterials; // PAKE MATERIAL INSTANCE BIAR GAK GANGGU PREFAB LAIN
    private Color[] originalColors; // Simpan warna asli

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        instanceMaterials = new Material[renderers.Length];
        originalColors = new Color[renderers.Length];
        
        for (int i = 0; i < renderers.Length; i++)
        {
            // BIKIN INSTANCE MATERIAL BARU (biar gak ganggu shared material)
            instanceMaterials[i] = renderers[i].material;
            originalColors[i] = instanceMaterials[i].color;
        }
    }

    public void Setup(float health, float speed, float baseDamage, float attackInterval, float expReward)
    {
        _health = health;
        _speed = speed;
        _damage = baseDamage;
        _attackInterval = attackInterval;
        this.expReward = expReward;
    }

    void SpawnDamageNumber(float damage)
    {
        if (damageNumberPrefab != null)
        {
            // Spawn damage number di posisi enemy + offset ke atas
            Vector3 spawnPos = transform.position + Vector3.up * 1.5f;
            GameObject dmgObj = Instantiate(damageNumberPrefab, spawnPos, Quaternion.identity);
            
            // GAK PERLU LookAt di sini, udah di DamageNumber.Start()
            
            if (dmgObj.TryGetComponent(out DamageNumber damageNumber))
            {
                damageNumber.Setup(damage);
            }
        }
    }

    IEnumerator FlashRed()
    {
        // Ubah semua material jadi merah
        for (int i = 0; i < renderers.Length; i++)
        {
            if (instanceMaterials[i] != null)
            {
                instanceMaterials[i].color = Color.red;
            }
        }
        
        yield return new WaitForSeconds(flashDuration);
        
        // KEMBALIKAN KE WARNA ASLI
        for (int i = 0; i < renderers.Length; i++)
        {
            if (instanceMaterials[i] != null)
            {
                instanceMaterials[i].color = originalColors[i];
            }
        }
    }

    void TakeDamage(IProjectile projectile)
    {
        _health -= projectile.Damage;
        
        SpawnDamageNumber(projectile.Damage);
        StartCoroutine(FlashRed());
        
        // Debug.Log($"Musuh Kena Serangan! Damage: {projectile.Damage}, Sisa Health: {_health}");
        
        if (_health <= 0)
        {
            EnemyDead();
        }
    }

    void OnTriggerEnter(Collider other)
    {   
        if (isDead) return; 
        
        if (other.CompareTag("ProjectileDamage")) 
        {
            if (other.TryGetComponent(out IProjectile projectile)) 
            {
                TakeDamage(projectile);
            }
        }
    }

    private float _nextDamageTime = 0f;

    void OnTriggerStay(Collider other)
    {
        if (isDead) return; 

        if (other.CompareTag("AreaDamage")) 
        {
            if (Time.time >= _nextDamageTime)
            {
                if (other.TryGetComponent(out IProjectile projectile)) 
                {
                    TakeDamage(projectile);
                    _nextDamageTime = Time.time + projectile.Cooldown;
                }
            }
        }
    }

    /// <summary>
    /// Dapetin posisi random di sekitar musuh
    /// side: 0 = random, 1 = kanan, -1 = kiri
    /// yPosition: Y absolute (0 untuk coin, 0.87 untuk soul, dll)
    /// </summary>
    Vector3 GetRandomDropPosition(int side = 0, float yPosition = 0f)
    {
        float spreadRadius = Random.Range(0.3f, 0.8f);
        
        float xOffset, zOffset;
        
        if (side == 0)
        {
            xOffset = Random.Range(-spreadRadius, spreadRadius);
            zOffset = Random.Range(-spreadRadius, spreadRadius);
        }
        else if (side == 1)
        {
            // Kanan
            xOffset = Random.Range(0.3f, spreadRadius);
            zOffset = Random.Range(-spreadRadius, spreadRadius);
        }
        else
        {
            // Kiri
            xOffset = Random.Range(-spreadRadius, -0.3f);
            zOffset = Random.Range(-spreadRadius, spreadRadius);
        }
        
        // ✅ Y ABSOLUTE
        Vector3 spawnPos = new Vector3(
            transform.position.x + xOffset,
            yPosition,
            transform.position.z + zOffset
        );
        return spawnPos;
    }

    void DropLoot()
    {
        // Track item yang sudah di-drop untuk menentukan posisi yang berbeda
        int droppedCount = 0;
        
        // Cek Drop Koin
        if (coinPrefab != null && coinDropChance > 0)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= coinDropChance)
            {
                Vector3 spawnPos = GetRandomDropPosition(0, 0f);
                Instantiate(coinPrefab, spawnPos, Quaternion.identity);
                droppedCount++;
            }
        }

        // Cek Drop Soul
        if (soulPrefab != null && soulDropChance > 0)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= soulDropChance)
            {
                int side = (droppedCount == 1) ? 1 : 0;
                Vector3 spawnPos = GetRandomDropPosition(side, 1.22f);
                Instantiate(soulPrefab, spawnPos, Quaternion.identity);
                droppedCount++;
            }
        }
        
        // Cek Drop Heal Item
        if (healItemPrefab != null && healDropChance > 0)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= healDropChance)
            {
                // Tentukan side berdasarkan jumlah item yang sudah di-drop
                int side = 0;
                if (droppedCount == 1) side = 1;
                else if (droppedCount == 2) side = -1;
                else if (droppedCount >= 3) side = Random.Range(-1, 2);
                
                // Y position untuk heal item (bisa disesuaikan)
                float healYPosition = 0.6f;
                Vector3 spawnPos = GetRandomDropPosition(side, healYPosition);
                Instantiate(healItemPrefab, spawnPos, Quaternion.identity);
            }
        }
    }

    void EnemyDead() 
    {
        if (isDead) return;
        isDead = true;

        // PANGGIL FUNGSI DROP DI SINI
        DropLoot();

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
            
        if (playerData != null) playerData.exp += expReward;

        Animator anim = GetComponent<Animator>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (anim != null) 
        {
            anim.SetTrigger("Die");
            float animDuration = 0.1f;
            
            RuntimeAnimatorController ac = anim.runtimeAnimatorController;
            if (ac != null)
            {
                foreach (AnimationClip clip in ac.animationClips)
                {
                    if (clip.name.Contains("Die") || clip.name.Contains("Dead"))
                    {
                        animDuration = clip.length;
                        break;
                    }
                }
            }
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