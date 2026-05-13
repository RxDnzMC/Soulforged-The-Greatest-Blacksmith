
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    private Vector3 PlayerPosition;
    // public float exp;
    // public float level;

    void Start()
    {
        
    }

    void Update()
    {
        PlayerPosition = transform.position;
        playerData.playerPosition = PlayerPosition;
        // exp = playerData.exp;
        // level = playerData.level;
    }
}
