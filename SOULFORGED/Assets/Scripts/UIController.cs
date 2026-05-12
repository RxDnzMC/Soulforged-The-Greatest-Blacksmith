using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement; // Untuk Restart

public class UIController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    
    private VisualElement root;
    private VisualElement menuContainer; // Wadah utama UI kamu (misal: Panel Overlay)
    
    private Button resumeButton;
    private Button restartButton;
    private Button exitButton;

    private InputSystem_Actions controls;
    private bool isPaused = false;

    void Awake()
    {
        // Inisialisasi Input System
        controls = new InputSystem_Actions();
    }

    void OnEnable()
    {
        // 1. Ambil Root Visual Element
        root = uiDocument.rootVisualElement;

        // 2. Query elemen berdasarkan NAMA yang kamu buat di UI Builder
        // Pastikan nama di tanda kutip ("") SAMA PERSIS dengan di UI Builder
        menuContainer = root.Q<VisualElement>("Pause"); 
        resumeButton = root.Q<Button>("Resume_Button");
        restartButton = root.Q<Button>("Restart_Button");
        exitButton = root.Q<Button>("Exit_Button");

        // 3. Pasang Event (Klik)
        resumeButton.clicked += ResumeGame;
        restartButton.clicked += RestartGame;
        exitButton.clicked += ExitGame;

        // 4. Aktifkan Input
        controls.Enable();
        // Ganti "Pause" dengan nama Action yang kamu buat untuk tombol Esc/Start
        controls.Player.Pause.performed += _ => ToggleMenu(); 

        // Sembunyikan menu di awal
        HideMenu();
        
    }

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
        isPaused = true;
        Time.timeScale = 0f;
        ShowMenu();

        // Panggil efek musik
        MusicManager.Instance.SetPauseEffect(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        HideMenu();

        // Matikan efek musik
        MusicManager.Instance.SetPauseEffect(false);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        // Load ulang scene yang sedang aktif
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        MusicManager.Instance.PlayTrack("Stage 1");
    }

    private void ExitGame()
    {
        Time.timeScale = 1f;
        Debug.Log("Keluar Game...");
        MusicManager.Instance.PlayTrack("Main Menu");
        MusicManager.Instance.SetPauseEffect(false);
        SceneManager.LoadSceneAsync(0);
        
    }

    private void ShowMenu()
    {
        // Menggunakan display style (mirip CSS)
        menuContainer.style.display = DisplayStyle.Flex;
    }

    private void HideMenu()
    {
        menuContainer.style.display = DisplayStyle.None;
    }

    public void UpdateUITimer(string timeString)
    {
        // Update teks timer di UI
        var timerLabel = root.Q<Label>("Timer");
        if (timerLabel != null)
        {
            timerLabel.text = timeString;
        }
    }
}