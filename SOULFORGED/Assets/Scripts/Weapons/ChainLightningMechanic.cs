using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChainLightningMechanic : MonoBehaviour, IProjectile
{
    [Header("Dynamic Stats (Diatur via SO)")]
    public float currentDamage;
    public float currentCooldown;
    public float detectionRadius = 12f;
    public float baseChainRange = 6f;
    public float sizeMultiplier = 1f;
    public int initialBoltCount = 1;
    public int chainCount = 2;

    [Header("Behavior Settings (Atur di Inspector SO)")]
    [Tooltip("Jeda waktu antar lompatan petir (semakin kecil semakin cepat merambat)")]
    public float chainDelay = 0.08f;
    [Tooltip("Aktifkan untuk memberikan Damage Area per titik sambaran")]
    public bool useAoEDamage = true;
    public float aoeRadius = 2.5f;

    private GameObject visualPrefab; // Disuplai otomatis oleh LightningSO
    private float timer = 0f;

    public float Damage => currentDamage;
    public float Cooldown => currentCooldown;

    // Dipanggil pertama kali oleh LightningSO saat senjata baru diambil
    public void InitializeManager(int count, GameObject prefab)
    {
        initialBoltCount = count;
        visualPrefab = prefab; 
    }

    // Fungsi WAJIB bawaan antarmuka IProjectile
    public void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle, float cooldown, float size)
    {
        detectionRadius = speed > 0 ? speed : 12f;
        currentDamage = damage;
        currentCooldown = cooldown > 0 ? cooldown : 2.5f;
        sizeMultiplier = size > 0 ? size : 1f;
        chainCount = scatterAngle > 0 ? Mathf.RoundToInt(scatterAngle) : 2;
        
        timer = currentCooldown; // Langsung siap tembak saat level 1
    }

    // Dipanggil oleh LightningSO saat senjata naik level
    public void UpdateStats(float speed, float damage, float size, int count, float spreadAngle)
    {
        detectionRadius = speed > 0 ? speed : detectionRadius;
        currentDamage = damage;
        sizeMultiplier = size > 0 ? size : sizeMultiplier;
        initialBoltCount = count > 0 ? count : initialBoltCount;
        chainCount = spreadAngle > 0 ? Mathf.RoundToInt(spreadAngle) : chainCount;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= currentCooldown)
        {
            CastLightning();
        }
    }

    private void CastLightning()
    {
        if (visualPrefab == null) return;

        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius);
        List<Transform> validTargets = new List<Transform>();

        foreach (var col in enemies)
        {
            if (col != null && col.CompareTag("Enemy") && col.GetComponent<EnemiesBase>() != null)
            {
                validTargets.Add(col.transform);
            }
        }

        // Jika tidak ada musuh, JANGAN reset cooldown. Tunggu sampai musuh mendekat.
        if (validTargets.Count == 0) return;
        
        // Reset Cooldown hanya ketika petir berhasil ditembakkan
        timer = 0f; 

        validTargets.Sort((a, b) => 
            Vector3.Distance(transform.position, a.position)
            .CompareTo(Vector3.Distance(transform.position, b.position))
        );

        int boltsToCast = Mathf.Min(initialBoltCount, validTargets.Count);
        HashSet<Transform> hitHistory = new HashSet<Transform>();

        for (int i = 0; i < boltsToCast; i++)
        {
            StartCoroutine(ChainRoutine(validTargets[i], hitHistory));
        }
    }

    private IEnumerator ChainRoutine(Transform firstTarget, HashSet<Transform> history)
    {
        Transform currentTarget = firstTarget;
        float actualRange = baseChainRange * sizeMultiplier;

        for (int step = 0; step <= chainCount; step++)
        {
            // Hentikan jika target hilang/mati di tengah jalan
            if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy) break;

            history.Add(currentTarget);
            Vector3 hitPos = currentTarget.position;

            // 1. Spawn VFX Petir
            GameObject vfx = Instantiate(visualPrefab, hitPos, Quaternion.identity);
            Destroy(vfx, 1f);

            // 2. Berikan Damage (Aman dari error karena menggunakan pengecekan null)
            if (useAoEDamage)
            {
                Collider[] splash = Physics.OverlapSphere(hitPos, aoeRadius);
                foreach (var s in splash)
                {
                    if (s != null && s.CompareTag("Enemy") && s.TryGetComponent(out EnemiesBase splashEnemy))
                    {
                        splashEnemy.TakeDamage(this);
                    }
                }
            }
            else
            {
                if (currentTarget.TryGetComponent(out EnemiesBase enemy))
                {
                    enemy.TakeDamage(this);
                }
            }

            // 3. Jeda untuk memberikan efek sambaran listrik yang dramatis (satisfying)
            yield return new WaitForSeconds(chainDelay);

            // 4. Cari target terdekat selanjutnya dari titik musuh terakhir
            currentTarget = FindNextTarget(hitPos, actualRange, history);
            if (currentTarget == null) break;
        }
    }

    private Transform FindNextTarget(Vector3 origin, float range, HashSet<Transform> history)
    {
        Collider[] nearby = Physics.OverlapSphere(origin, range);
        Transform best = null;
        float closest = Mathf.Infinity;

        foreach (var col in nearby)
        {
            if (col != null && col.CompareTag("Enemy") && !history.Contains(col.transform))
            {
                float dist = Vector3.Distance(origin, col.transform.position);
                if (dist < closest)
                {
                    closest = dist;
                    best = col.transform;
                }
            }
        }
        return best;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}