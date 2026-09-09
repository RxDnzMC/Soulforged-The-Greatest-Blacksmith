using UnityEngine;
using System.Collections.Generic;

public class CurrencyDrop : MonoBehaviour
{
    public enum CurrencyType { Gold, Soul }
    
    [Header("Settings")]
    [SerializeField] private CurrencyType type;
    [SerializeField] private int amount = 1;
    [SerializeField] private PlayerData playerData;

    [Header("Feedback (Optional)")]
    [SerializeField] private GameObject collectEffectPrefab;

    private static List<CurrencyDrop> activeGoldDrops = new List<CurrencyDrop>();
    private static List<CurrencyDrop> activeSoulDrops = new List<CurrencyDrop>();
    private const int MAX_DROPS = 40;

    private void Start()
    {
        List<CurrencyDrop> targetList = (type == CurrencyType.Gold) ? activeGoldDrops : activeSoulDrops;

        while (targetList.Count >= MAX_DROPS)
        {
            if (targetList[0] != null) Destroy(targetList[0].gameObject);
            targetList.RemoveAt(0);
        }

        targetList.Add(this);
        Destroy(gameObject, 30f);
    }

    private void OnDestroy()
    {
        if (type == CurrencyType.Gold) activeGoldDrops.Remove(this);
        else if (type == CurrencyType.Soul) activeSoulDrops.Remove(this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (playerData != null)
            {
                if (type == CurrencyType.Gold) playerData.gold += amount;
                else if (type == CurrencyType.Soul) playerData.souls += amount;

                if (collectEffectPrefab != null) Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
    
    // ✅ TAMBAHKAN METHOD INI
    public void SetAmount(int newAmount)
    {
        amount = newAmount;
    }
    
    // ✅ TAMBAHKAN METHOD INI (opsional, jika perlu set type)
    public void SetType(CurrencyType newType)
    {
        type = newType;
    }
    
    // ✅ TAMBAHKAN METHOD INI (opsional, untuk get amount)
    public int GetAmount()
    {
        return amount;
    }
}