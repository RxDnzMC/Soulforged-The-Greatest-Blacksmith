using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{   
    [SerializeField] PlayerData playerData;
    
    [Header("Spawn Points")]
    public Transform defaultSpawnPoint;  // Buat fireball (agak jauh dari player)
    public Transform groundSpawnPoint;   // Buat catalyst (dekat kaki player)
    
    public List<ItemsSO> equippedActiveItems = new List<ItemsSO>();
    public List<ItemsPassiveSO> equippedPassiveItems = new List<ItemsPassiveSO>();

    void Update()
    {
        if (equippedActiveItems == null || equippedActiveItems.Count == 0) return;

        foreach (ItemsSO item in equippedActiveItems)
        {
            if (item == null) continue;
            
            if (item.isReady)
            {
                // Pilih spawnPoint sesuai item
                Transform spawnPoint = item.itemName.Contains("Catalyst") ? groundSpawnPoint : defaultSpawnPoint;
                item.Use(spawnPoint);
            }
        }
    }
}