using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetSpecific : MonoBehaviour
{
    public DropdownManager dropdownManager;
    public BasicSelectionController selectionController; // Added reference to BasicSelectionController

    [Header("Sprite Position Settings")]
    public float heightAbove = 0.3f;

    [Header("Screen Size Based Sprite Settings")]
    public float relativeSpriteSize = 0.2f; // Pourcentage de l'écran utilisé pour la taille du sprite

    private GameObject lastHoveredObject = null; // Variable pour stocker l'objet survolé précédemment
    private GameObject lastOutlinedObject = null; // Variable pour stocker l'objet outliné par le hover

    [Header("Distance-based Scaling Settings")]
    public float minScaleDistance = 0.1f; // Distance minimale pour laquelle l'icône sera à sa taille minimum
    public float maxScaleDistance = 100f; // Distance maximale pour laquelle l'icône sera à sa taille maximum
    public float minScaleFactor = 0.1f; // Facteur de taille minimum à distance proche
    public float maxScaleFactor = 0.11f; // Facteur de taille maximum à distance éloignée

    [Header("Raycast Settings")]
    public float raycastMaxDistance = 1000f;

    // Ajout des sprites spécifiques pour chaque famille
    [Header("Sprites for Each Family")]
    public Sprite sensorsSprite;

    private Dictionary<GameObject, Vector3> originalSpriteScales = new Dictionary<GameObject, Vector3>(); // Stocke les tailles originales des sprites
    private Camera mainCamera;
    private Vector3 initialCameraPosition;

    [Header("Sprite Appearance Settings")]
    public Color spriteColor = Color.white; // Couleur par défaut du sprite (blanc)
    public Color hoverColor = Color.yellow; // Couleur du sprite lors du hover

    [Header("Camera Movement Settings")]
    public float cameraMoveDuration = 1f; // Durée du mouvement de la caméra
    public float cameraOffset = 2f; // Décalage de la caméra pour qu'elle regarde l'icône
 
    private GameObject currentlyHighlightedObject = null; // Variable pour l'objet actuellement surligné
    // Ajout d'un dictionnaire pour mapper chaque pin-icon à son outlineObject
    private Dictionary<GameObject, GameObject> pinIconToOutlineMap = new Dictionary<GameObject, GameObject>();

    // SUPPRIMÉ: public GameObject sendSelectedObjectToUnity;
    // SUPPRIMÉ: private HighlightAndSend highlightAndSend;

    void Start()
    {
        if (dropdownManager == null)
        {
            Debug.LogError("DropdownManager n'est pas assigné dans l'inspecteur.");
            return;
        }

        // Trouver le BasicSelectionController si non assigné
        if (selectionController == null)
        {
            selectionController = FindFirstObjectByType<BasicSelectionController>();
            if (selectionController == null)
            {
                Debug.LogError("BasicSelectionController n'est pas disponible dans la scène.");
            }
        }

        // SUPPRIMÉ: Initialisation de HighlightAndSend

        // Récupérer la référence de la caméra principale
        mainCamera = Camera.main;
        
        // Stocker la position initiale de la caméra
        initialCameraPosition = mainCamera.transform.position;

        // Créer les icônes pour chaque famille avec leurs sprites respectifs
        CreateIconsForFamily(dropdownManager.sensors, "Sensors", sensorsSprite);
    }

    // Modification pour accepter un sprite spécifique pour chaque famille
    private void CreateIconsForFamily(List<CameraTargetItem> items, string familyName, Sprite familySprite)
    {
        if (items == null || items.Count == 0)
        {
            Debug.LogWarning($"{familyName} n'a pas d'éléments ou est null.");
            return;
        }

        foreach (CameraTargetItem item in items)
        {
            if (item != null && item.outlineObject != null)
            {
                // Créer un GameObject pour le pin-icon
                GameObject pinIconObject = new GameObject(item.name + "_circleIcon");

                // Calculer la position max Y de l'objet
                float objectHeight = GetObjectHeight(item.outlineObject);
                Vector3 positionAbove = item.outlineObject.transform.position + Vector3.up * (objectHeight + heightAbove);

                // Positionner au-dessus de l'objet
                pinIconObject.transform.position = positionAbove;

                // Ajouter un composant SpriteRenderer
                SpriteRenderer pinIconSpriteRenderer = pinIconObject.AddComponent<SpriteRenderer>();
                pinIconSpriteRenderer.sprite = familySprite; // Utiliser le sprite spécifique de la famille
                pinIconSpriteRenderer.color = spriteColor; // Appliquer la couleur choisie
                pinIconSpriteRenderer.sortingOrder = 5000; // S'assurer que le sprite est visible

                // Ajouter un BoxCollider pour le hover
                BoxCollider boxCollider = pinIconObject.AddComponent<BoxCollider>();

                // Redimensionner le sprite selon la taille de l'écran
                float screenRelativeScale = GetRelativeScale();
                pinIconObject.transform.localScale = new Vector3(screenRelativeScale, screenRelativeScale, screenRelativeScale);

                // Ajuster la taille du BoxCollider à la création
                AdjustBoxColliderSize(pinIconObject, boxCollider);

                originalSpriteScales[pinIconObject] = pinIconObject.transform.localScale; // Sauvegarder la taille d'origine
                
                // Associer l'icône à son outlineObject
                pinIconToOutlineMap[pinIconObject] = item.outlineObject; // Ajout ici
            }
            else
            {
                Debug.LogWarning($"Un CameraTargetItem ou son outlineObject est null dans {familyName}.");
            }
        }
    }

    private float GetObjectHeight(GameObject obj)
    {
        // Récupérer le Renderer pour obtenir la taille de l'objet
        Renderer objRenderer = obj.GetComponent<Renderer>();
        if (objRenderer != null)
        {
            // Retourner la hauteur en utilisant le Bounds de l'objet
            return objRenderer.bounds.extents.y; // extents.y donne la moitié de la hauteur, donc on multiplie par 2
        }
        else
        {
            Debug.LogWarning($"Le Renderer de l'objet {obj.name} est nul.");
            return 0f; // Si l'objet n'a pas de Renderer, on retourne 0
        }
    }

    void Update()
    {
        bool shouldShowSprites = IsCameraOnGlobalView();

        // Toujours orienter les sprites vers la caméra et ajuster la taille selon la distance
        foreach (var entry in pinIconToOutlineMap)
        {
            GameObject pinIconObject = entry.Key;
            if (pinIconObject != null)
            {
                pinIconObject.SetActive(shouldShowSprites);
                if (shouldShowSprites)
                {
                    // Orienter les sprites vers la caméra
                    pinIconObject.transform.LookAt(mainCamera.transform);
                    pinIconObject.transform.Rotate(0, 180, 0);

                    // Ajuster la taille du sprite selon la distance
                    float distanceToCamera = Vector3.Distance(pinIconObject.transform.position, mainCamera.transform.position);
                    AdjustSpriteSizeBasedOnDistance(pinIconObject, distanceToCamera);
                }
            }
        }

        // Raycast pour détecter le hover
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Initialiser un booléen pour vérifier si l'utilisateur a survolé un pin-icon ou un objet
        bool hoveredPinOrObject = false;

        if (Physics.Raycast(ray, out hit, raycastMaxDistance))
        {
            GameObject hitObject = hit.collider.gameObject;
            GameObject outlineObject = null;

            // Vérifier si c'est un pin-icon ou l'objet associé
            if (pinIconToOutlineMap.ContainsKey(hitObject))
            {
                // C'est un pin-icon, récupérer l'objet associé
                outlineObject = pinIconToOutlineMap[hitObject];
            }
            else if (pinIconToOutlineMap.ContainsValue(hitObject))
            {
                // C'est un objet associé, l'utiliser directement
                outlineObject = hitObject;
            }

            // Si c'est soit un pin-icon soit un objet associé
            if (outlineObject != null)
            {
                // Marquer que l'on survole un élément interactif
                hoveredPinOrObject = true;

                // Appliquer l'effet de hover
                ShowHoverEffect(hitObject);

                // Vérifier si c'est un nouvel objet survolé
                if (lastHoveredObject != hitObject)
                {
                    // Réinitialiser l'outline de l'objet précédemment survolé si nécessaire
                    if (lastOutlinedObject != null && lastOutlinedObject != outlineObject)
                    {
                        selectionController.RemoveHoverOutline(lastOutlinedObject);
                    }

                    // Appliquer l'outline sur l'objet survolé
                    selectionController.ApplyHoverOutline(outlineObject);
                    lastOutlinedObject = outlineObject;
                }

                // Mettre à jour l'objet actuellement survolé
                lastHoveredObject = hitObject;

                // Lors d'un clic, déplacer la caméra vers la cible associée
                if (Input.GetMouseButtonDown(0))
                {
                    // SUPPRIMÉ: Envoi vers HighlightAndSend
                    Debug.Log("Objet sélectionné : " + outlineObject.name);

                    CameraTargetItem cameraTargetItem = dropdownManager.GetCameraTargetItemByOutlineObject(outlineObject);
                    if (cameraTargetItem != null)
                    {
                        dropdownManager.MoveCameraToTarget(cameraTargetItem.target);

                        // Désactiver l'outline de l'objet précédemment sélectionné, si existant
                        if (currentlyHighlightedObject != null && currentlyHighlightedObject != outlineObject)
                        {
                            dropdownManager.DeselectObject(currentlyHighlightedObject);
                        }

                        // Activer l'outline de l'objet sélectionné
                        dropdownManager.HighlightObject(outlineObject);

                        // Mettre à jour l'objet sélectionné
                        currentlyHighlightedObject = outlineObject;
                    }
                    else
                    {
                        Debug.LogWarning("Aucun CameraTargetItem trouvé pour l'outlineObject : " + outlineObject.name);
                    }
                }
            }
        }

        // Si aucun objet n'est survolé, réinitialiser la couleur de l'icône et supprimer l'outline
        if (!hoveredPinOrObject)
        {
            // Réinitialiser la couleur de l'icône précédemment survolée
            if (lastHoveredObject != null)
            {
                HideHoverEffect(lastHoveredObject);
                lastHoveredObject = null;
            }

            // Supprimer l'outline de l'objet associé
            if (lastOutlinedObject != null && selectionController != null)
            {
                selectionController.RemoveHoverOutline(lastOutlinedObject);
                lastOutlinedObject = null;
            }
        }
    }

    private bool IsCameraOnGlobalView()
    {
        foreach (var target in dropdownManager.globalViews)
        {
            if (target != null && target.target != null && Vector3.Distance(mainCamera.transform.position, target.target.transform.position) < 0.1f)
            {
                return true;
            }
        }
        return false;
    }

    // Fonction pour redimensionner le BoxCollider du sprite pour correspondre à ses dimensions
    private void AdjustBoxColliderSize(GameObject pinIconObject, BoxCollider boxCollider)
    {
        SpriteRenderer spriteRenderer = pinIconObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
            boxCollider.size = spriteSize; // Redimensionner le BoxCollider selon la taille du sprite
        }
    }

    // Fonction pour ajuster la taille du sprite en fonction de la distance
    private void AdjustSpriteSizeBasedOnDistance(GameObject spriteObject, float distance)
    {
        if (originalSpriteScales.ContainsKey(spriteObject))
        {
            // Calculer un facteur de mise à l'échelle en fonction de la distance
            float t = Mathf.InverseLerp(minScaleDistance, maxScaleDistance, distance);
            float scaleFactor = Mathf.Lerp(minScaleFactor, maxScaleFactor, t);

            // Appliquer le facteur de mise à l'échelle à la taille d'origine
            spriteObject.transform.localScale = originalSpriteScales[spriteObject] * scaleFactor;
        }
    }

    // Fonction pour obtenir la taille relative de l'écran pour le sprite
    private float GetRelativeScale()
    {
        return relativeSpriteSize;
    }

    // Fonction pour afficher l'effet de hover
    private void ShowHoverEffect(GameObject pinIconObject)
    {
        SpriteRenderer spriteRenderer = pinIconObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hoverColor; // Changer la couleur à l'effet de survol
        }
    }

    // Fonction pour cacher l'effet de hover
    private void HideHoverEffect(GameObject pinIconObject)
    {
        SpriteRenderer spriteRenderer = pinIconObject.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = spriteColor; // Réinitialiser la couleur
        }
    }
}