using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameObjectSequenceController : MonoBehaviour
{
    [Header("Boutons de la Séquence")]
    public GameObject[] sequenceButtons = new GameObject[7];

    [Header("Configuration")]
    public Color highlightColor = Color.yellow;
    public float highlightDuration = 1.0f;
    public UnityEvent onSequenceCompleted;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip sequenceCompletedSound;

    [Header("Debug")]
    public bool showDebugInfo = true;

    // Variables privées
    private int currentStep = 0;
    private List<Renderer> buttonRenderers = new List<Renderer>();
    private List<Material> buttonMaterials = new List<Material>();
    private List<Color> originalColors = new List<Color>();
    private bool tutorialActive = false;
    private List<GameObject> sequenceOrder = new List<GameObject>();
    private int currentPulseButtonIndex = -1;
    
    // Référence au système de formation du bras robotique
    private RoboticArmTraining roboticArmTraining;

    void Start()
    {
        if (onSequenceCompleted == null)
        {
            onSequenceCompleted = new UnityEvent();
        }

        // Trouver le système de formation du bras robotique
        roboticArmTraining = FindFirstObjectByType<RoboticArmTraining>();

        // Initialiser la séquence
        sequenceOrder.Clear();
        foreach (GameObject button in sequenceButtons)
        {
            if (button != null)
            {
                sequenceOrder.Add(button);
            }
        }

        // Initialiser les boutons
        for (int i = 0; i < sequenceButtons.Length; i++)
        {
            GameObject button = sequenceButtons[i];
            if (button != null)
            {
                InitializeButton(button, i);
            }
        }
    }

    private void InitializeButton(GameObject button, int index)
    {
        Renderer renderer = button.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Créer un matériau unique pour chaque bouton
            Material uniqueMaterial = new Material(renderer.sharedMaterial ?? new Material(Shader.Find("Standard")));
            renderer.material = uniqueMaterial;
            
            buttonRenderers.Add(renderer);
            buttonMaterials.Add(uniqueMaterial);
            originalColors.Add(uniqueMaterial.color);
        }

        // Ajouter ClickableButton si nécessaire
        ClickableButton clickable = button.GetComponent<ClickableButton>();
        if (clickable == null)
        {
            clickable = button.AddComponent<ClickableButton>();
        }
        clickable.sequenceController = this;

        // Ajouter Collider si nécessaire
        if (button.GetComponent<Collider>() == null)
        {
            button.AddComponent<BoxCollider>();
        }
    }

    public void StartTraining()
    {
        DebugLog("Tutoriel démarré");
        currentStep = 0;
        tutorialActive = true;
        HighlightNextButton();
    }

    // Méthode pour compatibilité avec l'ancien code
    public void StartTutorial()
    {
        StartTraining();
    }

    public void StopTutorial()
    {
        DebugLog("Tutoriel arrêté");
        tutorialActive = false;
        StopAllCoroutines();
        ResetAllButtonColors();
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

        if (currentStep >= sequenceOrder.Count)
        {
            return;
        }

        GameObject expectedButton = sequenceOrder[currentStep];

        if (clickedButton == expectedButton)
        {
            // Bonne action !
            StopAllCoroutines();
            DebugLog("Bouton correct!");
            PlaySound(correctSound);
            
            // COMMUNICATION AVEC LE SYSTÈME DE FORMATION DU BRAS ROBOTIQUE
            if (roboticArmTraining != null)
            {
                roboticArmTraining.OnObjectClicked(clickedButton.name);
            }

            // Gérer les interactions spéciales
            HandleSpecialButtonInteractions(clickedButton);

            currentStep++;

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
            // Mauvaise action - redémarrer
            DebugLog("Bouton incorrect! Redémarrage de la séquence.");
            PlaySound(wrongSound);
            
            // Notifier le système de formation du mauvais clic
            if (roboticArmTraining != null)
            {
                roboticArmTraining.OnObjectClicked(clickedButton.name);
            }
            
            // Effect visuel d'erreur
            StartCoroutine(ShowErrorEffect());
        }
    }

    private void HandleSpecialButtonInteractions(GameObject clickedButton)
    {
        // Interactions avec les composants spéciaux
        var switchCle = clickedButton.GetComponent<SwitchMoverCleDAcces>();
        if (switchCle != null)
        {
            switchCle.ToggleSwitch();
        }

        var switchCommutateur = clickedButton.GetComponent<SwitchMoverCommutateur>();
        if (switchCommutateur != null)
        {
            switchCommutateur.ToggleSwitch();
        }

        var removeKey = clickedButton.GetComponent<RemoveKeyOnClick>();
        if (removeKey != null)
        {
            removeKey.OnMouseDown();
        }

        var poigneeLOTO = clickedButton.GetComponent<PoigneeLOTO>();
        if (poigneeLOTO != null)
        {
            poigneeLOTO.TogglePosition();
        }

        var consignation = clickedButton.GetComponent<Consignation>();
        if (consignation != null)
        {
            consignation.OnMouseDown();
        }

        var porteRotation = clickedButton.GetComponent<PorteRotation>();
        if (porteRotation != null)
        {
            porteRotation.RotateDoor();
        }
    }

    private IEnumerator ShowErrorEffect()
    {
        // Simple effet de flash rouge
        for (int i = 0; i < 3; i++)
        {
            // Flash rouge sur tous les boutons
            foreach (var material in buttonMaterials)
            {
                material.color = Color.red;
            }
            yield return new WaitForSeconds(0.1f);
            
            // Retour aux couleurs normales
            ResetAllButtonColors();
            yield return new WaitForSeconds(0.1f);
        }
        
        // Redémarrer la séquence
        currentStep = 0;
        HighlightNextButton();
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
        for (int i = 0; i < buttonMaterials.Count && i < originalColors.Count; i++)
        {
            buttonMaterials[i].color = originalColors[i];
        }
    }

    private void HighlightNextButton()
    {
        if (currentStep < sequenceOrder.Count && currentStep < buttonMaterials.Count)
        {
            StopAllCoroutines();
            ResetAllButtonColors();
            StartCoroutine(PulseHighlight(currentStep));
            
            DebugLog("Mise en surbrillance du bouton " + currentStep + ": " + 
                    (sequenceOrder[currentStep] != null ? sequenceOrder[currentStep].name : "null"));
        }
    }
    
    private IEnumerator PulseHighlight(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= buttonMaterials.Count)
        {
            yield break;
        }
        
        Material buttonMaterial = buttonMaterials[buttonIndex];
        Color originalColor = originalColors[buttonIndex];
        currentPulseButtonIndex = buttonIndex;
        
        while (tutorialActive && currentPulseButtonIndex == buttonIndex)
        {
            // Fade vers la couleur de surbrillance
            float time = 0;
            float halfDuration = highlightDuration / 2;
            
            while (time < halfDuration && tutorialActive && currentPulseButtonIndex == buttonIndex)
            {
                time += Time.deltaTime;
                float t = time / halfDuration;
                buttonMaterial.color = Color.Lerp(originalColor, highlightColor, t);
                yield return null;
            }
            
            // Fade vers la couleur originale
            time = 0;
            while (time < halfDuration && tutorialActive && currentPulseButtonIndex == buttonIndex)
            {
                time += Time.deltaTime;
                float t = time / halfDuration;
                buttonMaterial.color = Color.Lerp(highlightColor, originalColor, t);
                yield return null;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
        
        if (currentPulseButtonIndex == buttonIndex)
        {
            buttonMaterial.color = originalColor;
            currentPulseButtonIndex = -1;
        }
    }

    private void SequenceCompleted()
    {
        PlaySound(sequenceCompletedSound);
        tutorialActive = false;
        
        if (onSequenceCompleted != null)
        {
            onSequenceCompleted.Invoke();
        }
    }
}