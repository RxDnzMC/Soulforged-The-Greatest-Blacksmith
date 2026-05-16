using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] TextMeshPro damageText;
    [SerializeField] float moveSpeed = 2f;
    [SerializeField] float fadeDuration = 1f;
    
    private Color originalColor;
    private float timer;

    void Start()
    {
        if (Camera.main != null)
        {
            transform.rotation = Camera.main.transform.rotation;
        }
    }

    public void Setup(float damage)
    {
        // ✅ CEK APAKAH DAMAGE NUMBER DIAKTIFKAN
        if (SettingsManager.Instance != null && !SettingsManager.Instance.ShowDamageNumbers)
        {
            Destroy(gameObject); // Langsung hancurin kalo dimatiin
            return;
        }
        
        damageText.text = Mathf.RoundToInt(damage).ToString();
        originalColor = damageText.color;
        timer = fadeDuration;
        
        transform.position += new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0.5f, 1f), 0);
    }

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;
        
        timer -= Time.deltaTime;
        float alpha = Mathf.Clamp01(timer / fadeDuration);
        damageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
        
        if (timer <= 0)
        {
            Destroy(gameObject);
        }
    }
}