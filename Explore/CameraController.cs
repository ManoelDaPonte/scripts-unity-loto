using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class CameraController : MonoBehaviour
{
    public float freeLookSensitivity = 2.0f; // Sensibilité du mouvement libre
    public float moveSpeed = 1.0f; // Vitesse de déplacement
    public Camera targetCamera; // Référence à la caméra à contrôler

    private bool isFreeLooking = false; // Mode "free look" activé ou non
    private float fixedY; // Hauteur fixe pour verrouiller le déplacement vertical

    private void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main; // Utilise la caméra principale si aucune caméra n'est spécifiée
        }
        fixedY = targetCamera.transform.position.y; // Sauvegarde la hauteur initiale
    }

    private void Update()
    {
        if (targetCamera == null) return;

        // Active/désactive le "free look" avec le bouton droit de la souris
        if (Input.GetMouseButtonDown(1))
        {
            isFreeLooking = true;
            Cursor.lockState = CursorLockMode.Locked; // Cache le curseur
        }
        if (Input.GetMouseButtonUp(1))
        {
            isFreeLooking = false;
            Cursor.lockState = CursorLockMode.None; // Montre le curseur
        }

        // Rotation de la caméra si "free look" est activé
        if (isFreeLooking)
        {
            float rotationX = targetCamera.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
            float rotationY = targetCamera.transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
            targetCamera.transform.localEulerAngles = new Vector3(rotationY, rotationX, 0f);
        }

        // Déplacement de la caméra (X et Z seulement, Y est verrouillé)
        Vector3 moveDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) moveDirection += targetCamera.transform.forward;
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) moveDirection -= targetCamera.transform.forward;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) moveDirection -= targetCamera.transform.right;
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) moveDirection += targetCamera.transform.right;

        // Normalisation pour éviter un déplacement plus rapide en diagonale
        moveDirection = moveDirection.normalized * moveSpeed * Time.deltaTime;

        // Appliquer le déplacement en gardant Y fixe
        Vector3 newPosition = targetCamera.transform.position + moveDirection;
        newPosition.y = fixedY; // Verrouille la position Y
        targetCamera.transform.position = newPosition;
    }

    private bool IsPointerOverUIElement()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }
}
