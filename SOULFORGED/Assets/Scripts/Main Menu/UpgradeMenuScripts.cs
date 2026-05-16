using UnityEngine;
using UnityEngine.UI;
using TMPro; // Gunakan ini jika teksmu menggunakan TextMeshPro
using System.Collections.Generic;

public class UpgradeMenuScripts : MonoBehaviour
{
    public enum TabType { Actives, Passives }

    [Header("Data References")]
    public PlayerData playerData;
    public List<ItemsSO> weaponUpgrades; // Isi semua senjata di Inspector
    public List<ItemsPassiveSO> passiveUpgrades; // Isi semua item pasif di Inspector

    [Header("UI: Upgrade Slots (8 Kotak)")]
    public Button[] slotButtons; // Masukkan Image - Image (8) ke sini
    public Image[] slotIcons;    // Masukkan komponen Image dari slot ke sini

    [Header("UI: Chosen Item Panel")]
    public TMP_Text itemNameText;
    public TMP_Text itemDescText;
    public TMP_Text itemCostText;
    public Button buyButton;     // Tombol untuk membeli (opsional, tambahkan jika ada)

    [Header("UI: Navigation")]
    public Button weaponsTabBtn;
    public Button passivesTabBtn;
    public Button nextPageBtn;
    public Button prevPageBtn;

    // State Variables
    private TabType currentTab = TabType.Passives;
    private int currentPage = 0;
    private const int ITEMS_PER_PAGE = 8;
    
    // Menyimpan index item yang sedang dipilih
    private int currentlySelectedIndex = -1; 

    void Start()
    {
        // Hubungkan tombol navigasi
        weaponsTabBtn.onClick.AddListener(() => SwitchTab(TabType.Actives));
        passivesTabBtn.onClick.AddListener(() => SwitchTab(TabType.Passives));
        nextPageBtn.onClick.AddListener(NextPage);
        prevPageBtn.onClick.AddListener(PrevPage);

        // Hubungkan ke-8 tombol slot
        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i; // Perlu dicopy ke variabel lokal untuk delegate
            slotButtons[i].onClick.AddListener(() => OnSlotClicked(index));
        }

        // Tampilan awal
        SwitchTab(TabType.Passives);
        ClearChosenItemPanel();
    }

    void SwitchTab(TabType newTab)
    {
        currentTab = newTab;
        currentPage = 0; // Kembali ke halaman 1 setiap ganti tab
        ClearChosenItemPanel();
        RefreshUI();
    }

    void NextPage()
    {
        int maxItems = (currentTab == TabType.Actives) ? weaponUpgrades.Count : passiveUpgrades.Count;
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

    void RefreshUI()
    {
        // Cek kita sedang pakai list yang mana
        int totalItems = (currentTab == TabType.Actives) ? weaponUpgrades.Count : passiveUpgrades.Count;
        int startIndex = currentPage * ITEMS_PER_PAGE;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int itemIndex = startIndex + i;

            // Jika slot ini memiliki item yang sesuai
            if (itemIndex < totalItems)
            {
                slotButtons[i].gameObject.SetActive(true);
                
                // Set Icon (Sesuaikan "icon" dengan nama variabel di ItemsSO milikmu)
                if (currentTab == TabType.Actives)
                    slotIcons[i].sprite = weaponUpgrades[itemIndex].itemIcon; // Ganti .itemIcon sesuai scriptmu
                else
                    slotIcons[i].sprite = passiveUpgrades[itemIndex].itemIcon; // Ganti .itemIcon sesuai scriptmu
            }
            else
            {
                // Kosongkan slot jika tidak ada barang (misal barang cuma 5, sisa 3 slot disembunyikan)
                slotButtons[i].gameObject.SetActive(false);
            }
        }

        // Atur tombol Next/Prev aktif atau tidak
        prevPageBtn.interactable = (currentPage > 0);
        int maxPage = (totalItems - 1) / ITEMS_PER_PAGE;
        nextPageBtn.interactable = (currentPage < maxPage);
    }

    void OnSlotClicked(int slotUIIndex)
    {
        // Hitung index asli barang di dalam List
        int actualItemIndex = (currentPage * ITEMS_PER_PAGE) + slotUIIndex;
        currentlySelectedIndex = actualItemIndex;

        // Tampilkan info ke panel Chosen Item
        if (currentTab == TabType.Actives)
        {
            ItemsSO weapon = weaponUpgrades[actualItemIndex];
            
            // SESUAIKAN VARIABEL DI BAWAH INI DENGAN SCRIPT ItemsSO MILIKMU
            itemNameText.text = weapon.name; // atau weapon.itemName
            itemDescText.text = "Deskripsi Senjata di sini..."; // atau weapon.description
            itemCostText.text = $"Cost: 10 Souls\n50 Gold"; // Ganti dengan sistem hargamu
        }
        else
        {
            ItemsPassiveSO passive = passiveUpgrades[actualItemIndex];
            
            // SESUAIKAN VARIABEL DI BAWAH INI DENGAN SCRIPT ItemsPassiveSO MILIKMU
            itemNameText.text = passive.name;
            itemDescText.text = "Deskripsi Pasif di sini...";
            itemCostText.text = $"Cost: 5 Souls\n20 Gold"; 
        }
    }

    void ClearChosenItemPanel()
    {
        currentlySelectedIndex = -1;
        itemNameText.text = "Select an Upgrade";
        itemDescText.text = "";
        itemCostText.text = "";
    }
}