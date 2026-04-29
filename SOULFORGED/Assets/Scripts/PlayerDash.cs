using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Memastikan script ini menempel di GameObject yang sama dengan PlayerMovement
[RequireComponent(typeof(PlayerMovement))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Input References")]
    [SerializeField] private InputActionReference dashActionReference;
    [SerializeField] private TrailRenderer dashTrail; // Optional: Efek visual saat dash

    // [Header("Layer Settings")]
    // [SerializeField] private string dashLayerName = "PlayerDash"; // Pindah Layer saat Dash
    // [SerializeField] private string EnemyLayerName = "Enemy"; // Layer musuh untuk pengecekan overlap

    // Referensi internal
    private PlayerMovement playerController;
    private Rigidbody rb;
    private bool isDashing = false;
    private float nextDashTime = 0f;
    // private int originalLayer;

    private void Awake()
    {
        // Mengambil referensi dari script sebelah dan komponen Rigidbody
        playerController = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody>();
        dashTrail.emitting = false; // Pastikan efek trail mati saat tidak dash
    }

    private void OnEnable()
    {
        if (dashActionReference != null)
        {
            dashActionReference.action.Enable();
            // Mendaftarkan fungsi OnDashPerformed agar dipanggil otomatis saat tombol ditekan
            dashActionReference.action.performed += OnDashPerformed;
        }
    }

    private void OnDisable()
    {
        if (dashActionReference != null)
        {
            dashActionReference.action.Disable();
            // Melepas fungsi saat karakter mati/hilang agar tidak error
            dashActionReference.action.performed -= OnDashPerformed;
        }
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        // Cek apakah sedang tidak dash DAN cooldown sudah selesai
        if (!isDashing && Time.time >= nextDashTime)
        {
            StartCoroutine(DashRoutine());
        }
    }

    private IEnumerator DashRoutine()
    {
        rb.mass = rb.mass * 10f; 
        isDashing = true;
        nextDashTime = Time.time + dashCooldown; 
        dashTrail.emitting = true; 
        // originalLayer = gameObject.layer; 
        // int EnemyLayer = LayerMask.NameToLayer(EnemyLayerName); // Pastikan ini sesuai dengan layer musuh di projectmu
        // int dashLayer = LayerMask.NameToLayer(dashLayerName); // Pastikan ini sesuai dengan layer dash di projectmu
        // Physics.IgnoreLayerCollision(dashLayer, EnemyLayer, true); // Abaikan tabrakan dengan musuh saat dash

        Vector2 input = playerController.moveInput;
        Vector3 dashDirection = new Vector3(input.x, 0, input.y).normalized;

        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.forward;
        }

        // 1. Matikan kontrol & ubah layer
        playerController.enabled = false;
        // gameObject.layer = LayerMask.NameToLayer(dashLayerName); 
        
        // 2. Melesat
        rb.linearVelocity = dashDirection * dashSpeed;

        // 3. Tunggu durasi dash habis
        yield return new WaitForSeconds(dashDuration);

        // float checkRadius = 0.6f; // Sesuaikan dengan ukuran karaktermu
        // while (Physics.CheckSphere(transform.position, checkRadius, 1 << EnemyLayer))
        // {
        //     // Player tetap bisa gerak lewat kontroler, tapi statusnya masih tembus musuh
        //     playerController.enabled = true; 
        //     yield return null; 
        // }

        // 4. KEMBALIKAN TABRAKAN: Sekarang Player bisa menabrak musuh lagi
        // Physics.IgnoreLayerCollision(dashLayer, EnemyLayer, false);
        // gameObject.layer = originalLayer;
        
        playerController.enabled = true;
        if (dashTrail != null) dashTrail.emitting = false;
        isDashing = false;
        rb.mass = 200; 
    }
}