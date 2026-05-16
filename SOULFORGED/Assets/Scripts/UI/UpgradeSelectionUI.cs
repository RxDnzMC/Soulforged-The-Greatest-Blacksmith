using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

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
    
    // ✅ SIMPAN DELEGATE REFERENCES
    private System.Action clickAction1;
    private System.Action clickAction2;
    private System.Action clickAction3;
    
    // ✅ CEK UNTUK MENCEGAH DOUBLE REGISTRATION
    private bool isRegistered = false;
    
    // ✅ COOLDOWN UNTUK MENCEGAH MULTIPLE CLICK
    private float lastSelectionTime = -999f;
    private float selectionCooldown = 0.3f;
    private bool isWaitingForResponse = false;
    
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
        
        // Choice 2
        choice2 = root.Q<VisualElement>("Choice2");
        icon2 = root.Q<Image>("Icon2");
        name2 = root.Q<Label>("Name2");
        type2 = root.Q<Label>("Type2");
        level2 = root.Q<Label>("Level2");
        desc2 = root.Q<Label>("Desc2");
        button2 = root.Q<Button>("Button2");
        
        // Choice 3
        choice3 = root.Q<VisualElement>("Choice3");
        icon3 = root.Q<Image>("Icon3");
        name3 = root.Q<Label>("Name3");
        type3 = root.Q<Label>("Type3");
        level3 = root.Q<Label>("Level3");
        desc3 = root.Q<Label>("Desc3");
        button3 = root.Q<Button>("Button3");
        
        // ✅ BUAT DELEGATE REFERENCES
        clickAction1 = () => OnChoiceSelected(0);
        clickAction2 = () => OnChoiceSelected(1);
        clickAction3 = () => OnChoiceSelected(2);
        
        // ✅ REGISTER EVENT SEKALI SAJA DI AWAKE
        RegisterEvents();
        
        // Sembunyikan pas awal
        upgradePanel.style.display = DisplayStyle.None;
    }
    
    void OnDestroy()
    {
        // ✅ UNREGISTER EVENT SAAT OBJECT DESTROY
        UnregisterEvents();
    }
    
    void RegisterEvents()
    {
        if (isRegistered) return;
        
        // ✅ PAKE DELEGATE REFERENCES
        button1.clicked += clickAction1;
        button2.clicked += clickAction2;
        button3.clicked += clickAction3;
        
        isRegistered = true;
        Debug.Log("UpgradeSelectionUI: Events registered");
    }
    
    void UnregisterEvents()
    {
        if (!isRegistered) return;
        
        // ✅ PAKE DELEGATE REFERENCES YANG SAMA
        button1.clicked -= clickAction1;
        button2.clicked -= clickAction2;
        button3.clicked -= clickAction3;
        
        isRegistered = false;
        Debug.Log("UpgradeSelectionUI: Events unregistered");
    }
    
    /// <summary>
    /// Filter item - Hanya item yang bisa di-upgrade (belum MAX LEVEL) yang muncul
    /// </summary>
    private List<object> FilterUpgradeableItems(List<object> items)
    {
        List<object> upgradeable = new List<object>();
        
        foreach (var item in items)
        {
            bool canUpgrade = false;
            
            if (item is ItemsSO activeItem)
            {
                canUpgrade = !activeItem.IsMaxLevel;
            }
            else if (item is ItemsPassiveSO passiveItem)
            {
                canUpgrade = !passiveItem.IsMaxLevel;
            }
            
            if (canUpgrade)
            {
                upgradeable.Add(item);
            }
        }
        
        return upgradeable;
    }
    
    /// <summary>
    /// Buat pilihan EMPTY jika tidak ada item yang bisa di-upgrade
    /// </summary>
    private List<object> GetChoicesWithEmptyFallback(List<object> originalChoices)
    {
        // Filter dulu yang bisa di-upgrade
        List<object> upgradeable = FilterUpgradeableItems(originalChoices);
        
        // Jika masih ada yang bisa di-upgrade, return yang upgradeable
        if (upgradeable.Count > 0)
        {
            return upgradeable;
        }
        
        // Jika TIDAK ADA yang bisa di-upgrade, buat pilihan EMPTY
        List<object> emptyChoices = new List<object>();
        for (int i = 0; i < 3; i++)
        {
            emptyChoices.Add(new EmptyUpgradeChoice());
        }
        
        Debug.LogWarning("Semua item sudah MAX LEVEL! Menampilkan pilihan kosong.");
        return emptyChoices;
    }
    
    /// <summary>
    /// Dipanggil dari GameManager saat level up
    /// </summary>
    public void ShowUpgradeChoices(List<object> choices)
    {
        if (choices == null || choices.Count == 0)
        {
            Debug.LogWarning("Tidak ada pilihan upgrade!");
            // Tampilkan empty choices
            choices = GetChoicesWithEmptyFallback(new List<object>());
            if (choices.Count == 0)
            {
                // Force close jika benar-benar tidak ada
                ForceClosePanel();
                return;
            }
        }
        
        // ✅ FILTER DAN TAMBAHKAN EMPTY FALLBACK
        List<object> finalChoices = GetChoicesWithEmptyFallback(choices);
        
        // ✅ RESET STATE
        currentChoices = finalChoices;
        isWaitingForResponse = false;
        
        upgradePanel.style.display = DisplayStyle.Flex;
        
        // Sembunyikan dulu semua
        choice1.style.display = DisplayStyle.None;
        choice2.style.display = DisplayStyle.None;
        choice3.style.display = DisplayStyle.None;
        
        // Tampilin sesuai jumlah pilihan (maks 3)
        int showCount = Mathf.Min(finalChoices.Count, 3);
        for (int i = 0; i < showCount; i++)
        {
            SetupChoice(i, finalChoices[i]);
        }
        
        // ✅ ENABLE BUTTONS
        EnableButtons(true);
        
        Debug.Log($"Upgrade panel shown with {showCount} choices (filtered from {choices.Count})");
    }
    
    void SetupChoice(int index, object item)
    {
        VisualElement choice = null;
        Image icon = null;
        Label name = null;
        Label type = null;
        Label level = null;
        Label desc = null;
        Button button = null;
        
        switch (index)
        {
            case 0:
                choice = choice1; icon = icon1; name = name1;
                type = type1; level = level1; desc = desc1;
                button = button1;
                break;
            case 1:
                choice = choice2; icon = icon2; name = name2;
                type = type2; level = level2; desc = desc2;
                button = button2;
                break;
            case 2:
                choice = choice3; icon = icon3; name = name3;
                type = type3; level = level3; desc = desc3;
                button = button3;
                break;
        }
        
        if (choice == null) return;
        choice.style.display = DisplayStyle.Flex;
        
        // ==========================================
        // EMPTY CHOICE (Semua item sudah MAX LEVEL)
        // ==========================================
        if (item is EmptyUpgradeChoice)
        {
            type.text = "[INFO]";
            type.style.color = new StyleColor(Color.gray);
            name.text = "Nothing to Upgrade";
            level.text = "";
            desc.text = "All items are already MAX LEVEL!\nContinue fighting without upgrade.";
            button.SetEnabled(true);
            
            // Icon kosong
            icon.sprite = null;
            icon.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0.5f));
        }
        // ==========================================
        // ITEM AKTIF (ItemsSO)
        // ==========================================
        else if (item is ItemsSO activeItem)
        {
            type.text = "[AKTIF]";
            type.style.color = new StyleColor(new Color(0f, 0.78f, 1f));
            name.text = activeItem.itemName;
            level.text = $"Lv.{activeItem.CurrentLevel}/{activeItem.MaxLevel}";
            
            // PASTIKAN TIDAK MAX LEVEL (sudah difilter)
            button.SetEnabled(true);
            
            ActiveItemLevelData currentData = activeItem.GetCurrentLevelData();
            ActiveItemLevelData nextData = activeItem.GetLevelData(activeItem.CurrentLevel + 1);
            
            string descText = "";
            if (!string.IsNullOrEmpty(currentData.levelDescription))
            {
                descText = $"\"{currentData.levelDescription}\"";
            }
            
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
            
            button.SetEnabled(true);
            
            ItemLevelData currentData = passiveItem.GetLevelData(passiveItem.CurrentLevel);
            ItemLevelData nextData = passiveItem.GetLevelData(passiveItem.CurrentLevel + 1);
            
            string buffName = passiveItem.buffType.ToString();
            
            string descText = "";
            if (!string.IsNullOrEmpty(currentData.levelDescription))
            {
                descText = $"\"{currentData.levelDescription}\"";
            }
            
            descText += $"\n\n<color=yellow>Next Lv →</color>";
            
            if (passiveItem.buffType == PassiveBuffType.CooldownReduction)
            {
                descText += $"\n{buffName}: {currentData.modifierValue * 100:F0}% → {nextData.modifierValue * 100:F0}%";
            }
            else if (passiveItem.buffType == PassiveBuffType.MaxHealth || 
                     passiveItem.buffType == PassiveBuffType.Defense)
            {
                descText += $"\n{buffName}: +{currentData.modifierValue:F0} → +{nextData.modifierValue:F0}";
            }
            else
            {
                descText += $"\n{buffName}: +{currentData.modifierValue:F1} → +{nextData.modifierValue:F1}";
            }
            
            desc.text = descText;
            
            if (passiveItem.itemIcon != null)
                icon.sprite = passiveItem.itemIcon;
        }
    }
    
    void EnableButtons(bool enable)
    {
        if (button1 != null) button1.SetEnabled(enable);
        if (button2 != null) button2.SetEnabled(enable);
        if (button3 != null) button3.SetEnabled(enable);
    }
    
    void OnChoiceSelected(int index)
    {
        // ✅ CEK COOLDOWN - MENCEGAH MULTIPLE CLICK
        if (isWaitingForResponse || Time.time < lastSelectionTime + selectionCooldown)
        {
            Debug.Log("Selection ignored: cooldown or already waiting");
            return;
        }
        
        if (currentChoices == null || index >= currentChoices.Count)
        {
            Debug.LogWarning("Invalid selection index");
            return;
        }
        
        object selectedItem = currentChoices[index];
        
        if (selectedItem == null)
        {
            Debug.LogWarning("Selected item is null!");
            return;
        }
        
        // ✅ TANDAI SEDANG MEMPROSES
        isWaitingForResponse = true;
        lastSelectionTime = Time.time;
        
        // ✅ DISABLE BUTTON SEMENTARA
        EnableButtons(false);
        
        // ✅ SEMBUNYIKAN PANEL
        upgradePanel.style.display = DisplayStyle.None;
        
        // ✅ JIKA EMPTY CHOICE, LANGSUNG CLOSE TANPA UPGRADE
        if (selectedItem is EmptyUpgradeChoice)
        {
            Debug.Log("Empty choice selected, closing panel without upgrade");
            isWaitingForResponse = false;
            EnableButtons(true);
            return;
        }
        
        Debug.Log($"Selected choice {index}: {selectedItem}");
        
        // ✅ KIRIM KE GAMEMANAGER
        if (gameManager != null)
        {
            gameManager.OnUpgradeSelected(selectedItem);
        }
        else
        {
            Debug.LogError("GameManager reference is null!");
            isWaitingForResponse = false;
            EnableButtons(true);
        }
    }
    
    /// <summary>
    /// Force close panel jika benar-benar tidak ada pilihan
    /// </summary>
    private void ForceClosePanel()
    {
        Debug.LogWarning("Force closing upgrade panel - no valid choices!");
        upgradePanel.style.display = DisplayStyle.None;
        isWaitingForResponse = false;
        EnableButtons(true);
        
        // Resume game langsung
        Time.timeScale = 1f;
        if (gameManager != null)
        {
            // Panggil method untuk resume game
            gameManager.OnUpgradeSelected(null);
        }
    }
    
    public void OnUpgradeComplete()
    {
        isWaitingForResponse = false;
        currentChoices = null;
        EnableButtons(true);
        Debug.Log("Upgrade complete, UI ready for next selection");
    }
    
    public void HidePanel()
    {
        upgradePanel.style.display = DisplayStyle.None;
        isWaitingForResponse = false;
        EnableButtons(true);
    }
}

/// <summary>
/// Class dummy untuk pilihan kosong ketika semua item sudah MAX LEVEL
/// </summary>
public class EmptyUpgradeChoice
{
    // Empty class sebagai marker
}