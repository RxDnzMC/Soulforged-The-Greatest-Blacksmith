using UnityEngine;

public class EnemyMelee : MonoBehaviour
{
    private EnemiesBase stats; 
    private float lastDamageTime = -999f;
    private Animator animator; 
    
    // Variabel baru untuk mengecek status menyerang
    private bool isAttacking = false;

    void Start()
    {
        stats = GetComponent<EnemiesBase>();

        if (animator == null)
        {
            animator = GetComponent<Animator>(); 
            if (animator == null) animator = GetComponentInChildren<Animator>(); 
        }
    }

    void Update()
    {
        if (stats.playerData == null) return;

        // --- LOGIKA BERHENTI SAAT MENYERANG ---
        // Jika sedang menyerang, jangan jalankan kode pergerakan di bawah
        if (isAttacking) return; 

        Vector3 targetPos = stats.playerData.playerPosition;
        targetPos.y = transform.position.y;
        
        transform.position = Vector3.MoveTowards(transform.position, targetPos, stats._speed * Time.deltaTime);
        transform.LookAt(targetPos);
    }

    void OnTriggerStay(Collider other)
    {
        if (isAttacking) return; 

        if (other.gameObject.CompareTag("Player"))
        {
            if (stats.playerData.health <= 0) return; 

            if (Time.time >= lastDamageTime + stats._attackInterval)
            {
                StartAttack();
            }
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        lastDamageTime = Time.time;
        stats.playerData.health -= stats._damage;

        if (animator != null)
        {
            animator.SetTrigger("Punch");
        }

        // Panggil fungsi untuk berhenti menyerang setelah animasi selesai (3.8 detik)
        // Kita gunakan 3.8f karena 92 frame / 24 fps = 3.83
        Invoke("StopAttack", 3.8f); 
    }

    void StopAttack()
    {
        isAttacking = false;
        Debug.Log("Selesai memukul, lanjut mengejar!");
    }
}