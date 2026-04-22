
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    private Vector3 PlayerPosition;

    void Update()
    {
        PlayerPosition = transform.position;
        playerData.playerPosition = PlayerPosition;
    }
}
