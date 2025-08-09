using System.Collections;
using UnityEngine;

public class RemoveKeyOnClick : MonoBehaviour
{
    public float moveDistance = -1f; // Déplacement sur l'axe X
    public float moveDuration = 0.5f; // Durée du déplacement
    private bool isMoving = false; // Vérifie si l'objet est en mouvement
    private Vector3 initialPosition; // Position initiale de l'objet
    private Renderer objectRenderer; // Pour gérer la visibilité de l'objet
    private float visibilityThreshold = 0.1f; // Seuil de visibilité

    private void Start()
    {
        initialPosition = transform.position;
        objectRenderer = GetComponent<Renderer>(); // Récupère le Renderer de l'objet
    }

    private void Update()
    {
        // Vérifie si l'objet est proche de sa position initiale
        float distanceFromInitial = Vector3.Distance(transform.position, initialPosition);
        objectRenderer.enabled = (distanceFromInitial <= visibilityThreshold);
    }

    public void OnMouseDown()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveObject());
        }
    }

    private IEnumerator MoveObject()
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + new Vector3(moveDistance, 0, 0);
        float time = 0;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPos, time / moveDuration);
            yield return null;
        }

        transform.position = targetPos;
        isMoving = false;
    }

    public void ResetPosition()
    {
        if (!isMoving)
        {
            StartCoroutine(MoveBackToInitialPosition());
        }
    }

    private IEnumerator MoveBackToInitialPosition()
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float time = 0;

        while (time < moveDuration)
        {
            time += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, initialPosition, time / moveDuration);
            yield return null;
        }

        transform.position = initialPosition;
        isMoving = false;
    }
}