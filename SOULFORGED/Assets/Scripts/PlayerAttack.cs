using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{   
    [SerializeField] PlayerData playerData;
    public Transform spawnPoint;
    public List<ItemsSO> equippedItems = new List<ItemsSO>();
    private Dictionary<ItemsSO, float> lastUsedTimes = new Dictionary<ItemsSO, float>();

    void Start()
    {
        // Ambil list dari PlayerData
        if (playerData != null)
        {
            Debug.Log("Jumlah item yang dipakai: " + equippedItems.Count);
        }
        else
        {
            Debug.LogError("PlayerData tidak di-assign di Inspector!");
        }
    }
    void TakeItemToList()
    {
        
    }
    void Update()
    {
        if (equippedItems == null || equippedItems.Count == 0) return;

        foreach (ItemsSO item in equippedItems)
        {
            if (item == null) continue; // Skip kalau item kosong

            if (!lastUsedTimes.ContainsKey(item))
                lastUsedTimes[item] = -999f;
            
            if (Time.time >= lastUsedTimes[item] + item.cooldown)
            {
                item.Use(spawnPoint);
                lastUsedTimes[item] = Time.time;
            }
        }
    }

    
}