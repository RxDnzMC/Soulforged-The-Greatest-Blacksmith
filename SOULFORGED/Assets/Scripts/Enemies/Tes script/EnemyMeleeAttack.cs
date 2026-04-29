using UnityEngine;
using System.Collections;

public class EnemyMeleeAttack : MonoBehaviour
{
    private EnemiesBase enemyStats;
    private float lastAttackTime = -999f;
    private Animator anim; 
    
    private bool isAttacking = false;
    
    // --- STATUS CHARGING & EFEK ---
    private bool isCharging = false;
    private EnemyVFX vfxScript;

    [Header("Melee Settings")]
    public float attackRange = 2.5f; 
    public float attackDuration = 1.5f; 
    
    [Tooltip("Daftar detik kapan saja tangan musuh mengenai player. Kalau 1x pukul isi Size: 1. Kalau combo 4x isi Size: 4.")]
    public float[] damageDelays = { 0.5f }; 

    [Header("AoE Warning Settings")]
    [Tooltip("Masukkan objek IndicatorAttack (lingkaran merah) dari Hierarchy ke sini")]
    public GameObject attackIndicator;
    [Tooltip("Berapa lama indikator merah muncul sebelum musuh memukul? (Isi 0 jika ingin musuh langsung memukul tanpa nunggu)")]
    public float warningDuration = 5f;

    [Header("Animation Settings")]
    public string attackTriggerName = "Attack"; 
    
    [Tooltip("Centang ini JIKA musuh punya parameter 'AttackIndex' untuk serangan acak (seperti musuh pertama)")]
    public bool useRandomAttacks = false;
    
    [Tooltip("Jika kotak di atas dicentang, berapa jumlah variasi serangannya? (Misal: 3)")]
    public int totalRandomAttacks = 3;

    void Start()
    {
        enemyStats = GetComponent<EnemiesBase>();
        vfxScript = GetComponent<EnemyVFX>();
        
        if (anim == null)
        {
            anim = GetComponent<Animator>(); 
            if (anim == null) anim = GetComponentInChildren<Animator>(); 
        }
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;

        // Pastikan indikator mati saat game baru mulai
        if (attackIndicator != null) attackIndicator.SetActive(false);
    }

    void Update()
    {
        // Berhenti jika tidak ada player atau musuh sudah mati
        if (enemyStats.playerData == null || enemyStats._health <= 0) return;

        // Musuh hanya diam mematung SAAT benar-benar memukul (animasi jalan).
        // Kalau baru fase charging (indikator merah nyala), dia tetap bisa mengejar player.
        if (isAttacking) return; 

        Vector3 targetPos = enemyStats.playerData.playerPosition;
        targetPos.y = transform.position.y; 
        
        transform.position = Vector3.MoveTowards(transform.position, targetPos, enemyStats._speed * Time.deltaTime);
        transform.LookAt(targetPos);

        float distanceToPlayer = Vector3.Distance(transform.position, targetPos);
        
        // Jika player masuk dalam jangkauan pukulan
        if (distanceToPlayer <= attackRange)
        {
            // Cek apakah musuh tidak sedang nunggu (nge-charge) DAN cooldown serangannya sudah siap
            if (!isCharging && Time.time >= lastAttackTime + enemyStats._attackInterval)
            {
                // Kalau ada waktu delay indikator (Bos), nyalakan fase Charging
                if (warningDuration > 0)
                {
                    StartCoroutine(ChargeAttackRoutine());
                }
                // Kalau waktu delaynya 0 (Musuh Kroco), langsung pukul tanpa basa-basi!
                else
                {
                    StartAttack();
                }
            }
        }
    }

    // --- FUNGSI MENGHITUNG WAKTU SEBELUM MUKUL ---
    IEnumerator ChargeAttackRoutine()
    {
        isCharging = true; // Tandai bahwa musuh sedang menghitung mundur
        
        // 1. Nyalakan lingkaran merah
        if (attackIndicator != null) attackIndicator.SetActive(true);

        // 2. Tunggu selama beberapa detik sesuai angka warningDuration (musuh tetap jalan)
        yield return new WaitForSeconds(warningDuration);

        // 3. Matikan lingkaran merah
        if (attackIndicator != null) attackIndicator.SetActive(false);
        
        isCharging = false; // Selesai menghitung mundur

        // Jika saat nunggu musuhnya keburu mati ditembak, batalkan serangan!
        if (enemyStats._health <= 0) yield break;

        // 4. Lanjut mengeksekusi serangan sesungguhnya
        StartAttack();
    }

    // --- FUNGSI ANIMASI & MULAI MUKUL ---
    void StartAttack()
    {
        isAttacking = true; // Tandai sedang memukul (membuat musuh diam di tempat di dalam Update)
        lastAttackTime = Time.time; // Catat waktu terakhir mukul
        
        if (anim != null)
        {
            // Atur variasi animasi jika tercentang
            if (useRandomAttacks)
            {
                int randomNum = Random.Range(0, totalRandomAttacks);
                anim.SetInteger("AttackIndex", randomNum);
            }
            
            anim.SetTrigger(attackTriggerName); 
        }

        // Mulai menghitung timer damage (bisa 1 kali, bisa 4 kali combo)
        foreach (float delay in damageDelays)
        {
            StartCoroutine(DealDamageRoutine(delay));
        }

        // Setel alarm kapan pukulan dianggap selesai untuk mereset posenya
        Invoke("StopAttack", attackDuration); 
    }

    // --- FUNGSI MEMBERIKAN DAMAGE & PARTIKEL ---
    IEnumerator DealDamageRoutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        // Panggil efek partikel tepat saat pukulan mendarat
        if (vfxScript != null)
        {
            vfxScript.NyalakanPartikelSerangan();
        }

        Vector3 currentPlayerPos = enemyStats.playerData.playerPosition;
        currentPlayerPos.y = transform.position.y;
        float currentDistance = Vector3.Distance(transform.position, currentPlayerPos);

        // Jika player masih di dalam area, beri damage
        if (currentDistance <= attackRange + 0.5f)
        {
            enemyStats.playerData.health -= enemyStats._damage;
            Debug.Log($"[{gameObject.name}] Pukulan Kena!");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Serangan Meleset! Player berhasil kabur!");
        }
    }

    void StopAttack()
    {
        isAttacking = false;
        
        if (anim != null)
        {
            if (useRandomAttacks)
                anim.SetInteger("AttackIndex", 0);
                
            anim.ResetTrigger(attackTriggerName);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}