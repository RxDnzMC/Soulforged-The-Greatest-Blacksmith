using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UpgradeSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] UIDocument uiDocument;
    
    // Root element
    private VisualElement upgradePanel;
    private VisualElement choicesContainer;
    
    // Choice elements
    private VisualElement choice1, choice2, choice3;
    private Button button1, button2, button3;
    private Image icon1, icon2, icon3;
    private Label name1, name2, name3;
    private Label type1, type2, type3;
    private Label level1, level2, level3;
    private Label desc1, desc2, desc3;
    
    private List<object> currentChoices;
    
    void Awake()
    {
        // Dapetin root
        var root = uiDocument.rootVisualElement;
        
        upgradePanel = root.Q<VisualElement>("UpgradePanel");
        choicesContainer = root.Q<VisualElement>("ChoicesContainer");
        
        // Setup Choice 1
        choice1 = root.Q<VisualElement>("Choice1");
        icon1 = root.Q<Image>("Icon1");
        name1 = root.Q<Label>("Name1");
        type1 = root.Q<Label>("Type1");
        level1 = root.Q<Label>("Level1");
        desc1 = root.Q<Label>("Desc1");
        button1 = root.Q<Button>("Button1");
        button1.clicked += () => OnChoiceSelected(0);
        
        // Setup Choice 2
        choice2 = root.Q<VisualElement>("Choice2");
        icon2 = root.Q<Image>("Icon2");
        name2 = root.Q<Label>("Name2");
        type2 = root.Q<Label>("Type2");
        level2 = root.Q<Label>("Level2");
        desc2 = root.Q<Label>("Desc2");
        button2 = root.Q<Button>("Button2");
        button2.clicked += () => OnChoiceSelected(1);
        
        // Setup Choice 3
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
        
        // Pilih element berdasarkan index
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
        
        // Isi data
        if (item is ItemsSO activeItem)
        {
            type.text = "[AKTIF]";
            type.style.color = Color.cyan;
            name.text = activeItem.itemName;
            level.text = $"Lv.{activeItem.CurrentLevel}";
            
            // Deskripsi: preview next level
            if (activeItem.IsMaxLevel)
            {
                desc.text = "MAX LEVEL";
            }
            else
            {
                ActiveItemLevelData nextData = activeItem.GetLevelData(activeItem.CurrentLevel + 1);
                desc.text = $"Damage: {nextData.damage}\nCooldown: {nextData.cooldown}s";
            }
            
            // Icon (kalau ada sprite)
            if (activeItem.itemIcon != null)
                icon.sprite = activeItem.itemIcon;
        }
        else if (item is ItemsPassiveSO passiveItem)
        {
            type.text = "[PASIF]";
            type.style.color = Color.green;
            name.text = passiveItem.itemName;
            level.text = $"Lv.{passiveItem.CurrentLevel}";
            
            if (passiveItem.IsMaxLevel)
            {
                desc.text = "MAX LEVEL";
            }
            else
            {
                ItemLevelData nextData = passiveItem.GetLevelData(passiveItem.CurrentLevel + 1);
                desc.text = $"{passiveItem.buffType}\n+{nextData.modifierValue}";
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
    
    /// <summary>
    /// Hide panel (buat manual close)
    /// </summary>
    public void HidePanel()
    {
        upgradePanel.style.display = DisplayStyle.None;
    }
}