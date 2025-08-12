using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoboticArmUICreator : MonoBehaviour
{
    [Header("UI Creation")]
    [SerializeField] private bool autoCreateOnStart = false;
    
    [Header("UI Settings")]
    [SerializeField] private Vector2 panelSize = new Vector2(400, 600);
    [SerializeField] private Vector2 panelPosition = new Vector2(-220, 0); // Position depuis le bord droit
    
    void Start()
    {
        if (autoCreateOnStart)
        {
            CreateRoboticArmUI();
        }
    }
    
    [ContextMenu("Create Robotic Arm UI")]
    public void CreateRoboticArmUI()
    {
        Debug.Log("🎨 Création automatique de l'UI pour le bras robotique...");
        
        // 1. Créer le Canvas principal
        GameObject canvasGO = CreateCanvas();
        
        // 2. Créer le panel principal (déplaçable)
        GameObject panelGO = CreateMainPanel(canvasGO.transform);
        
        // 3. Créer le header avec titre et bouton close
        CreateHeader(panelGO.transform);
        
        // 4. Créer la zone de contenu avec scroll
        GameObject contentArea = CreateContentArea(panelGO.transform);
        
        // 5. Ajouter le script RoboticArmTraining et configurer les références
        SetupTrainingScript(canvasGO, panelGO, contentArea);
        
        // 6. Rendre le panel déplaçable
        MakePanelDraggable(panelGO);
        
        Debug.Log("✅ UI créée avec succès ! Vous pouvez maintenant cliquer sur Controller_Robotic_ARM");
    }
    
    GameObject CreateCanvas()
    {
        GameObject canvasGO = new GameObject("RoboticArmCanvas");
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasGO.AddComponent<GraphicRaycaster>();
        
        return canvasGO;
    }
    
    GameObject CreateMainPanel(Transform parent)
    {
        GameObject panelGO = new GameObject("TrainingPanel");
        panelGO.transform.SetParent(parent);
        
        RectTransform rect = panelGO.AddComponent<RectTransform>();
        
        // Ancrer à droite de l'écran
        rect.anchorMin = new Vector2(1, 0.5f);
        rect.anchorMax = new Vector2(1, 0.5f);
        rect.sizeDelta = panelSize;
        rect.anchoredPosition = panelPosition;
        
        // Background du panel
        Image panelImage = panelGO.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // SUPPRIMÉ: Border avec Outline car il ne fonctionne pas comme prévu
        // On peut ajouter une bordure différemment si besoin
        
        // Shadow
        Shadow shadow = panelGO.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(3, -3);
        
        return panelGO;
    }
    
    void CreateHeader(Transform parent)
    {
        // Header container
        GameObject headerGO = new GameObject("Header");
        headerGO.transform.SetParent(parent);
        
        RectTransform headerRect = headerGO.AddComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0, 1);
        headerRect.anchorMax = new Vector2(1, 1);
        headerRect.sizeDelta = new Vector2(0, 60);
        headerRect.anchoredPosition = new Vector2(0, -30);
        
        Image headerBg = headerGO.AddComponent<Image>();
        headerBg.color = new Color(0.2f, 0.3f, 0.5f, 1f);
        
        // Titre
        GameObject titleGO = new GameObject("Title");
        titleGO.transform.SetParent(headerGO.transform);
        
        RectTransform titleRect = titleGO.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0);
        titleRect.anchorMax = new Vector2(0.8f, 1);
        titleRect.offsetMin = new Vector2(15, 5);
        titleRect.offsetMax = new Vector2(-5, -5);
        
        TextMeshProUGUI titleText = titleGO.AddComponent<TextMeshProUGUI>();
        titleText.text = "🤖 Formation LOTO";
        titleText.fontSize = 18;
        titleText.fontStyle = FontStyles.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.MidlineLeft;
        
        // Bouton Close
        GameObject closeBtnGO = new GameObject("CloseButton");
        closeBtnGO.transform.SetParent(headerGO.transform);
        
        RectTransform closeBtnRect = closeBtnGO.AddComponent<RectTransform>();
        closeBtnRect.anchorMin = new Vector2(0.8f, 0.2f);
        closeBtnRect.anchorMax = new Vector2(0.95f, 0.8f);
        closeBtnRect.offsetMin = Vector2.zero;
        closeBtnRect.offsetMax = Vector2.zero;
        
        Image closeBtnImage = closeBtnGO.AddComponent<Image>();
        closeBtnImage.color = new Color(0.8f, 0.2f, 0.2f, 1f);
        
        Button closeButton = closeBtnGO.AddComponent<Button>();
        
        // Texte du bouton
        GameObject closeBtnTextGO = new GameObject("Text");
        closeBtnTextGO.transform.SetParent(closeBtnGO.transform);
        
        RectTransform closeBtnTextRect = closeBtnTextGO.AddComponent<RectTransform>();
        closeBtnTextRect.anchorMin = Vector2.zero;
        closeBtnTextRect.anchorMax = Vector2.one;
        closeBtnTextRect.offsetMin = Vector2.zero;
        closeBtnTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI closeBtnText = closeBtnTextGO.AddComponent<TextMeshProUGUI>();
        closeBtnText.text = "✕";
        closeBtnText.fontSize = 20;
        closeBtnText.fontStyle = FontStyles.Bold;
        closeBtnText.color = Color.white;
        closeBtnText.alignment = TextAlignmentOptions.Center;
    }
    
    GameObject CreateContentArea(Transform parent)
    {
        // Content Area
        GameObject contentAreaGO = new GameObject("ContentArea");
        contentAreaGO.transform.SetParent(parent);
        
        RectTransform contentRect = contentAreaGO.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 0);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.offsetMin = new Vector2(10, 10);
        contentRect.offsetMax = new Vector2(-10, -70); // Laisser place au header
        
        // Scroll View
        GameObject scrollViewGO = new GameObject("ScrollView");
        scrollViewGO.transform.SetParent(contentAreaGO.transform);
        
        RectTransform scrollRect = scrollViewGO.AddComponent<RectTransform>();
        scrollRect.anchorMin = Vector2.zero;
        scrollRect.anchorMax = Vector2.one;
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;
        
        Image scrollBg = scrollViewGO.AddComponent<Image>();
        scrollBg.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
        
        ScrollRect scroll = scrollViewGO.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        
        // Viewport
        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollViewGO.transform);
        
        RectTransform viewportRect = viewportGO.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.offsetMin = Vector2.zero;
        viewportRect.offsetMax = Vector2.zero;
        
        Image viewportImage = viewportGO.AddComponent<Image>();
        viewportImage.color = new Color(1, 1, 1, 0.01f);
        
        Mask mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        
        // Content (liste des étapes)
        GameObject contentGO = new GameObject("StepsList");
        contentGO.transform.SetParent(viewportGO.transform);
        
        RectTransform stepsListRect = contentGO.AddComponent<RectTransform>();
        stepsListRect.anchorMin = new Vector2(0, 1);
        stepsListRect.anchorMax = new Vector2(1, 1);
        stepsListRect.anchoredPosition = Vector2.zero;
        stepsListRect.sizeDelta = new Vector2(0, 500);
        
        // Layout pour les étapes
        VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(10, 10, 10, 10);
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = true;
        
        ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Configurer le ScrollRect
        scroll.viewport = viewportRect;
        scroll.content = stepsListRect;
        
        return contentGO;
    }
    
    void SetupTrainingScript(GameObject canvasGO, GameObject panelGO, GameObject stepsListGO)
    {
        // Ajouter le script RoboticArmTraining
        RoboticArmTraining training = canvasGO.AddComponent<RoboticArmTraining>();
        
        // Configurer les références
        training.trainingCanvas = canvasGO.GetComponent<Canvas>();
        training.trainingPanel = panelGO;
        training.stepsList = stepsListGO.transform;
        
        // Trouver le bouton close
        Button closeButton = panelGO.GetComponentInChildren<Button>();
        if (closeButton != null)
        {
            training.closeButton = closeButton;
        }
        
        Debug.Log("✅ Script RoboticArmTraining configuré avec les références UI");
    }
    
    void MakePanelDraggable(GameObject panelGO)
    {
        // Ajouter la capacité de déplacer le panel
        DraggablePanel draggable = panelGO.AddComponent<DraggablePanel>();
        
        // Le header sera la zone de drag
        Transform header = panelGO.transform.Find("Header");
        if (header != null)
        {
            draggable.dragHandle = header.GetComponent<RectTransform>();
        }
        
        Debug.Log("✅ Panel rendu déplaçable");
    }
}

// Script pour rendre le panel déplaçable
public class DraggablePanel : MonoBehaviour
{
    public RectTransform dragHandle;
    
    private RectTransform panelRect;
    private Canvas canvas;
    private Vector2 offset;
    private bool isDragging = false;
    
    void Start()
    {
        panelRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        
        if (dragHandle == null)
        {
            dragHandle = panelRect;
        }
    }
    
    void Update()
    {
        if (isDragging)
        {
            Vector2 mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out mousePosition
            );
            
            panelRect.localPosition = mousePosition - offset;
        }
        
        // Détecter le début du drag
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 localMousePosition;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dragHandle,
                Input.mousePosition,
                canvas.worldCamera,
                out localMousePosition))
            {
                if (dragHandle.rect.Contains(localMousePosition))
                {
                    isDragging = true;
                    
                    Vector2 mousePos;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvas.transform as RectTransform,
                        Input.mousePosition,
                        canvas.worldCamera,
                        out mousePos
                    );
                    
                    offset = mousePos - (Vector2)panelRect.localPosition;
                }
            }
        }
        
        // Arrêter le drag
        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(RoboticArmUICreator))]
public class RoboticArmUICreatorEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        RoboticArmUICreator creator = (RoboticArmUICreator)target;
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("🎨 Créer l'UI du Bras Robotique", GUILayout.Height(40)))
        {
            creator.CreateRoboticArmUI();
        }
        
        GUILayout.Space(10);
        
        UnityEditor.EditorGUILayout.HelpBox(
            "Ce bouton va créer automatiquement :\n" +
            "• Canvas avec UI déplaçable\n" +
            "• Panel à droite de l'écran\n" +
            "• Liste des étapes de formation\n" +
            "• Scripts de gestion automatique", 
            UnityEditor.MessageType.Info
        );
        
        GUILayout.Space(10);
        
        UnityEditor.EditorGUILayout.HelpBox(
            "N'oubliez pas d'ajouter le script ControllerRoboticArmTrigger sur l'objet 'Controller_Robotic_ARM' !", 
            UnityEditor.MessageType.Warning
        );
    }
}
#endif