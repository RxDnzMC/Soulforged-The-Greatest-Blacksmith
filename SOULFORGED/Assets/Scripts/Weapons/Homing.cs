using UnityEngine;

public class Homing : MonoBehaviour, IProjectile
{
    // Variabel internal untuk menyimpan data kiriman SO
    private float _speed;
    private float _damage;
    public float Damage => _damage;
    private float _turnSpeed;
    private float _homingDelay;
    public float Cooldown => cooldown;
    private float cooldown;
    
    private Transform _target;
    private float _timer = 0f;

    // Implementasi Interface IProjectileHoming: Tempat menerima paket data
    public void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle, float cooldown, float size)
    {
        _speed = speed;
        _damage = damage;
        _turnSpeed = turnSpeed;
        _homingDelay = homingDelay;

        // Logika Menyebar: Rotasi acak di sumbu Y (kiri-kanan) saja
        transform.Rotate(0f, Random.Range(-scatterAngle, scatterAngle), 0f);

        // Cari target pertama kali saat muncul
        FindNearestEnemy();

        // Hancurkan diri sendiri sesuai lifetime
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        _timer += Time.deltaTime;

        // Logika Homing: Belok kalau sudah lewat delay dan target masih hidup
        if (_timer > _homingDelay && _target != null)
        {
            Vector3 direction = (_target.position - transform.position);
            direction.y = 0; // Mengabaikan ketinggian (tetap datar)

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // Slerp membuat pergerakan belok jadi halus (smooth)
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
            }
        }

        // Selalu bergerak maju ke depan arah moncong peluru
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        
        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                _target = enemy.transform;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // Debug.Log($"Target Hit: {other.name} | Damage: {_damage}");
            // Tambahkan logika damage musuh di sini (misal: other.GetComponent<Enemy>().TakeDamage(_damage))
            Destroy(gameObject);
        }
    }
}