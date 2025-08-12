using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Newtonsoft.Json;

public class RoboticArmTraining : MonoBehaviour
{
    [Header("UI References")]
    public Canvas trainingCanvas;
    public GameObject trainingPanel;
    public Transform stepsList;
    public GameObject stepPrefab;
    public Button closeButton;
    
    [Header("Step Item Prefab Components")]
    public TextMeshProUGUI stepNumberText;
    public TextMeshProUGUI stepDescriptionText;
    public Image stepStatusImage;
    
    [Header("Training Configuration")]
    public List<TrainingStep> steps = new List<TrainingStep>();
    
    [Header("WiseTwin Integration")]
    public bool useWiseTwinData = true;
    private bool metadataLoaded = false;
    
    [System.Serializable]
    public class TrainingStep
    {
        public string description;
        public string targetObjectName; // Nom de l'objet à cliquer
        public bool completed = false;
        
        // Données supplémentaires pour l'extension future
        public string title;
        public string questionText;
        public string[] questionOptions;
        public int correctAnswer;
        public string feedback;
        public string incorrectFeedback;
    }
    
    private int currentStepIndex = 0;
    private bool trainingActive = false;
    private List<GameObject> stepUIElements = new List<GameObject>();
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
        // Trouver le contrôleur de séquence
        sequenceController = FindFirstObjectByType<GameObjectSequenceController>();
        
        // Cacher l'UI au début
        if (trainingPanel != null)
            trainingPanel.SetActive(false);
        
        // Charger les données selon la configuration
        if (useWiseTwinData)
        {
            LoadStepsFromWiseTwin();
        }
        else
        {
            InitializeSteps();
            SetupUI();
        }
    }
    
    private void LoadStepsFromWiseTwin()
    {
        Debug.Log("[RoboticArmTraining] Tentative de chargement depuis WiseTwin...");
        
        if (MetadataLoader.Instance == null)
        {
            Debug.LogWarning("[RoboticArmTraining] MetadataLoader.Instance non trouvé, retry dans 1s");
            Invoke(nameof(RetryLoadStepsFromWiseTwin), 1f);
            return;
        }
        
        if (!MetadataLoader.Instance.IsLoaded)
        {
            Debug.Log("[RoboticArmTraining] En attente du chargement des métadonnées...");
            MetadataLoader.Instance.OnMetadataLoaded += OnWiseTwinDataLoaded;
            MetadataLoader.Instance.OnLoadError += OnWiseTwinDataError;
            return;
        }
        
        ParseWiseTwinData();
    }
    
    private void RetryLoadStepsFromWiseTwin()
    {
        if (!metadataLoaded)
        {
            LoadStepsFromWiseTwin();
        }
    }
    
    private void OnWiseTwinDataLoaded(System.Collections.Generic.Dictionary<string, object> metadata)
    {
        Debug.Log("[RoboticArmTraining] Données WiseTwin reçues, parsing...");
        ParseWiseTwinData();
    }
    
    private void OnWiseTwinDataError(string error)
    {
        Debug.LogError($"[RoboticArmTraining] Erreur chargement WiseTwin: {error}");
        Debug.Log("[RoboticArmTraining] Fallback vers les données par défaut");
        InitializeSteps();
        SetupUI();
    }
    
    private void ParseWiseTwinData()
    {
        try
        {
            var unityData = MetadataLoader.Instance.GetUnityData();
            Debug.Log($"[RoboticArmTraining] Unity data keys: {(unityData != null ? string.Join(", ", unityData.Keys) : "null")}");
            
            if (unityData == null || unityData.Count == 0)
            {
                Debug.LogWarning("[RoboticArmTraining] Aucune donnée Unity trouvée, utilisation des données par défaut");
                InitializeSteps();
                SetupUI();
                return;
            }
            
            steps.Clear();
            
            // Créer une liste temporaire pour trier par ordre
            var tempSteps = new List<(TrainingStep step, int order)>();
            
            foreach (var kvp in unityData)
            {
                string objectId = kvp.Key;
                Debug.Log($"[RoboticArmTraining] Processing object: {objectId}");
                
                var objectData = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    JsonConvert.SerializeObject(kvp.Value));
                
                if (objectData.ContainsKey("step_info"))
                {
                    var stepInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        JsonConvert.SerializeObject(objectData["step_info"]));
                    
                    var step = new TrainingStep
                    {
                        title = stepInfo.ContainsKey("title") ? stepInfo["title"].ToString() : $"Étape {objectId}",
                        description = stepInfo.ContainsKey("description") ? stepInfo["description"].ToString() : "Instruction non définie",
                        targetObjectName = objectId
                    };
                    
                    // Charger les questions si disponibles
                    if (objectData.ContainsKey("question_1"))
                    {
                        var questionData = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                            JsonConvert.SerializeObject(objectData["question_1"]));
                        
                        step.questionText = questionData.ContainsKey("text") ? questionData["text"].ToString() : "";
                        step.feedback = questionData.ContainsKey("feedback") ? questionData["feedback"].ToString() : "";
                        step.incorrectFeedback = questionData.ContainsKey("incorrectFeedback") ? questionData["incorrectFeedback"].ToString() : "";
                        
                        if (questionData.ContainsKey("options"))
                        {
                            var optionsJson = JsonConvert.SerializeObject(questionData["options"]);
                            step.questionOptions = JsonConvert.DeserializeObject<string[]>(optionsJson);
                        }
                        
                        if (questionData.ContainsKey("correctAnswer"))
                        {
                            if (int.TryParse(questionData["correctAnswer"].ToString(), out int parsedCorrectAnswer))
                            {
                                step.correctAnswer = parsedCorrectAnswer;
                            }
                            else if (bool.TryParse(questionData["correctAnswer"].ToString(), out bool boolAnswer))
                            {
                                step.correctAnswer = boolAnswer ? 1 : 0;
                            }
                        }
                    }
                    
                    int order = 999;
                    if (stepInfo.ContainsKey("order"))
                    {
                        if (int.TryParse(stepInfo["order"].ToString(), out int parsedOrder))
                        {
                            order = parsedOrder;
                        }
                    }
                    tempSteps.Add((step, order));
                }
            }
            
            // Trier par ordre et ajouter à la liste finale
            var sortedSteps = tempSteps.OrderBy(x => x.order).ToList();
            foreach (var (step, _) in sortedSteps)
            {
                steps.Add(step);
            }
            
            metadataLoaded = true;
            SetupUI();
            
            Debug.Log($"[RoboticArmTraining] {steps.Count} étapes chargées depuis WiseTwin");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RoboticArmTraining] Erreur parsing données WiseTwin: {e.Message}");
            InitializeSteps();
            SetupUI();
        }
    }
    
    void InitializeSteps()
    {
        Debug.Log("[RoboticArmTraining] Chargement des étapes par défaut (hardcode)");
        // Définir les étapes de la formation LOTO
        steps.Clear();
        steps.Add(new TrainingStep { description = "Mettre le commutateur en manuel", targetObjectName = "commutateur" });
        steps.Add(new TrainingStep { description = "Faire une demande d'accès", targetObjectName = "demande-d-acces" });
        steps.Add(new TrainingStep { description = "Tourner la clé opérateur position 0", targetObjectName = "operateur-cle-acces-1" });
        steps.Add(new TrainingStep { description = "Enlever la clé et la garder", targetObjectName = "cle-1" });
        steps.Add(new TrainingStep { description = "Glisser la poignée pour ouvrir", targetObjectName = "poignee" });
        steps.Add(new TrainingStep { description = "S'identifier avec le badge", targetObjectName = "Lock" });
        steps.Add(new TrainingStep { description = "Ouvrir la porte et entrer", targetObjectName = "porte" });
    }
    
    void SetupUI()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseTraining);
        }
        
        CreateStepsUI();
    }
    
    void CreateStepsUI()
    {
        // Nettoyer les anciens éléments
        foreach (var element in stepUIElements)
        {
            if (element != null)
                Destroy(element);
        }
        stepUIElements.Clear();
        
        // Créer les éléments UI pour chaque étape
        for (int i = 0; i < steps.Count; i++)
        {
            GameObject stepElement = CreateStepElement(i);
            stepUIElements.Add(stepElement);
        }
        
        UpdateStepsDisplay();
    }
    
    GameObject CreateStepElement(int index)
    {
        GameObject stepElement;
        
        if (stepPrefab != null)
        {
            stepElement = Instantiate(stepPrefab, stepsList);
        }
        else
        {
            // Créer un élément simple
            stepElement = new GameObject($"Step_{index}");
            stepElement.transform.SetParent(stepsList);
            
            // Ajouter RectTransform
            RectTransform rect = stepElement.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(350, 50);
            
            // Background
            Image bg = stepElement.AddComponent<Image>();
            bg.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
            
            // Layout Element pour le scroll
            LayoutElement layout = stepElement.AddComponent<LayoutElement>();
            layout.minHeight = 50;
            
            // Numéro de l'étape
            GameObject numberObj = new GameObject("Number");
            numberObj.transform.SetParent(stepElement.transform);
            TextMeshProUGUI numberText = numberObj.AddComponent<TextMeshProUGUI>();
            numberText.text = (index + 1).ToString();
            numberText.fontSize = 16;
            numberText.color = Color.black;
            numberText.alignment = TextAlignmentOptions.Center;
            
            RectTransform numberRect = numberText.GetComponent<RectTransform>();
            numberRect.anchorMin = new Vector2(0, 0);
            numberRect.anchorMax = new Vector2(0.15f, 1);
            numberRect.offsetMin = Vector2.zero;
            numberRect.offsetMax = Vector2.zero;
            
            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(stepElement.transform);
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = steps[index].description;
            descText.fontSize = 14;
            descText.color = Color.black;
            
            RectTransform descRect = descText.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.15f, 0);
            descRect.anchorMax = new Vector2(0.85f, 1);
            descRect.offsetMin = new Vector2(5, 5);
            descRect.offsetMax = new Vector2(-5, -5);
            
            // Status (cercle coloré)
            GameObject statusObj = new GameObject("Status");
            statusObj.transform.SetParent(stepElement.transform);
            Image statusImage = statusObj.AddComponent<Image>();
            statusImage.color = Color.gray;
            
            RectTransform statusRect = statusImage.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.85f, 0.3f);
            statusRect.anchorMax = new Vector2(0.95f, 0.7f);
            statusRect.offsetMin = Vector2.zero;
            statusRect.offsetMax = Vector2.zero;
        }
        
        return stepElement;
    }
    
    public void StartTraining()
    {
        Debug.Log("Démarrage de la formation bras robotique");
        
        trainingActive = true;
        currentStepIndex = 0;
        
        // NOUVEAU: Désactiver le collider du parent pour permettre les clics sur les enfants
        ControllerRoboticArmTrigger trigger = FindFirstObjectByType<ControllerRoboticArmTrigger>();
        if (trigger != null)
        {
            Collider parentCollider = trigger.GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = false;
                Debug.Log("🔧 Collider parent désactivé pour permettre les clics sur les objets de séquence");
            }
        }
        
        // Réinitialiser toutes les étapes
        foreach (var step in steps)
        {
            step.completed = false;
        }
        
        // Afficher l'UI
        if (trainingPanel != null)
            trainingPanel.SetActive(true);
        
        UpdateStepsDisplay();
        
        // Démarrer la séquence Unity si disponible
        if (sequenceController != null)
        {
            sequenceController.StartTraining();
        }
    }
    
    public void OnObjectClicked(string objectName)
    {
        if (!trainingActive) return;
        
        Debug.Log($"Objet cliqué dans la formation : {objectName}");
        
        if (currentStepIndex < steps.Count)
        {
            TrainingStep currentStep = steps[currentStepIndex];
            
            if (currentStep.targetObjectName == objectName)
            {
                // Bonne action !
                Debug.Log($"✓ Étape {currentStepIndex + 1} validée : {currentStep.description}");
                
                currentStep.completed = true;
                currentStepIndex++;
                
                if (currentStepIndex >= steps.Count)
                {
                    CompleteTraining();
                }
                else
                {
                    UpdateStepsDisplay();
                }
            }
            else
            {
                // Mauvaise action - retour au début
                Debug.Log($"✗ Mauvaise action ! Attendu: {currentStep.targetObjectName}, reçu: {objectName}");
                RestartTraining();
            }
        }
    }
    
    void RestartTraining()
    {
        Debug.Log("Redémarrage de la formation");
        
        currentStepIndex = 0;
        
        // Réinitialiser toutes les étapes
        foreach (var step in steps)
        {
            step.completed = false;
        }
        
        UpdateStepsDisplay();
        
        // Redémarrer la séquence Unity
        if (sequenceController != null)
        {
            sequenceController.StopTutorial();
            sequenceController.StartTraining();
        }
    }
    
    void UpdateStepsDisplay()
    {
        for (int i = 0; i < stepUIElements.Count && i < steps.Count; i++)
        {
            GameObject stepElement = stepUIElements[i];
            TrainingStep step = steps[i];
            
            if (stepElement != null)
            {
                // Mettre à jour le background
                Image bg = stepElement.GetComponent<Image>();
                if (bg != null)
                {
                    if (step.completed)
                    {
                        bg.color = new Color(0.6f, 1f, 0.6f, 0.9f); // Vert
                    }
                    else if (i == currentStepIndex)
                    {
                        bg.color = new Color(1f, 1f, 0.6f, 0.9f); // Jaune
                    }
                    else
                    {
                        bg.color = new Color(0.9f, 0.9f, 0.9f, 0.8f); // Gris
                    }
                }
                
                // Mettre à jour le statut (cercle coloré)
                Transform statusTransform = stepElement.transform.Find("Status");
                if (statusTransform != null)
                {
                    Image statusImage = statusTransform.GetComponent<Image>();
                    if (statusImage != null)
                    {
                        if (step.completed)
                        {
                            statusImage.color = Color.green;
                        }
                        else if (i == currentStepIndex)
                        {
                            statusImage.color = Color.yellow;
                        }
                        else
                        {
                            statusImage.color = Color.gray;
                        }
                    }
                }
            }
        }
    }
    
    void CompleteTraining()
    {
        Debug.Log("🎉 Formation terminée avec succès !");
        
        trainingActive = false;
        UpdateStepsDisplay();
        
        // Arrêter la séquence Unity
        if (sequenceController != null)
        {
            sequenceController.StopTutorial();
        }
        
        // Optionnel : fermer l'UI après quelques secondes
        Invoke(nameof(CloseTraining), 3f);
    }
    
    public void CloseTraining()
    {
        Debug.Log("Fermeture de la formation");
        
        trainingActive = false;
        
        // NOUVEAU: Réactiver le collider du parent
        ControllerRoboticArmTrigger trigger = FindFirstObjectByType<ControllerRoboticArmTrigger>();
        if (trigger != null)
        {
            Collider parentCollider = trigger.GetComponent<Collider>();
            if (parentCollider != null)
            {
                parentCollider.enabled = true;
                Debug.Log("✅ Collider parent réactivé");
            }
        }
        
        if (trainingPanel != null)
            trainingPanel.SetActive(false);
        
        if (sequenceController != null)
        {
            sequenceController.StopTutorial();
        }
    }
    
    private void OnDestroy()
    {
        CancelInvoke();
        
        // Nettoyer les événements WiseTwin
        if (MetadataLoader.Instance != null)
        {
            MetadataLoader.Instance.OnMetadataLoaded -= OnWiseTwinDataLoaded;
            MetadataLoader.Instance.OnLoadError -= OnWiseTwinDataError;
        }
    }
}