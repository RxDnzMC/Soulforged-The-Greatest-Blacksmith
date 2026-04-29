using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float health;
    public Vector3 playerPosition;
    public int level;
    public float exp;
    public float expToNextLevel;
    public ItemsSO DefaultItem;
    [Header("Default Slot untuk Item Aktif (Jangan DIISI)")]
    public List<ItemsSO> DefaultSlot = new List<ItemsSO>(4); // Asumsi ada 4 slot untuk item aktif

    [Header("List Item Aktif dan Pasif")]
    public List<ItemsSO> ActiveItems = new List<ItemsSO>();
    public List<ItemsSO> PassivesItems = new List<ItemsSO>();

    public void ResetData()
    {
        health = 1000;
        playerPosition = Vector3.zero;
        level = 1;
        exp = 0;
        expToNextLevel = 100;
        ActiveItems.Clear(); // Hapus semua item
        PassivesItems.Clear();
        ResetInventory();
    }

    public void ResetInventory()
    {
        ActiveItems = new List<ItemsSO>(DefaultSlot); // Hapus semua item
        PassivesItems = new List<ItemsSO>(DefaultSlot);
    }
}
