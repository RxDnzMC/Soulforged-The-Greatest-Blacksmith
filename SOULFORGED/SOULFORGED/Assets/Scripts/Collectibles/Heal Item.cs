using UnityEngine;

public class HealItem : MonoBehaviour
{
    [SerializeField] PlayerData playerData;
    [SerializeField] int HealAmount = 100;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) {
            playerData.health += HealAmount;
            Destroy(gameObject);
        }
    }
}
