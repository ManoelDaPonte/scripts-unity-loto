using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectCameraManager : MonoBehaviour
{
    [System.Serializable]
    public class GameObjectCameraTarget
    {
        public GameObject gameObject;    // Le GameObject à cibler
        public Transform cameraTarget;   // La cible de la caméra associée (position et rotation)
    }

    public List<GameObjectCameraTarget> objectCameraPairs;  // Liste des paires GameObject et cibles de caméra
    public float transitionDuration = 1.5f;                 // Durée de transition pour déplacer la caméra
    public Transform initialCameraTarget;                   // Cible initiale de la caméra pour le reset
    public Color outlineColor = Color.red;                  // Couleur du surlignage
    public float outlineWidth = 5f;                         // Largeur du surlignage

    private GameObject currentlyHighlightedObject = null;   // Objet actuellement surligné
    private Vector3 initialCameraPosition;
    private Quaternion initialCameraRotation;

    void Start()
    {
        if (initialCameraTarget != null)
        {
            initialCameraPosition = initialCameraTarget.position;
            initialCameraRotation = initialCameraTarget.rotation;
        }
        else
        {
            Debug.LogError("Initial Camera Target is not assigned.");
        }
    }

    // Méthode pour déplacer la caméra en fonction du Name
    public void MoveCameraToTargetByName(string name)
    {
        //Debug.Log($"Received Name: {name}"); // Log du Name reçu pour le débogage

        // Trouver le GameObject associé au Name reçu
        GameObject targetGameObject = null;

        foreach (var item in objectCameraPairs)
        {
            if (item.gameObject.name == name)
            {
                targetGameObject = item.gameObject;
                break; // Quitter la boucle une fois le bon GameObject trouvé
            }
        }

        if (targetGameObject != null)
        {
            Transform targetCameraTransform = objectCameraPairs.Find(item => item.gameObject == targetGameObject).cameraTarget;

            if (targetCameraTransform != null)
            {
                //Debug.Log($"Moving camera to target for GameObject: {targetGameObject.name}"); // Log pour confirmation
                
                // Désélectionner l'objet actuellement surligné
                if (currentlyHighlightedObject != null)
                {
                    DeselectObject(currentlyHighlightedObject);
                }

                // Surbrillance du nouvel objet
                HighlightObject(targetGameObject, true);

                // Déplacer la caméra vers la nouvelle cible
                StartCoroutine(MoveCameraToTarget(targetCameraTransform));

                // Stocker le nouvel objet surligné
                currentlyHighlightedObject = targetGameObject;
            }
            else
            {
                Debug.LogWarning("Cible de caméra nulle pour le GameObject: " + targetGameObject.name);
            }
        }
        else
        {
            Debug.LogWarning("Aucun GameObject trouvé avec le Name spécifié: " + name);
        }
    }

    public void ResetCamera()
    {
        if (currentlyHighlightedObject != null)
        {
            DeselectObject(currentlyHighlightedObject); // Désélectionner l'outline si on fait un reset
        }

        StartCoroutine(MoveCameraToInitialPosition());
    }

    private IEnumerator MoveCameraToTarget(Transform target)
    {
        float elapsedTime = 0f;
        Vector3 startingPos = Camera.main.transform.position;
        Quaternion startingRot = Camera.main.transform.rotation;

        while (elapsedTime < transitionDuration)
        {
            Camera.main.transform.position = Vector3.Lerp(startingPos, target.position, elapsedTime / transitionDuration);
            Camera.main.transform.rotation = Quaternion.Lerp(startingRot, target.rotation, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = target.position;
        Camera.main.transform.rotation = target.rotation;
    }

    private IEnumerator MoveCameraToInitialPosition()
    {
        float elapsedTime = 0f;
        Vector3 startingPos = Camera.main.transform.position;
        Quaternion startingRot = Camera.main.transform.rotation;

        while (elapsedTime < transitionDuration)
        {
            Camera.main.transform.position = Vector3.Lerp(startingPos, initialCameraPosition, elapsedTime / transitionDuration);
            Camera.main.transform.rotation = Quaternion.Lerp(startingRot, initialCameraRotation, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = initialCameraPosition;
        Camera.main.transform.rotation = initialCameraRotation;
    }

    // Méthode pour surligner l'objet
    private void HighlightObject(GameObject outlineObject, bool fromName)
    {
        if (outlineObject != null)
        {
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
    }

    // Méthode pour désélectionner l'objet actuellement surligné
    private void DeselectObject(GameObject objectToDeselect)
    {
        if (objectToDeselect != null)
        {
            Outline outline = objectToDeselect.GetComponent<Outline>();
            if (outline != null)
            {
                outline.enabled = false; // Désactiver l'outline
            }
        }
    }
}
