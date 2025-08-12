using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownManager : MonoBehaviour
{
    public List<CameraTargetItem> globalViews = new List<CameraTargetItem>();
    public List<CameraTargetItem> sensors = new List<CameraTargetItem>();

    [Header("Reset Camera")]
    [SerializeField]
    private Transform initialcameraTarget;

    public Camera cameraToMove;
    public float transitionDuration = 1.5f; 
    public Color outlineColor = Color.red;
    public float outlineWidth = 5f;

    private GameObject currentlyHighlightedObject = null; // Variable pour l'objet actuellement surligné
    // SUPPRIMÉ: private HighlightAndSend highlightAndSend;

    private BasicSelectionController selectionController; // Référence au contrôleur de sélection

    private void Start()
    {
        // Récupérer la référence au BasicSelectionController
        selectionController = FindFirstObjectByType<BasicSelectionController>();
        if (selectionController == null)
        {
            Debug.LogError("BasicSelectionController n'est pas trouvé dans la scène.");
        }
    }

    // Méthode pour récupérer un CameraTargetItem basé sur l'outlineObject
    public CameraTargetItem GetCameraTargetItemByOutlineObject(GameObject outlineObject)
    {
        // Parcourir les listes GlobalViews, situations, Sensors, Valves
        foreach (var item in globalViews)
        {
            if (item.outlineObject == outlineObject)
            {
                return item;
            }
        }

        foreach (var item in sensors)
        {
            if (item.outlineObject == outlineObject)
            {
                return item;
            }
        }

        return null; // Aucun CameraTargetItem trouvé
    }

    public void MoveCameraToTarget(Transform target)
    {
        if (cameraToMove == null || target == null)
        {
            Debug.LogError("La caméra ou la cible n'est pas assignée.");
            return;
        }

        StartCoroutine(MoveCamera(target.position, target.rotation));
    }

    private IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation)
    {
        float elapsedTime = 0f;
        Vector3 startingPos = cameraToMove.transform.position;
        Quaternion startingRot = cameraToMove.transform.rotation;

        while (elapsedTime < transitionDuration)
        {
            cameraToMove.transform.position = Vector3.Lerp(startingPos, targetPosition, elapsedTime / transitionDuration);
            cameraToMove.transform.rotation = Quaternion.Lerp(startingRot, targetRotation, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cameraToMove.transform.position = targetPosition;
        cameraToMove.transform.rotation = targetRotation;
    }

    public void HighlightObject(GameObject outlineObject)
    {
        if (outlineObject == null)
        {
            Debug.LogError("Tentative d'ajouter un Outline sur un objet NULL !");
            return;
        }

        Outline outline = outlineObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = outlineObject.AddComponent<Outline>();
            outline.OutlineMode = Outline.Mode.OutlineAll;
        }

        outline.OutlineColor = outlineColor;
        outline.OutlineWidth = outlineWidth;
        outline.enabled = true;
    }

    public void DeselectObject(GameObject objectToDeselect)
    {
        if (objectToDeselect != null)
        {
            // Retirer l'outline de l'objet sélectionné
            Outline outline = objectToDeselect.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false; // Désactiver l'outline
            }
        }
    }
}