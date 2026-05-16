using UnityEngine;

public class HealItem : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    [SerializeField] int HealAmount = 100;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {
            // ✅ CEK AGAR TIDAK MELEBIHI MAX HEALTH
            float newHealth = playerData.health + HealAmount;
            
            if (newHealth > playerData.maxHealth)
            {
                playerData.health = playerData.maxHealth;
                Debug.Log($"Healed to max: {playerData.health}/{playerData.maxHealth}");
            }
            else
            {
                playerData.health = newHealth;
                Debug.Log($"Healed +{HealAmount}: {playerData.health}/{playerData.maxHealth}");
            }
            
            Destroy(gameObject);
        }
    }
}