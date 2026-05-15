using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UpgradeSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] UIDocument uiDocument;
    
    // Root elements
    private VisualElement upgradePanel;
    
    // Choice 1
    private VisualElement choice1;
    private Image icon1;
    private Label name1, type1, level1, desc1;
    private Button button1;
    
    // Choice 2
    private VisualElement choice2;
    private Image icon2;
    private Label name2, type2, level2, desc2;
    private Button button2;
    
    // Choice 3
    private VisualElement choice3;
    private Image icon3;
    private Label name3, type3, level3, desc3;
    private Button button3;
    
    private List<object> currentChoices;
    
    void Awake()
    {
        var root = uiDocument.rootVisualElement;
        
        // Upgrade Panel
        upgradePanel = root.Q<VisualElement>("UpgradePanel");
        
        // Choice 1
        choice1 = root.Q<VisualElement>("Choice1");
        icon1 = root.Q<Image>("Icon1");
        name1 = root.Q<Label>("Name1");
        type1 = root.Q<Label>("Type1");
        level1 = root.Q<Label>("Level1");
        desc1 = root.Q<Label>("Desc1");
        button1 = root.Q<Button>("Button1");
        button1.clicked += () => OnChoiceSelected(0);
        
        // Choice 2
        choice2 = root.Q<VisualElement>("Choice2");
        icon2 = root.Q<Image>("Icon2");
        name2 = root.Q<Label>("Name2");
        type2 = root.Q<Label>("Type2");
        level2 = root.Q<Label>("Level2");
        desc2 = root.Q<Label>("Desc2");
        button2 = root.Q<Button>("Button2");
        button2.clicked += () => OnChoiceSelected(1);
        
        // Choice 3
        choice3 = root.Q<VisualElement>("Choice3");
        icon3 = root.Q<Image>("Icon3");
        name3 = root.Q<Label>("Name3");
        type3 = root.Q<Label>("Type3");
        level3 = root.Q<Label>("Level3");
        desc3 = root.Q<Label>("Desc3");
        button3 = root.Q<Button>("Button3");
        button3.clicked += () => OnChoiceSelected(2);
        
        // Sembunyikan pas awal
        upgradePanel.style.display = DisplayStyle.None;
    }
    
    /// <summary>
    /// Dipanggil dari GameManager saat level up
    /// </summary>
    public void ShowUpgradeChoices(List<object> choices)
    {
        currentChoices = choices;
        upgradePanel.style.display = DisplayStyle.Flex;
        
        // Sembunyikan dulu semua
        choice1.style.display = DisplayStyle.None;
        choice2.style.display = DisplayStyle.None;
        choice3.style.display = DisplayStyle.None;
        
        // Tampilin sesuai jumlah pilihan
        for (int i = 0; i < choices.Count; i++)
        {
            SetupChoice(i, choices[i]);
        }
    }
    
    void SetupChoice(int index, object item)
    {
        VisualElement choice = null;
        Image icon = null;
        Label name = null;
        Label type = null;
        Label level = null;
        Label desc = null;
        
        switch (index)
        {
            case 0:
                choice = choice1; icon = icon1; name = name1;
                type = type1; level = level1; desc = desc1;
                break;
            case 1:
                choice = choice2; icon = icon2; name = name2;
                type = type2; level = level2; desc = desc2;
                break;
            case 2:
                choice = choice3; icon = icon3; name = name3;
                type = type3; level = level3; desc = desc3;
                break;
        }
        
        if (choice == null) return;
        choice.style.display = DisplayStyle.Flex;
        
        // ==========================================
        // ITEM AKTIF (ItemsSO)
        // ==========================================
        if (item is ItemsSO activeItem)
        {
            type.text = "[AKTIF]";
            type.style.color = new StyleColor(new Color(0f, 0.78f, 1f));
            name.text = activeItem.itemName;
            level.text = $"Lv.{activeItem.CurrentLevel}/{activeItem.MaxLevel}";
            
            if (activeItem.IsMaxLevel)
            {
                desc.text = "★ MAX LEVEL ★";
            }
            else
            {
                ActiveItemLevelData currentData = activeItem.GetCurrentLevelData();
                ActiveItemLevelData nextData = activeItem.GetLevelData(activeItem.CurrentLevel + 1);
                
                // Deskripsi sekarang
                string descText = "";
                if (!string.IsNullOrEmpty(currentData.levelDescription))
                {
                    descText = $"\"{currentData.levelDescription}\"";
                }
                
                // Preview next level (cuma yang berubah)
                descText += "\n\n<color=yellow>Next Lv →</color>";
                
                if (nextData.damage != currentData.damage)
                    descText += $"\nDamage: {currentData.damage} → {nextData.damage}";
                
                if (nextData.cooldown != currentData.cooldown)
                    descText += $"\nCooldown: {currentData.cooldown}s → {nextData.cooldown}s";
                
                if (nextData.speed != currentData.speed)
                    descText += $"\nSpeed: {currentData.speed} → {nextData.speed}";
                
                if (nextData.lifetime != currentData.lifetime)
                    descText += $"\nLifetime: {currentData.lifetime}s → {nextData.lifetime}s";
                
                if (nextData.sizeMultiplier != currentData.sizeMultiplier)
                    descText += $"\nSize: x{currentData.sizeMultiplier} → x{nextData.sizeMultiplier}";
                
                if (nextData.projectileCount != currentData.projectileCount)
                    descText += $"\nProjectile: {currentData.projectileCount} → {nextData.projectileCount}";
                
                desc.text = descText;
            }
            
            if (activeItem.itemIcon != null)
                icon.sprite = activeItem.itemIcon;
        }
        // ==========================================
        // ITEM PASIF (ItemsPassiveSO)
        // ==========================================
        else if (item is ItemsPassiveSO passiveItem)
        {
            type.text = "[PASIF]";
            type.style.color = new StyleColor(new Color(0f, 1f, 0.39f));
            name.text = passiveItem.itemName;
            level.text = $"Lv.{passiveItem.CurrentLevel}/{passiveItem.MaxLevel}";
            
            if (passiveItem.IsMaxLevel)
            {
                desc.text = "★ MAX LEVEL ★";
            }
            else
            {
                ItemLevelData currentData = passiveItem.GetLevelData(passiveItem.CurrentLevel);
                ItemLevelData nextData = passiveItem.GetLevelData(passiveItem.CurrentLevel + 1);
                
                string buffName = passiveItem.buffType.ToString();
                
                // Deskripsi sekarang
                string descText = "";
                if (!string.IsNullOrEmpty(currentData.levelDescription))
                {
                    descText = $"\"{currentData.levelDescription}\"";
                }
                
                // Preview next level
                descText += $"\n\n<color=yellow>Next Lv →</color>";
                descText += $"\n{buffName}: +{currentData.modifierValue} → +{nextData.modifierValue}";
                
                desc.text = descText;
            }
            
            if (passiveItem.itemIcon != null)
                icon.sprite = passiveItem.itemIcon;
        }
    }
    
    void OnChoiceSelected(int index)
    {
        if (currentChoices == null || index >= currentChoices.Count) return;
        
        // Sembunyikan panel
        upgradePanel.style.display = DisplayStyle.None;
        
        // Kirim ke GameManager
        gameManager.OnUpgradeSelected(currentChoices[index]);
    }
    
    public void HidePanel()
    {
        upgradePanel.style.display = DisplayStyle.None;
    }
}