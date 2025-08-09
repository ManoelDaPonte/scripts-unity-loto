using UnityEngine;

public class ObjectInformationBool : MonoBehaviour
{
    public bool isAlert; // Indique si l'objet est en alerte
    public bool isConnected; // Indique si l'objet est connecté
    public bool isBooked; // Indique si l'objet est réservé

    private GameObject warningSpriteInstance; // Instance du sprite de warning
    private Sprite warningSprite; // Sprite d'alerte chargé
    private LineRenderer lineRenderer; // Pour la ligne pointillée

    private Renderer objectRenderer; // Pour changer la couleur de l'objet
    private Color originalColor; // Couleur originale de l'objet

    private float warningYOffset = 10.0f; // Décalage vertical pour afficher le warning au-dessus de l'objet

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
        else
        {
            Debug.LogError("Renderer non trouvé sur l'objet: " + gameObject.name);
        }

        // Charger le sprite depuis Resources
        warningSprite = Resources.Load<Sprite>("png/warning-icon");

        if (warningSprite != null)
        {
            // Créer un nouvel objet pour afficher le sprite au-dessus de l'objet
            warningSpriteInstance = new GameObject("WarningSprite");
            SpriteRenderer sr = warningSpriteInstance.AddComponent<SpriteRenderer>();
            sr.sprite = warningSprite;

            // Réinitialiser la rotation pour éviter une inclinaison sur l'axe Y
            warningSpriteInstance.transform.rotation = Quaternion.identity;

            // Réduire la taille du sprite (diviser par 5)
            warningSpriteInstance.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            // Positionner le sprite
            Vector3 warningPosition = new Vector3(transform.position.x, transform.position.y + warningYOffset, transform.position.z);
            warningSpriteInstance.transform.position = warningPosition;
            warningSpriteInstance.transform.SetParent(transform); // Associer le sprite à cet objet
            warningSpriteInstance.SetActive(false); // Désactivé par défaut

            // Créer un LineRenderer pour la ligne pointillée
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.05f;
            lineRenderer.endWidth = 0.05f;
            lineRenderer.positionCount = 2; // Une ligne de deux points

            // Charger un matériau transparent rouge
            Material lineMaterial = new Material(Shader.Find("Standard"));
            lineMaterial.color = new Color(1f, 0f, 0f, 0.9f); // Rouge avec 90% de transparence
            lineMaterial.SetFloat("_Mode", 3); // Mode transparent
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.DisableKeyword("_ALPHATEST_ON");
            lineMaterial.EnableKeyword("_ALPHABLEND_ON");
            lineMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            lineMaterial.renderQueue = 3000;

            lineRenderer.material = lineMaterial;

            // Configuration de la ligne pour qu'elle soit pointillée
            lineRenderer.textureMode = LineTextureMode.Tile;
            lineRenderer.numCapVertices = 2;
            lineRenderer.startColor = new Color(1f, 0f, 0f, 0.2f); // Rouge transparent à 50%
            lineRenderer.endColor = new Color(1f, 0f, 0f, 0.2f);   // Rouge transparent à 50%
        }
        else
        {
            Debug.LogError("Warning sprite not found!");
        }
    }

    void Update()
    {
        if (warningSpriteInstance != null && lineRenderer != null)
        {
            // Mettre à jour la position de la ligne pour relier l'objet au sprite
            Vector3 warningPosition = warningSpriteInstance.transform.position;
            lineRenderer.SetPosition(0, transform.position); // Point de départ : position de l'objet
            lineRenderer.SetPosition(1, warningPosition);    // Point d'arrivée : position du sprite

            // Afficher ou cacher le sprite et la ligne en fonction de l'état d'alerte
            bool showWarning = isAlert;
            warningSpriteInstance.SetActive(showWarning);
            lineRenderer.enabled = showWarning; // Activer/désactiver la ligne selon l'état d'alerte

            // Faire face à la caméra sur l'axe Y
            if (showWarning)
            {
                Vector3 directionToCamera = Camera.main.transform.position - warningSpriteInstance.transform.position;
                directionToCamera.y = 0; // Ignorer la composante Y pour éviter l'inclinaison
                warningSpriteInstance.transform.rotation = Quaternion.LookRotation(directionToCamera);
            }
        }
    }

    // Méthode pour mettre à jour isAlert
    public void SetAlertState(int alertState)
    {
        isAlert = (alertState == 1);
        UpdateColor(); // Met à jour la couleur après le changement d'état
    }

    // Méthode pour changer l'état de réservation et la couleur en fonction de l'état de réservation
    public void ChangeBookedSeatState(bool booked)
    {
        isBooked = booked;
        UpdateColor(); // Met à jour la couleur après le changement d'état
    }

    // Méthode pour changer la couleur en fonction des booléens
    private void UpdateColor()
    {
        if (objectRenderer == null)
        {
            return; // Sortir si l'objet renderer est null
        }

        if (isBooked)
        {
            objectRenderer.material.color = Color.red; // Couleur pour un siège réservé
        }
        else if (isAlert)
        {
            objectRenderer.material.color = Color.red;
        }
        else if (isConnected)
        {
            objectRenderer.material.color = Color.blue;
        }
        else
        {
            objectRenderer.material.color = isBooked ? Color.red : Color.green; // Vert si non réservé
        }
    }
}
