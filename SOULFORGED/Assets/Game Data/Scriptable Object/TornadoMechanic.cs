using UnityEngine;

public class TornadoMechanic : MonoBehaviour
{
    private float moveSpeed;
    private float damageAmount;
    private float lifeTime;
    
    private Vector2 moveDirection;
    private float changeDirTimer;

    // Fungsi Setup dipanggil oleh TornadoSO
    public void Setup(float speed, float damage, float force, float knockback, float lifetime, float stun, float cooldown, float size)
    {
        moveSpeed = speed;
        damageAmount = damage;
        this.lifeTime = lifetime;
        
        // Sesuaikan ukuran tornado berdasarkan level
        transform.localScale = new Vector3(size, size, size);
        
        // Tentukan arah acak pertama kali saat di-spawn
        PickRandomDirection();
        
        // Hancurkan tornado setelah durasi lifetime habis
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        // Menggerakkan tornado
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Timer untuk mengubah arah tornado secara acak (misal tiap 0.5 - 1.5 detik)
        changeDirTimer -= Time.deltaTime;
        if (changeDirTimer <= 0)
        {
            PickRandomDirection();
        }
    }

    private void PickRandomDirection()
    {
        // Memilih sudut acak 360 derajat
        float randomAngle = Random.Range(0f, 360f);
        moveDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
        
        // Waktu acak sebelum berubah arah lagi
        changeDirTimer = Random.Range(0.5f, 1.5f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Masukkan logika damage musuh di sini
        // if (collision.CompareTag("Enemy")) { collision.GetComponent<Enemy>().TakeDamage(damageAmount); }
    }
}