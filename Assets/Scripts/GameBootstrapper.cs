using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds the entire playable scene so no Inspector setup is required.</summary>
public class GameBootstrapper : MonoBehaviour
{
    private static readonly Color Navy = new Color(0.035f, 0.075f, 0.12f);
    private static readonly Color Cream = new Color(1f, 0.91f, 0.72f);
    private static readonly Color Orange = new Color(1f, 0.42f, 0.12f);

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.SetResolution(1280, 720, false);
        BuildGame();
    }

    private void BuildGame()
    {
        Camera.main.backgroundColor = Navy;
        BuildBackground();

        GameObject manager = new GameObject("GameManager");
        manager.AddComponent<GameManager>();
        manager.AddComponent<OrderManager>();
        manager.AddComponent<AudioManager>();

        Texture2D sheet = Resources.Load<Texture2D>("Art/character_ingredient_sheet");
        Sprite chefSprite = Slice(sheet, 0, 1);
        Sprite[] foods = { Slice(sheet, 1, 1), Slice(sheet, 2, 1), Slice(sheet, 0, 0), Slice(sheet, 1, 0) };
        Sprite burnt = Slice(sheet, 2, 0);

        GameObject chef = CreateSpriteObject("Chef", chefSprite, new Vector3(0f, -3.45f, 0f), new Vector3(1.45f, 1.45f, 1f), null, 3);
        chef.AddComponent<ChefController>();

        Sprite flatPlate = MakeRoundedPlate(false);
        Sprite tiltedPlate = MakeRoundedPlate(true);
        GameObject plate = CreateSpriteObject("CatchPlate", flatPlate, new Vector3(0.45f, -2.55f, -0.1f), new Vector3(1.25f, 0.7f, 1f), chef.transform, 4);
        BoxCollider2D plateCollider = plate.AddComponent<BoxCollider2D>(); plateCollider.isTrigger = true; plateCollider.size = new Vector2(1.8f, 1.3f);
        Rigidbody2D plateBody = plate.AddComponent<Rigidbody2D>(); plateBody.bodyType = RigidbodyType2D.Kinematic; plateBody.gravityScale = 0f;
        PlateController plateController = plate.AddComponent<PlateController>(); plateController.flatSprite = flatPlate; plateController.tiltedSprite = tiltedPlate;
        plate.AddComponent<PlateInventory>();

        GameObject container = new GameObject("IngredientContainer");
        SpawnManager spawner = manager.AddComponent<SpawnManager>();
        spawner.ingredientContainer = container.transform; spawner.ingredientSprites = foods; spawner.burntSprite = burnt;
        CreateUI(manager);
    }

    private static void BuildBackground()
    {
        Texture2D texture = Resources.Load<Texture2D>("Art/kitchen_background");
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), texture.height / 11.25f);
        CreateSpriteObject("KitchenBackground", sprite, Vector3.zero, Vector3.one, null, -10);
        GameObject shade = CreateSpriteObject("GameplayShade", MakeSolidSprite(new Color(0f, 0.08f, 0.12f, 0.18f)), new Vector3(0f, 0f, -0.1f), new Vector3(18f, 11.25f, 1f), null, -5);
        shade.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.38f);
    }

    private static Sprite Slice(Texture2D sheet, int column, int row)
    {
        if (sheet == null) return MakeSolidSprite(Color.magenta);
        float width = sheet.width / 3f;
        float height = sheet.height / 2f;
        return Sprite.Create(sheet, new Rect(column * width, row * height, width, height), new Vector2(0.5f, 0.5f), 255f);
    }

    private void CreateUI(GameObject parent)
    {
        GameObject canvasObject = new GameObject("Canvas"); canvasObject.transform.SetParent(parent.transform);
        Canvas canvas = canvasObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject hud = MakeUIObject("HUD", canvasObject.transform);
        GameObject topBar = MakePanel("TopBar", hud.transform, new Color(0.025f, 0.055f, 0.09f, 0.94f), new Vector2(0f, 315f), new Vector2(1280f, 90f));
        Text timer = MakeText(topBar.transform, "60s", new Vector2(-540f, 0f), new Vector2(150f, 70f), 36, Color.white, TextAnchor.MiddleLeft);
        Text lives = MakeText(topBar.transform, "♥♥♥", new Vector2(-355f, 0f), new Vector2(190f, 70f), 32, new Color(1f, 0.3f, 0.32f), TextAnchor.MiddleCenter);
        Text level = MakeText(topBar.transform, "PREPARATION", Vector2.zero, new Vector2(320f, 70f), 24, Cream, TextAnchor.MiddleCenter);
        Text score = MakeText(topBar.transform, "SCORE  0000", new Vector2(485f, 0f), new Vector2(260f, 70f), 30, Color.white, TextAnchor.MiddleRight);

        Text[] customerTexts = new Text[3];
        Image[] patienceFills = new Image[3];
        Image[] customerCardImages = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            GameObject card = MakePanel("CustomerTicket" + (i + 1), hud.transform, new Color(0.96f, 0.9f, 0.75f, 0.92f), new Vector2((i - 1) * 390f, 200f), new Vector2(350f, 120f));
            customerCardImages[i] = card.GetComponent<Image>();
            customerTexts[i] = MakeText(card.transform, "WAITING FOR CUSTOMER", new Vector2(0f, 9f), new Vector2(330f, 88f), 17, new Color(0.16f, 0.075f, 0.035f), TextAnchor.MiddleCenter);
            GameObject patienceBack = MakePanel("PatienceBack", card.transform, new Color(0.18f, 0.1f, 0.07f, 0.8f), new Vector2(0f, -49f), new Vector2(306f, 10f));
            GameObject patience = MakePanel("PatienceFill", patienceBack.transform, new Color(0.25f, 0.85f, 0.35f), Vector2.zero, new Vector2(300f, 6f));
            patienceFills[i] = patience.GetComponent<Image>(); patienceFills[i].type = Image.Type.Filled; patienceFills[i].fillMethod = Image.FillMethod.Horizontal;
        }
        Text order = MakeText(hud.transform, "TARGET", new Vector2(0f, 115f), new Vector2(880f, 48f), 21, Cream, TextAnchor.MiddleCenter);
        Text combo = MakeText(hud.transform, "BUILD A COMBO", new Vector2(445f, -300f), new Vector2(330f, 50f), 19, new Color(1f, 0.72f, 0.18f), TextAnchor.MiddleCenter);

        GameObject energyBack = MakePanel("TiltEnergy", hud.transform, new Color(0.02f, 0.05f, 0.08f, 0.9f), new Vector2(-455f, -300f), new Vector2(310f, 52f));
        MakeText(energyBack.transform, "TILT", new Vector2(-112f, 0f), new Vector2(70f, 45f), 19, Color.white, TextAnchor.MiddleCenter);
        GameObject fillObject = MakePanel("Fill", energyBack.transform, new Color(0.1f, 0.82f, 1f, 1f), new Vector2(36f, 0f), new Vector2(205f, 20f));
        Image tiltFill = fillObject.GetComponent<Image>(); tiltFill.type = Image.Type.Filled; tiltFill.fillMethod = Image.FillMethod.Horizontal; tiltFill.fillOrigin = 0;

        GameObject titlePanel = MakeFullPanel("TitlePanel", canvasObject.transform, new Color(0.01f, 0.025f, 0.045f, 0.88f));
        GameObject pausePanel = MakeFullPanel("PausePanel", canvasObject.transform, new Color(0.01f, 0.025f, 0.045f, 0.88f)); pausePanel.SetActive(false);
        GameObject resultsPanel = MakeFullPanel("ResultsPanel", canvasObject.transform, new Color(0.01f, 0.025f, 0.045f, 0.9f)); resultsPanel.SetActive(false);
        Text message = MakeText(canvasObject.transform, "", Vector2.zero, new Vector2(1000f, 610f), 42, Color.white, TextAnchor.MiddleCenter);

        UIManager ui = parent.AddComponent<UIManager>();
        ui.timerText = timer; ui.scoreText = score; ui.livesText = lives; ui.orderText = order; ui.comboText = combo; ui.levelText = level; ui.messageText = message; ui.tiltFill = tiltFill;
        ui.customerTexts = customerTexts; ui.patienceFills = patienceFills; ui.customerCardImages = customerCardImages;
        ui.titlePanel = titlePanel; ui.pausePanel = pausePanel; ui.resultsPanel = resultsPanel; ui.hud = hud;
    }

    private static GameObject MakeUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; return obj;
    }

    private static GameObject MakeFullPanel(string name, Transform parent, Color color)
    {
        GameObject panel = MakeUIObject(name, parent); panel.AddComponent<Image>().color = color; return panel;
    }

    private static GameObject MakePanel(string name, Transform parent, Color color, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>(); rect.sizeDelta = size; rect.anchoredPosition = position; panel.GetComponent<Image>().color = color; return panel;
    }

    private static Text MakeText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize; text.fontStyle = FontStyle.Bold; text.color = color; text.alignment = alignment;
        RectTransform rect = textObject.GetComponent<RectTransform>(); rect.sizeDelta = size; rect.anchoredPosition = position; return text;
    }

    private static GameObject CreateSpriteObject(string name, Sprite sprite, Vector3 position, Vector3 scale, Transform parent, int sortingOrder)
    {
        GameObject obj = new GameObject(name); if (parent != null) obj.transform.SetParent(parent);
        obj.transform.position = position; obj.transform.localScale = scale;
        SpriteRenderer renderer = obj.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = sortingOrder; return obj;
    }

    private static Sprite MakeSolidSprite(Color color)
    {
        Texture2D texture = new Texture2D(16, 16); Color[] pixels = new Color[256]; for (int i = 0; i < pixels.Length; i++) pixels[i] = color; texture.SetPixels(pixels); texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
    }

    private static Sprite MakeRoundedPlate(bool tilted)
    {
        Texture2D texture = new Texture2D(128, 32); Color[] pixels = new Color[128 * 32];
        for (int y = 0; y < 32; y++) for (int x = 0; x < 128; x++)
        {
            float nx = (x - 63.5f) / 63.5f; float ny = (y - 15.5f) / 15.5f;
            bool inside = nx * nx + ny * ny <= 1f;
            pixels[y * 128 + x] = inside ? (tilted ? new Color(0.45f, 0.9f, 1f) : Color.white) : Color.clear;
        }
        texture.SetPixels(pixels); texture.Apply(); return Sprite.Create(texture, new Rect(0, 0, 128, 32), new Vector2(0.5f, 0.5f), 64f);
    }
}
