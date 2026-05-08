using System;
using Unity.VisualScripting;
using UnityEngine;

public class Enemies1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] PlayerData playerData;

    public float _health;
    public float _speed;
    public float _damage;
    public float _attackInterval;
    public float lastDamageTime = -999f;
    private bool isDead = false;    
    public float expReward = 100f; // Contoh exp yang diberikan saat musuh mati
    public void Setup(float health, float speed, float baseDamage, float attackInterval, float expReward)
    {
        _health = health;
        _speed = speed;
        _damage = baseDamage;
        _attackInterval = attackInterval;
        this.expReward = expReward;

    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) {
            if (playerData.health <= 0) return; // Cegah player yang sudah mati kena damage lagi
            if (Time.time >= lastDamageTime + _attackInterval) {
                playerData.health -= _damage;
                lastDamageTime = Time.time;
            }
        }

    }

    void OnTriggerEnter(Collider other)
    {   
        if (isDead) return; // Cegah musuh yang sudah mati kena damage lagi
        
        if (other.gameObject.CompareTag("ProjectileDamage")) {
            // Asumsikan Projectile punya script FireballProjectile yang punya variabel damage
            if (other.TryGetComponent(out FireballProjectile projectile)) {
                _health -= projectile.Damage;
                Debug.Log($"Kena serangan! Health sekarang: {_health}");
            }
            if (other.TryGetComponent(out FireballProjectile2 projectile2)) {
                _health -= projectile2.Damage;
                Debug.Log($"Kena serangan! Health sekarang: {_health}");
            }
        }
    }
    void EnemyDead() {
        if (isDead) return; // Cegah multiple call
        isDead = true;
        Debug.Log("Musuh mati, kasih exp ke player");
        playerData.exp += expReward; // Contoh exp yang diberikan
        Destroy(gameObject);
    }
    void Update()
    {
        if (playerData != null) 
        {   
            Vector3 targetPos = playerData.playerPosition;

            // 2. PAKSA nilai Y target sama dengan nilai Y musuh sekarang
            targetPos.y = transform.position.y;
            // Gerak simpel nembus tembok sesuai speed dari SO
            transform.position = Vector3.MoveTowards(
                    transform.position, 
                   targetPos, // Pastikan PlayerData punya variabel posisi player
                    _speed * Time.deltaTime
                );

            transform.LookAt(playerData.playerPosition);
        }

        if (_health <= 0) {
            EnemyDead();
            Destroy(gameObject);
        }   
    }
}
