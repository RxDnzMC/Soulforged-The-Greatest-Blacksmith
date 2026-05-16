using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class GameOverPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] GameManager gameManager;
    [SerializeField] UIDocument uiDocument;
    [SerializeField] PlayerData playerData;
    [SerializeField] Sprite goldIcon;
    [SerializeField] Sprite soulIcon;
    
    private VisualElement gameOverPanel;
    private Label gameOverTimer;
    private Label gameOverGold;
    private Label gameOverSouls;
    private Label gameOverTotalGold;
    private Label gameOverTotalSouls;
    private Button exitGameButton;
    
    private int earnedGold;
    private int earnedSouls;
    
    void Awake()
    {
        var root = uiDocument.rootVisualElement;
        
        gameOverPanel = root.Q<VisualElement>("GameOverPanel");
        gameOverTimer = root.Q<Label>("GameOverTimer");
        gameOverGold = root.Q<Label>("GameOverGold");
        gameOverSouls = root.Q<Label>("GameOverSouls");
        gameOverTotalGold = root.Q<Label>("GameOverTotalGold");
        gameOverTotalSouls = root.Q<Label>("GameOverTotalSouls");
        exitGameButton = root.Q<Button>("ExitGameButton");
        
        var goldIconElement = root.Q<Image>("GoldIcon");
        var soulIconElement = root.Q<Image>("SoulIcon");
        if (goldIconElement != null && goldIcon != null) goldIconElement.sprite = goldIcon;
        if (soulIconElement != null && soulIcon != null) soulIconElement.sprite = soulIcon;
        
        if (exitGameButton != null)
            exitGameButton.clicked += OnExitGame;
        
        gameOverPanel.style.display = DisplayStyle.None;
    }
    
    public void ShowGameOver()
    {
        earnedGold = playerData.gold;
        earnedSouls = playerData.souls;
        
        // ✅ LANGSUNG SAVE KE PLAYERDATA (ScriptableObject otomatis ke-save)
        playerData.Globalgold += earnedGold;
        playerData.Globalsouls += earnedSouls;
        
        playerData.gold = 0;
        playerData.souls = 0;
        
        // ✅ Tandain PlayerData sebagai dirty biar ke-save
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(playerData);
        #endif
        
        gameOverTimer.text = $"Waktu: {gameManager.timerString}";
        gameOverGold.text = $"Gold: {earnedGold}";
        gameOverSouls.text = $"Souls: {earnedSouls}";
        gameOverTotalGold.text = $"Gold: {playerData.Globalgold}";
        gameOverTotalSouls.text = $"Souls: {playerData.Globalsouls}";
        
        gameOverPanel.style.display = DisplayStyle.Flex;
        Time.timeScale = 0f;
    }
    
    void OnExitGame()
    {
        Time.timeScale = 1f;
        
        MusicManager.Instance?.PlayTrack("Main Menu");
        MusicManager.Instance?.SetPauseEffect(false);
        
        SceneManager.LoadSceneAsync(0);
    }
    
    public void HidePanel()
    {
        gameOverPanel.style.display = DisplayStyle.None;
    }
}