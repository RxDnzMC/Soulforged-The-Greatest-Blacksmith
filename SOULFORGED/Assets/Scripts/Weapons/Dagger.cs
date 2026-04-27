using UnityEngine;

public class ProjectileLogic : MonoBehaviour
{
    private float _speed;
    private float _damage;
    public float Damage => _damage;

    public void Setup(float speed, float damage, float lifetime)
    {
        _speed = speed;
        _damage = damage;
        Destroy(gameObject, lifetime); // Hancur otomatis setelah lifetime habis
    }

    void Update()
    {
        // Rumus gerak maju yang kita bahas tadi
        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Logika kalau kena musuh
        if (other.CompareTag("Enemy"))
        {
            Debug.Log($"Kena {other.name}! Damage: {_damage}");
            Destroy(gameObject);
        }
    }
}