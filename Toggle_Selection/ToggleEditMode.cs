using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToggleEditMode : MonoBehaviour
{
    public Toggle selectionToggle;  // Le Toggle pour activer/désactiver le mode de sélection
    private bool isSelectionMode = false;  // Indicateur du mode de sélection
    private List<GameObject> selectableObjects = new List<GameObject>();  // Liste des objets sélectionnables
    private List<GameObject> selectedObjects = new List<GameObject>();  // Liste des objets marqués comme sélectionnés

    private Color highlightColor = Color.green;  // Couleur pour surligner les objets sélectionnés
    private Dictionary<GameObject, Color> originalColors = new Dictionary<GameObject, Color>();  // Dictionnaire pour stocker les couleurs originales

    void Start()
    {
        // Assigner la fonction ToggleSelectionMode au Toggle
        selectionToggle.onValueChanged.AddListener(delegate { ToggleSelectionMode(selectionToggle.isOn); });
        Debug.Log("ToggleEditMode script started. Toggle assigned.");
    }

    void ToggleSelectionMode(bool isOn)
    {
        isSelectionMode = isOn;
        Debug.Log($"ToggleSelectionMode called. isSelectionMode: {isSelectionMode}");
        UpdateObjectClickability();
    }

    void UpdateObjectClickability()
    {
        Debug.Log("UpdateObjectClickability called.");
        foreach (GameObject obj in selectableObjects)
        {
            Collider objCollider = obj.GetComponent<Collider>();
            Renderer objRenderer = obj.GetComponent<Renderer>();

            if (objCollider != null)
            {
                objCollider.enabled = !isSelectionMode || selectedObjects.Contains(obj);
                //Debug.Log($"Object {obj.name} collider enabled: {objCollider.enabled}");
            }
            else
            {
                Debug.LogWarning($"Object {obj.name} has no collider.");
            }

            if (objRenderer != null)
            {
                Material objMaterial = objRenderer.material;  // Utiliser le matériau de l'objet
                if (selectedObjects.Contains(obj) && isSelectionMode)
                {
                    objMaterial.color = highlightColor;
                    //Debug.Log($"Object {obj.name} color changed to highlight.");
                }
                else
                {
                    if (originalColors.TryGetValue(obj, out Color originalColor))
                    {
                        objMaterial.color = originalColor;
                        //Debug.Log($"Object {obj.name} color reset to original.");
                    }
                }
            }
            else
            {
                Debug.LogWarning($"Object {obj.name} has no renderer.");
            }
        }
    }

    void Update()
    {
        if (isSelectionMode && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;
                //Debug.Log($"Clicked Object: {clickedObject.name}");

                if (!selectedObjects.Contains(clickedObject))
                {
                    // Ajouter l'objet à la liste des objets sélectionnés
                    AddObjectToSelectableList(clickedObject);
                }
                else
                {
                    // Gérer la désélection de l'objet
                    Renderer objRenderer = clickedObject.GetComponent<Renderer>();
                    if (objRenderer != null)
                    {
                        Material objMaterial = objRenderer.material;  // Utiliser le matériau de l'objet

                        selectedObjects.Remove(clickedObject);
                        if (originalColors.TryGetValue(clickedObject, out Color originalColor))
                        {
                            objMaterial.color = originalColor;
                            //Debug.Log($"Object {clickedObject.name} deselected.");
                        }
                    }
                }
            }
        }
    }

    // Méthode publique pour ajouter des objets à la liste des objets sélectionnables
    public void AddObjectToSelectableList(GameObject obj)
    {
        if (!selectedObjects.Contains(obj))
        {
            selectedObjects.Add(obj);

            Renderer objRenderer = obj.GetComponent<Renderer>();
            if (objRenderer != null)
            {
                Material objMaterial = objRenderer.material;  // Utiliser le matériau de l'objet
                if (!originalColors.ContainsKey(obj))
                {
                    originalColors[obj] = objMaterial.color;  // Stocker la couleur originale
                    objMaterial.color = highlightColor;  // Changer la couleur en vert
                    Debug.Log($"Object {obj.name} added to selectable objects with original color: {originalColors[obj]}");
                }
            }
            else
            {
                //Debug.LogWarning($"Object {obj.name} has no renderer. Cannot store original color.");
            }

            Collider objCollider = obj.GetComponent<Collider>();
            if (objCollider != null)
            {
                objCollider.enabled = isSelectionMode;
                //Debug.Log($"Object {obj.name} collider initially enabled: {objCollider.enabled}");
            }
            else
            {
                //Debug.LogWarning($"Object {obj.name} has no collider. Cannot set initial collider state.");
            }
        }
        else
        {
            //Debug.LogWarning($"Object {obj.name} is already in the selectable objects list.");
        }
    }
}
