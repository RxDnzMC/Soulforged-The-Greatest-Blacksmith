    using UnityEngine;
    // INI UNTUK MANAJEMEN SETTINGS SEPERTI TOGGLE DAMAGE NUMBER, DLL. BISA DI-EXTEND KEMUDIAN UNTUK SETTINGS LAINNYA JUGA
    // DI ATTACH DI SCENCE MAIN MENU
    public class SettingsManager : MonoBehaviour
    {
        private const string DAMAGE_NUMBER_KEY = "ShowDamageNumbers";
        
        public static SettingsManager Instance { get; private set; }
        
        public bool ShowDamageNumbers { get; private set; } = true;
        
        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            // DontDestroyOnLoad(gameObject);
            
            // Load settings
            LoadSettings();
        }
        
        void LoadSettings()
        {
            ShowDamageNumbers = PlayerPrefs.GetInt(DAMAGE_NUMBER_KEY, 1) == 1;
            Debug.Log($"Settings Loaded - Show Damage Numbers: {ShowDamageNumbers}");
        }
        
        // Optional: Reload settings dari PlayerPrefs
        public void RefreshSettings()
        {
            LoadSettings();
        }
    }