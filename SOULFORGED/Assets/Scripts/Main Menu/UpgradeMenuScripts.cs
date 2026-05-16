using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UpgradeMenuScripts : MonoBehaviour
{
    public enum TabType { Actives, Passives }

    [Header("Data References")]
    public PlayerData playerData;
    public List<ItemsSO> weaponUpgrades;
    
    // ==========================================
    // STAT UPGRADE (PASIF)
    // ==========================================
    [System.Serializable]
    public class StatUpgrade
    {
        public string statName;
        public Sprite icon;
        public string description;
        public int maxLevel = 10;
        public float increasePerLevel;
        
        [Header("Cost Per Level (Isi berurutan dari Level 1 ke atas)")]
        public int[] goldCostPerLevel;   // Index 0 = biaya ke Level 1, dst
        public int[] soulsCostPerLevel;
    }
    
    public List<StatUpgrade> statUpgrades = new List<StatUpgrade>();
    // ==========================================

    [Header("UI: Upgrade Slots (8 Kotak)")]
    public Button[] slotButtons;
    public Image[] slotIcons;

    [Header("UI: Chosen Item Panel")]
    public TMP_Text itemNameText;
    public TMP_Text itemDescText;
    public TMP_Text itemCostText;
    public TMP_Text currentLevelText;
    public Button buyButton;

    [Header("UI: Currency Display")]
    public TMP_Text goldText;
    public TMP_Text soulsText;

    [Header("UI: Navigation")]
    public Button weaponsTabBtn;
    public Button passivesTabBtn;
    public Button nextPageBtn;
    public Button prevPageBtn;

    private TabType currentTab = TabType.Actives;
    private int currentPage = 0;
    private const int ITEMS_PER_PAGE = 8;
    private int currentlySelectedIndex = -1;

    void Start()
    {
        // Setup playerData reference
        foreach (var item in weaponUpgrades)
        {
            if (item != null) item.playerData = playerData;
        }
        
        // Hubungkan navigasi
        if (weaponsTabBtn != null) weaponsTabBtn.onClick.AddListener(() => SwitchTab(TabType.Actives));
        if (passivesTabBtn != null) passivesTabBtn.onClick.AddListener(() => SwitchTab(TabType.Passives));
        if (nextPageBtn != null) nextPageBtn.onClick.AddListener(NextPage);
        if (prevPageBtn != null) prevPageBtn.onClick.AddListener(PrevPage);
        if (buyButton != null) buyButton.onClick.AddListener(BuyUpgrade);

        // Hubungkan slot buttons
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] != null)
            {
                int index = i;
                slotButtons[i].onClick.RemoveAllListeners();
                slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
            }
        }

        SwitchTab(TabType.Actives);
        ClearChosenItemPanel();
        UpdateCurrencyUI();
    }

    void Update()
    {
        UpdateCurrencyUI();
    }

    void UpdateCurrencyUI()
    {
        if (goldText != null) goldText.text = $"Gold: {playerData.Globalgold}";
        if (soulsText != null) soulsText.text = $"Souls: {playerData.Globalsouls}";
    }

    void SwitchTab(TabType newTab)
    {
        currentTab = newTab;
        currentPage = 0;
        ClearChosenItemPanel();
        RefreshUI();
    }

    void NextPage()
    {
        int maxItems = GetTotalItems();
        int maxPage = (maxItems - 1) / ITEMS_PER_PAGE;

        if (currentPage < maxPage)
        {
            currentPage++;
            ClearChosenItemPanel();
            RefreshUI();
        }
    }

    void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ClearChosenItemPanel();
            RefreshUI();
        }
    }

    int GetTotalItems()
    {
        return (currentTab == TabType.Actives) ? weaponUpgrades.Count : statUpgrades.Count;
    }

    void RefreshUI()
    {
        int totalItems = GetTotalItems();
        int startIndex = currentPage * ITEMS_PER_PAGE;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int itemIndex = startIndex + i;

            if (itemIndex < totalItems)
            {
                slotButtons[i].gameObject.SetActive(true);
                
                if (currentTab == TabType.Actives)
                {
                    slotIcons[i].sprite = weaponUpgrades[itemIndex].itemIcon;
                }
                else
                {
                    slotIcons[i].sprite = statUpgrades[itemIndex].icon;
                }
            }
            else
            {
                slotButtons[i].gameObject.SetActive(false);
            }
        }

        if (prevPageBtn != null) prevPageBtn.interactable = (currentPage > 0);
        int maxPage = (totalItems - 1) / ITEMS_PER_PAGE;
        if (nextPageBtn != null) nextPageBtn.interactable = (currentPage < maxPage);
    }

    void OnSlotClicked(int slotUIIndex)
    {
        int actualItemIndex = (currentPage * ITEMS_PER_PAGE) + slotUIIndex;
        currentlySelectedIndex = actualItemIndex;
        
        Debug.Log($"Slot {slotUIIndex} diklik! Index: {actualItemIndex}, Tab: {currentTab}");

        if (currentTab == TabType.Actives)
        {
            if (actualItemIndex >= weaponUpgrades.Count) return;
            ShowActiveItemInfo(weaponUpgrades[actualItemIndex]);
        }
        else
        {
            if (actualItemIndex >= statUpgrades.Count) return;
            ShowStatInfo(statUpgrades[actualItemIndex]);
        }
    }

    // ==========================================
    // TAMPILIN INFO ITEM AKTIF
    // ==========================================
    void ShowActiveItemInfo(ItemsSO weapon)
    {
        itemNameText.text = weapon.itemName;
        
        int currentLevel = weapon.CurrentLevel;
        if (currentLevelText != null)
            currentLevelText.text = $"Level: {currentLevel}/{weapon.MaxLevel}";
        
        if (weapon.IsMaxLevel)
        {
            itemDescText.text = "★ MAX LEVEL ★";
            itemCostText.text = "MAXED";
            if (buyButton != null) buyButton.interactable = false;
        }
        else
        {
            ActiveItemLevelData currentData = weapon.GetCurrentLevelData();
            ActiveItemLevelData nextData = weapon.GetLevelData(currentLevel + 1);
            
            itemDescText.text = $"\"{currentData.levelDescription}\"\n\n";
            itemDescText.text += "<color=yellow>Next Lv →</color>";
            
            if (nextData.damage != currentData.damage)
                itemDescText.text += $"\nDamage: {currentData.damage} → {nextData.damage}";
            
            if (nextData.cooldown != currentData.cooldown)
                itemDescText.text += $"\nCooldown: {currentData.cooldown}s → {nextData.cooldown}s";
            
            if (nextData.projectileCount != currentData.projectileCount)
                itemDescText.text += $"\nProjectile: {currentData.projectileCount} → {nextData.projectileCount}";
            
            if (nextData.sizeMultiplier != currentData.sizeMultiplier)
                itemDescText.text += $"\nSize: x{currentData.sizeMultiplier} → x{nextData.sizeMultiplier}";
            
            // HARGA DARI LEVEL DATA
            int goldCost = nextData.goldCost;
            int soulsCost = nextData.soulsCost;
            
            string costText = $"Cost: {goldCost} Gold";
            if (soulsCost > 0) costText += $"\n{soulsCost} Souls";
            itemCostText.text = costText;
            
            if (buyButton != null) buyButton.interactable = true;
        }
    }

    // ==========================================
    // TAMPILIN INFO STAT (PASIF)
    // ==========================================
    void ShowStatInfo(StatUpgrade stat)
    {
        itemNameText.text = stat.statName;
        
        int currentLevel = GetStatLevel(stat.statName);
        int maxLevel = stat.maxLevel;
        
        if (currentLevelText != null)
            currentLevelText.text = $"Level: {currentLevel}/{maxLevel}";
        
        if (currentLevel >= maxLevel)
        {
            itemDescText.text = "★ MAX LEVEL ★";
            itemCostText.text = "MAXED";
            if (buyButton != null) buyButton.interactable = false;
        }
        else
        {
            float currentValue = GetCurrentStatValue(stat.statName);
            float nextValue = currentValue + stat.increasePerLevel;
            
            itemDescText.text = $"\"{stat.description}\"\n\n";
            itemDescText.text += $"<color=yellow>Next Lv →</color>\n";
            itemDescText.text += $"{stat.statName}: {currentValue} → {nextValue}";
            
            // AMBIL HARGA DARI ARRAY (index = currentLevel - 1 untuk biaya ke next level)
            int nextLevelIndex = currentLevel; // Karena array index 0 = biaya ke level 1
            int goldCost = 0;
            int soulsCost = 0;
            
            if (stat.goldCostPerLevel != null && nextLevelIndex < stat.goldCostPerLevel.Length)
                goldCost = stat.goldCostPerLevel[nextLevelIndex];
            
            if (stat.soulsCostPerLevel != null && nextLevelIndex < stat.soulsCostPerLevel.Length)
                soulsCost = stat.soulsCostPerLevel[nextLevelIndex];
            
            string costText = $"Cost: {goldCost} Gold";
            if (soulsCost > 0) costText += $"\n{soulsCost} Souls";
            itemCostText.text = costText;
            
            if (buyButton != null) buyButton.interactable = true;
        }
    }

    // ==========================================
    // BUY UPGRADE
    // ==========================================
    void BuyUpgrade()
    {
        if (currentlySelectedIndex < 0) return;

        if (currentTab == TabType.Actives)
            BuyActiveUpgrade();
        else
            BuyStatUpgrade();
        
        UpdateCurrencyUI();
    }

    void BuyActiveUpgrade()
    {
        if (currentlySelectedIndex >= weaponUpgrades.Count) return;
        
        ItemsSO weapon = weaponUpgrades[currentlySelectedIndex];
        
        if (weapon.IsMaxLevel)
        {
            Debug.Log("Udah max level!");
            return;
        }
        
        // Ambil harga dari next level
        ActiveItemLevelData nextData = weapon.GetLevelData(weapon.CurrentLevel + 1);
        int goldCost = nextData.goldCost;
        int soulsCost = nextData.soulsCost;
        
        if (playerData.Globalgold >= goldCost && playerData.Globalsouls >= soulsCost)
        {
            playerData.Globalgold -= goldCost;
            playerData.Globalsouls -= soulsCost;
            
            int newLevel = weapon.CurrentLevel + 1;
            playerData.SetPermanentLevel(weapon, newLevel);
            
            Debug.Log($"UPGRADE: {weapon.itemName} ke Level {newLevel}!");
            
            // Refresh panel
            ShowActiveItemInfo(weapon);
        }
        else
        {
            Debug.Log("Gold/Souls gak cukup!");
        }
    }

    void BuyStatUpgrade()
    {
        if (currentlySelectedIndex >= statUpgrades.Count) return;
        
        StatUpgrade stat = statUpgrades[currentlySelectedIndex];
        int currentLevel = GetStatLevel(stat.statName);
        
        if (currentLevel >= stat.maxLevel)
        {
            Debug.Log("Udah max level!");
            return;
        }
        
        // Ambil harga dari array
        int nextLevelIndex = currentLevel;
        int goldCost = 0;
        int soulsCost = 0;
        
        if (stat.goldCostPerLevel != null && nextLevelIndex < stat.goldCostPerLevel.Length)
            goldCost = stat.goldCostPerLevel[nextLevelIndex];
        
        if (stat.soulsCostPerLevel != null && nextLevelIndex < stat.soulsCostPerLevel.Length)
            soulsCost = stat.soulsCostPerLevel[nextLevelIndex];
        
        if (playerData.Globalgold >= goldCost && playerData.Globalsouls >= soulsCost)
        {
            playerData.Globalgold -= goldCost;
            playerData.Globalsouls -= soulsCost;
            
            // Naikin level stat
            SetStatLevel(stat.statName, currentLevel + 1);
            
            // Apply ke PlayerData
            ApplyStatUpgrade(stat);
            
            Debug.Log($"UPGRADE STAT: {stat.statName} ke Level {currentLevel + 1}!");
            
            // Refresh panel
            ShowStatInfo(stat);
        }
        else
        {
            Debug.Log("Gold/Souls gak cukup!");
        }
    }

    // ==========================================
    // FUNGSI STAT LEVEL
    // ==========================================
    
    int GetStatLevel(string statName)
    {
        switch (statName)
        {
            case "Max Health": return playerData.maxHealthLevel;
            case "Defense": return playerData.defenseLevel;
            case "Move Speed": return playerData.moveSpeedLevel;
            case "Projectile Damage": return playerData.projectileDamageLevel;
            case "Cooldown Reduction": return playerData.cooldownReductionLevel;
            default: return 1;
        }
    }

    void SetStatLevel(string statName, int level)
    {
        switch (statName)
        {
            case "Max Health": playerData.maxHealthLevel = level; break;
            case "Defense": playerData.defenseLevel = level; break;
            case "Move Speed": playerData.moveSpeedLevel = level; break;
            case "Projectile Damage": playerData.projectileDamageLevel = level; break;
            case "Cooldown Reduction": playerData.cooldownReductionLevel = level; break;
        }
    }
    
    float GetCurrentStatValue(string statName)
    {
        int level = GetStatLevel(statName);
        
        switch (statName)
        {
            case "Max Health": return playerData.maxHealth;
            case "Defense": return playerData.defense;
            case "Move Speed": return PlayerPrefs.GetFloat("MoveSpeed", 5f);
            case "Projectile Damage": return playerData.projectileDamageMultiplier;
            case "Cooldown Reduction": return playerData.cooldownReductionMultiplier;
            default: return 0;
        }
    }
    
    void ApplyStatUpgrade(StatUpgrade stat)
    {
        switch (stat.statName)
        {
            case "Max Health":
                playerData.maxHealth += stat.increasePerLevel;
                playerData.health += stat.increasePerLevel;
                break;
                
            case "Defense":
                playerData.defense += stat.increasePerLevel;
                break;
                
            case "Move Speed":
                float currentSpeed = PlayerPrefs.GetFloat("MoveSpeed", 5f);
                PlayerPrefs.SetFloat("MoveSpeed", currentSpeed + stat.increasePerLevel);
                break;
                
            case "Projectile Damage":
                playerData.projectileDamageMultiplier += stat.increasePerLevel;
                break;
                
            case "Cooldown Reduction":
                playerData.cooldownReductionMultiplier += stat.increasePerLevel;
                break;
        }
    }

    void ClearChosenItemPanel()
    {
        currentlySelectedIndex = -1;
        if (itemNameText != null) itemNameText.text = "Select an Upgrade";
        if (itemDescText != null) itemDescText.text = "";
        if (itemCostText != null) itemCostText.text = "";
        if (currentLevelText != null) currentLevelText.text = "";
        if (buyButton != null) buyButton.interactable = false;
    }
}