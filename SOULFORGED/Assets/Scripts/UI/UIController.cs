using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    [SerializeField] private Material hpMaterial; // Tarik file Material-nya ke sini di Inspector
    
    private VisualElement root;
    private VisualElement menuContainer; 
    
    // Elemen baru untuk Health & XP
    private ProgressBar xpBar;
    private VisualElement hpCircle;

    private Button resumeButton;
    private Button restartButton;
    private Button exitButton;
    private Label hpTextLabel; // Label untuk menampilkan teks HP

    private InputSystem_Actions controls;
    private bool isPaused = false;
    private Material hpMaterialInstance; // Instance material untuk manipulasi
    
    void Awake()
    {
        controls = new InputSystem_Actions();
    }

    void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        // --- REFERENSI LAMA (Jangan dihapus) ---
        menuContainer = root.Q<VisualElement>("Pause"); 
        resumeButton = root.Q<Button>("Resume_Button");
        restartButton = root.Q<Button>("Restart_Button");
        exitButton = root.Q<Button>("Exit_Button");

        // --- REFERENSI BARU (HP & XP) ---
        xpBar = root.Q<ProgressBar>("xp-bar");
        hpCircle = root.Q<VisualElement>("HP_BAR");
        hpTextLabel = root.Q<Label>("HP");

        resumeButton.clicked += ResumeGame;
        restartButton.clicked += RestartGame;
        exitButton.clicked += ExitGame;

        controls.Enable();
        controls.Player.Pause.performed += _ => ToggleMenu(); 

        HideMenu();
    }

    void OnDestroy()
{
    // Hapus material buatan tadi biar gak menuh-menuhin RAM
    if (hpMaterialInstance != null)
    {
        Destroy(hpMaterialInstance);
    }
}
    // --- FUNGSI UPDATE BARU ---

    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {

        if (hpCircle != null && hpMaterial != null)
        {
            // Buat instance material sekali saja
            if (hpMaterialInstance == null)
            {
                hpMaterialInstance = new Material(hpMaterial);
                // ✅ Perbaikan: Assign material langsung, bukan lewat backgroundImage
                hpCircle.style.unityMaterial = hpMaterialInstance;
            }
            
            float ratio = currentHealth / maxHealth;
            float shaderValue = 1f - ratio; // 0 = penuh, 1 = kosong
            
            // Update nilai ke shader
            hpMaterialInstance.SetFloat("_RemovedSegment", shaderValue);
        }
    }
    public void UpdateXPUI(float currentXP, float targetXP)
    {
        if (xpBar != null)
        {
            // ProgressBar biasanya range 0-100
            float xpPercentage = (currentXP / targetXP) * 100f;
            xpBar.value = xpPercentage;
            xpBar.title = $"XP: {(int)currentXP} / {(int)targetXP}";
        }
    }

    // --- LOGIKA LAMA (Tetap Aman) ---

    void OnDisable()
    {
        controls.Disable();
    }

    private void ToggleMenu()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    private void PauseGame()
    {
        if (menuContainer == null) Debug.LogError("Gagal nemu elemen Pause!");
        isPaused = true;
        Time.timeScale = 0f;
        ShowMenu();
        MusicManager.Instance.SetPauseEffect(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        HideMenu();
        MusicManager.Instance.SetPauseEffect(false);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        MusicManager.Instance.PlayTrack("Stage 1");
    }

    private void ExitGame()
    {
        Time.timeScale = 1f;
        
        // ✅ Panggil GameOver dulu
        GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.TriggerGameOver(); // Ini nanti yang panggil LoadScene
        }
        else
        {
            // Fallback kalau GameManager gak ada
            Debug.Log("Keluar Game...");
            MusicManager.Instance.PlayTrack("Main Menu");
            MusicManager.Instance.SetPauseEffect(false);
            SceneManager.LoadSceneAsync(0);
        }
    }

    private void ShowMenu()
    {
        menuContainer.style.display = DisplayStyle.Flex;
    }

    private void HideMenu()
    {
        menuContainer.style.display = DisplayStyle.None;
    }

    public void UpdateUITimer(string timeString)
    {
        var timerLabel = root.Q<Label>("Timer");
        if (timerLabel != null)
        {
            timerLabel.text = timeString;
        }
    }
}