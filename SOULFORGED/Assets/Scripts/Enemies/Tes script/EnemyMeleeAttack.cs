using UnityEngine;
using System.Collections;

public class EnemyMeleeAttack : MonoBehaviour
{
    private EnemiesBase enemyStats;
    private float lastAttackTime = -999f;
    private Animator anim; 
    
    private bool isAttacking = false;

    [Header("Melee Settings")]
    public float attackRange = 2.5f; 
    public float attackDuration = 1.5f; 
    
    [Tooltip("Daftar detik kapan saja tangan musuh mengenai player. Kalau 1x pukul isi Size: 1. Kalau combo 4x isi Size: 4.")]
    public float[] damageDelays = { 0.5f }; 

    [Header("Animation Settings")]
    public string attackTriggerName = "Attack"; 
    
    // --- FITUR BARU UNTUK MUSUH PERTAMA ---
    [Tooltip("Centang ini JIKA musuh punya parameter 'AttackIndex' untuk serangan acak (seperti musuh pertama)")]
    public bool useRandomAttacks = false;
    
    [Tooltip("Jika kotak di atas dicentang, berapa jumlah variasi serangannya? (Misal: 3)")]
    public int totalRandomAttacks = 3;

    void Start()
    {
        enemyStats = GetComponent<EnemiesBase>();
        
        if (anim == null)
        {
            anim = GetComponent<Animator>(); 
            if (anim == null) anim = GetComponentInChildren<Animator>(); 
        }
        
        //FREEZE ROTASI RIGIDBODY
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;
    }

    void Update()
    {
        if (enemyStats.playerData == null || enemyStats._health <= 0) return;

        if (isAttacking) return; 

        Vector3 targetPos = enemyStats.playerData.playerPosition;
        targetPos.y = transform.position.y; 
        
        transform.position = Vector3.MoveTowards(transform.position, targetPos, enemyStats._speed * Time.deltaTime);
        transform.LookAt(targetPos);

        float distanceToPlayer = Vector3.Distance(transform.position, targetPos);
        if (distanceToPlayer <= attackRange)
        {
            if (Time.time >= lastAttackTime + enemyStats._attackInterval)
            {
                StartAttack();
            }
        }
    }

    void StartAttack()
    {
        isAttacking = true; 
        lastAttackTime = Time.time;
        
        if (anim != null)
        {
            // Cek apakah musuh ini pakai sistem Random Index (Musuh Pertama)
            if (useRandomAttacks)
            {
                int randomNum = Random.Range(0, totalRandomAttacks);
                anim.SetInteger("AttackIndex", randomNum);
            }
            
            // Selalu panggil Trigger utamanya (Apapun nama serangannya)
            anim.SetTrigger(attackTriggerName); 
        }

        // Jalankan proses pemberian damage (bisa 1x, bisa 4x combo)
        foreach (float delay in damageDelays)
        {
            StartCoroutine(DealDamageRoutine(delay));
        }

        Invoke("StopAttack", attackDuration); 
    }

    IEnumerator DealDamageRoutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        Vector3 currentPlayerPos = enemyStats.playerData.playerPosition;
        currentPlayerPos.y = transform.position.y;
        float currentDistance = Vector3.Distance(transform.position, currentPlayerPos);

        if (currentDistance <= attackRange + 0.5f)
        {
            enemyStats.playerData.health -= enemyStats._damage;
            Debug.Log($"[{gameObject.name}] Pukulan Kena!");
        }
        else
        {
            Debug.Log($"[{gameObject.name}] Serangan Meleset!");
        }
    }

    void StopAttack()
    {
        isAttacking = false;
        
        //RESET PARAMETER ANIMASI
        if (anim != null)
        {
            if (useRandomAttacks)
                anim.SetInteger("AttackIndex", 0);
                
            // Optional: reset trigger biar gak keulang
            anim.ResetTrigger(attackTriggerName);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}