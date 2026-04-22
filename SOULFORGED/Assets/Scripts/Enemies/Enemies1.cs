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
    
    public void Setup(float health, float speed, float baseDamage, float attackInterval)
    {
        _health = health;
        _speed = speed;
        _damage = baseDamage;
        _attackInterval = attackInterval;

    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) {
            if (Time.time >= lastDamageTime + _attackInterval) {
                playerData.health -= _damage;
                lastDamageTime = Time.time;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("ProjectileDamage")) {
            // Asumsikan Projectile punya script ProjectileLogic yang punya variabel damage
            if (other.TryGetComponent(out ProjectileLogic projectile)) {
                _health -= projectile.Damage;
                Debug.Log($"Kena serangan! Health sekarang: {_health}");
            }
        }
    }

    void Update()
    {
        if (playerData != null) 
        {
            // Gerak simpel nembus tembok sesuai speed dari SO
            transform.position = Vector3.MoveTowards(
                    transform.position, 
                    playerData.playerPosition, // Pastikan PlayerData punya variabel posisi player
                    _speed * Time.deltaTime
                );

            transform.LookAt(playerData.playerPosition);
        }

        if (_health <= 0) {
            Destroy(gameObject);
        }   
    }
}
