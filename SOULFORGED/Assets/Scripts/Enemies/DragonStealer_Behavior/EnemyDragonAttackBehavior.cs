using UnityEngine;
using System.Collections;
using UnityEngine.VFX; 

public class EnemyDragonAttackBehavior : MonoBehaviour
{
    private EnemiesBase enemyStats;
    private Animator anim; 
    private EnemyVFX vfxScript;

    // --- SISTEM COOLDOWN GLOBAL & SKILL ---
    private float lastAttackTime = -999f;
    private bool isAttacking = false;
    private bool isDead = false;

    [Header("Death Settings")]
    public string dieAnimStateName = "Die";
    public float destroyAfterDelay = 5.0f;

    [Header("Passive Ability (Acid Puddle)")]
    public GameObject puddleIndicatorPrefab; 
    public float puddleWarningTime = 1.2f; 
    public GameObject acidPuddlePrefab; 
    public float passiveInterval = 3.0f; 
    public float puddleDuration = 4.0f; 
    public float puddleDamagePerTick = 5.0f; 
    public float puddleRadius = 2.5f; 
    public float puddleRandomRange = 5.0f;
    private float passiveTimer = 0f;

    [Header("1. Basic Attack Settings")]
    public float basicAttackRange = 4.5f; 
    public float basicDamageDelay = 0.5f; 
    public float basicAnimDuration = 1.5f; 
    public string basicAnimStateName = "BasicAttack";

    [Header("2. Scream & Summon Settings (Jurus Menengah)")]
    public float screamRadius = 8.0f; 
    public float screamDamageDelay = 0.8f; 
    public float screamAnimDuration = 2.0f;
    public string screamAnimStateName = "Scream";
    public EnemiesData goldBoarData;
    public int summonCount = 4;
    public float summonRadius = 3.5f;
    [Tooltip("Berapa detik sekali naga boleh memanggil babi?")]
    public float screamCooldown = 22.0f; 
    private float lastScreamTime = -999f;

    [Header("3. Fly Sequence Settings (Jurus Berat)")]
    public float flyAttackRange = 12.0f; 
    public float meteorDamageRadius = 3.5f;
    public GameObject acidMeteorPrefab; 
    public Transform mouthTransform; 
    public string flyAnimStateName = "FlySequence";
    public Vector3 meteorScaleDivider = new Vector3(8f, 8f, 8f);
    public Vector3 manualSpawnOffset = Vector3.zero;
    public float meteorSpawnDelay = 2.2f; 
    public float meteorImpactDelay = 1.0f;
    public float flyShootTime = 1.0f;   
    public float flyLandTime = 1.5f;    
    [Tooltip("Berapa detik sekali naga boleh terbang?")]
    public float flyCooldown = 12.0f;
    private float lastFlyTime = -999f;

    private Vector3 currentImpactPos;
    private bool isMeteorActive = false;

    void Start()
    {
        enemyStats = GetComponent<EnemiesBase>();
        vfxScript = GetComponent<EnemyVFX>();
        
        anim = GetComponent<Animator>(); 
        if (anim == null) anim = GetComponentInChildren<Animator>(); 
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;

        // Memberikan sedikit offset awal agar naga tidak membuang semua skill beratnya secara bersamaan di detik pertama
        lastScreamTime = Time.time - screamCooldown + 5.0f; 
        lastFlyTime = Time.time - flyCooldown + 2.0f;
    }

    void Update()
    {
        if (enemyStats == null || enemyStats.playerData == null) return;

        if (enemyStats._health <= 0)
        {
            if (!isDead) Die();
            return;
        }

        if (isDead) return;

        // Manajemen Pasif (Genangan Asam)
        passiveTimer += Time.deltaTime;
        if (passiveTimer >= passiveInterval)
        {
            passiveTimer = 0f;
            SpawnAcidPuddles();
        }

        if (isAttacking) return; 

        Vector3 targetPos = enemyStats.playerData.playerPosition;
        targetPos.y = transform.position.y; 
        float distanceToPlayer = Vector3.Distance(transform.position, targetPos);
        
        // --- LOGIKA KECERDASAN AI (DINAMIS MENYESUAIKAN JARAK) ---
        bool canFly = Time.time >= lastFlyTime + flyCooldown;
        bool canScream = Time.time >= lastScreamTime + screamCooldown;

        float currentAttackRange = basicAttackRange; // Bawaan: Kejar sampai dekat
        
        if (canFly) currentAttackRange = flyAttackRange;       // Jika siap terbang, berhenti di 12 meter
        else if (canScream) currentAttackRange = screamRadius; // Jika siap auman, berhenti di 8 meter
        
        if (distanceToPlayer > currentAttackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, enemyStats._speed * Time.deltaTime);
        }
        
        if (targetPos != transform.position) transform.LookAt(targetPos);

        // Jika sudah masuk jangkauan yang dituju DAN Global Cooldown selesai
        if (distanceToPlayer <= currentAttackRange && Time.time >= lastAttackTime + enemyStats._attackInterval)
        {
            ChooseAndExecuteAttack(canFly, canScream, distanceToPlayer);
        }
    }

    void ChooseAndExecuteAttack(bool canFly, bool canScream, float distance)
    {
        isAttacking = true;

        // Eksekusi berdasarkan hierarki (Jurus Terberat -> Menengah -> Biasa)
        if (canFly && distance <= flyAttackRange)
        {
            lastFlyTime = Time.time;
            StartCoroutine(FlySequenceRoutine());
        }
        else if (canScream && distance <= screamRadius)
        {
            lastScreamTime = Time.time;
            StartCoroutine(ScreamRoutine());
        }
        else if (distance <= basicAttackRange + 0.5f)
        {
            StartCoroutine(BasicAttackRoutine());
        }
        else
        {
            isAttacking = false; // Fallback pengaman jika perhitungan jarak meleset
        }
    }

    IEnumerator BasicAttackRoutine()
    {
        if (!string.IsNullOrEmpty(basicAnimStateName)) anim.Play(basicAnimStateName, 0, 0f);
        else { anim.ResetTrigger("trig_BasicAttack"); anim.SetTrigger("trig_BasicAttack"); }

        yield return new WaitForSeconds(basicDamageDelay);

        float distanceToPlayer = Vector3.Distance(transform.position, enemyStats.playerData.playerPosition);
        if (distanceToPlayer <= basicAttackRange + 1.0f)
        {
            enemyStats.playerData.TakeDamage(enemyStats._damage);
        }

        float remainingAnimTime = basicAnimDuration - basicDamageDelay;
        if (remainingAnimTime > 0) yield return new WaitForSeconds(remainingAnimTime);

        lastAttackTime = Time.time;
        isAttacking = false;
    }

    IEnumerator ScreamRoutine() 
    { 
        if (!string.IsNullOrEmpty(screamAnimStateName)) anim.Play(screamAnimStateName, 0, 0f);
        else { anim.ResetTrigger("trig_Scream"); anim.SetTrigger("trig_Scream"); }

        yield return new WaitForSeconds(screamDamageDelay);
        SummonGoldBoars();

        float remainingAnimTime = screamAnimDuration - screamDamageDelay;
        if (remainingAnimTime > 0) yield return new WaitForSeconds(remainingAnimTime);

        lastAttackTime = Time.time; 
        isAttacking = false; 
    }

    void SummonGoldBoars()
    {
        // Pastikan data babi hutan dan prefab di dalamnya tidak kosong
        if (goldBoarData == null || goldBoarData.enemyPrefab == null) return;
        
        float angleStep = 360f / summonCount;
        for (int i = 0; i < summonCount; i++)
        {
            float currentAngle = i * angleStep;
            float dirX = Mathf.Sin(currentAngle * Mathf.Deg2Rad);
            float dirZ = Mathf.Cos(currentAngle * Mathf.Deg2Rad);
            Vector3 spawnOffset = new Vector3(dirX, 0f, dirZ) * summonRadius;
            Vector3 spawnPosition = transform.position + spawnOffset;
            spawnPosition.y = transform.position.y; 
            
            // Spawn prefab yang ada di dalam EnemiesData
            GameObject spawnedBoar = Instantiate(goldBoarData.enemyPrefab, spawnPosition, Quaternion.LookRotation(spawnOffset));
            
            // Inisialisasi stat musuh menggunakan EnemiesData
            EnemiesBase boarStats = spawnedBoar.GetComponent<EnemiesBase>();
            if (boarStats != null)
            {
                // Asumsi: Skrip EnemiesBase kamu memiliki fungsi Setup() atau Initialize() 
                // yang menerima parameter EnemiesData untuk mengatur HP, Damage, dll.
                // Jika tidak ada fungsi khusus, kamu mungkin perlu membuat setup dasar, misalnya:
                // boarStats.Initialize(goldBoarData, enemyStats.playerData);
            }
        }
    }

    IEnumerator FlySequenceRoutine()
    {
        if (!string.IsNullOrEmpty(flyAnimStateName)) anim.Play(flyAnimStateName, 0, 0f);
        else { anim.ResetTrigger("trig_FlySequence"); anim.SetTrigger("trig_FlySequence"); }
        
        yield return new WaitForSeconds(meteorSpawnDelay);
        
        Vector3 targetPos = enemyStats.playerData.playerPosition;
        targetPos.y = 0.1f; 
        currentImpactPos = targetPos;
        isMeteorActive = true; 

        if (acidMeteorPrefab != null && enemyStats.playerData != null)
        {
            GameObject meteorObj = Instantiate(acidMeteorPrefab, targetPos, Quaternion.identity);
            VisualEffect vfx = meteorObj.GetComponent<VisualEffect>();
            if (vfx != null && mouthTransform != null)
            {
                Vector3 rawOffset = mouthTransform.position - targetPos;
                float divX = meteorScaleDivider.x != 0 ? meteorScaleDivider.x : 1f;
                float divY = meteorScaleDivider.y != 0 ? meteorScaleDivider.y : 1f;
                float divZ = meteorScaleDivider.z != 0 ? meteorScaleDivider.z : 1f;
                Vector3 finalOffset = new Vector3((rawOffset.x + manualSpawnOffset.x) / divX, (rawOffset.y + manualSpawnOffset.y) / divY, (rawOffset.z + manualSpawnOffset.z) / divZ);
                vfx.SetVector3("MeteorSpawn", finalOffset);
            }
            Destroy(meteorObj, 3.5f);
        }

        yield return new WaitForSeconds(meteorImpactDelay);
        
        float distanceToExplosion = Vector3.Distance(enemyStats.playerData.playerPosition, targetPos);
        if (distanceToExplosion <= meteorDamageRadius) enemyStats.playerData.TakeDamage(enemyStats._damage * 4.0f);

        isMeteorActive = false; 
        yield return new WaitForSeconds(flyShootTime);
        yield return new WaitForSeconds(flyLandTime);
        
        lastAttackTime = Time.time; 
        isAttacking = false;
    }

    void SpawnAcidPuddles()
    {
        if (acidPuddlePrefab == null || enemyStats.playerData == null) return;
        Vector3 playerPos = enemyStats.playerData.playerPosition;
        playerPos.y = 0.05f; 
        StartCoroutine(SpawnPuddleWithWarning(playerPos));
        StartCoroutine(SpawnPuddleWithWarning(GetRandomPuddlePosition(playerPos)));
        StartCoroutine(SpawnPuddleWithWarning(GetRandomPuddlePosition(playerPos)));
    }

    IEnumerator SpawnPuddleWithWarning(Vector3 spawnPos)
    {
        GameObject indicator = null;
        if (puddleIndicatorPrefab != null) indicator = Instantiate(puddleIndicatorPrefab, spawnPos, Quaternion.identity);
        yield return new WaitForSeconds(puddleWarningTime);
        if (indicator != null) Destroy(indicator);
        CreatePuddleAt(spawnPos);
    }

    Vector3 GetRandomPuddlePosition(Vector3 center)
    {
        Vector2 randomCircle = Random.insideUnitCircle * puddleRandomRange;
        Vector3 pos = center + new Vector3(randomCircle.x, 0, randomCircle.y);
        pos.y = 0.05f;
        return pos;
    }

    void CreatePuddleAt(Vector3 spawnPos)
    {
        GameObject puddleObj = Instantiate(acidPuddlePrefab, spawnPos, Quaternion.identity);
        AcidPuddle puddleScript = puddleObj.GetComponent<AcidPuddle>();
        if (puddleScript == null) puddleScript = puddleObj.AddComponent<AcidPuddle>();
        puddleScript.Setup(enemyStats.playerData, puddleDamagePerTick, puddleDuration, puddleRadius);
    }

    void Die()
    {
        isDead = true;
        isAttacking = false;
        isMeteorActive = false;
        StopAllCoroutines();

        if (!string.IsNullOrEmpty(dieAnimStateName)) anim.Play(dieAnimStateName, 0, 0f);
        else { anim.ResetTrigger("trig_Die"); anim.SetTrigger("trig_Die"); }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        if (destroyAfterDelay > 0) Destroy(gameObject, destroyAfterDelay);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, basicAttackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, flyAttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, screamRadius);

        if (isMeteorActive)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentImpactPos, meteorDamageRadius);
        }
    }
}