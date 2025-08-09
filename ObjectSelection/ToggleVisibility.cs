using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToggleVisibility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;  // La caméra principale
    [SerializeField] private Transform cameraTarget;  // La target de la caméra
    [SerializeField] private Toggle toggleButton;  // Le toggle qui apparaîtra
    [SerializeField] private GameObject objectToToggle;  // L'objet dont la visibilité sera contrôlée

    [Header("Settings")]
    [SerializeField] private float visibilityDistance = 5f;  // Distance à laquelle le toggle devient visible

    private void Start()
    {
        // Vérifier si toutes les références sont correctement définies
        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null || cameraTarget == null || toggleButton == null || objectToToggle == null)
        {
            Debug.LogError("Certaines références sont manquantes !");
            enabled = false;  // Désactiver le script si des références manquent
            return;
        }

        // S'assurer qu'un EventSystem existe pour l'interactivité UI
        if (FindObjectOfType<EventSystem>() == null)
        {
            Debug.LogError("Aucun EventSystem trouvé dans la scène !");
        }

        // Initialiser le toggle
        toggleButton.gameObject.SetActive(false);  // Initialement caché
        toggleButton.onValueChanged.AddListener(HandleToggleValueChanged);  // Ajouter l'écouteur pour le toggle
        toggleButton.isOn = false;
        objectToToggle.SetActive(true);  // L'objet n'est pas caché au début
    }

    private void Update()
    {
        // Vérifier la distance entre la caméra et la target
        if (mainCamera != null && cameraTarget != null)
        {
            float distance = Vector3.Distance(mainCamera.transform.position, cameraTarget.position);
            
            // Afficher le toggle si on est assez proche
            if (distance < visibilityDistance)
            {
                if (!toggleButton.gameObject.activeSelf)  // Si le toggle n'est pas déjà visible
                {
                    toggleButton.gameObject.SetActive(true);  // Rendre le toggle visible
                }
            }
            else
            {
                if (toggleButton.gameObject.activeSelf)  // Si le toggle est visible mais que la caméra est trop loin
                {
                    toggleButton.gameObject.SetActive(false);  // Cacher le toggle
                }
            }
        }
    }

    // Fonction appelée quand l'état du toggle change
    private void HandleToggleValueChanged(bool isOn)
    {
        if (objectToToggle != null)
        {
            objectToToggle.SetActive(isOn);  // Change la visibilité de l'objet
            Debug.Log("L'objet a été " + (isOn ? "activé" : "désactivé"));
        }
    }
}