using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] PlayerData playerData;
    bool isPlayerDead = false;
    void Start()
    {
        playerData.health = 1000;
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
        
    }
}
