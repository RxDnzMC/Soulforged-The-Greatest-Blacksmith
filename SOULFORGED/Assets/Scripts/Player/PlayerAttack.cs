using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{   
    [SerializeField] PlayerData playerData;
    public Transform spawnPoint;
    public List<ItemsSO> equippedItems = new List<ItemsSO>();


    void Start()
    {
        // Ambil list dari PlayerData
    }
    void TakeItemToList()
    {
        
    }
    void Update()
    {
        if (equippedItems == null || equippedItems.Count == 0) return;

        foreach (IAttackItem item in equippedItems)
        {
            // PlayerAttack tidak perlu tahu cooldown-nya berapa.
            // Dia cuma nanya: "Kamu siap?"
            if (item == null) continue;
            
            if (item.isReady)
            {
                item.Use(spawnPoint);
            }
        }
    }

    
}