using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Newtonsoft.Json;

public class SimpleTrainingUI : MonoBehaviour
{
    [Header("UI References")]
    public Canvas trainingCanvas;
    public GameObject trainingPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI currentStepText;
    public TextMeshProUGUI stepCounterText;
    public Button startButton;
    public Button restartButton;
    
    [Header("Steps List UI")]
    public Transform stepsListParent;
    public GameObject stepItemPrefab; // Prefab pour chaque étape dans la liste
    
    [Header("Training Steps")]
    public List<TrainingStep> trainingSteps = new List<TrainingStep>();
    
    private int currentStepIndex = 0;
    private bool trainingActive = false;
    private GameObjectSequenceController sequenceController;
    private List<GameObject> stepUIItems = new List<GameObject>();
    
    [Header("WiseTwin Integration")]
    public bool useWiseTwinData = true;
    private bool metadataLoaded = false;
    
    public static SimpleTrainingUI Instance { get; private set; }
    
    [System.Serializable]
    public class TrainingStep
    {
        public string title;
        public string instruction;
        public string objectName; // Nom de l'objet à cliquer dans la scène 3D
        public bool completed = false;
        
        // Données supplémentaires pour l'extension future
        public string questionText;
        public string[] questionOptions;
        public int correctAnswer;
        public string feedback;
        public string incorrectFeedback;
    }
    
    private void Awake()
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
    
    private void Start()
    {
        InitializeUI();
        
        sequenceController = FindFirstObjectByType<GameObjectSequenceController>();
        if (sequenceController == null)
        {
            Debug.LogWarning("GameObjectSequenceController non trouvé !");
        }
        
        // Charger les données selon la configuration
        if (useWiseTwinData)
        {
            LoadStepsFromWiseTwin();
        }
        else
        {
            LoadDefaultSteps();
            CreateStepsList();
        }
    }
    
    private void InitializeUI()
    {
        if (startButton != null)
            startButton.onClick.AddListener(StartTraining);
            
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartTraining);
        
        if (trainingPanel != null)
            trainingPanel.SetActive(false);
    }
    
    private void LoadStepsFromWiseTwin()
    {
        Debug.Log("[SimpleTrainingUI] Tentative de chargement depuis WiseTwin...");
        
        if (MetadataLoader.Instance == null)
        {
            Debug.LogWarning("[SimpleTrainingUI] MetadataLoader.Instance non trouvé, retry dans 1s");
            Invoke(nameof(RetryLoadStepsFromWiseTwin), 1f);
            return;
        }
        
        if (!MetadataLoader.Instance.IsLoaded)
        {
            Debug.Log("[SimpleTrainingUI] En attente du chargement des métadonnées...");
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
        Debug.Log("[SimpleTrainingUI] Données WiseTwin reçues, parsing...");
        ParseWiseTwinData();
    }
    
    private void OnWiseTwinDataError(string error)
    {
        Debug.LogError($"[SimpleTrainingUI] Erreur chargement WiseTwin: {error}");
        Debug.Log("[SimpleTrainingUI] Fallback vers les données par défaut");
        LoadDefaultSteps();
        CreateStepsList();
    }
    
    private void ParseWiseTwinData()
    {
        try
        {
            var unityData = MetadataLoader.Instance.GetUnityData();
            if (unityData == null || unityData.Count == 0)
            {
                Debug.LogWarning("[SimpleTrainingUI] Aucune donnée Unity trouvée, utilisation des données par défaut");
                LoadDefaultSteps();
                CreateStepsList();
                return;
            }
            
            trainingSteps.Clear();
            
            // Créer une liste temporaire pour trier par ordre
            var tempSteps = new List<(TrainingStep step, int order)>();
            
            foreach (var kvp in unityData)
            {
                string objectId = kvp.Key;
                var objectData = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                    JsonConvert.SerializeObject(kvp.Value));
                
                if (objectData.ContainsKey("step_info"))
                {
                    var stepInfo = JsonConvert.DeserializeObject<Dictionary<string, object>>(
                        JsonConvert.SerializeObject(objectData["step_info"]));
                    
                    var step = new TrainingStep
                    {
                        title = stepInfo.ContainsKey("title") ? stepInfo["title"].ToString() : $"Étape {objectId}",
                        instruction = stepInfo.ContainsKey("description") ? stepInfo["description"].ToString() : "Instruction non définie",
                        objectName = objectId
                    };
                    
                    int order = stepInfo.ContainsKey("order") ? int.Parse(stepInfo["order"].ToString()) : 999;
                    tempSteps.Add((step, order));
                }
            }
            
            // Trier par ordre et ajouter à la liste finale
            var sortedSteps = tempSteps.OrderBy(x => x.order).ToList();
            foreach (var (step, _) in sortedSteps)
            {
                trainingSteps.Add(step);
            }
            
            metadataLoaded = true;
            CreateStepsList();
            
            Debug.Log($"[SimpleTrainingUI] {trainingSteps.Count} étapes chargées depuis WiseTwin");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SimpleTrainingUI] Erreur parsing données WiseTwin: {e.Message}");
            LoadDefaultSteps();
            CreateStepsList();
        }
    }
    
    private void LoadDefaultSteps()
    {
        Debug.Log("[SimpleTrainingUI] Chargement des étapes par défaut (hardcode)");
        trainingSteps.Clear();
        trainingSteps.Add(new TrainingStep 
        { 
            title = "Étape 1", 
            instruction = "Je mets le commutateur en manuel",
            objectName = "commutateur"
        });
        trainingSteps.Add(new TrainingStep 
        { 
            title = "Étape 2", 
            instruction = "Je fais une demande d'accès et j'active le mode réglage",
            objectName = "demande-d-acces"
        });
        trainingSteps.Add(new TrainingStep 
        { 
            title = "Étape 3", 
            instruction = "Je tourne la clé opérateur et la mettre en position 0",
            objectName = "operateur-cle-acces-1"
        });
        trainingSteps.Add(new TrainingStep 
        { 
            title = "Étape 4", 
            instruction = "J'enlève la clé et la garde avec moi",
            objectName = "cle-1"
        });
        trainingSteps.Add(new TrainingStep 
        { 
            title = "Étape 5", 
            instruction = "Je fais glisser la poignée de la porte pour l'ouvrir et la bloquer",
            objectName = "poignee"
        });
        trainingSteps.Add(new TrainingStep 
        { 
            title = "Étape 6", 
            instruction = "J'identifie que j'entre dans la zone avec mon badge",
            objectName = "Lock"
        });
        trainingSteps.Add(new TrainingStep 
        { 
            title = "Étape 7", 
            instruction = "J'ouvre la porte et j'entre dans la zone",
            objectName = "porte"
        });
    }
    
    private void CreateStepsList()
    {
        // Nettoyer la liste existante
        foreach (GameObject item in stepUIItems)
        {
            if (item != null)
                Destroy(item);
        }
        stepUIItems.Clear();
        
        // Créer les éléments UI pour chaque étape
        for (int i = 0; i < trainingSteps.Count; i++)
        {
            GameObject stepItem = CreateStepItem(trainingSteps[i], i);
            stepUIItems.Add(stepItem);
        }
        
        UpdateStepsDisplay();
    }
    
    private GameObject CreateStepItem(TrainingStep step, int index)
    {
        GameObject stepItem;
        
        if (stepItemPrefab != null)
        {
            stepItem = Instantiate(stepItemPrefab, stepsListParent);
        }
        else
        {
            // Créer un élément simple si pas de prefab
            stepItem = new GameObject("Step_" + index);
            stepItem.transform.SetParent(stepsListParent);
            
            // Ajouter un composant Image pour le background
            Image bg = stepItem.AddComponent<Image>();
            bg.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
            
            // Ajouter un Layout Element
            LayoutElement layout = stepItem.AddComponent<LayoutElement>();
            layout.minHeight = 60f;
            
            // Créer le texte
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(stepItem.transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{step.title}: {step.instruction}";
            text.fontSize = 14f;
            text.color = Color.black;
            
            // Positionner le texte
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 5);
            textRect.offsetMax = new Vector2(-10, -5);
        }
        
        return stepItem;
    }
    
    public void StartTraining()
    {
        if (trainingSteps.Count == 0)
        {
            Debug.LogError("Aucune étape de formation définie !");
            return;
        }
        
        trainingActive = true;
        currentStepIndex = 0;
        
        // Réinitialiser toutes les étapes
        foreach (var step in trainingSteps)
        {
            step.completed = false;
        }
        
        if (trainingPanel != null)
            trainingPanel.SetActive(true);
        
        if (titleText != null)
            titleText.text = "Formation LOTO - Accès Zone Robot";
        
        UpdateCurrentStepDisplay();
        UpdateStepsDisplay();
        
        // Démarrer le tutoriel Unity
        if (sequenceController != null)
        {
            sequenceController.StartTraining();
        }
        
        Debug.Log("Formation démarrée");
    }
    
    public void RestartTraining()
    {
        Debug.Log("Redémarrage de la formation");
        
        // Arrêter le tutoriel actuel
        if (sequenceController != null)
        {
            sequenceController.StopTutorial();
        }
        
        // Redémarrer
        StartTraining();
    }
    
    public void OnObjectClicked(string objectName)
    {
        if (!trainingActive) return;
        
        Debug.Log($"Objet cliqué : {objectName}");
        
        if (currentStepIndex < trainingSteps.Count)
        {
            TrainingStep currentStep = trainingSteps[currentStepIndex];
            
            if (currentStep.objectName == objectName)
            {
                // Bonne action !
                Debug.Log($"Bonne action ! Étape {currentStepIndex + 1} validée");
                
                currentStep.completed = true;
                currentStepIndex++;
                
                if (currentStepIndex >= trainingSteps.Count)
                {
                    // Formation terminée
                    CompleteTraining();
                }
                else
                {
                    // Passer à l'étape suivante
                    UpdateCurrentStepDisplay();
                    UpdateStepsDisplay();
                }
            }
            else
            {
                // Mauvaise action - retour au début
                Debug.Log("Mauvaise action ! Retour au début");
                RestartTraining();
            }
        }
    }
    
    private void UpdateCurrentStepDisplay()
    {
        if (currentStepIndex < trainingSteps.Count)
        {
            TrainingStep currentStep = trainingSteps[currentStepIndex];
            
            if (currentStepText != null)
                currentStepText.text = $"<b>{currentStep.title}</b>\n{currentStep.instruction}";
            
            if (stepCounterText != null)
                stepCounterText.text = $"Étape {currentStepIndex + 1} / {trainingSteps.Count}";
        }
    }
    
    private void UpdateStepsDisplay()
    {
        for (int i = 0; i < stepUIItems.Count && i < trainingSteps.Count; i++)
        {
            GameObject stepItem = stepUIItems[i];
            TrainingStep step = trainingSteps[i];
            
            if (stepItem != null)
            {
                Image bg = stepItem.GetComponent<Image>();
                if (bg != null)
                {
                    if (step.completed)
                    {
                        // Étape terminée - vert
                        bg.color = new Color(0.6f, 1f, 0.6f, 0.8f);
                    }
                    else if (i == currentStepIndex)
                    {
                        // Étape actuelle - jaune
                        bg.color = new Color(1f, 1f, 0.6f, 0.8f);
                    }
                    else
                    {
                        // Étape non commencée - gris
                        bg.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                    }
                }
                
                // Mettre à jour le texte
                TextMeshProUGUI text = stepItem.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    string prefix = step.completed ? "✓ " : (i == currentStepIndex ? "► " : "");
                    text.text = $"{prefix}{step.title}: {step.instruction}";
                    text.color = step.completed ? Color.black : (i == currentStepIndex ? Color.black : Color.gray);
                }
            }
        }
    }
    
    private void CompleteTraining()
    {
        Debug.Log("Formation terminée avec succès !");
        
        trainingActive = false;
        
        if (currentStepText != null)
            currentStepText.text = "<b>Formation Terminée !</b>\nFélicitations ! Vous avez terminé la formation avec succès.";
        
        if (stepCounterText != null)
            stepCounterText.text = "TERMINÉ";
        
        UpdateStepsDisplay();
        
        // Arrêter le tutoriel Unity
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