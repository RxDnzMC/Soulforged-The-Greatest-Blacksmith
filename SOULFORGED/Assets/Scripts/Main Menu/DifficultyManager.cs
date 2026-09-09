using UnityEngine;

public enum DifficultyLevel
{
    Normal,
    Hard,
    Expert,
    Insane
}

[System.Serializable]
public class DifficultySettings
{
    public string difficultyName;
    public float goldMultiplier = 1f;
    public float soulMultiplier = 1f;
    public float monsterHealthMultiplier = 1f;
    public float monsterDamageMultiplier = 1f;
    public Color buttonColor = Color.white;
}

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }
    
    private const string DIFFICULTY_KEY = "SelectedDifficulty";
    
    [Header("Difficulty Settings")]
    [SerializeField] private DifficultySettings[] difficulties = new DifficultySettings[]
    {
        new DifficultySettings 
        { 
            difficultyName = "Normal", 
            goldMultiplier = 1f, 
            soulMultiplier = 1f, 
            monsterHealthMultiplier = 1f, 
            monsterDamageMultiplier = 1f,
        },
        new DifficultySettings 
        { 
            difficultyName = "Hard", 
            goldMultiplier = 2f, 
            soulMultiplier = 2f, 
            monsterHealthMultiplier = 2f, 
            monsterDamageMultiplier = 1.2f,
        },
        new DifficultySettings 
        { 
            difficultyName = "Expert", 
            goldMultiplier = 5f, 
            soulMultiplier = 5f, 
            monsterHealthMultiplier = 5f, 
            monsterDamageMultiplier = 1.5f,
        },
        new DifficultySettings 
        { 
            difficultyName = "Insane", 
            goldMultiplier = 10f, 
            soulMultiplier = 10f, 
            monsterHealthMultiplier = 10f, 
            monsterDamageMultiplier = 2.5f,
        }
    };
    
    private DifficultyLevel selectedDifficulty = DifficultyLevel.Normal;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadDifficulty();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SelectDifficulty(int difficultyIndex)
    {
        if (difficultyIndex < 0 || difficultyIndex >= difficulties.Length) return;
        
        selectedDifficulty = (DifficultyLevel)difficultyIndex;
        SaveDifficulty();
        Debug.Log($"Difficulty Selected: {difficulties[difficultyIndex].difficultyName}");
    }
    
    public DifficultySettings GetCurrentDifficulty()
    {
        return difficulties[(int)selectedDifficulty];
    }
    
    public string GetDifficultyName()
    {
        return difficulties[(int)selectedDifficulty].difficultyName;
    }
    
    void SaveDifficulty()
    {
        PlayerPrefs.SetInt(DIFFICULTY_KEY, (int)selectedDifficulty);
        PlayerPrefs.Save();
    }
    
    void LoadDifficulty()
    {
        selectedDifficulty = (DifficultyLevel)PlayerPrefs.GetInt(DIFFICULTY_KEY, 0);
    }
}