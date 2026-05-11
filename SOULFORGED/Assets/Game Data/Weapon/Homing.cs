using UnityEngine;

public class Homing : MonoBehaviour
{
    private float _speed;
    private float _damage;
    public float Damage => _damage;

    // Variabel Homing
    private Transform _target;
    private float _turnSpeed;
    private float _homingDelay;
    private float _timer = 0f;

    public void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle)
    {
        _speed = speed;
        _damage = damage;
        _turnSpeed = turnSpeed;
        _homingDelay = homingDelay;

        // PERBAIKAN: Hanya menyebar ke kiri-kanan (Sumbu Y) agar tidak menabrak tanah
        Vector3 randomRotation = new Vector3(
            0f, 
            Random.Range(-scatterAngle, scatterAngle), 
            0f
        );
        transform.Rotate(randomRotation);

        // Cari musuh terdekat secara otomatis
        FindNearestEnemy();

        // Hancurkan peluru otomatis jika sudah mencapai batas waktu (lifetime)
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        _timer += Time.deltaTime;

        // FASE MENGEJAR: Kalau delay sudah lewat dan target masih ada
        if (_timer > _homingDelay && _target != null)
        {
            Vector3 directionToTarget = _target.position - transform.position;
            
            // Mengabaikan perbedaan tinggi (Y) agar peluru tidak menukik ke tanah atau terbang ke atas
            directionToTarget.y = 0; 

            if (directionToTarget != Vector3.zero)
            {
                // Putar peluru menghadap musuh perlahan-lahan
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
            }
        }

        // FASE MAJU: Peluru selalu bergerak maju ke depan
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    // Fungsi untuk otomatis mendeteksi musuh terdekat dengan tag "Enemy"
    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);
            if (distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            _target = nearestEnemy.transform;
        }
    }

    // Fungsi saat peluru menabrak sesuatu
    private void OnTriggerEnter(Collider other)
    {
        // Hanya cek musuh saja
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Kena {other.name}! Damage: {_damage}");
            Destroy(gameObject);
        }
    }
}