using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Transform spawnPoint;
    public ItemsSO equippedItem; // Isi dengan DaggerSO atau ArrowSO kamu
    private float lastDamageTime = -999f;


    void Update()
    {
        // 1. Cek dulu apakah ada item yang dipakai (biar gak error)
        if (equippedItem == null) return;

        // 2. Gunakan equippedItem.cooldown sebagai pengganti damageInterval
        if (Time.time >= lastDamageTime + equippedItem.cooldown) 
        {
            equippedItem.Use(spawnPoint);
            lastDamageTime = Time.time;
        }
    }
}