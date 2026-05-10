using UnityEngine;

public class SimpleFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform playerTransform; // Tarik objek Player ke sini di Inspector
    [SerializeField] private Vector3 offset;             // Jarak tambahan (misal: biar nggak pas di tengah player)

    // LateUpdate menjamin pergerakan mulus tanpa getar (jitter)
    private void LateUpdate()
    {
        if (playerTransform != null)
        {
            // Set posisi objek ini sama persis dengan posisi player + offset
            transform.position = playerTransform.position + offset;
        }
    }
}