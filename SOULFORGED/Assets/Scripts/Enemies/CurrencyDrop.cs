using UnityEngine;

public class CurrencyDrop : MonoBehaviour
{
    public enum CurrencyType { Gold, Soul }
    
    [Header("Settings")]
    [SerializeField] private CurrencyType type;
    [SerializeField] private int amount = 1;
    [SerializeField] private PlayerData playerData;

    [Header("Feedback (Optional)")]
    [SerializeField] private GameObject collectEffectPrefab;

    private void OnTriggerEnter(Collider other)
    {
        // Pastikan objek yang menyentuh memiliki Tag "Player"
        if (other.CompareTag("Player"))
        {
            Collect();
        }
    }

    private void Collect()
    {
        if (playerData != null)
        {
            // Logika penambahan berdasarkan tipe
            if (type == CurrencyType.Gold)
            {
                // Sesuaikan nama variabel 'gold' dengan yang ada di PlayerData-mu
                playerData.gold += amount;
                Debug.Log($"Gold terkumpul! Total: {playerData.gold}");
            }
            else if (type == CurrencyType.Soul)
            {
                // Sesuaikan nama variabel 'souls' dengan yang ada di PlayerData-mu
                playerData.souls += amount;
                Debug.Log($"Soul terkumpul! Total: {playerData.souls}");
            }

            // Spawn efek visual jika ada
            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab);
            }

            // Hancurkan objek drop setelah diambil
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("PlayerData belum dimasukkan ke dalam prefab Drop!");
        }
    }
}