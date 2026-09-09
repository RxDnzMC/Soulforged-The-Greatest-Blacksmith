using UnityEngine;

public class WindCatalystMechanic : MonoBehaviour, IProjectile
{
    [Header("AoE Settings")]
    [Tooltip("Berapa detik sekali musuh terkena damage di dalam area")]
    public float damageTickRate = 0.5f; 
    
    [Tooltip("Radius dasar area lingkaran saat Size Multiplier di level 1")]
    public float baseRadius = 2.5f; 

    private float currentDamage;
    private float currentSizeMultiplier = 1f;
    private float tickTimer = 0f;
    
    private Vector3 baseScale;
    private bool isScaleInitialized = false;

    // Implementasi Interface IProjectile
    public float Damage => currentDamage;
    public float Cooldown => 0f; 

    public void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle, float cooldown, float size)
    {
        InitScale();
        UpdateStats(damage, size);
    }

    public void UpdateStats(float newDamage, float newSize)
    {
        InitScale();
        currentDamage = newDamage;
        currentSizeMultiplier = newSize;
        
        // Memperbesar visual objek 3D/Sprite secara proporsional sesuai data item
        transform.localScale = baseScale * newSize; 
    }

    private void InitScale()
    {
        if (!isScaleInitialized)
        {
            baseScale = transform.localScale;
            isScaleInitialized = true;
        }
    }

    void Update()
    {
        // PAKSA rotasi dunianya selalu konstan (Tidur datar di tanah 90 derajat), 
        // tidak peduli seberapa cepat Player atau Pivot berputar menghadap mouse.
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        tickTimer += Time.deltaTime;
        if (tickTimer >= damageTickRate)
        {
            ApplyAreaDamage();
            tickTimer = 0f; 
        }
    }

    private void ApplyAreaDamage()
    {
        // Hitung radius asli gabungan jangkauan dasar dan pengali size dari item
        float actualRadius = baseRadius * currentSizeMultiplier;
        
        // Deteksi seluruh objek collider dalam bentuk bola matematika
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, actualRadius);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                // Ambil script utama musuh dan kirim damage
                if (hit.TryGetComponent(out EnemiesBase enemyScript))
                {
                    enemyScript.TakeDamage(this); // 'this' memberikan data script ini sebagai IProjectile
                }

                Debug.Log($"[Wind Catalyst] Mengenai {hit.name} dengan {currentDamage} damage!");
            }
        }
    }

    // Menggambar indikator lingkaran bantu berwarna cyan di Unity Editor (tidak muncul di game)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 1f, 0.25f);
        float radius = baseRadius * (isScaleInitialized ? currentSizeMultiplier : 1f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}