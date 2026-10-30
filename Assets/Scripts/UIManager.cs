using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text timerText, scoreText, livesText, orderText, comboText, levelText, messageText, tiltPromptText;
    public Image tiltFill, damageFlash, successFlash;
    public Image[] orderIcons, orderFrames;
    public Sprite[] ingredientSprites;
    public Text customerText;
    public Image patienceFill, customerCardImage, customerPortrait;
    public Sprite[] customerSprites;
    public GameObject titlePanel, pausePanel, resultsPanel, hud;

    private float damageFlashStrength;
    private float successFlashStrength;
    private GameManager game;
    private OrderManager order;
    private PlateController plate;
    private PlateInventory inventory;
    private AudioManager audioManager;
    private CustomerTicket displayedCustomer;

    private void Awake()
    {
        game = GetComponent<GameManager>();
        order = GetComponent<OrderManager>();
        audioManager = GetComponent<AudioManager>();
        plate = FindAnyObjectByType<PlateController>();
        inventory = FindAnyObjectByType<PlateInventory>();
    }

    private void Update()
    {
        damageFlashStrength = Mathf.MoveTowards(damageFlashStrength, 0f, Time.unscaledDeltaTime * 2.8f);
        successFlashStrength = Mathf.MoveTowards(successFlashStrength, 0f, Time.unscaledDeltaTime * 1.8f);
        if (damageFlash != null) damageFlash.color = new Color(1f, 0.08f, 0.03f, damageFlashStrength * 0.42f);
        if (successFlash != null) successFlash.color = new Color(1f, 0.82f, 0.15f, successFlashStrength * 0.28f);
    }

    public void Refresh()
    {
        if (game == null || order == null) return;

        int remainingSeconds = Mathf.CeilToInt(game.TimeRemaining);
        timerText.text = (remainingSeconds / 60) + ":" + (remainingSeconds % 60).ToString("00");
        timerText.color = game.TimeRemaining <= 10f ? new Color(1f, 0.3f, 0.25f) : Color.white;
        scoreText.text = "SCORE  " + game.Score.ToString("0000");
        comboText.text = game.Combo >= 2 ? "COMBO " + game.Combo + "  x" + game.Multiplier : "BUILD A COMBO";
        livesText.text = new string('♥', game.Lives) + new string('♡', 3 - game.Lives);
        levelText.text = DifficultyLabel(game.Difficulty);
        if (tiltFill != null && plate != null)
        {
            tiltFill.rectTransform.sizeDelta = new Vector2(205f * plate.EnergyNormalized, 20f);
            bool readyToDiscard = inventory != null && inventory.Count > 0 && plate.EnergyNormalized > 0.12f;
            tiltPromptText.text = plate.TiltEnergy == 0f ? "RELEASE SPACE TO RECHARGE" :
                readyToDiscard ? "SPACE: TILT + DISCARD" : "HOLD SPACE TO TILT";
            tiltPromptText.color = readyToDiscard ? new Color(0.35f, 1f, 0.65f) : Color.white;
        }

        CustomerTicket ticket = order.CurrentCustomer;
        customerCardImage.gameObject.SetActive(ticket != null);
        if (ticket != null)
        {
            if (ticket != displayedCustomer)
            {
                customerPortrait.sprite = customerSprites[(int)ticket.type];
                customerText.text = ticket.displayName.ToUpper() + "\n" +
                    ticket.recipeName.ToUpper() + "\n" + RecipeLine(ticket.ingredients);
                displayedCustomer = ticket;
            }
            // Keep the left edge fixed as the remaining time gets shorter.
            float patienceRatio = Mathf.Clamp01(ticket.patience / ticket.maxPatience);
            RectTransform meter = patienceFill.rectTransform;
            meter.sizeDelta = new Vector2(300f * patienceRatio, 12f);
            meter.anchoredPosition = new Vector2(-150f + 150f * patienceRatio, 0f);
        }

        for (int i = 0; orderIcons != null && i < orderIcons.Length; i++)
        {
            IngredientType type = order.CurrentOrder[i];
            orderIcons[i].sprite = ingredientSprites[(int)type];
            orderIcons[i].preserveAspect = true;
            if (i < order.Progress)
            {
                orderIcons[i].color = new Color(0.55f, 1f, 0.6f);
                orderFrames[i].color = new Color(0.15f, 0.72f, 0.25f, 0.98f);
            }
            else if (i == order.Progress)
            {
                float pulse = 0.82f + Mathf.Sin(Time.unscaledTime * 7f) * 0.18f;
                orderIcons[i].color = Color.white;
                orderFrames[i].color = new Color(1f, 0.62f, 0.08f, pulse);
            }
            else
            {
                orderIcons[i].color = new Color(1f, 1f, 1f, 0.45f);
                orderFrames[i].color = new Color(0.2f, 0.16f, 0.11f, 0.72f);
            }
        }

        orderText.text = "NEXT: " + ShortName(order.NextRequired) + "   •   Catch the glowing recipe icon";
    }

    public void ShowTitle()
    {
        SetPanels(titlePanel);
        hud.SetActive(false);
        messageText.color = Color.white;
        messageText.fontSize = 42;
        messageText.text = "LAST PLATE\n\n2-MINUTE KITCHEN RUSH\n\nCatch the three glowing recipe icons IN ORDER\nWrong food stays on the plate — press SPACE to tilt and discard\nBurnt food costs one life • Golden food gives bonus points\n\nA / D or ARROWS to move   •   ESC to pause\nM: music on/off   •   ENTER to start";
    }

    public void ShowPlaying()
    {
        SetPanels(null);
        hud.SetActive(true);
        ClearMessage();
    }

    public void ShowPaused()
    {
        SetPanels(pausePanel);
        hud.SetActive(false);
        messageText.color = Color.white;
        messageText.fontSize = 44;
        messageText.text = "KITCHEN PAUSED\n\nPress ESCAPE to resume";
    }

    public void ShowResults()
    {
        SetPanels(resultsPanel);
        hud.SetActive(false);
        string rank = game.Score >= 1800 ? "MASTER CHEF" : game.Score >= 900 ? "SOUS CHEF" : "LINE COOK";
        messageText.color = new Color(1f, 0.88f, 0.35f);
        messageText.fontSize = 42;
        messageText.text = "RUSH COMPLETE\n" + rank + "\n\nSCORE  " + game.Score + "\nORDERS  " + game.CompletedOrders + "\nBEST COMBO  " + game.BestCombo + "\n\nPress ENTER to play again";
        audioManager.PlayRoundFinished();
    }

    public void ShowCatchFeedback(string text, bool golden)
    {
        if (game.State != GameState.Playing) return;
        CancelInvoke(nameof(ClearMessage));
        messageText.fontSize = golden ? 38 : 30;
        messageText.color = golden ? new Color(1f, 0.8f, 0.1f) : Color.white;
        messageText.text = text;
        Invoke(nameof(ClearMessage), 0.55f);
    }

    public void ShowOrderComplete(int points)
    {
        CancelInvoke(nameof(ClearMessage));
        successFlashStrength = 1f;
        messageText.fontSize = 40;
        messageText.color = new Color(0.35f, 1f, 0.55f);
        messageText.text = "ORDER SERVED!  +" + points;
        Invoke(nameof(ClearMessage), 1f);
    }

    public void ShowDamageFlash()
    {
        damageFlashStrength = 1f;
    }

    public void ShowDifficulty(DifficultyLevel level)
    {
        if (game.State != GameState.Playing) return;
        CancelInvoke(nameof(ClearMessage));
        messageText.fontSize = 34;
        messageText.color = new Color(1f, 0.65f, 0.2f);
        messageText.text = DifficultyLabel(level);
        Invoke(nameof(ClearMessage), 1.2f);
    }

    private void SetPanels(GameObject active)
    {
        titlePanel.SetActive(active == titlePanel);
        pausePanel.SetActive(active == pausePanel);
        resultsPanel.SetActive(active == resultsPanel);
    }

    private void ClearMessage()
    {
        if (game != null && game.State == GameState.Playing) messageText.text = "";
    }

    private static string DifficultyLabel(DifficultyLevel level)
    {
        if (level == DifficultyLevel.Preparation) return "PREPARATION";
        if (level == DifficultyLevel.DinnerRush) return "DINNER RUSH";
        return "KITCHEN CHAOS";
    }

    private static string ShortName(IngredientType type)
    {
        if (type == IngredientType.Tomato) return "TOMATO";
        if (type == IngredientType.Cheese) return "CHEESE";
        if (type == IngredientType.Lettuce) return "LETTUCE";
        return "MUSHROOM";
    }

    private static string RecipeLine(IngredientType[] ingredients)
    {
        string line = "";
        for (int i = 0; i < ingredients.Length; i++)
        {
            if (i > 0) line += " > ";
            line += ShortName(ingredients[i]);
        }
        return line;
    }
}
