using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Memastikan script ini menempel di GameObject yang sama dengan PlayerController
[RequireComponent(typeof(PlayerController))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Input References")]
    [SerializeField] private InputActionReference dashActionReference;
    [SerializeField] private TrailRenderer dashTrail; // Optional: Efek visual saat dash

    // Referensi internal
    private PlayerController playerController;
    private Rigidbody rb;
    private bool isDashing = false;
    private float nextDashTime = 0f;

    private void Awake()
    {
        // Mengambil referensi dari script sebelah dan komponen Rigidbody
        playerController = GetComponent<PlayerController>();
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
        isDashing = true;
        nextDashTime = Time.time + dashCooldown; // Set waktu kapan bisa dash lagi
        dashTrail.emitting = true; // Aktifkan efek trail saat dash

        // 1. Dapatkan arah dash (mengambil data moveInput dari script PlayerController kamu)
        Vector2 input = playerController.moveInput;
        Vector3 dashDirection = new Vector3(input.x, 0f, input.y).normalized;

        // Jika pemain tidak menekan tombol arah, dash ke arah karakter menghadap (depan)
        if (dashDirection == Vector3.zero)
        {
            dashDirection = transform.forward;
        }

        // 2. MATIKAN PlayerController sementara agar tidak meng-override kecepatan fisik
        playerController.enabled = false;

        // 3. Terapkan kecepatan Dash
        rb.linearVelocity = dashDirection * dashSpeed;

        // 4. Tunggu selama durasi dash (misal: 0.2 detik)
        yield return new WaitForSeconds(dashDuration);

        // 5. NYALAKAN kembali PlayerController
        playerController.enabled = true;
        dashTrail.emitting = false; // Matikan efek trail saat dash selesai
        isDashing = false;
    }
}