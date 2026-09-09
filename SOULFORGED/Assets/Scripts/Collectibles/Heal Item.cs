using UnityEngine;
using System.Collections.Generic;

public class HealItem : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    [SerializeField] int HealAmount = 100;

    private static List<HealItem> activeHeals = new List<HealItem>();
    private const int MAX_DROPS = 50;

    private void Start()
    {
        // Jika penuh, hapus item yang paling lama di tanah (index 0)
        while (activeHeals.Count >= MAX_DROPS)
        {
            if (activeHeals[0] != null) Destroy(activeHeals[0].gameObject);
            activeHeals.RemoveAt(0);
        }

        activeHeals.Add(this);
        Destroy(gameObject, 30f);
    }

    private void OnDestroy()
    {
        // Otomatis kurangi hitungan jika diambil player atau hilang setelah 30 detik
        activeHeals.Remove(this);
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            float newHealth = playerData.health + HealAmount;
            playerData.health = newHealth > playerData.maxHealth ? playerData.maxHealth : newHealth;
            Destroy(gameObject);
        }
    }
}