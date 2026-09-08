using UnityEngine;

public class AcidPuddle : MonoBehaviour
{
    [Header("Puddle DoT Settings")]
    public float duration = 4.0f;        // Durasi aktif genangan di tanah
    public float damageInterval = 0.2f; // Jeda waktu ngedamage (tiap 0.2 detik)
    public float radius = 2.5f;         // Radius area genangan api
    public float damagePerTick = 5.0f;  // Damage per 0.2 detik

    private PlayerData targetPlayer;
    private float tickTimer = 0f;

    // Fungsi inisialisasi statistik dari naga
    public void Setup(PlayerData player, float dmg, float dur, float rad)
    {
        targetPlayer = player;
        damagePerTick = dmg;
        duration = dur;
        radius = rad;

        // Hapus genangan api otomatis dari game setelah durasi habis
        Destroy(gameObject, duration);
    }

    void Update()
    {
        if (targetPlayer == null) return;

        tickTimer += Time.deltaTime;

        // Hitung damage berulang setiap 0.2 detik
        if (tickTimer >= damageInterval)
        {
            tickTimer = 0f;

            // Cek apakah player berdiri di atas genangan
            float distance = Vector3.Distance(transform.position, targetPlayer.playerPosition);
            if (distance <= radius)
            {
                targetPlayer.TakeDamage(damagePerTick);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualisasi radius genangan api di Scene View
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}