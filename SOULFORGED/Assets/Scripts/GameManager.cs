using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] PlayerData playerData;
    [SerializeField] ListItemActive listItemActive;
    [SerializeField] PlayerAttack playerAttack;
    private List<ItemsSO> ItemList = new List<ItemsSO>();
    private List<ItemsSO> CurrentItem = new List<ItemsSO>();
    bool isPlayerDead = false;
    bool isItem1Added = false;
    void Start()
    {
        ItemList = listItemActive.activeItems; // DIPAKAI BUAT GACHA DI GAME MANAGER INI, INI LIST ITEM YANG UDH DIBUAT
        
        playerData.health = 1000;
        playerData.exp = 0;
        playerData.level = 1;
        playerData.expToNextLevel = 100;
        defaultItem();
    }

    // void ZeroHealth() {
    //     if (playerData.health <= 0) {
    //         Debug.Log("Player mati jir");
    //         isPlayerDead = true;
    //     }
    //     else {
    //        playerData.health -= 1;
    //     }
    // }
    // Update is called once per frame
    void defaultItem() {
        if (playerData.level == 1) {
            CurrentItem =  new List<ItemsSO>(playerData.DefaultSlot); // Ambil list dari PlayerData
            CurrentItem[0] = playerData.DefaultItem; // Set item ke slot 0
            playerData.ActiveItems = CurrentItem; // Simpan kembali ke PlayerData
            playerAttack.equippedItems = playerData.ActiveItems; // Set juga ke PlayerAttack
            Debug.Log("Item 0 ditambahkan ke Active Items!");
        }
    }
    void Update()
    {
        // if (isPlayerDead == false) ZeroHealth(); return;
        if (playerData.exp >= playerData.expToNextLevel) {
            playerData.level += 1; // Naikkan level
            playerData.exp -= playerData.expToNextLevel; // Reset ke sisa exp setelah naik level
            playerData.expToNextLevel *= 1.3f; // Next Level Requirement (naik 30% setiap level)
            Debug.Log($"Level Up! Sekarang level {playerData.level}");
        }

        if (playerData.level == 2 && !isItem1Added) {
            CurrentItem = playerData.ActiveItems; // Ambil list dari PlayerData
            CurrentItem[1] = ItemList[1]; // Set item ke slot 1
            playerData.ActiveItems = CurrentItem; // Simpan kembali ke PlayerData
            Debug.Log("Item 1 ditambahkan ke Active Items!");
            isItem1Added = true;
        }
        
    }

    void OnDestroy()
    {
        playerData.ResetData(); // Reset semua data ketiga meninggalkan game
    }
}
