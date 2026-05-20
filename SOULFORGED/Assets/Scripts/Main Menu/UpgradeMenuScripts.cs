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
    
    // MENGGUNAKAN SCRIPTABLE OBJECT PASIF SECARA LANGSUNG
    public List<ItemsPassiveSO> passiveUpgrades;

    [Header("UI: Upgrade Slots")]
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
        foreach (var item in weaponUpgrades)
            if (item != null) item.playerData = playerData;

        foreach (var item in passiveUpgrades)
            if (item != null) item.playerData = playerData;
        
        if (weaponsTabBtn != null) weaponsTabBtn.onClick.AddListener(() => SwitchTab(TabType.Actives));
        if (passivesTabBtn != null) passivesTabBtn.onClick.AddListener(() => SwitchTab(TabType.Passives));
        if (nextPageBtn != null) nextPageBtn.onClick.AddListener(NextPage);
        if (prevPageBtn != null) prevPageBtn.onClick.AddListener(PrevPage);
        if (buyButton != null) buyButton.onClick.AddListener(BuyUpgrade);

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
        return (currentTab == TabType.Actives) ? weaponUpgrades.Count : passiveUpgrades.Count;
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
                    slotIcons[i].sprite = weaponUpgrades[itemIndex].itemIcon;
                else
                    slotIcons[i].sprite = passiveUpgrades[itemIndex].itemIcon;
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

        if (currentTab == TabType.Actives)
        {
            if (actualItemIndex >= weaponUpgrades.Count) return;
            ShowActiveItemInfo(weaponUpgrades[actualItemIndex]);
        }
        else
        {
            if (actualItemIndex >= passiveUpgrades.Count) return;
            ShowPassiveItemInfo(passiveUpgrades[actualItemIndex]);
        }
    }

    void ShowActiveItemInfo(ItemsSO weapon)
    {
        itemNameText.text = weapon.itemName;
        int permLevel = weapon.PermanentLevel; 
        
        if (currentLevelText != null)
            currentLevelText.text = $"Lv: {permLevel}/{weapon.MaxPermanentLevel}";
        
        if (weapon.IsMaxPermanentLevel)
        {
            itemDescText.text = "★ MAX PERMANENT UPGRADE ★\n\nSenjata ini sudah mencapai potensi maksimal di luar game.";
            itemCostText.text = "MAXED";
            if (buyButton != null) buyButton.interactable = false;
        }
        else
        {
            float currentBonus = permLevel * weapon.permanentDamageBonusPercent; 
            float nextBonus = (permLevel + 1) * weapon.permanentDamageBonusPercent;
            
            itemDescText.text = "Permanent Weapon Boost\n(Memberikan tambahan Base Damage di dalam game)\n\n";
            itemDescText.text += "<color=yellow>Next Lv →</color>";
            itemDescText.text += $"\nBonus Damage: +{currentBonus}% → +{nextBonus}%";
            
            int goldCost = weapon.permanentGoldBaseCost * (permLevel + 1);
            int soulsCost = weapon.permanentSoulsBaseCost * (permLevel + 1);
            
            string costText = $"Cost: {goldCost} Gold";
            if (soulsCost > 0) costText += $"\n{soulsCost} Souls";
            itemCostText.text = costText;
            
            if (buyButton != null) buyButton.interactable = true;
        }
    }

    // ==========================================
    // SHOW PASSIVE INFO (FORMAT SAMA KAYA WEAPON)
    // ==========================================
    void ShowPassiveItemInfo(ItemsPassiveSO passive)
    {
        itemNameText.text = passive.itemName;
        int permLevel = passive.PermanentLevel; 
        
        if (currentLevelText != null)
            currentLevelText.text = $"Lv: {permLevel}/{passive.MaxPermanentLevel}";
        
        if (passive.IsMaxPermanentLevel)
        {
            itemDescText.text = "★ MAX PERMANENT UPGRADE ★\n\nStat pasif ini sudah mencapai batas maksimal.";
            itemCostText.text = "MAXED";
            if (buyButton != null) buyButton.interactable = false;
        }
        else
        {
            float currentBonus = permLevel * passive.permanentBonusPerLevel; 
            float nextBonus = (permLevel + 1) * passive.permanentBonusPerLevel;
            
            itemDescText.text = $"Permanent Stat Boost\n(Memberikan tambahan {passive.buffType} di awal game)\n\n";
            itemDescText.text += "<color=yellow>Next Lv →</color>";
            
            if (passive.buffType == PassiveBuffType.CooldownReduction || passive.buffType == PassiveBuffType.ProjectileDamage)
                itemDescText.text += $"\nBonus: +{currentBonus}% → +{nextBonus}%";
            else
                itemDescText.text += $"\nBonus: +{currentBonus} → +{nextBonus}";
            
            int goldCost = passive.permanentGoldBaseCost * (permLevel + 1);
            int soulsCost = passive.permanentSoulsBaseCost * (permLevel + 1);
            
            string costText = $"Cost: {goldCost} Gold";
            if (soulsCost > 0) costText += $"\n{soulsCost} Souls";
            itemCostText.text = costText;
            
            if (buyButton != null) buyButton.interactable = true;
        }
    }

    void BuyUpgrade()
    {
        if (currentlySelectedIndex < 0) return;

        if (currentTab == TabType.Actives)
            BuyActiveUpgrade();
        else
            BuyPassiveUpgrade();
        
        UpdateCurrencyUI();
    }

    void BuyActiveUpgrade()
    {
        if (currentlySelectedIndex >= weaponUpgrades.Count) return;
        
        ItemsSO weapon = weaponUpgrades[currentlySelectedIndex];
        if (weapon.IsMaxPermanentLevel) return;

        int permLevel = weapon.PermanentLevel;
        int goldCost = weapon.permanentGoldBaseCost * (permLevel + 1); 
        int soulsCost = weapon.permanentSoulsBaseCost * (permLevel + 1);
        
        if (playerData.Globalgold >= goldCost && playerData.Globalsouls >= soulsCost)
        {
            playerData.Globalgold -= goldCost;
            playerData.Globalsouls -= soulsCost;
            
            playerData.SetPermanentLevel(weapon, permLevel + 1);
            Debug.Log($"UPGRADE PERMANENT: {weapon.itemName} ke Level {permLevel + 1}!");
            
            ShowActiveItemInfo(weapon); 
        }
    }

    void BuyPassiveUpgrade()
    {
        if (currentlySelectedIndex >= passiveUpgrades.Count) return;
        
        ItemsPassiveSO passive = passiveUpgrades[currentlySelectedIndex];
        if (passive.IsMaxPermanentLevel) return;
        
        int permLevel = passive.PermanentLevel;
        int goldCost = passive.permanentGoldBaseCost * (permLevel + 1);
        int soulsCost = passive.permanentSoulsBaseCost * (permLevel + 1);
        
        if (playerData.Globalgold >= goldCost && playerData.Globalsouls >= soulsCost)
        {
            playerData.Globalgold -= goldCost;
            playerData.Globalsouls -= soulsCost;
            
            playerData.SetPermanentPassiveLevel(passive, permLevel + 1);
            playerData.RecalculatePermanentStats(); // <--- OTOMATIS MENGHITUNG STATS
            
            Debug.Log($"UPGRADE PERMANENT: {passive.itemName} ke Level {permLevel + 1}!");
            
            ShowPassiveItemInfo(passive);
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