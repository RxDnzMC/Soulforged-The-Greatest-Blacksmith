using UnityEngine;
using System.Collections.Generic;

public class LightHoleMechanic : MonoBehaviour, IProjectile
{
    [Header("Orbit Settings")]
    [Tooltip("Kecepatan memutari player")]
    public float orbitSpeed = 150f; 
    [Tooltip("Jarak bola cahaya dari player")]
    public float baseOrbitRadius = 3f; 
    [Tooltip("Ketinggian posisi bola cahaya dari tanah")]
    public float heightOffset = 0.5f; 
    [Tooltip("Besarnya area damage per bola cahaya (Ukuran Konstan Pas Partikel)")]
    public float baseHitRadius = 1.5f; 
    [Tooltip("Jeda waktu musuh terkena hit lagi")]
    public float damageTickRate = 0.5f;

    private float currentDamage;
    private float currentSizeMultiplier = 1f;
    private int currentCount = 1;
    private float currentAngle = 0f; // Menampung rotasi mandiri independen

    private GameObject visualPrefab;
    private List<Transform> activeHoles = new List<Transform>();
    
    // Dictionary untuk mencatat riwayat jam berapa setiap musuh terakhir kali terkena hit
    private Dictionary<Collider, float> enemyHitCooldowns = new Dictionary<Collider, float>();

    // Implementasi Interface IProjectile
    public float Damage => currentDamage;
    public float Cooldown => 0f;

    // Fungsi khusus untuk menyimpan data prefab sebelum Setup dipanggil
    public void InitializeManager(int count, GameObject prefab)
    {
        visualPrefab = prefab;
        currentCount = count;
    }

    // Fungsi WAJIB bawaan antarmuka IProjectile
    public void Setup(float speed, float damage, float lifetime, float turnSpeed, float homingDelay, float scatterAngle, float cooldown, float size)
    {
        orbitSpeed = speed > 0 ? speed : orbitSpeed; 
        currentDamage = damage;
        currentSizeMultiplier = size;
        
        RefreshHoles(); // Panggil pemunculan bola setelah data siap
    }

    // Fungsi untuk memperbarui data saat level senjata naik
    public void UpdateStats(float speed, float damage, float size, int count)
    {
        orbitSpeed = speed > 0 ? speed : orbitSpeed; 
        currentDamage = damage;
        currentSizeMultiplier = size;
        
        // Jika jumlah (Count) berubah, perbarui formasi bola
        if (currentCount != count || activeHoles.Count == 0)
        {
            currentCount = count;
            RefreshHoles();
        }
    }

    private void RefreshHoles()
    {
        // Hapus bola lama jika ada
        foreach (var hole in activeHoles) 
        { 
            if (hole != null) Destroy(hole.gameObject); 
        }
        activeHoles.Clear();

        // Bersihkan buku tamu musuh agar memori tetap ringan tiap kali ganti level
        enemyHitCooldowns.Clear();

        // Spawn bola baru membentuk formasi melingkar
        for (int i = 0; i < currentCount; i++)
        {
            GameObject holeObj = Instantiate(visualPrefab, transform);
            activeHoles.Add(holeObj.transform);
        }
    }

    void Update()
    {
        // 1. KUNCI ROTASI INDUK: Paksa rotasi selalu sejajar kompas dunia (Quaternion.identity)
        // Ini memastikan orbit TIDAK ikut berputar melintir saat Player memutar badannya
        transform.rotation = Quaternion.identity;

        // 2. HITUNG ROTASI MANDIRI
        currentAngle += orbitSpeed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        // Size Multiplier bertugas mengatur jarak orbit mutar dari Player
        float actualOrbitRadius = baseOrbitRadius * currentSizeMultiplier;
        float angleStep = 360f / Mathf.Max(1, currentCount);

        for (int i = 0; i < activeHoles.Count; i++)
        {
            if (activeHoles[i] == null) continue;
            
            // Sertakan currentAngle agar putaran berjalan alami terhadap dunia
            float angle = currentAngle + (i * angleStep);
            
            // Gunakan heightOffset pada sumbu Y agar bola terangkat dari tanah
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * actualOrbitRadius, 
                heightOffset, 
                Mathf.Sin(angle * Mathf.Deg2Rad) * actualOrbitRadius
            );
            
            activeHoles[i].localPosition = offset;
            
            // Memaksa bola selalu menghadap atas (0,0,0) agar visual particle/billboard aman
            activeHoles[i].rotation = Quaternion.identity; 
        }

        // Area Damage Kalkulasi dipanggil SETIAP FRAME untuk respon instan
        ApplyAreaDamage();
    }

    private void ApplyAreaDamage()
    {
        float actualHitRadius = baseHitRadius;
        
        foreach (Transform hole in activeHoles)
        {
            if (hole == null) continue;
            
            Collider[] hitColliders = Physics.OverlapSphere(hole.position, actualHitRadius);
            foreach (Collider hit in hitColliders)
            {
                if (hit.CompareTag("Enemy") && hit.TryGetComponent(out EnemiesBase enemyScript))
                {
                    // Cek apakah musuh baru masuk ATAU jeda tick-nya sudah lewat
                    if (!enemyHitCooldowns.ContainsKey(hit) || Time.time >= enemyHitCooldowns[hit] + damageTickRate)
                    {
                        enemyScript.TakeDamage(this); 
                        
                        // Perbarui catatan jam terakhir musuh ini kena hit
                        enemyHitCooldowns[hit] = Time.time; 
                    }
                }
            }
        }
    }

    // Menggambar garis radius orbit & area mematikan di Inspector
    private void OnDrawGizmosSelected()
    {
        float actualOrbit = baseOrbitRadius * currentSizeMultiplier;
        float actualHit = baseHitRadius;

        // Titik pusat lingkaran Gizmo dinaikkan sesuai heightOffset
        Vector3 centerPos = transform.position + new Vector3(0f, heightOffset, 0f);

        // Gambar jalur rel (garis orbit putih transparan)
        Gizmos.color = new Color(1f, 1f, 1f, 0.5f);
        Gizmos.DrawWireSphere(centerPos, actualOrbit);

        // Gambar bola hitbox tiap proyektil (kuning transparan)
        Gizmos.color = new Color(1f, 0.9f, 0f, 0.4f);
        int displayCount = Mathf.Max(1, currentCount);
        float angleStep = 360f / displayCount;

        for (int i = 0; i < displayCount; i++)
        {
            float angle = (Application.isPlaying ? currentAngle : 0f) + (i * angleStep);
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad) * actualOrbit, 
                heightOffset, 
                Mathf.Sin(angle * Mathf.Deg2Rad) * actualOrbit
            );
            
            Vector3 pos = Application.isPlaying && activeHoles.Count > i ? activeHoles[i].position : transform.position + offset;
            
            Gizmos.DrawSphere(pos, actualHit);
        }
    }
}