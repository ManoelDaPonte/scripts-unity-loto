using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TrainingCompletionNotifier : MonoBehaviour
{
    [Header("📋 Projet")]
    [SerializeField, Tooltip("Nom du projet Unity (automatique)")]
    private string projectName;
    
    [Header("API Configuration")]
    public string apiBaseURL = "https://your-api-domain.com/api";
    
    [Header("Debug Settings")]
    public bool enableDebugLogs = true;
    public bool simulateAPICall = false; // Pour tester sans vraie API
    
    [Header("Request Settings")]
    public float requestTimeout = 30f;
    public int maxRetryAttempts = 3;
    
    private int currentRetryAttempt = 0;
    
    void Start()
    {
        InitializeProjectName();
    }
    
    void InitializeProjectName()
    {
        projectName = Application.productName;
        if (string.IsNullOrEmpty(projectName))
        {
            projectName = "unity-project";
        }
        
        if (enableDebugLogs)
        {
            Debug.Log($"📋 Projet: {projectName}");
        }
    }
    
    // Propriété publique pour accéder au nom du projet
    public string ProjectName => projectName;
    
    /// <summary>
    /// Fonction principale pour notifier la completion d'une formation
    /// </summary>
    /// <param name="customProjectName">Nom de projet optionnel pour override le nom par défaut</param>
    public void NotifyTrainingCompleted(string customProjectName = null)
    {
        string finalProjectName = string.IsNullOrEmpty(customProjectName) ? projectName : customProjectName;
        
        if (enableDebugLogs)
        {
            Debug.Log($"🎓 Notification de fin de formation pour: {finalProjectName}");
        }
        
        if (simulateAPICall)
        {
            SimulateAPIResponse(finalProjectName);
        }
        else
        {
            StartCoroutine(SendCompletionNotification(finalProjectName));
        }
    }
    
    /// <summary>
    /// Envoi de la notification à l'API
    /// </summary>
    private IEnumerator SendCompletionNotification(string trainingProjectName)
    {
        currentRetryAttempt = 0;
        
        while (currentRetryAttempt < maxRetryAttempts)
        {
            if (enableDebugLogs)
            {
                Debug.Log($"📡 Tentative {currentRetryAttempt + 1}/{maxRetryAttempts} d'envoi pour {trainingProjectName}");
            }
            
            // Construire l'URL de l'endpoint
            string endpoint = $"{apiBaseURL}/training/completed";
            
            // Créer le payload JSON
            TrainingCompletionData data = new TrainingCompletionData
            {
                projectName = trainingProjectName,
                completedAt = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
                sessionId = System.Guid.NewGuid().ToString(),
                status = "completed"
            };
            
            string jsonPayload = JsonUtility.ToJson(data);
            
            if (enableDebugLogs)
            {
                Debug.Log($"📤 Envoi vers: {endpoint}");
                Debug.Log($"📦 Payload: {jsonPayload}");
            }
            
            // Créer la requête POST
            using (UnityWebRequest request = UnityWebRequest.Post(endpoint, jsonPayload, "application/json"))
            {
                request.timeout = (int)requestTimeout;
                
                // Ajouter des headers si nécessaire
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("User-Agent", "Unity-Training-App");
                
                // Envoyer la requête
                yield return request.SendWebRequest();
                
                // Vérifier le résultat
                if (request.result == UnityWebRequest.Result.Success)
                {
                    if (enableDebugLogs)
                    {
                        Debug.Log($"✅ Formation terminée notifiée avec succès!");
                        Debug.Log($"📥 Réponse: {request.downloadHandler.text}");
                    }
                    
                    OnNotificationSuccess(trainingProjectName, request.downloadHandler.text);
                    yield break; // Sortir de la boucle de retry
                }
                else
                {
                    if (enableDebugLogs)
                    {
                        Debug.LogWarning($"⚠️ Erreur lors de l'envoi (tentative {currentRetryAttempt + 1}):");
                        Debug.LogWarning($"   Status: {request.responseCode}");
                        Debug.LogWarning($"   Error: {request.error}");
                        Debug.LogWarning($"   Response: {request.downloadHandler?.text}");
                    }
                    
                    currentRetryAttempt++;
                    
                    if (currentRetryAttempt < maxRetryAttempts)
                    {
                        if (enableDebugLogs)
                        {
                            Debug.Log($"🔄 Nouvelle tentative dans 2 secondes...");
                        }
                        yield return new WaitForSeconds(2f);
                    }
                    else
                    {
                        OnNotificationFailed(trainingProjectName, request.error);
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Simulation d'appel API pour les tests
    /// </summary>
    private void SimulateAPIResponse(string trainingProjectName)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"🧪 SIMULATION: Formation {trainingProjectName} terminée notifiée");
        }
        
        // Simuler un délai réseau
        StartCoroutine(SimulateDelay(trainingProjectName));
    }
    
    private IEnumerator SimulateDelay(string trainingProjectName)
    {
        yield return new WaitForSeconds(1f);
        
        string fakeResponse = $"{{\"success\": true, \"projectName\": \"{trainingProjectName}\", \"timestamp\": \"{System.DateTime.UtcNow}\"}}";
        OnNotificationSuccess(trainingProjectName, fakeResponse);
    }
    
    /// <summary>
    /// Callback en cas de succès
    /// </summary>
    private void OnNotificationSuccess(string projectName, string response)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"🎉 Notification réussie pour {projectName}");
        }
        
        // Ici vous pouvez ajouter d'autres actions à effectuer en cas de succès
        // Par exemple: sauvegarder localement, afficher un message, etc.
    }
    
    /// <summary>
    /// Callback en cas d'échec
    /// </summary>
    private void OnNotificationFailed(string projectName, string error)
    {
        Debug.LogError($"❌ Échec de notification pour {projectName}: {error}");
        
        // Ici vous pouvez ajouter une logique de fallback
        // Par exemple: sauvegarder en local pour retry plus tard
        SaveFailedNotificationLocally(projectName);
    }
    
    /// <summary>
    /// Sauvegarde locale en cas d'échec (optionnel)
    /// </summary>
    private void SaveFailedNotificationLocally(string projectName)
    {
        string key = $"failed_notification_{projectName}_{System.DateTime.UtcNow.Ticks}";
        PlayerPrefs.SetString(key, projectName);
        PlayerPrefs.Save();
        
        if (enableDebugLogs)
        {
            Debug.Log($"💾 Notification sauvegardée localement: {key}");
        }
    }
    
    /// <summary>
    /// Méthodes publiques pour faciliter l'intégration
    /// </summary>
    
    [ContextMenu("Test Notification")]
    public void TestNotification()
    {
        NotifyTrainingCompleted();
    }
    
    public void NotifyCurrentProjectCompleted()
    {
        NotifyTrainingCompleted();
    }
    
    public void NotifyCustomProjectCompleted(string customProjectName)
    {
        NotifyTrainingCompleted(customProjectName);
    }
    
    /// <summary>
    /// Structure des données à envoyer
    /// </summary>
    [System.Serializable]
    public class TrainingCompletionData
    {
        public string projectName;
        public string completedAt;
        public string sessionId;
        public string status;
    }
    
    /// <summary>
    /// Interface pour les tests
    /// </summary>
    void OnGUI()
    {
        if (!enableDebugLogs) return;
        
        GUILayout.BeginArea(new Rect(10, 500, 400, 200));
        GUILayout.Label("🎓 Training Completion Notifier");
        GUILayout.Label($"Projet: {projectName}");
        GUILayout.Label($"API URL: {apiBaseURL}");
        GUILayout.Label($"Simulation: {(simulateAPICall ? "ON" : "OFF")}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("🧪 Test Notification"))
        {
            TestNotification();
        }
        
        if (GUILayout.Button($"🔄 Toggle Simulation ({(simulateAPICall ? "OFF" : "ON")})"))
        {
            simulateAPICall = !simulateAPICall;
        }
        
        GUILayout.EndArea();
    }
}