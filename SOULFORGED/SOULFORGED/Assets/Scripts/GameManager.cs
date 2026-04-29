using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] PlayerData playerData;
    bool isPlayerDead = false;
    void Start()
    {
        playerData.health = 1000;
        playerData.exp = 0;
        playerData.level = 1;
        playerData.expToNextLevel = 100;
    }

    // void ZeroHealth() {
    //     if (playerData.health <= 0) {
    //         Debug.Log("Player mati jir");
    //         isPlayerDead = true;
    //     }
    //     else {
    //        playerData.health -= 1;
    //     }
    // }
    // Update is called once per frame
    void Update()
    {
        // if (isPlayerDead == false) ZeroHealth(); return;
        if (playerData.exp >= playerData.expToNextLevel) {
            playerData.level += 1; // Naikkan level
            playerData.exp -= playerData.expToNextLevel; // Reset ke sisa exp setelah naik level
            playerData.expToNextLevel *= 1.3f; // Next Level Requirement (naik 30% setiap level)
            Debug.Log($"Level Up! Sekarang level {playerData.level}");
        }
        
    }
}
