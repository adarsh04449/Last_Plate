using UnityEngine;

/// <summary>Difficulty-aware spawner that deliberately keeps every order achievable.</summary>
public class SpawnManager : MonoBehaviour
{
    public float spawnInterval = 1.05f;
    public float ingredientSpeed = 3.2f;
    public float burntFoodChance = 0.03f;
    public Transform ingredientContainer;
    public Sprite[] ingredientSprites;
    public Sprite burntSprite;

    private float spawnTimer;
    private bool spawning;
    private int spawnsSinceTarget;

    private void Update()
    {
        if (!spawning || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f) { SpawnIngredient(); spawnTimer = spawnInterval * Random.Range(0.82f, 1.15f); }
    }

    public void ResetSpawner()
    {
        for (int i = ingredientContainer.childCount - 1; i >= 0; i--) Destroy(ingredientContainer.GetChild(i).gameObject);
        spawnTimer = 0.3f;
        spawnsSinceTarget = 2;
        spawning = true;
        FindAnyObjectByType<PlateController>()?.ResetEnergy();
    }

    public void StopSpawning() { spawning = false; }

    public void SetDifficulty(DifficultyLevel level)
    {
        if (level == DifficultyLevel.Preparation) { spawnInterval = 1.0f; ingredientSpeed = 3.1f; burntFoodChance = 0.03f; }
        else if (level == DifficultyLevel.DinnerRush) { spawnInterval = 0.72f; ingredientSpeed = 4.25f; burntFoodChance = 0.11f; }
        else { spawnInterval = 0.5f; ingredientSpeed = 5.35f; burntFoodChance = 0.22f; }
    }

    private void SpawnIngredient()
    {
        OrderManager orders = FindAnyObjectByType<OrderManager>();
        bool burnt = Random.value < burntFoodChance;
        int index;

        // At least one of every three food spawns is useful, preventing impossible-feeling orders.
        if (!burnt && (spawnsSinceTarget >= 2 || Random.value < 0.42f))
        {
            index = (int)orders.NextRequired;
            spawnsSinceTarget = 0;
        }
        else
        {
            index = Random.Range(0, ingredientSprites.Length);
            spawnsSinceTarget++;
        }

        bool golden = !burnt && Random.value < 0.08f;
        IngredientType type = burnt ? IngredientType.BurntFood : (IngredientType)index;
        GameObject item = new GameObject(golden ? "Golden " + type : "Falling " + type);
        item.transform.SetParent(ingredientContainer);
        item.transform.position = new Vector3(Random.Range(-8.1f, 8.1f), 5.8f, 0f);

        SpriteRenderer renderer = item.AddComponent<SpriteRenderer>();
        renderer.sprite = burnt ? burntSprite : ingredientSprites[index];
        renderer.sortingOrder = 2;
        if (golden) renderer.color = new Color(1f, 0.83f, 0.2f);

        CircleCollider2D collider = item.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = 0.42f;
        Rigidbody2D body = item.AddComponent<Rigidbody2D>(); body.bodyType = RigidbodyType2D.Kinematic; body.gravityScale = 0f;
        FallingIngredient falling = item.AddComponent<FallingIngredient>();
        falling.ingredientType = type; falling.fallSpeed = ingredientSpeed * Random.Range(0.9f, 1.1f); falling.isGolden = golden;
    }
}
