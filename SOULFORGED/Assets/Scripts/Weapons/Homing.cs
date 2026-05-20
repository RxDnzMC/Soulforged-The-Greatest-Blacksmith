using UnityEngine;

public class Homing : MonoBehaviour, IProjectile
{
    private float _speed;
    private float _damage;
    public float Damage => _damage;
    private float _turnSpeed;
    private float _homingDelay;
    public float Cooldown => cooldown;
    private float cooldown;
    
    private Transform _target;
    private float _timer = 0f;

    public void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle, float cooldown, float size)
    {
        _speed = speed;
        _damage = damage;
        _turnSpeed = turnSpeed;
        _homingDelay = homingDelay;
        this.cooldown = cooldown;

        // Menerapkan ukuran visual peluru berdasarkan stat size multiplier dari data senjata
        transform.localScale = Vector3.one * size;

        // Logika Menyebar: Rotasi acak awal di sumbu Y (kiri-kanan)
        transform.Rotate(0f, Random.Range(-scatterAngle, scatterAngle), 0f);

        // Mencari target terdekat pertama kali saat muncul
        FindNearestEnemy();

        // Mengatur durasi aktif peluru sebelum hancur otomatis
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        _timer += Time.deltaTime;

        bool isRetargetingMidFlight = false;

        // Cek jika target null ATAU target sudah dinonaktifkan (mati)
        if (_target == null || !_target.gameObject.activeInHierarchy)
        {
            FindNearestEnemy();
            
            // Jika dia ganti target saat delay sudah habis (di tengah jalan terbang)
            if (_timer > _homingDelay && _target != null)
            {
                isRetargetingMidFlight = true;
            }
        }

        // Logika Homing: Berbelok ke arah musuh jika masa delay sudah selesai
        if (_timer > _homingDelay && _target != null)
        {
            Vector3 direction = (_target.position - transform.position);
            direction.y = 0; // Mengabaikan sumbu Y agar peluru stabil bergerak mendatar di tanah

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                
                if (isRetargetingMidFlight)
                {
                    // SNAP ROTATION: Langsung hadap target baru biar nggak mutar-mutar!
                    transform.rotation = targetRotation; 
                }
                else
                {
                    // Slerp membuat pergerakan belok menjadi halus/smooth saat mengejar target biasa
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
                }
            }
        }

        // Peluru selalu bergerak maju ke depan sesuai arah moncong peluru saat ini
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); 
        float shortestDistance = Mathf.Infinity; 
        Transform nearestEnemy = null;
        
        foreach (GameObject enemy in enemies)
        {
            // Abaikan jika musuh null ATAU sudah mati/dinonaktifkan
            if (enemy == null || !enemy.activeInHierarchy) continue; 
            
            float dist = Vector3.Distance(transform.position, enemy.transform.position); 
            if (dist < shortestDistance) 
            {
                shortestDistance = dist; 
                nearestEnemy = enemy.transform; 
            }
        }
        _target = nearestEnemy; 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) 
        {
            // if (other.TryGetComponent(out EnemyHealth enemy)) enemy.TakeDamage(_damage);
            Destroy(gameObject); 
        }
    }
}