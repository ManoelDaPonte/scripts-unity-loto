using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityFactorySceneHDRP;
using System.Runtime.InteropServices;
using UnityEngine.UI;


public class GameObjectSequenceController : MonoBehaviour
{
    // Ce qui nécessite une fonction native JavaScript
    [DllImport("__Internal")]
    private static extern void WebGLDispatchValidationEvent(string data);

    [Header("Boutons de la Séquence")]
    public GameObject[] sequenceButtons = new GameObject[7];

    [Header("Configuration")]
    public Color highlightColor = Color.yellow;
    public float highlightDuration = 1.0f;
    public UnityEvent onSequenceCompleted;
    public bool startAutomatically = false; // Option pour démarrer automatiquement

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip sequenceCompletedSound;

    [Header("Debug")]
    public bool showDebugInfo = true;

    [Header("Références aux Scripts")]
    public HighlightAndSend highlightAndSend; // Référence au script HighlightandSend

    // Variables privées
    private int currentStep = 0;
    private List<Renderer> buttonRenderers = new List<Renderer>();
    private List<Material> buttonMaterials = new List<Material>();
    private List<Color> originalColors = new List<Color>();
    private bool tutorialActive = false;
    private List<GameObject> sequenceOrder = new List<GameObject>();
    private int currentPulseButtonIndex = -1; // Pour suivre le bouton actuellement en train de pulser

    void Start()
    {
        // Initialiser l'UnityEvent s'il est null
        if (onSequenceCompleted == null)
        {
            onSequenceCompleted = new UnityEvent();
        }

        // Initialiser la séquence en ajoutant tous les objets de sequenceButtons dans sequenceOrder
        sequenceOrder.Clear();
        foreach (GameObject button in sequenceButtons)
        {
            if (button != null)
            {
                sequenceOrder.Add(button);
            }
        }

        // Assurer que la séquence est correcte
        if (sequenceOrder.Count != sequenceButtons.Length)
        {
            Debug.LogWarning("Il y a des éléments manquants dans la séquence.");
        }

        // Stockage des références aux renderers et couleurs originales des boutons
        for (int i = 0; i < sequenceButtons.Length; i++)
        {
            GameObject button = sequenceButtons[i];
            if (button != null)
            {
                InitializeButton(button, i);
            }
            else
            {
                Debug.LogWarning("L'emplacement " + i + " du tableau de boutons est vide!");
            }
        }

        // Vérifier la synchronisation entre sequenceOrder et buttonRenderers
        VerifySynchronization();

        // Démarrer automatiquement si cette option est activée
        if (startAutomatically)
        {
            DebugLog("Démarrage automatique du tutoriel");
            StartTutorial();
        }
    }

    // Vérifier que les listes sont bien synchronisées
    private void VerifySynchronization()
    {
        if (sequenceOrder.Count != buttonRenderers.Count)
        {
            Debug.LogError("Désynchronisation entre sequenceOrder (" + sequenceOrder.Count + 
                           ") et buttonRenderers (" + buttonRenderers.Count + ")");
            
            // Réinitialiser les listes pour éviter des problèmes
            buttonRenderers.Clear();
            buttonMaterials.Clear();
            originalColors.Clear();
            
            // Reconstruire les listes
            foreach (GameObject button in sequenceOrder)
            {
                if (button != null)
                {
                    Renderer renderer = button.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // Créer un nouveau matériau unique
                        Material uniqueMaterial = new Material(renderer.sharedMaterial);
                        renderer.material = uniqueMaterial;
                        
                        buttonRenderers.Add(renderer);
                        buttonMaterials.Add(uniqueMaterial);
                        originalColors.Add(uniqueMaterial.color);
                    }
                    else
                    {
                        Debug.LogError("Le bouton " + button.name + " n'a pas de composant Renderer!");
                    }
                }
            }
        }
    }

   private void InitializeButton(GameObject button, int index)
{
    Renderer renderer = button.GetComponent<Renderer>();
    if (renderer != null)
    {
        // Vérifier si le sharedMaterial est null
        if (renderer.sharedMaterial == null)
        {
            Debug.LogWarning("Le matériau de " + button.name + " est null, un matériau par défaut sera utilisé.");
            
            // Créer un matériau par défaut si nécessaire
            Material defaultMaterial = new Material(Shader.Find("Standard"));
            renderer.material = defaultMaterial;
            buttonRenderers.Add(renderer);
            buttonMaterials.Add(defaultMaterial);
            originalColors.Add(defaultMaterial.color);
        }
        else
        {
            // Créer un nouveau matériau à partir de sharedMaterial
            Material uniqueMaterial = new Material(renderer.sharedMaterial);
            renderer.material = uniqueMaterial;
            buttonRenderers.Add(renderer);
            buttonMaterials.Add(uniqueMaterial);
            originalColors.Add(uniqueMaterial.color);
        }

        // Vérifier le composant ClickableButton
        ClickableButton clickable = button.GetComponent<ClickableButton>();
        if (clickable == null)
        {
            DebugLog("Ajout automatique de ClickableButton à " + button.name);
            clickable = button.AddComponent<ClickableButton>();
        }

        // Assigner ce contrôleur au bouton
        clickable.sequenceController = this;

        // Vérifier le collider
        if (button.GetComponent<Collider>() == null)
        {
            DebugLog("Ajout automatique d'un BoxCollider à " + button.name);
            button.AddComponent<BoxCollider>();
        }
    }
    else
    {
        Debug.LogError("Le bouton " + button.name + " n'a pas de composant Renderer! L'initialisation échoue.");
        
        // Essayer de trouver un renderer dans les enfants
        Renderer childRenderer = button.GetComponentInChildren<Renderer>();
        if (childRenderer != null)
        {
            Debug.Log("Renderer trouvé dans un enfant de " + button.name + ". Utilisation de ce renderer.");
            
            Material uniqueMaterial = new Material(childRenderer.sharedMaterial);
            childRenderer.material = uniqueMaterial;

            buttonRenderers.Add(childRenderer);
            buttonMaterials.Add(uniqueMaterial);
            originalColors.Add(uniqueMaterial.color);
            
            // Vérifier le composant ClickableButton
            ClickableButton clickable = button.GetComponent<ClickableButton>();
            if (clickable == null)
            {
                DebugLog("Ajout automatique de ClickableButton à " + button.name);
                clickable = button.AddComponent<ClickableButton>();
            }

            // Assigner ce contrôleur au bouton
            clickable.sequenceController = this;

            // Vérifier le collider
            if (button.GetComponent<Collider>() == null)
            {
                DebugLog("Ajout automatique d'un BoxCollider à " + button.name);
                button.AddComponent<BoxCollider>();
            }
        }
        else
        {
            Debug.LogError("Aucun Renderer trouvé pour " + button.name + " ni dans ses enfants. Ce bouton ne clignotera pas.");
        }
    }
}


    // Démarrer le tutoriel
    public void StartTutorial()
    {
        DebugLog("Tutoriel démarré");
        currentStep = 0;
        tutorialActive = true;
        HighlightNextButton();
    }

    // Arrêter le tutoriel
    public void StopTutorial()
    {
        DebugLog("Tutoriel arrêté");
        tutorialActive = false;
        StopAllPulseCoroutines();
        ResetAllButtonColors();
    }

    // Arrêter spécifiquement les coroutines de pulsation
    private void StopAllPulseCoroutines()
    {
        StopAllCoroutines();
        currentPulseButtonIndex = -1;
    }

    public void OnButtonClicked(GameObject clickedButton)
{
    DebugLog("Bouton cliqué: " + clickedButton.name);

    if (!tutorialActive)
    {
        DebugLog("Tutoriel inactif, le clic est ignoré");
        return;
    }

    // Vérifier si nous avons atteint la fin de la séquence
    if (currentStep >= sequenceOrder.Count)
    {
        return;
    }

    // Obtenir le bouton attendu à l'étape actuelle
    GameObject expectedButton = sequenceOrder[currentStep];

    // Vérifier si c'est le bon bouton
    if (clickedButton == expectedButton)
    {
        // Code du cas correct inchangé
        StopAllPulseCoroutines();
        DebugLog("Bouton correct!");
        PlaySound(correctSound);
        currentStep++;

        if (highlightAndSend != null)
        {
            highlightAndSend.SendSelectedGameObject(clickedButton);
            SendValidationEvent(clickedButton.name);
        }
        else
        {
            SendValidationEvent(clickedButton.name);
        }

        // Gestion des interactions spéciales pour ce bouton
        HandleSpecialButtonInteractions(clickedButton);

        if (currentStep >= sequenceOrder.Count)
        {
            DebugLog("Séquence complétée!");
            SequenceCompleted();
            return;
        }

        HighlightNextButton();
    }
    else
    {
        // C'est un bouton incorrect - qu'il soit dans la séquence ou non
        DebugLog("Bouton incorrect ou inattendu! Réinitialisation de la séquence.");
        PlaySound(wrongSound);
        
        // Activer l'écran rouge pour n'importe quel objet incorrect
        StartCoroutine(FlashScreenRedAndReset());
        
        if (highlightAndSend != null)
        {
            highlightAndSend.SendSelectedGameObject(clickedButton);
        }
    }
}

    private IEnumerator ReHighlightAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HighlightNextButton();
    }

    private void HandleSpecialButtonInteractions(GameObject clickedButton)
    {
        // Vérifier si le bouton a un SwitchMover_CLE
        SwitchMoverCleDAcces switchCle = clickedButton.GetComponent<SwitchMoverCleDAcces>();
        if (switchCle != null)
        {
            DebugLog("SwitchMover_CLE détecté sur " + clickedButton.name);
            switchCle.ToggleSwitch();
        }

        // Vérifier si le bouton a un SwitchMover_Commutateur
        SwitchMoverCommutateur switchCommutateur = clickedButton.GetComponent<SwitchMoverCommutateur>();
        if (switchCommutateur != null)
        {
            DebugLog("SwitchMover_Commutateur détecté sur " + clickedButton.name);
            switchCommutateur.ToggleSwitch();
        }

        // Vérifier si le bouton a un RemoveKeyOnClick
        RemoveKeyOnClick removeKey = clickedButton.GetComponent<RemoveKeyOnClick>();
        if (removeKey != null)
        {
            DebugLog("RemoveKeyOnClick détecté sur " + clickedButton.name);
            removeKey.OnMouseDown(); // Déclenche le déplacement de la clé
        }

        // Exemple pour changer la direction du mouvement d'un objet pendant le tutoriel
        PoigneeLOTO poigneeLOTO = clickedButton.GetComponent<PoigneeLOTO>();
        if (poigneeLOTO != null)
        {
            poigneeLOTO.TogglePosition();  // Déplacer vers -1.0f selon Z
        }

        // Vérifie si l'objet cliqué est le cadenas
        Consignation consignation = clickedButton.GetComponent<Consignation>();
        if (consignation != null)
        {
            Debug.Log("Cadenas cliqué !");
            consignation.OnMouseDown(); // Appelle la fonction pour réinitialiser la poignée
        }

        // Vérifie si l'objet cliqué est la porte
        PorteRotation porteRotation = clickedButton.GetComponent<PorteRotation>();
        if (porteRotation != null)
        {
            Debug.Log("Porte cliquée !");
            porteRotation.RotateDoor(); // Appelle la fonction pour réinitialiser la poignée
        }
    }

    private void SendValidationEvent(string buttonName)
    {
        // Cette fonction envoie un événement JavaScript pour la validation d'une étape
        string json = "{\"name\":\"" + buttonName + "\",\"eventName\":\"" + buttonName + "\"}";

    #if UNITY_WEBGL && !UNITY_EDITOR
        // En WebGL, utilisez la fonction JavaScript pour dispatcher l'événement
        WebGLDispatchValidationEvent(json);
    #else
        // En éditeur, log simplement pour debug
        DebugLog("Envoi d'événement de validation: " + json);
    #endif
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void DebugLog(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log("[GameObjectSequence] " + message);
        }
    }

    private void ResetAllButtonColors()
    {
        for (int i = 0; i < buttonMaterials.Count; i++)
        {
            buttonMaterials[i].color = originalColors[i];
        }
    }

    // Méthode améliorée pour mettre en surbrillance le prochain bouton
    private void HighlightNextButton()
    {
        if (currentStep < sequenceOrder.Count && currentStep < buttonMaterials.Count)
        {
            // Arrêter toutes les animations de pulsation en cours
            StopAllPulseCoroutines();
            
            // Réinitialiser toutes les couleurs
            ResetAllButtonColors();
            
            // Démarrer la pulsation pour le bouton suivant
            StartCoroutine(PulseHighlight(currentStep));
            
            DebugLog("Mise en surbrillance du bouton " + currentStep + ": " + 
                    (sequenceOrder[currentStep] != null ? sequenceOrder[currentStep].name : "null"));
        }
        else
        {
            DebugLog("Impossible de mettre en surbrillance le bouton " + currentStep + 
                    " - hors limites ou séquence terminée");
        }
    }
    
    // Méthode améliorée pour l'animation de pulsation
    private IEnumerator PulseHighlight(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= buttonMaterials.Count)
        {
            Debug.LogError("Index de bouton invalide dans PulseHighlight: " + buttonIndex);
            yield break;
        }
        
        Material buttonMaterial = buttonMaterials[buttonIndex];
        Color originalColor = originalColors[buttonIndex];
        currentPulseButtonIndex = buttonIndex;
        
        // Couleur de surbrillance plus vive
        Color pulseColor = new Color(
            highlightColor.r, 
            highlightColor.g, 
            highlightColor.b, 
            highlightColor.a
        );
        
        // Animation de pulsation continue
        while (tutorialActive && currentPulseButtonIndex == buttonIndex)
        {
            // Passage progressif à la couleur de surbrillance
            float time = 0;
            float halfDuration = highlightDuration / 2;
            
            while (time < halfDuration && tutorialActive && currentPulseButtonIndex == buttonIndex)
            {
                time += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, time / halfDuration); // Animation plus fluide
                buttonMaterial.color = Color.Lerp(originalColor, pulseColor, t);
                yield return null;
            }
            
            // Passage progressif à la couleur originale
            time = 0;
            while (time < halfDuration && tutorialActive && currentPulseButtonIndex == buttonIndex)
            {
                time += Time.deltaTime;
                float t = Mathf.SmoothStep(0, 1, time / halfDuration); // Animation plus fluide
                buttonMaterial.color = Color.Lerp(pulseColor, originalColor, t);
                yield return null;
            }
            
            // Petite pause avant la prochaine pulsation
            yield return new WaitForSeconds(0.1f);
        }
        
        // S'assurer que la couleur est réinitialisée à la fin si ce bouton est toujours le bouton en cours
        if (currentPulseButtonIndex == buttonIndex)
        {
            buttonMaterial.color = originalColor;
            currentPulseButtonIndex = -1;
        }
    }

    private void IncorrectButtonPressed(int buttonIndex)
    {
        // Toujours démarrer le clignotement de l'écran rouge
        StartCoroutine(FlashScreenRedAndReset());

        // Faire clignoter le bouton uniquement s'il est dans la séquence
        if (buttonIndex >= 0 && buttonIndex < buttonMaterials.Count)
        {
            StartCoroutine(FlashButtonRed(buttonIndex));
        }

        // Envoyer le bouton à highlightAndSend uniquement s'il est dans la séquence
        if (highlightAndSend != null && buttonIndex >= 0 && buttonIndex < sequenceButtons.Length)
        {
            GameObject button = sequenceButtons[buttonIndex];
            if (button != null)
            {
                highlightAndSend.SendSelectedGameObject(button);
            }
        }
    }
    
    // Nouvelle méthode qui combine le flash rouge et la réinitialisation
    private IEnumerator FlashScreenRedAndReset()
    {
        // Partie 1: Flash de l'écran rouge
        // Trouver ou créer un Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject canvasObject = null;
        bool createdNewCanvas = false;
        
        if (canvas == null)
        {
            DebugLog("Aucun Canvas trouvé, création d'un nouveau Canvas pour l'effet de flash");
            canvasObject = new GameObject("ScreenFlashCanvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            createdNewCanvas = true;
        }
        
        // Créer l'overlay rouge
        GameObject screenFlash = new GameObject("ScreenFlash");
        screenFlash.transform.SetParent(canvas.transform, false);
        
        Image flashImage = screenFlash.AddComponent<Image>();
        flashImage.color = new Color(1f, 0f, 0f, 0.5f);
        
        RectTransform rectTransform = flashImage.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Animation de clignotement
        for (int i = 0; i < 3; i++)
        {
            flashImage.color = new Color(1f, 0f, 0f, 0.5f);
            yield return new WaitForSeconds(0.1f);
            flashImage.color = new Color(1f, 0f, 0f, 0f);
            yield return new WaitForSeconds(0.1f);
        }
        
        // Partie 2: Réinitialisation de tous les objets de la séquence
        DebugLog("Réinitialisation de tous les objets modifiés dans la séquence");
        
        // Réinitialiser les objets avec SwitchMoverCleDAcces
        ResetAllComponentsOfType<SwitchMoverCleDAcces>();
        
        // Réinitialiser les objets avec SwitchMoverCommutateur
        ResetAllComponentsOfType<SwitchMoverCommutateur>();
        
        // Réinitialiser les objets avec RemoveKeyOnClick
        ResetAllComponentsOfType<RemoveKeyOnClick>();
        
        // Réinitialiser les objets avec PoigneeLOTO
        ResetAllComponentsOfType<PoigneeLOTO>();
        
        // Réinitialiser les objets avec CadenasController
        ResetAllComponentsOfType<PorteRotation>();
        
        // Réinitialiser le compteur d'étapes
        currentStep = 0;
        
        // Redémarrer la surbrillance du premier bouton
        HighlightNextButton();
        
        // Destruction garantie de l'image flash
        if (screenFlash != null)
        {
            DebugLog("Destruction de l'overlay de flash");
            Destroy(screenFlash);
        }
        
        // Si nous avons créé un nouveau Canvas, le détruire également
        if (createdNewCanvas && canvasObject != null)
        {
            DebugLog("Destruction du Canvas créé dynamiquement");
            Destroy(canvasObject);
        }
        
        DebugLog("Séquence réinitialisée et prête à recommencer");
    }

    // Méthode générique pour réinitialiser tous les composants d'un type spécifique
    private void ResetAllComponentsOfType<T>() where T : MonoBehaviour
    {
        T[] components = FindObjectsOfType<T>();
        foreach (T component in components)
        {
            DebugLog("Réinitialisation de " + component.gameObject.name + " (" + typeof(T).Name + ")");
            
            if (component is SwitchMoverCleDAcces switchCle)
            {
                // Réinitialiser à l'état initial (vous pourriez avoir besoin d'ajouter une méthode Reset() dans cette classe)
                switchCle.ResetSwitch();
            }
            else if (component is SwitchMoverCommutateur switchCommutateur)
            {
                switchCommutateur.ResetSwitch();
            }
            else if (component is RemoveKeyOnClick removeKey)
            {
                removeKey.ResetPosition();
            }
            else if (component is PoigneeLOTO poigneeLOTO)
            {
                poigneeLOTO.ResetPoignee();
            }
            else if (component is PorteRotation porteRotation)
            {
                porteRotation.ResetRotation();
            }
        }
    }


    // Faire clignoter le bouton en rouge pour indiquer une erreur
    private IEnumerator FlashButtonRed(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= buttonMaterials.Count)
        {
            Debug.LogError("Index de bouton invalide dans FlashButtonRed: " + buttonIndex);
            yield break;
        }

        Material buttonMaterial = buttonMaterials[buttonIndex];
        Color errorColor = Color.red;
        Color originalColor = originalColors[buttonIndex];

        // Faire clignoter en rouge plusieurs fois
        for (int i = 0; i < 3; i++)
        {
            buttonMaterial.color = errorColor;
            yield return new WaitForSeconds(0.1f);
            buttonMaterial.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }

    
    // Séquence complétée avec succès
    private void SequenceCompleted()
    {
        // Jouer le son de la fin de la séquence
        PlaySound(sequenceCompletedSound);

        // Animation de réussite
        StartCoroutine(SuccessAnimation());

        // Déclencher l'événement de complétion
        if (onSequenceCompleted != null)
        {
            DebugLog("Déclenchement de l'événement onSequenceCompleted");
            onSequenceCompleted.Invoke();
        }

        tutorialActive = false;
    }

    // Animation améliorée lors de la réussite de la séquence
    private IEnumerator SuccessAnimation()
    {
        Color successColor = Color.green;
        
        // Effet de vague - les boutons s'allument un par un
        for (int i = 0; i < sequenceOrder.Count; i++)
        {
            // Allumer le bouton actuel
            if (i < buttonMaterials.Count)
            {
                buttonMaterials[i].color = successColor;
                
                // Activer l'émission si disponible
                if (buttonMaterials[i].HasProperty("_EmissionColor"))
                {
                    buttonMaterials[i].EnableKeyword("_EMISSION");
                    buttonMaterials[i].SetColor("_EmissionColor", successColor * 2);
                }
                
                // // Effet de mise à l'échelle
                // if (i < sequenceButtons.Length && sequenceButtons[i] != null)
                // {
                //     Vector3 originalScale = sequenceButtons[i].transform.localScale;
                    
                //     // Animation de pulsation rapide
                //     float time = 0;
                //     while (time < 0.3f)
                //     {
                //         time += Time.deltaTime;
                //         float scale = 1 + 0.2f * Mathf.Sin(time * 20);
                //         sequenceButtons[i].transform.localScale = originalScale * scale;
                //         yield return null;
                //     }
                    
                //     sequenceButtons[i].transform.localScale = originalScale;
                // }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        // Effet de flash final
        for (int flash = 0; flash < 3; flash++)
        {
            // Tous les boutons en vert brillant
            for (int i = 0; i < buttonMaterials.Count; i++)
            {
                buttonMaterials[i].color = successColor * 1.2f;
                
                if (buttonMaterials[i].HasProperty("_EmissionColor"))
                {
                    buttonMaterials[i].SetColor("_EmissionColor", successColor * 3);
                }
            }
            
            yield return new WaitForSeconds(0.2f);
            
            // Boutons plus foncés
            for (int i = 0; i < buttonMaterials.Count; i++)
            {
                buttonMaterials[i].color = successColor * 0.8f;
                
                if (buttonMaterials[i].HasProperty("_EmissionColor"))
                {
                    buttonMaterials[i].SetColor("_EmissionColor", successColor);
                }
            }
            
            yield return new WaitForSeconds(0.2f);
        }
        
        // Retour progressif aux couleurs d'origine
        float fadeTime = 0;
        float fadeDuration = 1.0f;
        
        while (fadeTime < fadeDuration)
        {
            fadeTime += Time.deltaTime;
            float t = fadeTime / fadeDuration;
            
            for (int i = 0; i < buttonMaterials.Count; i++)
            {
                if (i < originalColors.Count)
                {
                    buttonMaterials[i].color = Color.Lerp(successColor, originalColors[i], t);
                    
                    if (buttonMaterials[i].HasProperty("_EmissionColor"))
                    {
                        buttonMaterials[i].SetColor("_EmissionColor", Color.Lerp(successColor, Color.black, t));
                    }
                }
            }
            
            yield return null;
        }
        
        // Désactiver l'émission
        for (int i = 0; i < buttonMaterials.Count; i++)
        {
            if (buttonMaterials[i].HasProperty("_EmissionColor"))
            {
                buttonMaterials[i].SetColor("_EmissionColor", Color.black);
                buttonMaterials[i].DisableKeyword("_EMISSION");
            }
        }
        
        // Réinitialiser les couleurs
        ResetAllButtonColors();
    }
}