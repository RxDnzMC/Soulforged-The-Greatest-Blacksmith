using UnityEngine;
using UnityEngine.InputSystem;

// 1. REQUIRE COMPONENT: Memastikan script ini tidak akan error karena lupa pasang Rigidbody
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    // 2. SERIALIZE FIELD: Menjaga variabel tetap 'private' (aman dari script lain) 
    // tapi tetap bisa diatur oleh Game Designer lewat Inspector.
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;

    // 3. INPUT ACTION REFERENCE: Standar industri untuk menghubungkan file Master Input ke Script
    [Header("Input References")]
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference aimActionReference;

    // Variabel internal yang disembunyikan
    public Rigidbody rb;
    private Camera mainCamera;
    public Vector2 moveInput;
    private Vector2 mousePosition;

    // Awake dipanggil paling pertama kali saat game dimulai
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main; // Caching kamera untuk menghemat memori
    }

    private void OnEnable()
    {
        // Menyalakan file Master Input
        if (moveActionReference != null) moveActionReference.action.Enable();
        if (aimActionReference != null) aimActionReference.action.Enable();
    }

    private void OnDisable()
    {
        // Mematikan saat karakter mati/hilang agar tidak memory leak
        if (moveActionReference != null) moveActionReference.action.Disable();
        if (aimActionReference != null) aimActionReference.action.Disable();
    }

    [Header("Footstep Settings")]
    
    [SerializeField] private float stepInterval = 0.5f; 
    private float stepTimer;

    private void HandleFootsteps()
    {
        // Cek apakah player bergerak di lantai
        if (moveInput.sqrMagnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                // MANGGIL SOUND MANAGER
                // Gunakan PlaySound3D agar suara terdengar dari posisi kaki player
                if (SoundManager.Instance != null) {
                    SoundManager.Instance.PlaySound3D("Footstep Grass", transform.position);
                } 
                else {
                }
                
                stepTimer = stepInterval; 
            }
        }
        else
        {
            stepTimer = 0; 
        }
    }
    private void Update()
    {
        // Update hanya digunakan untuk membaca tombol/input
        ReadInputs();
        HandleFootsteps();
    }

    private void FixedUpdate()
    {
        // FixedUpdate khusus untuk pergerakan Fisika agar mulus dan tidak tembus tembok
        HandleMovement();
        HandleRotation();
    }

    // 4. SINGLE RESPONSIBILITY: Fungsi dipecah-pecah agar mudah dibaca dan diperbaiki (Debug)
    private void ReadInputs()
    {
        if (moveActionReference != null)
        {
            moveInput = moveActionReference.action.ReadValue<Vector2>();
        }

        if (aimActionReference != null)
        {
            mousePosition = aimActionReference.action.ReadValue<Vector2>();
        }
    }

    private void HandleMovement()
    {
        // Normalized mencegah bug umum: Jalan serong (diagonal) lebih cepat dari jalan lurus
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        
        // Menggerakkan rigidbody menggunakan velocity (sangat cocok untuk game Top-Down)
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    private void HandleRotation()
    {
        // Menembakkan laser imajiner (Raycast) ke lantai untuk mencari titik kursor
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float hitDistance))
        {
            Vector3 pointToLook = ray.GetPoint(hitDistance);
            Vector3 lookDirection = new Vector3(pointToLook.x - transform.position.x, 0f, pointToLook.z - transform.position.z);

            // Cek jika mouse benar-benar bergerak (mencegah error rotasi)
            if (lookDirection.sqrMagnitude > 0.01f) 
            {
                transform.rotation = Quaternion.LookRotation(lookDirection);
            }
        }
    }

    
}