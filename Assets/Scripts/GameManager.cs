using UnityEngine;

public enum GameState { Title, Playing, Paused, Results }
public enum DifficultyLevel { Preparation, DinnerRush, KitchenChaos }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Round")]
    public float roundDuration = 120f;
    public int startingLives = 3;

    public GameState State { get; private set; } = GameState.Title;
    public float TimeRemaining { get; private set; }
    public int Lives { get; private set; }
    public int Score { get; private set; }
    public int CompletedOrders { get; private set; }
    public int Combo { get; private set; }
    public int BestCombo { get; private set; }
    public DifficultyLevel Difficulty { get; private set; } = DifficultyLevel.Preparation;
    public int Multiplier => 1 + Mathf.Min(Combo / 3, 3);

    private OrderManager orderManager;
    private SpawnManager spawnManager;
    private UIManager uiManager;
    private AudioManager audioManager;
    private ChefReactionController chefReactions;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        orderManager = GetComponent<OrderManager>();
        spawnManager = GetComponent<SpawnManager>();
        uiManager = GetComponent<UIManager>();
        audioManager = GetComponent<AudioManager>();
        chefReactions = FindAnyObjectByType<ChefReactionController>();
        Time.timeScale = 1f;
        ShowTitle();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            if (State == GameState.Title || State == GameState.Results) StartRound();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (State == GameState.Playing) PauseGame();
            else if (State == GameState.Paused) ResumeGame();
        }

        if (State != GameState.Playing) return;
        TimeRemaining = Mathf.Max(0f, TimeRemaining - Time.deltaTime);
        // Divide the configured round into three equal phases (40 seconds each by default).
        DifficultyLevel newDifficulty = TimeRemaining > roundDuration * (2f / 3f) ? DifficultyLevel.Preparation :
            TimeRemaining > roundDuration / 3f ? DifficultyLevel.DinnerRush : DifficultyLevel.KitchenChaos;
        if (newDifficulty != Difficulty)
        {
            Difficulty = newDifficulty;
            uiManager.ShowDifficulty(Difficulty);
            audioManager.PlayLevelUp();
            spawnManager.SetDifficulty(Difficulty);
        }
        uiManager.Refresh();
        if (TimeRemaining <= 0f) EndRound();
    }

    public void StartRound()
    {
        Time.timeScale = 1f;
        State = GameState.Playing;
        audioManager.ResetPlayback();
        TimeRemaining = roundDuration;
        Lives = startingLives;
        Score = CompletedOrders = Combo = BestCombo = 0;
        Difficulty = DifficultyLevel.Preparation;
        chefReactions.ResetPose();
        spawnManager.SetDifficulty(Difficulty);
        orderManager.CreateNewOrder();
        spawnManager.ResetSpawner();
        uiManager.ShowPlaying();
        uiManager.ShowDifficulty(Difficulty);
        uiManager.Refresh();
    }

    public void RegisterCorrectIngredient(bool golden)
    {
        Combo++;
        BestCombo = Mathf.Max(BestCombo, Combo);
        int points = (golden ? 40 : 15) * Multiplier;
        Score += points;
        uiManager.ShowCatchFeedback(golden ? "GOLDEN +" + points : "+" + points, golden);
    }

    public void AddCompletedOrder(int customerBonus = 0)
    {
        CompletedOrders++;
        int points = (100 + customerBonus) * Multiplier;
        Score += points;
        uiManager.ShowOrderComplete(points);
        chefReactions.ShowCelebration();
        audioManager.PlayOrderComplete();
    }

    public void BreakCombo()
    {
        Combo = 0;
        uiManager.Refresh();
    }

    public void LoseLife(string feedback = "BURNT FOOD!")
    {
        if (State != GameState.Playing) return;
        Combo = 0;
        Lives = Mathf.Max(0, Lives - 1);
        uiManager.ShowDamageFlash();
        chefReactions.ShowWorried();
        uiManager.ShowCatchFeedback(feedback, false);
        uiManager.Refresh();
        if (Lives == 0) EndRound();
    }

    public void PauseGame()
    {
        State = GameState.Paused;
        Time.timeScale = 0f;
        audioManager.PausePlayback();
        uiManager.ShowPaused();
    }

    public void ResumeGame()
    {
        State = GameState.Playing;
        Time.timeScale = 1f;
        audioManager.ResumePlayback();
        uiManager.ShowPlaying();
    }

    public void EndRound()
    {
        if (State == GameState.Results) return;
        State = GameState.Results;
        Time.timeScale = 0f;
        spawnManager.StopSpawning();
        uiManager.ShowResults();
    }

    private void ShowTitle()
    {
        State = GameState.Title;
        Time.timeScale = 1f;
        uiManager.ShowTitle();
    }
}
