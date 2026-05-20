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

        // STANDAR GAME: Jika musuh yang diincar mati duluan sebelum tertabrak, 
        // peluru otomatis mencari musuh terdekat baru agar tidak terbang lurus tak berguna
        if (_target == null)
        {
            FindNearestEnemy();
        }

        // Logika Homing: Berbelok mulus ke arah musuh jika masa delay sudah selesai
        if (_timer > _homingDelay && _target != null)
        {
            Vector3 direction = (_target.position - transform.position);
            direction.y = 0; // Mengabaikan sumbu Y agar peluru stabil bergerak mendatar di tanah

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                // Slerp membuat pergerakan belok menjadi halus/smooth sesuai turnSpeed
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
            }
        }

        // Peluru selalu bergerak maju ke depan sesuai arah moncong peluru saat ini
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy"); //
        float shortestDistance = Mathf.Infinity; //
        Transform nearestEnemy = null;
        
        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue; // Antisipasi jika musuh hancur di frame ini
            
            float dist = Vector3.Distance(transform.position, enemy.transform.position); //
            if (dist < shortestDistance) //
            {
                shortestDistance = dist; //
                nearestEnemy = enemy.transform; //
            }
        }
        _target = nearestEnemy; //
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) //
        {
            // Di sini kamu bisa memanggil script musuh untuk mengurangi HP-nya, contoh:
            // if (other.TryGetComponent(out EnemyHealth enemy)) enemy.TakeDamage(_damage);
            
            Destroy(gameObject); //
        }
    }
}