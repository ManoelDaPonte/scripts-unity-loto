using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using WiseTwin;

public class RoboticArmTraining : MonoBehaviour
{
    [Header("📋 UI References")]
    public Canvas trainingCanvas;
    public GameObject trainingPanel;
    public Transform stepsList;
    public Button closeButton;
    public Button restartButton;
    
    [Header("🎨 Enhanced UI (Optional)")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI stepTitleText;
    public TextMeshProUGUI stepDescriptionText;
    public TextMeshProUGUI stepCounterText;
    public TextMeshProUGUI safetyInfoText;
    public Slider progressBar;
    
    [Header("🔧 Dependencies")]
    public MetadataLoader metadataLoader;
    public TrainingCompletionNotifier completionNotifier;
    
    [System.Serializable]
    public class TrainingStep
    {
        public string stepId;
        public string title;
        public string description;
        public string targetObjectName;
        public string safetyWarning;
        public string safetyConsequences;
        public bool completed = false;
    }
    
    public List<TrainingStep> steps = new List<TrainingStep>();
    
    private int currentStepIndex = 0;
    private bool trainingActive = false;
    private GameObjectSequenceController sequenceController;
    
    public static RoboticArmTraining Instance { get; private set; }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        InitializeDependencies();
        InitializeDefaultSteps();
        LoadMetadata();
        SetupUI();
        
        sequenceController = FindFirstObjectByType<GameObjectSequenceController>();
        
        if (trainingPanel != null)
            trainingPanel.SetActive(false);
    }
    
    private void InitializeDependencies()
    {
        if (metadataLoader == null)
            metadataLoader = FindFirstObjectByType<MetadataLoader>();
            
        if (completionNotifier == null)
            completionNotifier = FindFirstObjectByType<TrainingCompletionNotifier>();
    }
    
    private void InitializeDefaultSteps()
    {
        steps.Clear();
        
        // Étapes de base (seront enrichies par les métadonnées)
        string[] stepIds = { "commutateur", "demande-d-acces", "operateur-cle-acces-1", "cle-1", "poignee", "Lock", "porte" };
        string[] defaultTitles = { 
            "Étape 1: Commutateur", "Étape 2: Demande d'accès", "Étape 3: Clé opérateur", 
            "Étape 4: Retrait clé", "Étape 5: Poignée LOTO", "Étape 6: Badge", "Étape 7: Ouverture porte" 
        };
        string[] defaultDescriptions = {
            "Mettre le commutateur en manuel", "Faire une demande d'accès", "Tourner la clé en position 0",
            "Enlever la clé", "Glisser la poignée LOTO", "Scanner le badge", "Ouvrir la porte"
        };
        
        for (int i = 0; i < stepIds.Length; i++)
        {
            steps.Add(new TrainingStep
            {
                stepId = stepIds[i],
                title = defaultTitles[i],
                description = defaultDescriptions[i],
                targetObjectName = stepIds[i]
            });
        }
    }
    
    private void LoadMetadata()
    {
        if (metadataLoader == null)
        {
            Debug.LogWarning("⚠️ Pas de MetadataLoader - utilisation des données par défaut");
            return;
        }
        
        if (metadataLoader.IsLoaded)
        {
            Debug.Log("📦 Métadonnées déjà chargées");
            EnhanceStepsWithMetadata();
        }
        else
        {
            Debug.Log("📦 En attente des métadonnées...");
            metadataLoader.OnMetadataLoaded += OnMetadataLoaded;
        }
    }
    
    private void OnMetadataLoaded(Dictionary<string, object> metadata)
    {
        Debug.Log($"📦 Métadonnées reçues: {metadata?.Count} éléments");
        EnhanceStepsWithMetadata();
    }
    
    private void EnhanceStepsWithMetadata()
    {
        if (metadataLoader == null || !metadataLoader.IsLoaded) return;
        
        var metadata = metadataLoader.GetMetadata();
        
        // Chercher dans la section "unity" ou directement à la racine
        Dictionary<string, object> stepsData = null;
        
        if (metadata.ContainsKey("unity"))
        {
            stepsData = metadata["unity"] as Dictionary<string, object>;
        }
        else
        {
            stepsData = metadata;
        }
        
        if (stepsData == null) return;
        
        foreach (var step in steps)
        {
            if (stepsData.ContainsKey(step.stepId))
            {
                LoadStepFromMetadata(step, stepsData[step.stepId] as Dictionary<string, object>);
            }
        }
        
        Debug.Log("✅ Étapes enrichies avec les métadonnées");
    }
    
    // private void LoadStepFromMetadata(TrainingStep step, Dictionary<string, object> data)
    // {
    //     if (data == null) return;
        
    //     // Charger les données simples
    //     if (data.ContainsKey("title"))
    //         step.title = data["title"].ToString();
            
    //     if (data.ContainsKey("description"))
    //         step.description = data["description"].ToString();
            
    //     if (data.ContainsKey("safety_warning"))
    //         step.safetyWarning = data["safety_warning"].ToString();
            
    //     if (data.ContainsKey("safety_consequences"))
    //         step.safetyConsequences = data["safety_consequences"].ToString();
    // }
    
    private void SetupUI()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseTraining);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartTraining);
            
        if (titleText != null)
            titleText.text = "Formation LOTO - Accès Zone Robot";
            
        if (progressBar != null)
        {
            progressBar.minValue = 0;
            progressBar.maxValue = 1;
            progressBar.value = 0;
        }
    }
    
    public void StartTraining()
    {
        Debug.Log("🎓 Démarrage de la formation");
        
        trainingActive = true;
        currentStepIndex = 0;
        
        // Réinitialiser
        foreach (var step in steps)
            step.completed = false;
        
        // Afficher l'UI
        if (trainingPanel != null)
            trainingPanel.SetActive(true);
        
        UpdateUI();
        
        // Démarrer la séquence
        if (sequenceController != null)
            sequenceController.StartTraining();
            
        // Désactiver le collider parent
        DisableParentCollider();
    }
    
    public void OnObjectClicked(string objectName)
    {
        if (!trainingActive || currentStepIndex >= steps.Count) return;
        
        TrainingStep currentStep = steps[currentStepIndex];
        
        if (currentStep.targetObjectName == objectName)
        {
            // Bonne action
            Debug.Log($"✅ Étape {currentStepIndex + 1} validée");
            
            currentStep.completed = true;
            currentStepIndex++;
            
            if (currentStepIndex >= steps.Count)
            {
                CompleteTraining();
            }
            else
            {
                UpdateUI();
            }
        }
        else
        {
            // Mauvaise action
            Debug.Log($"❌ Mauvaise action ! Redémarrage...");
            RestartTraining();
        }
    }
    
    private void UpdateUI()
    {
        if (currentStepIndex < steps.Count)
        {
            TrainingStep currentStep = steps[currentStepIndex];
            
            // Mettre à jour les textes
            if (stepTitleText != null)
                stepTitleText.text = currentStep.title;
                
            if (stepDescriptionText != null)
                stepDescriptionText.text = currentStep.description;
                
            if (stepCounterText != null)
                stepCounterText.text = $"Étape {currentStepIndex + 1} / {steps.Count}";
            
            // Informations de sécurité
            UpdateSafetyInfo(currentStep);
        }
        
        // Barre de progression
        if (progressBar != null && steps.Count > 0)
        {
            progressBar.value = (float)currentStepIndex / steps.Count;
        }
    }
    
    // private void UpdateSafetyInfo(TrainingStep step)
    // {
    //     if (safetyInfoText == null) return;
        
    //     string safetyText = "";
        
    //     if (!string.IsNullOrEmpty(step.safetyWarning))
    //         safetyText += $"⚠️ ATTENTION: {step.safetyWarning}\n";
            
    //     if (!string.IsNullOrEmpty(step.safetyConsequences))
    //         safetyText += $"🚨 RISQUES: {step.safetyConsequences}";
        
    //     safetyInfoText.text = safetyText;
    //     safetyInfoText.gameObject.SetActive(!string.IsNullOrEmpty(safetyText));
        
    //     Debug.Log($"🛡️ Safety info: {safetyText}");
    // }
    
    /// <summary>
    /// Réinitialise tous les objets de la séquence à leur état initial
    /// </summary>
    private void ResetAllObjectsToInitialState()
    {
        Debug.Log("🔄 Réinitialisation de tous les objets à leur état initial...");
        
        // Réinitialiser le commutateur
        var switchCommutateur = FindFirstObjectByType<SwitchMoverCommutateur>();
        if (switchCommutateur != null)
        {
            switchCommutateur.ResetSwitch();
            Debug.Log("✅ Commutateur réinitialisé");
        }
        
        // Réinitialiser la clé d'accès
        var switchCle = FindFirstObjectByType<SwitchMoverCleDAcces>();
        if (switchCle != null)
        {
            switchCle.ResetSwitch();
            Debug.Log("✅ Clé d'accès réinitialisée");
        }
        
        // Réinitialiser la clé (RemoveKeyOnClick) - la remettre en position visible
        var removeKey = FindFirstObjectByType<RemoveKeyOnClick>();
        if (removeKey != null)
        {
            removeKey.ResetPosition();
            Debug.Log("✅ Clé réinitialisée");
        }
        
        // Réinitialiser la poignée LOTO
        var poigneeLOTO = FindFirstObjectByType<PoigneeLOTO>();
        if (poigneeLOTO != null)
        {
            poigneeLOTO.ResetPoignee();
            Debug.Log("✅ Poignée LOTO réinitialisée");
        }
        
        // Réinitialiser la porte
        var porteRotation = FindFirstObjectByType<PorteRotation>();
        if (porteRotation != null)
        {
            porteRotation.ResetRotation();
            Debug.Log("✅ Porte réinitialisée");
        }
        
        // Réinitialiser le badge/consignation si nécessaire
        var consignation = FindFirstObjectByType<Consignation>();
        if (consignation != null && consignation.GetComponent<Renderer>() != null)
        {
            // Si le composant Consignation a une méthode Reset, l'appeler
            // Sinon, s'assurer qu'il est visible
            consignation.GetComponent<Renderer>().enabled = true;
            Debug.Log("✅ Badge/Consignation réinitialisé");
        }
        
        Debug.Log("🎯 Réinitialisation terminée - tous les objets sont revenus à leur état initial");
    }
    
    public void RestartTraining()
    {
        currentStepIndex = 0;
        
        foreach (var step in steps)
            step.completed = false;
            
        // Réinitialiser tous les objets à leur état initial
        ResetAllObjectsToInitialState();
            
        UpdateUI();
        
        if (sequenceController != null)
        {
            sequenceController.StopTutorial();
            sequenceController.StartTraining();
        }
    }
    
    private void CompleteTraining()
    {
        Debug.Log("🎉 Formation terminée !");
        
        trainingActive = false;
        
        // UI de fin
        if (stepTitleText != null)
            stepTitleText.text = "🎉 Formation Terminée !";
            
        if (stepDescriptionText != null)
            stepDescriptionText.text = "Félicitations ! Formation LOTO réussie.";
            
        if (stepCounterText != null)
            stepCounterText.text = "TERMINÉ";
            
        if (progressBar != null)
            progressBar.value = 1.0f;
            
        if (safetyInfoText != null)
            safetyInfoText.gameObject.SetActive(false);
        
        // Notification de completion
        if (completionNotifier != null)
        {
            Debug.Log("📡 Envoi notification de fin...");
            completionNotifier.FormationCompleted();
        }
        
        if (sequenceController != null)
            sequenceController.StopTutorial();
            
        EnableParentCollider();
        
        // Fermeture automatique après 5 secondes
        Invoke(nameof(CloseTraining), 5f);
    }
    
    public void CloseTraining()
    {
        trainingActive = false;
        
        if (trainingPanel != null)
            trainingPanel.SetActive(false);
            
        if (sequenceController != null)
            sequenceController.StopTutorial();
            
        EnableParentCollider();
    }
    
    private void DisableParentCollider()
    {
        var trigger = FindFirstObjectByType<ControllerRoboticArmTrigger>();
        if (trigger != null)
        {
            var collider = trigger.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = false;
        }
    }
    
    private void EnableParentCollider()
    {
        var trigger = FindFirstObjectByType<ControllerRoboticArmTrigger>();
        if (trigger != null)
        {
            var collider = trigger.GetComponent<Collider>();
            if (collider != null)
                collider.enabled = true;
        }
    }
    
    // Debug helper
    [ContextMenu("🔍 Debug Current Step")]
    public void DebugCurrentStep()
    {
        if (currentStepIndex < steps.Count)
        {
            var step = steps[currentStepIndex];
            Debug.Log($"📊 Étape actuelle: {step.stepId}");
            Debug.Log($"📊 Titre: {step.title}");
            Debug.Log($"📊 Description: {step.description}");
            Debug.Log($"📊 Safety Warning: {step.safetyWarning}");
            Debug.Log($"📊 Safety Consequences: {step.safetyConsequences}");
        }
    }


    // 🔍 ÉTAPE 1: Ajoutez ces logs dans LoadStepFromMetadata pour voir ce qui arrive

private void LoadStepFromMetadata(TrainingStep step, Dictionary<string, object> data)
{
    if (data == null) 
    {
        Debug.LogWarning($"❌ Pas de données pour {step.stepId}");
        return;
    }
    
    Debug.Log($"🔍 === CHARGEMENT {step.stepId.ToUpper()} ===");
    Debug.Log($"🔍 Données disponibles: {string.Join(", ", data.Keys)}");
    
    // Charger les données simples
    if (data.ContainsKey("title"))
    {
        step.title = data["title"].ToString();
        Debug.Log($"✅ Titre chargé: {step.title}");
    }
        
    if (data.ContainsKey("description"))
    {
        step.description = data["description"].ToString();
        Debug.Log($"✅ Description chargée: {step.description}");
    }
        
    if (data.ContainsKey("safety_warning"))
    {
        step.safetyWarning = data["safety_warning"].ToString();
        Debug.Log($"🛡️ Safety Warning chargé: {step.safetyWarning}");
    }
    else
    {
        Debug.LogWarning($"❌ Pas de 'safety_warning' pour {step.stepId}");
    }
        
    if (data.ContainsKey("safety_consequences"))
    {
        step.safetyConsequences = data["safety_consequences"].ToString();
        Debug.Log($"🛡️ Safety Consequences chargé: {step.safetyConsequences}");
    }
    else
    {
        Debug.LogWarning($"❌ Pas de 'safety_consequences' pour {step.stepId}");
    }
    
    Debug.Log($"🔍 === FIN CHARGEMENT {step.stepId.ToUpper()} ===");
}

// 🔍 ÉTAPE 2: Modifiez UpdateSafetyInfo pour forcer l'affichage et débugger

private void UpdateSafetyInfo(TrainingStep step)
{
    Debug.Log($"🔍 === UPDATE SAFETY INFO ===");
    Debug.Log($"🔍 Step ID: {step.stepId}");
    Debug.Log($"🔍 SafetyInfoText null? {safetyInfoText == null}");
    
    if (safetyInfoText == null) 
    {
        Debug.LogError("❌ safetyInfoText est NULL ! Vérifiez l'assignation dans l'inspector !");
        return;
    }
    
    Debug.Log($"🔍 Safety Warning: '{step.safetyWarning}'");
    Debug.Log($"🔍 Safety Consequences: '{step.safetyConsequences}'");
    Debug.Log($"🔍 Warning vide? {string.IsNullOrEmpty(step.safetyWarning)}");
    Debug.Log($"🔍 Consequences vide? {string.IsNullOrEmpty(step.safetyConsequences)}");
    
    string safetyText = "";
    
    if (!string.IsNullOrEmpty(step.safetyWarning))
        safetyText += $"⚠️ ATTENTION: {step.safetyWarning}\n";
        
    if (!string.IsNullOrEmpty(step.safetyConsequences))
        safetyText += $"🚨 RISQUES: {step.safetyConsequences}";
    
    Debug.Log($"🔍 Texte final: '{safetyText}'");
    Debug.Log($"🔍 Texte vide? {string.IsNullOrEmpty(safetyText)}");
    
    // FORCER l'affichage pour test
    if (string.IsNullOrEmpty(safetyText))
    {
        safetyText = $"🧪 TEST FORCÉ - Étape: {step.stepId}\nWarning: '{step.safetyWarning}'\nConsequences: '{step.safetyConsequences}'";
        Debug.Log($"🔍 Texte de test forcé: {safetyText}");
    }
    
    safetyInfoText.text = safetyText;
    safetyInfoText.gameObject.SetActive(true); // FORCER l'activation
    
    Debug.Log($"🔍 GameObject actif? {safetyInfoText.gameObject.activeSelf}");
    Debug.Log($"🔍 Texte assigné dans le composant: '{safetyInfoText.text}'");
    Debug.Log($"🔍 === FIN UPDATE SAFETY INFO ===");
}

// 🔍 ÉTAPE 3: Ajoutez cette méthode pour debug les métadonnées brutes

[ContextMenu("🔍 Debug Metadata Raw")]
public void DebugMetadataRaw()
{
    if (metadataLoader == null)
    {
        Debug.LogError("❌ MetadataLoader null");
        return;
    }
    
    if (!metadataLoader.IsLoaded)
    {
        Debug.LogWarning("⚠️ Métadonnées pas chargées");
        return;
    }
    
    var metadata = metadataLoader.GetMetadata();
    Debug.Log($"📦 === METADATA RAW DEBUG ===");
    Debug.Log($"📦 Total keys: {metadata.Count}");
    
    foreach (var kvp in metadata)
    {
        Debug.Log($"📦 Key: {kvp.Key} | Type: {kvp.Value?.GetType()?.Name}");
    }
    
    // Test spécifique commutateur
    if (metadata.ContainsKey("unity"))
    {
        var unity = metadata["unity"] as Dictionary<string, object>;
        if (unity?.ContainsKey("commutateur") == true)
        {
            var comm = unity["commutateur"] as Dictionary<string, object>;
            Debug.Log($"📦 Commutateur keys: {string.Join(", ", comm.Keys)}");
            
            if (comm.ContainsKey("safety_warning"))
            {
                Debug.Log($"📦 Safety warning trouvé: {comm["safety_warning"]}");
            }
        }
    }
    else if (metadata.ContainsKey("commutateur"))
    {
        var comm = metadata["commutateur"] as Dictionary<string, object>;
        Debug.Log($"📦 Commutateur direct keys: {string.Join(", ", comm.Keys)}");
    }
    
    Debug.Log($"📦 === FIN METADATA RAW DEBUG ===");
}

// 🔍 ÉTAPE 4: Ajoutez cette méthode pour debug l'état actuel des steps

[ContextMenu("🔍 Debug All Steps")]
public void DebugAllSteps()
{
    Debug.Log($"🔍 === DEBUG TOUTES LES ÉTAPES ===");
    Debug.Log($"🔍 Nombre d'étapes: {steps.Count}");
    
    for (int i = 0; i < steps.Count; i++)
    {
        var step = steps[i];
        Debug.Log($"🔍 --- ÉTAPE {i} ---");
        Debug.Log($"🔍 ID: {step.stepId}");
        Debug.Log($"🔍 Titre: {step.title}");
        Debug.Log($"🔍 Description: {step.description}");
        Debug.Log($"🔍 Safety Warning: '{step.safetyWarning}'");
        Debug.Log($"🔍 Safety Consequences: '{step.safetyConsequences}'");
        Debug.Log($"🔍 Warning vide? {string.IsNullOrEmpty(step.safetyWarning)}");
        Debug.Log($"🔍 Consequences vide? {string.IsNullOrEmpty(step.safetyConsequences)}");
    }
    
    Debug.Log($"🔍 === FIN DEBUG ÉTAPES ===");
}
}

