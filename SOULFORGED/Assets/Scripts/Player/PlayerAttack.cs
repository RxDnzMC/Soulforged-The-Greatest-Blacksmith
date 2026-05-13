using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{   
    [SerializeField] PlayerData playerData;
    public Transform spawnPoint;
    public List<ItemsSO> equippedActiveItems = new List<ItemsSO>();
    public List<ItemsPassiveSO> equippedPassiveItems = new List<ItemsPassiveSO>();



    void Update()
    {
        if (equippedActiveItems == null || equippedActiveItems.Count == 0) return;

        foreach (IAttackItem item in equippedActiveItems)
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