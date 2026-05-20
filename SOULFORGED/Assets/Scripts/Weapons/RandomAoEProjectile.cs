using UnityEngine;

public class RandomAoEProjectile : MonoBehaviour, IProjectile
{
    [Header("AoE Explosion Settings")]
    [Tooltip("Waktu tunggu sebelum ledakan memberikan damage")]
    public float damageTickRate = 0.5f; 
    public float baseRadius = 2.5f; 

    private float _damage;
    private float _sizeMultiplier = 1f;
    private float tickTimer = 0f;
    
    // ✅ Variabel untuk mengunci target
    private Transform myTarget; 

    public float Damage => _damage;
    public float Cooldown => 0f; 

    // ✅ Fungsi untuk menerima target dari RandomAoESO
    public void SetTarget(Transform target)
    {
        myTarget = target;
    }

    public void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle, float cooldown, float size)
    {
        _damage = damage;
        _sizeMultiplier = size;
        transform.localScale = new Vector3(size, size, size);
        Destroy(gameObject, lifetime); 
    }

    void Update()
    {
        // ✅ 1. SISTEM LOCK-ON (MENGKUTI MUSUH)
        // Jika targetnya masih ada dan masih hidup, tempel posisinya!
        if (myTarget != null && myTarget.gameObject.activeInHierarchy)
        {
            Vector3 followPos = myTarget.position;
            followPos.y = 0.1f; // Pastikan tetap rata di tanah
            transform.position = followPos;
        }

        // ✅ 2. SISTEM TIMER LEDAKAN
        tickTimer += Time.deltaTime;
        if (tickTimer >= damageTickRate)
        {
            ApplyAreaDamage();
            tickTimer = 0f; 
        }
    }

    private void ApplyAreaDamage()
    {
        float actualRadius = baseRadius * _sizeMultiplier;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, actualRadius);

        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Enemy"))
            {
                if (hit.TryGetComponent(out EnemiesBase enemyScript))
                {
                    enemyScript.TakeDamage(this); 
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        float radius = baseRadius * (_sizeMultiplier > 0 ? _sizeMultiplier : 1f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}