using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject {
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float health;
    public Vector3 playerPosition;
    public int level;
    public float exp;
    public float expToNextLevel;

}
