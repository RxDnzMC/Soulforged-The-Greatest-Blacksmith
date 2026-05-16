using UnityEngine;
using System.Collections.Generic;

public class PlayerAttack : MonoBehaviour
{   
    [SerializeField] PlayerData playerData;
    
    [Header("Spawn Points")]
    public Transform defaultSpawnPoint;
    public Transform groundSpawnPoint;
    
    public List<ItemsSO> equippedActiveItems = new List<ItemsSO>();
    public List<ItemsPassiveSO> equippedPassiveItems = new List<ItemsPassiveSO>();

    void Start()
    {
        foreach (var item in equippedActiveItems)
        {
            if (item != null) item.coroutineRunner = this;
        }
    }

    void Update()
    {
        if (equippedActiveItems == null || equippedActiveItems.Count == 0) return;

        foreach (ItemsSO item in equippedActiveItems)
        {
            if (item == null) continue;
            
            if (item.coroutineRunner == null) item.coroutineRunner = this; // BARU
            
            if (item.isReady)
            {
                Transform spawnPoint = item.itemName.Contains("Catalyst") ? groundSpawnPoint : defaultSpawnPoint;
                item.Use(spawnPoint);
            }
        }
    }
    
    // Dipanggil pas item baru ditambahin
    public void OnItemEquipped(ItemsSO item)
    {
        if (item != null) item.coroutineRunner = this;
    }
}