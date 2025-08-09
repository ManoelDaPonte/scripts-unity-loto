using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTargetGlobal : MonoBehaviour
{
    public DropdownManager dropdownManager;

    [Header("Sprite Position Settings")]
    public float heightAbove = 2.3f; // Position initiale du sprite

    [Header("Sprite Size Settings")]
    public Vector3 defaultSpriteScale = new Vector3(0.03f, 0.03f, 0.03f); // Échelle par défaut du sprite
    public float hoverScaleFactor = 1.05f; // Facteur d'agrandissement lors du hover (5% ici)

    [Header("Raycast Settings")]
    public float raycastMaxDistance = 1000f; // Distance maximale du raycast

    public float animationDuration = 0.05f; // Durée de l'animation du texte

    [Header("Hover Outline Settings")]
    public Color hoverOutlineColor = Color.green; // Couleur de l'outline lors du hover
    public float hoverOutlineWidth = 3f; // Largeur de l'outline lors du hover

    private Vector3 initialCameraPosition;
    private Camera activeCamera;

    private Sprite pinIconSprite; // Le sprite chargé
    private Dictionary<GameObject, Vector3> originalSpriteScales = new Dictionary<GameObject, Vector3>(); // Stocke les tailles originales des sprites
    // Associer les pin-icons avec leurs outlineObjects
    private Dictionary<GameObject, GameObject> pinIconToOutlineMap = new Dictionary<GameObject, GameObject>();
    private GameObject currentlySelectedOutline; // Stocke l'outline de l'objet actuellement sélectionné
    private GameObject currentlyHoveredOutline; // Stocke l'outline de l'objet actuellement survolé
    
    public GameObject sendSelectedObjectToUnity;
    private HighlightAndSend highlightAndSend;  // Instance de HighlightAndSend

    void Start()
    {
        activeCamera = Camera.main; // Initialise activeCamera avec la caméra principale

        if (activeCamera != null)
        {
            initialCameraPosition = activeCamera.transform.position;
        }
        // Charge le sprite depuis Resources
        pinIconSprite = Resources.Load<Sprite>("png/warning-icon");
        if (pinIconSprite == null)
        {
            Debug.LogError("Le sprite pin-icon n'a pas été trouvé dans Resources/png.");
            return;
        }

        if (dropdownManager == null)
        {
            Debug.LogError("DropdownManager n'est pas assigné dans l'inspecteur.");
            return;
        }

        // Assure-toi que le GameObject est assigné avant de récupérer le script
        if (sendSelectedObjectToUnity != null)
        {
            highlightAndSend = sendSelectedObjectToUnity.GetComponent<HighlightAndSend>();

            if (highlightAndSend == null)
            {
                Debug.LogError("Le script HighlightAndSend n'est pas attaché à SendSelectedObjectToUnity !");
            }
        }
        else
        {
            Debug.LogError("Le GameObject SendSelectedObjectToUnity n'est pas assigné dans l'inspecteur !");
        }

        List<CameraTargetItem> globalViewsItems = dropdownManager.globalViews;
        if (globalViewsItems == null || globalViewsItems.Count == 0)
        {
            Debug.LogError("globalViewsItems est null ou vide.");
            return;
        }

        foreach (CameraTargetItem item in globalViewsItems)
        {
            if (item != null && item.outlineObject != null)
            {
                // Créer un GameObject pour le pin-icon
                GameObject pinIconObject = new GameObject(item.name + "_PinIcon");
                pinIconObject.transform.position = item.outlineObject.transform.position + Vector3.up * heightAbove; // Positionner l'icône juste au-dessus de l'objet outline

                // Ajouter un composant SpriteRenderer pour le pin-icon
                SpriteRenderer pinIconSpriteRenderer = pinIconObject.AddComponent<SpriteRenderer>();
                pinIconSpriteRenderer.sprite = pinIconSprite;
                pinIconSpriteRenderer.sortingOrder = 5; // Assurez-vous que le sprite est dessiné au-dessus des autres objets

                // Ajouter BoxCollider pour détecter les hover
                BoxCollider boxCollider = pinIconObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(10, 10, 1f); // Ajustez la taille du collider selon vos besoins

                // Redimensionner le sprite
                pinIconObject.transform.localScale = defaultSpriteScale; // Appliquer la taille par défaut

                // Créer un GameObject pour le texte
                GameObject textObject = new GameObject("Text");
                textObject.transform.SetParent(pinIconObject.transform);

                originalSpriteScales[pinIconObject] = defaultSpriteScale; // Enregistrer la taille originale
                pinIconToOutlineMap[pinIconObject] = item.outlineObject; // Associer le pin-icon à son outlineObject
            }
            else
            {
                Debug.LogWarning("Un CameraTargetItem ou son outlineObject est null dans globalViewsItems.");
            }
        }
    }
    
    private void DeselectCurrentObject()
    {
        if (currentlySelectedOutline != null)
        {
            dropdownManager.DeselectObject(currentlySelectedOutline); // Désélectionner l'objet dans le DropdownManager
            currentlySelectedOutline = null; // Réinitialiser la référence à l'objet actuellement sélectionné
        }
    }

    // Nouvelle méthode pour gérer l'outline lors du hover
    private void HighlightObjectOnHover(GameObject outlineObject, bool isHovered)
    {
        if (outlineObject == null) return;

        // Ne pas changer l'outline si l'objet est déjà sélectionné
        if (currentlySelectedOutline == outlineObject) return;

        if (isHovered)
        {
            // Activer l'outline avec la couleur de hover
            Outline outline = outlineObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = outlineObject.AddComponent<Outline>();
                outline.OutlineMode = Outline.Mode.OutlineAll;
            }
            
            outline.OutlineColor = hoverOutlineColor;
            outline.OutlineWidth = hoverOutlineWidth;
            outline.enabled = true;
            
            // Enregistrer l'objet actuellement survolé
            currentlyHoveredOutline = outlineObject;
        }
        else
        {
            // Désactiver l'outline seulement si c'est l'objet actuellement survolé
            if (outlineObject == currentlyHoveredOutline)
            {
                Outline outline = outlineObject.GetComponent<Outline>();
                if (outline != null)
                {
                    outline.enabled = false;
                }
                currentlyHoveredOutline = null;
            }
        }
    }

    public void SetActiveCamera(Camera newCamera)
    {
        activeCamera = newCamera;
    }

    void Update()
    {
        if (activeCamera == null)
            {
                Debug.LogWarning("Aucune caméra active n'est définie !");
                return;
            }

            // Vérifier si la caméra a bougé
            bool cameraMoved = activeCamera.transform.position != initialCameraPosition;

            // Gérer la visibilité des icônes
            foreach (var pinIconObject in pinIconToOutlineMap.Keys)
            {
                pinIconObject.SetActive(!cameraMoved);
            }


        // Gestion des sprites face à la caméra
        // Gestion des sprites face à la caméra
        foreach (var pinIconObject in pinIconToOutlineMap.Keys)
        {
            if (pinIconObject != null)
            {
                // Faire en sorte que le sprite fasse toujours face à la caméra
                pinIconObject.transform.LookAt(activeCamera.transform);
                pinIconObject.transform.Rotate(0, 180, 0); // S'assurer que le sprite est orienté correctement

                // Ajouter un effet de flottement pour les sprites
                float floatSpeed = 3f;
                float floatAmount = 0.0005f;
                pinIconObject.transform.position += new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatAmount, 0);
            }
        }

        // Raycast pour détecter le hover de la souris sur les sprites
        Ray ray = activeCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Variable pour suivre si un objet est survolé
        bool anyIconHovered = false;
        GameObject hoveredObject = null;

        if (Physics.Raycast(ray, out hit, raycastMaxDistance))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Si l'objet survolé est un pin-icon
            if (hitObject != null && 
                (pinIconToOutlineMap.ContainsKey(hitObject) || pinIconToOutlineMap.ContainsValue(hitObject)))
            {
                GameObject outlineObject;

                // Si on a cliqué sur le pin-icon, on récupère l'objet associé
                if (pinIconToOutlineMap.ContainsKey(hitObject))
                {
                    outlineObject = pinIconToOutlineMap[hitObject];

                    // Animation du hover pour l'icône
                    StartCoroutine(AnimateHover(hitObject, true));
                    anyIconHovered = true;
                    hoveredObject = hitObject;
                }
                else
                {
                    // Si on a cliqué directement sur l'objet, on le prend tel quel
                    outlineObject = hitObject;
                }

                // Highlight l'objet associé à l'icône lors du hover
                HighlightObjectOnHover(outlineObject, true);

                // Lors d'un clic sur le pin-icon OU l'objet, déplacer la caméra vers la cible associée
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Clic détecté sur : " + hitObject.name);

                    if (outlineObject == null)
                    {
                        Debug.LogWarning("OutlineObject est null pour : " + hitObject.name);
                        return;
                    }

                    // Envoie les données de l'objet
                    if (highlightAndSend != null)
                    {
                        highlightAndSend.SendSelectedGameObject(outlineObject);
                    }
                    else
                    {
                        Debug.LogError("highlightAndSend n'est pas défini !");
                    }

                    CameraTargetItem cameraTargetItem = dropdownManager.GetCameraTargetItemByOutlineObject(outlineObject);
                    if (cameraTargetItem != null)
                    {
                        dropdownManager.MoveCameraToTarget(cameraTargetItem.target);

                        // Désactiver l'outline de l'objet précédemment sélectionné, si existant
                        if (currentlySelectedOutline != null && currentlySelectedOutline != outlineObject)
                        {
                            dropdownManager.DeselectObject(currentlySelectedOutline);
                        }

                        // Désactiver l'outline de hover si présent
                        if (currentlyHoveredOutline != null && currentlyHoveredOutline != outlineObject)
                        {
                            HighlightObjectOnHover(currentlyHoveredOutline, false);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Aucun CameraTargetItem trouvé pour l'outlineObject : " + outlineObject.name);
                    }
                }
            }
        }
        

        // Si le raycast ne touche rien ou si on a cliqué ailleurs
        if (!anyIconHovered && Input.GetMouseButtonDown(0))
        {
            DeselectCurrentObject();
        }

        // Gestion des objets qui ne sont plus survolés
        foreach (var entry in pinIconToOutlineMap)
        {
            GameObject pinIconObject = entry.Key;
            GameObject outlineObject = entry.Value;

            // Si on ne survole pas ce sprite
            if (pinIconObject != hoveredObject)
            {
                // Animation pour réduire le sprite
                StartCoroutine(AnimateHover(pinIconObject, false));
                
                // Désactiver l'outline de hover sauf si l'objet est sélectionné
                if (outlineObject != currentlySelectedOutline)
                {
                    HighlightObjectOnHover(outlineObject, false);
                }
            }
        }
    }

    IEnumerator AnimateHover(GameObject pinIconObject, bool hovering)
    {
 
        float time = 0f;

        // Définir la taille de départ et de fin pour le sprite
        Vector3 initialScale = originalSpriteScales[pinIconObject]; // Récupérer la taille originale stockée
        Vector3 targetScale = hovering ? initialScale * hoverScaleFactor : initialScale;

        // Boucle pour animer sur la durée spécifiée
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = time / animationDuration;

            // Lerp pour agrandir ou réduire le sprite
            pinIconObject.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);

            yield return null;
        }

        // S'assurer que les valeurs finales sont bien appliquées
        pinIconObject.transform.localScale = targetScale;
    }
}