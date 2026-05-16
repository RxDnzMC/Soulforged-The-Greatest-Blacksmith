using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f; 

    [Header("Input References")]
    [SerializeField] private InputActionReference moveActionReference;
    [SerializeField] private InputActionReference aimActionReference;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;

    //TAMBAHKAN REFERENSI DAN PENGATURAN PIVOT TEMBAKAN
    [Header("Aiming Settings")]
    [Tooltip("Empty GameObject (Child) yang digunakan untuk menembak")]
    [SerializeField] private Transform weaponPivot;
    [Tooltip("Radius lingkaran luar untuk posisi pivot (jarak dari tengah player ke titik tembak)")]
    [SerializeField] private float aimRadius = 1.2f; // Sesuaikan agar lebih besar dari kolider player

    [Tooltip("Ketinggian posisi tembak (Y axis offset dari posisi kaki player)")]
    [SerializeField] private float aimHeightOffset = 1.0f;

    public Rigidbody rb;
    private Camera mainCamera;
    public Vector2 moveInput;
    private Vector2 mousePosition;

    private Vector3 pointToLook; // Menyimpan posisi mouse di dunia

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        if (moveActionReference != null) moveActionReference.action.Enable();
        if (aimActionReference != null) aimActionReference.action.Enable();
    }

    private void OnDisable()
    {
        if (moveActionReference != null) moveActionReference.action.Disable();
        if (aimActionReference != null) aimActionReference.action.Disable();
    }

    [Header("Footstep Settings")]
    [SerializeField] private float stepInterval = 0.5f; 
    private float stepTimer;

    private void HandleFootsteps()
    {
        if (moveInput.sqrMagnitude > 0.1f)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0)
            {
                if (SoundManager.Instance != null) {
                    SoundManager.Instance.PlaySound3D("Footstep Grass", transform.position);
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
        ReadInputs();
        HandleFootsteps();
        HandleAnimation();
        
        // Memindahkan pembacaan posisi mouse dunia ke Update
        UpdateMouseWorldPosition();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleCharacterRotation(); 
        HandleAimPivotPosition(); // Panggil fungsi pemindahan posisi pivot di sini
    }

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
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        rb.linearVelocity = moveDirection * moveSpeed;
    }

    private void HandleCharacterRotation()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
    }

    //FUNGSI UPDATE UNTUK MENGHITUNG POSISI MOUSE DI DUNIA
    private void UpdateMouseWorldPosition()
    {
        if (weaponPivot == null) return; 

        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        
        // Gunakan posisi player ditambah aimHeightOffset untuk tinggi landasan raycast
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, transform.position.y + aimHeightOffset, 0));

        if (groundPlane.Raycast(ray, out float hitDistance))
        {
            pointToLook = ray.GetPoint(hitDistance);
        }
    }

    // FUNGSI FIXEDUPDATE UNTUK MEMINDAHKAN POSISI PIVOT (PADA LINGKARAN LUAR)
    private void HandleAimPivotPosition()
    {
        if (weaponPivot == null) return;

        Vector3 lookDirection = new Vector3(pointToLook.x - transform.position.x, 0f, pointToLook.z - transform.position.z);

        if (lookDirection.sqrMagnitude > 0.01f)
        {
            Vector3 targetPivotPosition = transform.position + (lookDirection.normalized * aimRadius);
            
            // Naikkan posisi Y sesuai dengan offset yang kita tentukan
            targetPivotPosition.y += aimHeightOffset;
            
            // Pindahkan weaponPivot ke posisi target ini (posisi dunia)
            weaponPivot.position = targetPivotPosition;

            // Pastikan weaponPivot juga menghadap ke arah mouse
            weaponPivot.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    private void HandleAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat("Speed", moveInput.magnitude);
        }
    }
}