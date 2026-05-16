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

    void DropLoot()
    {
        // Cek Drop Koin
        if (coinPrefab != null)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= coinDropChance)
            {
                Instantiate(coinPrefab, transform.position, Quaternion.identity);
            }
        }

        // Cek Drop Soul
        if (soulPrefab != null)
        {
            float roll = Random.Range(0f, 100f);
            if (roll <= soulDropChance)
            {
                Instantiate(soulPrefab, transform.position, Quaternion.identity);
                soulPrefab.transform.position += Vector3.up * 2f;
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