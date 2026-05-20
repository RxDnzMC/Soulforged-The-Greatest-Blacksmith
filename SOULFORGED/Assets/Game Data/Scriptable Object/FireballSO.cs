using UnityEngine;

[CreateAssetMenu(fileName = "New Fireball", menuName = "ScriptableObjects/Weapons/Fireball")]
public class FireballSO : ItemsSO 
{
    // Variabel spreadAngle dihapus karena sekarang sudah ada di Level Data (Element)

    protected override void SpawnProjectiles(Transform spawnPoint, int count, float damage, float size, float speed, float lifetime, float spreadAngle)
    {
        // Kalau peluru cuma 1, tembak lurus biasa
        if (count <= 1)
        {
            FireSingle(spawnPoint, damage, size, speed, lifetime, 0f);
            return;
        }

        // Kalau peluru lebih dari 1, tembak menyebar (Spread) menggunakan spreadAngle dari level saat ini
        float startAngle = -spreadAngle / 2f;
        float angleStep = spreadAngle / (count - 1);

        for (int i = 0; i < count; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            FireSingle(spawnPoint, damage, size, speed, lifetime, currentAngle);
        }
    }
}