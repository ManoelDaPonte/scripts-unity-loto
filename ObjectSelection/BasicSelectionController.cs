using UnityEngine;

public class BasicSelectionController : MonoBehaviour
{
    [Header("Layer Masks")]
    public LayerMask clickableLayers;  // Masque de layers cliquables
    public LayerMask UI;  // Masque pour les icônes UI

    [Header("Selection Settings")]
    public Color selectionColor = Color.white;  // Couleur de la sélection
    public Color hoverColor = Color.cyan;       // Couleur pour le hover
    public float outlineWidth = 5f;             // Largeur de l'outline
    public float hoverOutlineWidth = 3f;        // Largeur de l'outline pour le hover

    public Camera mainCamera;  // Référence à la caméra principale (optionnel si Camera.main échoue)

    private CameraController cameraController;  // Référence au CameraController
    private DropdownManager dropdownManager;    // Référence au DropdownManager

    private GameObject selectedObject;          // Objet actuellement sélectionné
    private Outline currentOutline;             // Référence à l'outline de l'objet sélectionné
    private GameObject hoveredObject;           // Objet actuellement survolé
    private Outline hoverOutline;               // Référence à l'outline de l'objet survolé

    private HighlightAndSend highlightAndSend;  // Instance de HighlightAndSend

void Start()
{
    // Initialisation des références
    cameraController = FindFirstObjectByType<CameraController>();
    if (cameraController == null)
    {
        Debug.LogWarning("CameraController non trouvé dans la scène.");
    }

    dropdownManager = FindFirstObjectByType<DropdownManager>();
    if (dropdownManager == null)
    {
        Debug.LogWarning("DropdownManager non trouvé dans la scène.");
    }

    // Correction: Utilisation de AddComponent au lieu de new
    highlightAndSend = gameObject.AddComponent<HighlightAndSend>();

    // Assigner Camera.main si aucune caméra n'a été assignée manuellement
    if (mainCamera == null)
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Aucune caméra principale (MainCamera) trouvée et aucune caméra assignée manuellement.");
        }
    }
}

    void Update()
    {
        // Vérifier si le bouton gauche de la souris est cliqué
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
    }

    void HandleSelection()
    {
        // Vérifier si mainCamera est disponible, sinon sortir
        if (mainCamera == null)
        {
            Debug.LogError("Aucune caméra principale disponible pour le raycast.");
            return;
        }

        // Créer un ray depuis la position de la souris en utilisant la caméra principale
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Vérifier si un objet UI est cliqué (sur le layer UI)
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, UI))
        {
            HandleIconClick(hit.collider.gameObject);
        }
        // Vérifier si un objet dans les layers cliquables (incluant SafetyIssue) est cliqué
        else if (Physics.Raycast(ray, out hit, Mathf.Infinity, clickableLayers))
        {
            GameObject clickedObject = hit.collider.gameObject;
            Debug.Log("Objet cliqué : " + clickedObject.name);

            // Si l'objet cliqué est différent de l'objet sélectionné, le sélectionner
            if (clickedObject != selectedObject)
            {
                SelectObject(clickedObject); // Sélectionner le nouvel objet
            }
        }
        else
        {
            // Si aucun objet cliquable n'est sélectionné, désélectionner l'objet actuel
            if (selectedObject != null)
            {
                DeselectObject(selectedObject);
            }
        }
    }

    void HandleIconClick(GameObject icon)
    {
        // Désélectionner l'objet actuellement sélectionné
        DeselectObject(selectedObject);

        // Logique pour gérer la sélection de l'icône
        Debug.Log("Icône sélectionnée : " + icon.name);
    }

    void SelectObject(GameObject obj)
    {
        DeselectObject(selectedObject); // Désélectionner l'ancien objet si nécessaire

        selectedObject = obj;

        // Gérer l'outline de l'objet sélectionné
        currentOutline = selectedObject.GetComponent<Outline>() ?? selectedObject.AddComponent<Outline>();
        ConfigureOutline(currentOutline, selectionColor, outlineWidth);

        // Envoyer l'objet sélectionné à HighlightAndSend
        highlightAndSend.SendSelectedGameObject(selectedObject);

        Debug.Log("Outline activé pour : " + selectedObject.name);
    }

    // Méthode générique pour configurer un outline avec des paramètres spécifiques
    void ConfigureOutline(Outline outline, Color color, float width)
    {
        outline.OutlineColor = color;   // Définir la couleur de l'outline
        outline.OutlineWidth = width;   // Définir la largeur de l'outline
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.enabled = true;         // Activer l'outline
    }

    // Nouvelle méthode pour appliquer un outline de survol
    public void ApplyHoverOutline(GameObject objToHover)
    {
        // Ne pas appliquer de hover outline si l'objet est déjà sélectionné
        if (objToHover == selectedObject) return;
        
        hoveredObject = objToHover;
        
        // Ajouter ou obtenir le composant Outline
        hoverOutline = hoveredObject.GetComponent<Outline>();
        if (hoverOutline == null)
        {
            hoverOutline = hoveredObject.AddComponent<Outline>();
        }
        
        // Configurer l'outline avec la couleur et la largeur de hover
        ConfigureOutline(hoverOutline, hoverColor, hoverOutlineWidth);
        
        Debug.Log("Hover outline appliqué à : " + hoveredObject.name);
    }
    
    // Nouvelle méthode pour supprimer l'outline de survol
    public void RemoveHoverOutline(GameObject objToUnhover)
    {
        // Si l'objet n'est pas l'objet actuellement survolé ou est l'objet sélectionné, ne rien faire
        if (objToUnhover != hoveredObject || objToUnhover == selectedObject) return;
        
        Outline outline = objToUnhover.GetComponent<Outline>();
        if (outline != null)
        {
            // Si cet objet est l'objet sélectionné, ne pas désactiver l'outline
            if (objToUnhover == selectedObject)
            {
                // Reconfigurer l'outline pour qu'il corresponde à l'outline de sélection
                ConfigureOutline(outline, selectionColor, outlineWidth);
            }
            else
            {
                outline.enabled = false;
            }
        }
        
        hoveredObject = null;
        hoverOutline = null;
        
        Debug.Log("Hover outline supprimé de : " + objToUnhover.name);
    }

    public void DeselectObject(GameObject objectToDeselect)
    {
        // Désactiver l'outline de l'objet précédemment sélectionné
        if (objectToDeselect != null && objectToDeselect == selectedObject)
        {
            if (currentOutline != null)
            {
                // Si l'objet est également survolé, garder l'outline mais changer sa configuration
                if (objectToDeselect == hoveredObject && hoverOutline != null)
                {
                    ConfigureOutline(currentOutline, hoverColor, hoverOutlineWidth);
                }
                else
                {
                    currentOutline.enabled = false; // Désactiver l'outline
                }
                Debug.Log("Outline désactivé pour : " + objectToDeselect.name); // Log pour vérification
            }
            selectedObject = null; // Réinitialiser l'objet sélectionné
            currentOutline = null; // Réinitialiser l'outline actuel
        }
    }

    public GameObject GetSelectedObject()
    {
        return selectedObject; // Retourner l'objet actuellement sélectionné
    }
    
    public GameObject GetHoveredObject()
    {
        return hoveredObject; // Retourner l'objet actuellement survolé
    }
}