using UnityEngine;

[CreateAssetMenu(fileName = "HealthData", menuName = "ScriptableObjects/Health")]
public class HealthData : ScriptableObject
{
    public float currentHealth;
    public float maxHealth = 1000f;

    public float HealthRatio => currentHealth / maxHealth;

    public void Reset()
    {
        currentHealth = maxHealth;
    }
}