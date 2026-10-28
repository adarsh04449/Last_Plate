using UnityEngine;
using UnityEngine.UI;

/// <summary>Builds the complete multi-page cooking prototype without Inspector setup.</summary>
public class GameBootstrapper : MonoBehaviour
{
    private static readonly Color Cream = new Color(1f, 0.93f, 0.76f);
    private static readonly Color Ink = new Color(0.06f, 0.08f, 0.11f);

    private void Awake()
    {
        Application.targetFrameRate = 60;
        Screen.SetResolution(1280, 720, false);
        if (Camera.main != null && Camera.main.GetComponent<AudioListener>() == null) Camera.main.gameObject.AddComponent<AudioListener>();
        BuildGame();
    }

    private void BuildGame()
    {
        GameObject systems = new GameObject("KitchenFlow");
        KitchenFlowManager flow = systems.AddComponent<KitchenFlowManager>();
        systems.AddComponent<AudioManager>();

        Texture2D sheet = Resources.Load<Texture2D>("Art/character_ingredient_sheet");
        Sprite chefSprite = Slice(sheet, 0, 1);
        Sprite[] foodSprites = { Slice(sheet, 1, 1), Slice(sheet, 2, 1), Slice(sheet, 0, 0), Slice(sheet, 1, 0) };

        GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1280f, 720f); scaler.matchWidthOrHeight = 0.5f;

        GameObject title = CreatePage("TitlePage", canvasObject.transform, new Color(0.025f, 0.08f, 0.12f));
        AddPageDecor(title.transform, new Color(0.05f, 0.42f, 0.48f), "RESTAURANT FRONT");
        Text titleText = MakeText(title.transform, "LAST PLATE", Vector2.zero, new Vector2(1000f, 620f), 42, Cream, TextAnchor.MiddleCenter);

        GameObject order = CreatePage("OrderPage", canvasObject.transform, new Color(0.22f, 0.075f, 0.055f));
        AddPageDecor(order.transform, new Color(0.72f, 0.25f, 0.12f), "CUSTOMER COUNTER");
        AddCharacter(order.transform, chefSprite, new Vector2(-465f, -80f));
        Text orderText = MakeText(order.transform, "ORDER", new Vector2(170f, -15f), new Vector2(830f, 590f), 30, Cream, TextAnchor.MiddleCenter);

        GameObject collection = CreatePage("CollectionPage", canvasObject.transform, new Color(0.035f, 0.18f, 0.2f));
        AddPageDecor(collection.transform, new Color(0.04f, 0.56f, 0.54f), "INGREDIENT PANTRY");
        MakeText(collection.transform, "COLLECT THE THREE ORDER INGREDIENTS", new Vector2(0f, 278f), new Vector2(900f, 52f), 27, Color.white, TextAnchor.MiddleCenter);
        MakeText(collection.transform, "A / D or ARROWS move the basket • Let unwanted food pass", new Vector2(0f, 238f), new Vector2(1000f, 38f), 19, Cream, TextAnchor.MiddleCenter);
        Text collectionStatus = MakeText(collection.transform, "BASKET", new Vector2(0f, 192f), new Vector2(1100f, 66f), 20, Color.white, TextAnchor.MiddleCenter);
        GameObject fallingRootObject = MakeUIObject("FallingIngredients", collection.transform);
        RectTransform basket = MakePanel("Basket", collection.transform, new Color(0.95f, 0.56f, 0.15f), new Vector2(0f, -275f), new Vector2(145f, 58f)).GetComponent<RectTransform>();
        MakeText(basket, "BASKET", Vector2.zero, new Vector2(140f, 52f), 17, Ink, TextAnchor.MiddleCenter);
        CollectionStage collectionStage = systems.AddComponent<CollectionStage>();
        collectionStage.basket = basket; collectionStage.fallingRoot = fallingRootObject.GetComponent<RectTransform>();
        collectionStage.statusText = collectionStatus; collectionStage.ingredientSprites = foodSprites;

        GameObject preparation = CreatePage("PreparationPage", canvasObject.transform, new Color(0.07f, 0.2f, 0.1f));
        AddPageDecor(preparation.transform, new Color(0.2f, 0.62f, 0.28f), "PREPARATION COUNTER");
        Text preparationText = MakeText(preparation.transform, "PREPARE", new Vector2(0f, 165f), new Vector2(1000f, 260f), 31, Color.white, TextAnchor.MiddleCenter);
        GameObject chopBar = MakePanel("ChopTimingBar", preparation.transform, new Color(0.03f, 0.07f, 0.04f), new Vector2(0f, -65f), new Vector2(560f, 56f));
        RectTransform perfectZone = MakePanel("PerfectZone", chopBar.transform, new Color(0.25f, 0.9f, 0.35f), Vector2.zero, new Vector2(110f, 48f)).GetComponent<RectTransform>();
        RectTransform chopMarker = MakePanel("KnifeMarker", chopBar.transform, Color.white, Vector2.zero, new Vector2(12f, 82f)).GetComponent<RectTransform>();
        Text cutProgress = MakeText(preparation.transform, "CUTS: □ □ □", new Vector2(0f, -135f), new Vector2(900f, 50f), 18, Cream, TextAnchor.MiddleCenter);
        PreparationStage preparationStage = systems.AddComponent<PreparationStage>();
        preparationStage.marker = chopMarker; preparationStage.perfectZone = perfectZone;
        preparationStage.instructionText = preparationText; preparationStage.cutProgressText = cutProgress;

        GameObject cooking = CreatePage("CookingPage", canvasObject.transform, new Color(0.25f, 0.105f, 0.025f));
        AddPageDecor(cooking.transform, new Color(0.92f, 0.38f, 0.06f), "STOVE AND MIXING AREA");
        Text cookingText = MakeText(cooking.transform, "COOK", new Vector2(0f, 165f), new Vector2(1000f, 260f), 31, Color.white, TextAnchor.MiddleCenter);
        GameObject cookBar = MakePanel("CookingBar", cooking.transform, new Color(0.08f, 0.035f, 0.015f), new Vector2(0f, -65f), new Vector2(560f, 62f));
        GameObject fillObject = MakePanel("CookingFill", cookBar.transform, new Color(1f, 0.65f, 0.15f), Vector2.zero, new Vector2(548f, 50f));
        Image cookingFill = fillObject.GetComponent<Image>(); cookingFill.type = Image.Type.Filled; cookingFill.fillMethod = Image.FillMethod.Horizontal; cookingFill.fillOrigin = 0;
        Text mealProgressLabel = MakeText(cooking.transform, "MEAL PROGRESS", new Vector2(0f, -112f), new Vector2(760f, 36f), 16, Cream, TextAnchor.MiddleCenter);
        GameObject heatBar = MakePanel("HeatControlBar", cooking.transform, new Color(0.08f, 0.035f, 0.015f), new Vector2(0f, -190f), new Vector2(560f, 48f));
        MakePanel("PerfectHeatZone", heatBar.transform, new Color(0.2f, 0.92f, 0.32f), Vector2.zero, new Vector2(110f, 40f));
        RectTransform heatMarker = MakePanel("FlameMarker", heatBar.transform, Color.white, new Vector2(-75f, 0f), new Vector2(12f, 68f)).GetComponent<RectTransform>();
        Text heatStatus = MakeText(cooking.transform, "COOKING 0%     HEAT: LOW", new Vector2(0f, -252f), new Vector2(760f, 42f), 18, Color.white, TextAnchor.MiddleCenter);
        CookingStage cookingStage = systems.AddComponent<CookingStage>();
        cookingStage.cookingFill = cookingFill; cookingStage.heatMarker = heatMarker;
        cookingStage.instructionText = cookingText; cookingStage.heatStatusText = heatStatus;
        cookingStage.cookingControls = new[] { cookBar, mealProgressLabel.gameObject, heatBar, heatStatus.gameObject };

        GameObject serving = CreatePage("ServingPage", canvasObject.transform, new Color(0.12f, 0.07f, 0.2f));
        AddPageDecor(serving.transform, new Color(0.48f, 0.25f, 0.72f), "PLATING AND SERVICE");
        Text servingText = MakeText(serving.transform, "PLATING COUNTER", new Vector2(0f, 190f), new Vector2(1000f, 180f), 31, Color.white, TextAnchor.MiddleCenter);
        GameObject plate = MakePanel("DinnerPlate", serving.transform, new Color(0.92f, 0.94f, 1f), new Vector2(0f, -65f), new Vector2(520f, 190f));
        Image[] plateSlots = new Image[3];
        for (int i = 0; i < plateSlots.Length; i++)
            plateSlots[i] = MakeSpriteImage("PlateIngredient" + (i + 1), plate.transform, null, new Vector2((i - 1) * 135f, 0f), new Vector2(115f, 115f));
        Text plateStatus = MakeText(serving.transform, "Press SPACE to plate", new Vector2(0f, -220f), new Vector2(900f, 60f), 21, Cream, TextAnchor.MiddleCenter);
        PlatingStage platingStage = systems.AddComponent<PlatingStage>();
        platingStage.instructionText = servingText; platingStage.plateStatusText = plateStatus;
        platingStage.ingredientSlots = plateSlots; platingStage.ingredientSprites = foodSprites;

        GameObject result = CreatePage("OrderResultPage", canvasObject.transform, new Color(0.05f, 0.15f, 0.12f));
        AddPageDecor(result.transform, new Color(0.12f, 0.65f, 0.4f), "CUSTOMER REVIEW");
        Text resultText = MakeText(result.transform, "RESULT", Vector2.zero, new Vector2(960f, 600f), 34, Color.white, TextAnchor.MiddleCenter);

        GameObject session = CreatePage("SessionResultPage", canvasObject.transform, new Color(0.12f, 0.06f, 0.03f));
        AddPageDecor(session.transform, new Color(0.85f, 0.52f, 0.1f), "END OF DAY");
        Text sessionText = MakeText(session.transform, "SESSION", Vector2.zero, new Vector2(980f, 610f), 36, Cream, TextAnchor.MiddleCenter);

        GameObject pause = CreatePage("PauseOverlay", canvasObject.transform, new Color(0.01f, 0.02f, 0.03f, 0.94f));
        MakeText(pause.transform, "KITCHEN PAUSED\n\nPress ESCAPE to continue", Vector2.zero, new Vector2(900f, 500f), 42, Color.white, TextAnchor.MiddleCenter);
        pause.SetActive(false);

        flow.titlePage = title; flow.orderPage = order; flow.collectionPage = collection; flow.preparationPage = preparation;
        flow.cookingPage = cooking; flow.servingPage = serving; flow.resultPage = result; flow.sessionPage = session; flow.pauseOverlay = pause;
        flow.titleText = titleText; flow.orderText = orderText; flow.servingText = servingText; flow.resultText = resultText; flow.sessionText = sessionText;
        flow.collectionStage = collectionStage; flow.preparationStage = preparationStage; flow.cookingStage = cookingStage; flow.platingStage = platingStage;
    }

    private static GameObject CreatePage(string name, Transform parent, Color color)
    {
        GameObject page = MakeUIObject(name, parent);
        page.AddComponent<Image>().color = color;
        return page;
    }

    private static void AddPageDecor(Transform page, Color accent, string roomName)
    {
        MakePanel("TopAccent", page, accent, new Vector2(0f, 340f), new Vector2(1280f, 40f));
        MakePanel("BottomCounter", page, new Color(accent.r * 0.55f, accent.g * 0.55f, accent.b * 0.55f), new Vector2(0f, -335f), new Vector2(1280f, 70f));
        MakeText(page, roomName, new Vector2(-500f, 310f), new Vector2(260f, 40f), 17, Cream, TextAnchor.MiddleLeft);
    }

    private static void AddCharacter(Transform parent, Sprite sprite, Vector2 position)
    {
        GameObject portrait = new GameObject("ChefPortrait", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        portrait.transform.SetParent(parent, false);
        RectTransform rect = portrait.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(330f, 430f); rect.anchoredPosition = position;
        Image image = portrait.GetComponent<Image>(); image.sprite = sprite; image.preserveAspect = true;
    }

    private static Sprite Slice(Texture2D sheet, int column, int row)
    {
        if (sheet == null) return null;
        float width = sheet.width / 3f; float height = sheet.height / 2f;
        return Sprite.Create(sheet, new Rect(column * width, row * height, width, height), new Vector2(0.5f, 0.5f), 255f);
    }

    private static GameObject MakeUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform)); obj.transform.SetParent(parent, false);
        RectTransform rect = obj.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        return obj;
    }

    private static GameObject MakePanel(string name, Transform parent, Color color, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>(); rect.sizeDelta = size; rect.anchoredPosition = position;
        panel.GetComponent<Image>().color = color; return panel;
    }

    private static Text MakeText(Transform parent, string value, Vector2 position, Vector2 size, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); textObject.transform.SetParent(parent, false);
        Text text = textObject.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = fontSize; text.fontStyle = FontStyle.Bold; text.color = color; text.alignment = alignment;
        RectTransform rect = textObject.GetComponent<RectTransform>(); rect.sizeDelta = size; rect.anchoredPosition = position;
        return text;
    }

    private static Image MakeSpriteImage(string name, Transform parent, Sprite sprite, Vector2 position, Vector2 size)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>(); rect.sizeDelta = size; rect.anchoredPosition = position;
        Image image = imageObject.GetComponent<Image>(); image.sprite = sprite; image.preserveAspect = true;
        return image;
    }
}
