using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float health;
    public float maxHealth;
    public float defense;
    public Vector3 playerPosition;
    public int level;
    public float exp;
    public float expToNextLevel;
    public float expMultiplier;
    public float projectileDamageMultiplier;
    public float projectileSpeedMultiplier;
    public float projectileLifetimeMultiplier;
    public float cooldownReductionMultiplier;
    public float projectileCountMultiplier;
    public int Globalgold;
    public int Globalsouls;

    public ItemsSO DefaultItem;
    [Header("Default Slot untuk Item Aktif (Jangan DIISI)")]
    public List<ItemsSO> DefaultSlot = new List<ItemsSO>(4); // Asumsi ada 4 slot untuk item aktif
    public List<ItemsPassiveSO> DefaultPassiveSlot = new List<ItemsPassiveSO>(4); // Asumsi ada 4 slot untuk item pasif

    [Header("List Item Aktif dan Pasif")]
    public List<ItemsSO> ActiveItems = new List<ItemsSO>();
    public List<ItemsPassiveSO> PassivesItems = new List<ItemsPassiveSO>();

    public void ResetData()
    {
        ActiveItems.Clear(); // Hapus semua item
        PassivesItems.Clear();
        ResetInventory();
    }

    public void ResetInventory()
    {
        ActiveItems = new List<ItemsSO>(DefaultSlot); // Hapus semua item
        PassivesItems = new List<ItemsPassiveSO>(DefaultPassiveSlot);
    }
}
