using UnityEngine;
using TMPro;

public class ToggleCameraSwitcher : MonoBehaviour
{
    private GameObjectCameraManager cameraManager; // Référence au GameObjectCameraManager

    private void Start()
    {
        // Cherche automatiquement le GameObjectCameraManager dans la scène
        cameraManager = FindFirstObjectByType<GameObjectCameraManager>();
        
        if (cameraManager == null)
        {
            Debug.LogError("GameObjectCameraManager non trouvé dans la scène !");
        }
    }

    public void ReceiveToggleChange(string toggleState)
    {
        // On interprète le clic comme un déclencheur d'action (pas de booléen)
        Debug.Log("Clic reçu pour recentrer la caméra.");

        // Si le GameObjectCameraManager est trouvé, on appelle le reset de caméra
        if (cameraManager != null)
        {
            cameraManager.ResetCamera();
        }
    }
}
