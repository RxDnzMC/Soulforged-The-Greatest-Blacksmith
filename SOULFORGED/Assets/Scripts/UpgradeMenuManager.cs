using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenuManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] PlayerData playerData;
    [SerializeField] ListItemActive listItemActive;
    
    [Header("UI")]
    [SerializeField] Text itemNameText;
    [SerializeField] Text currentLevelText;
    [SerializeField] Text damageText;
    [SerializeField] Text cooldownText;
    [SerializeField] Button upgradeButton;
    [SerializeField] Text upgradeCostText;
    
    private ItemsSO selectedItem;
    private int upgradeCost = 100;
    
    void Start()
    {
        foreach (var item in listItemActive.activeItems)
        {
            if (item != null) item.playerData = playerData;
        }
        
        if (listItemActive.activeItems.Count > 0)
        {
            SelectItem(listItemActive.activeItems[0]);
        }
    }
    
    public void SelectItem(ItemsSO item)
    {
        selectedItem = item;
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (selectedItem == null) return;
        
        itemNameText.text = selectedItem.itemName;
        currentLevelText.text = $"Level: {selectedItem.CurrentLevel}/{selectedItem.MaxLevel}";
        
        ActiveItemLevelData currentData = selectedItem.GetCurrentLevelData();
        damageText.text = $"Damage: {currentData.damage}";
        cooldownText.text = $"Cooldown: {currentData.cooldown}s";
        
        if (selectedItem.IsMaxLevel)
        {
            upgradeButton.interactable = false;
            upgradeCostText.text = "MAX LEVEL";
        }
        else
        {
            upgradeButton.interactable = true;
            upgradeCostText.text = $"Upgrade: {upgradeCost} Gold";
            
            // Preview next level
            ActiveItemLevelData nextData = selectedItem.GetLevelData(selectedItem.CurrentLevel + 1);
            damageText.text = $"Damage: {currentData.damage} → <color=green>{nextData.damage}</color>";
            cooldownText.text = $"Cooldown: {currentData.cooldown}s → <color=green>{nextData.cooldown}s</color>";
        }
    }
    
    public void UpgradeSelectedItem()
    {
        if (selectedItem == null) return;
        if (selectedItem.IsMaxLevel) return;
        
        if (playerData.Globalgold >= upgradeCost)
        {
            playerData.Globalgold -= upgradeCost;
            
            // ==========================================
            // FIX: Pake SetPermanentLevel langsung
            // ==========================================
            int newLevel = selectedItem.CurrentLevel + 1;
            playerData.SetPermanentLevel(selectedItem, newLevel);
            
            UpdateUI();
            
            // Ambil data level baru
            ActiveItemLevelData newData = selectedItem.GetCurrentLevelData();
            
            Debug.Log($"UPGRADE BERHASIL! {selectedItem.itemName} sekarang level {selectedItem.CurrentLevel}");
            Debug.Log($"  Damage: {newData.damage}");
            Debug.Log($"  Cooldown: {newData.cooldown}");
            Debug.Log($"  Size: {newData.sizeMultiplier}");
            Debug.Log($"  Projectile Count: {newData.projectileCount}");
        }
        else
        {
            Debug.Log("Gold gak cukup!");
        }
    }
}