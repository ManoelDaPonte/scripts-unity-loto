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

}
