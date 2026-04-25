using UnityEngine;

public class EnemiesBase : MonoBehaviour
{
    [SerializeField] public PlayerData playerData; // Tetap pakai SO PlayerData kamu

    public float _health;
    public float _speed;
    public float _damage;
    public float _attackInterval;
    public float expReward;
    
    private bool isDead = false;

    // Fungsi ini dipanggil oleh Spawner untuk memberi nilai stats
    public void Setup(float health, float speed, float baseDamage, float attackInterval, float expReward)
    {
        _health = health;
        _speed = speed;
        _damage = baseDamage;
        _attackInterval = attackInterval;
        this.expReward = expReward;
    }

    // Hanya urus saat musuh kena peluru
    void OnTriggerEnter(Collider other)
    {   
        if (isDead) return; 
        
        if (other.gameObject.CompareTag("ProjectileDamage")) {
            if (other.TryGetComponent(out ProjectileLogic projectile)) {
                _health -= projectile.Damage;
                Debug.Log($"Musuh Kena Serangan! Sisa Health: {_health}");
                
                if (_health <= 0) {
                    EnemyDead();
                }
            }
        }
    }

    void EnemyDead() {
        if (isDead) return;
        isDead = true;
        playerData.exp += expReward;
        Destroy(gameObject);
    }
}