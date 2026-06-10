using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Mouse Sensitivity")]
    [Tooltip("Mouse kitna fast camera ghumayega")]
    public float mouseSensitivity = 200f;

    [Header("Vertical Look Limits")]
    [Tooltip("Camera kitna upar dekh sakta hai")]
    public float maxLookUp = 60f;

    [Tooltip("Camera kitna neeche dekh sakta hai")]
    public float maxLookDown = -60f;

    [Header("Cursor Settings")]
    [Tooltip("Game start hote hi cursor lock ho jaye")]
    public bool lockCursorOnStart = true;

    [Header("Control Settings")]
    [Tooltip("Camera control enabled/disabled")]
    public bool controlEnabled = true;

    // Private variables - internal use ke liye
    private float verticalRotation = 0f;
    private float horizontalRotation = 0f;

    // Reference to player position (parent of camera)
    private Transform playerBody;

    void Start()
    {
        // Cursor lock kar do center mein, invisible bhi
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // PlayerCamera ka parent (PlayerPosition) reference le lo
        playerBody = transform.parent;

        // Initial rotation save kar lo
        if (playerBody != null)
        {
            horizontalRotation = playerBody.eulerAngles.y;
        }
    }

    void Update()
    {
        if (controlEnabled) {
            HandleMouseLook();
        }

        HandleCursorToggle();
    }

    void HandleMouseLook()
    {
        // Mouse input lo (X = horizontal, Y = vertical)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Vertical rotation calculate karo (up/down)
        verticalRotation -= mouseY;
        // Limit lagao taaki poori 360 na ghume
        verticalRotation = Mathf.Clamp(verticalRotation, maxLookDown, maxLookUp);

        // Horizontal rotation add karo (left/right)
        horizontalRotation += mouseX;

        // Camera ko vertical rotate karo (sirf X axis)
        transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // Player body ko horizontal rotate karo (sirf Y axis)
        if (playerBody != null)
        {
            playerBody.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
        }
    }

    void HandleCursorToggle()
    {
        // Escape press karne par cursor unlock ho jaye (testing ke liye)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}