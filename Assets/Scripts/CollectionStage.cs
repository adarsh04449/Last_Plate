using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Untimed basket minigame that ends after the three recipe ingredients are found.</summary>
public class CollectionStage : MonoBehaviour
{
    private sealed class FallingItem
    {
        public IngredientType type;
        public RectTransform rect;
        public GameObject gameObject;
        public float speed;
    }

    public RectTransform basket;
    public RectTransform fallingRoot;
    public Text statusText;
    public Sprite[] ingredientSprites;

    private readonly List<FallingItem> falling = new List<FallingItem>();
    private MealRecipe recipe;
    private KitchenFlowManager flow;
    private bool[] collected;
    private bool active;
    private float spawnTimer;
    private int mistakes;
    private int randomSpawns;

    public void Begin(MealRecipe nextRecipe, KitchenFlowManager manager)
    {
        ClearFalling();
        recipe = nextRecipe;
        flow = manager;
        collected = new bool[recipe.ingredients.Length];
        mistakes = 0;
        randomSpawns = 2;
        spawnTimer = 0.35f;
        basket.anchoredPosition = new Vector2(0f, -275f);
        active = true;
        RefreshStatus();
    }

    private void Update()
    {
        if (!active || Time.timeScale == 0f) return;

        float direction = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) direction = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) direction = 1f;
        Vector2 basketPosition = basket.anchoredPosition;
        basketPosition.x = Mathf.Clamp(basketPosition.x + direction * 620f * Time.deltaTime, -535f, 535f);
        basket.anchoredPosition = basketPosition;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnIngredient();
            spawnTimer = 0.82f;
        }

        for (int i = falling.Count - 1; i >= 0; i--)
        {
            FallingItem item = falling[i];
            item.rect.anchoredPosition += Vector2.down * item.speed * Time.deltaTime;
            Vector2 position = item.rect.anchoredPosition;
            if (position.y < -225f && position.y > -325f && Mathf.Abs(position.x - basketPosition.x) < 72f)
            {
                IngredientType caughtType = item.type;
                RemoveItem(i);
                if (Catch(caughtType)) return;
            }
            else if (position.y < -370f) RemoveItem(i);
        }
    }

    private bool Catch(IngredientType type)
    {
        int neededIndex = -1;
        for (int i = 0; i < recipe.ingredients.Length; i++)
        {
            if (!collected[i] && recipe.ingredients[i] == type) { neededIndex = i; break; }
        }

        AudioManager audio = FindAnyObjectByType<AudioManager>();
        if (neededIndex >= 0)
        {
            collected[neededIndex] = true;
            audio?.PlayCatch(false);
        }
        else
        {
            mistakes++;
            audio?.PlayWrongIngredient();
        }
        RefreshStatus();

        foreach (bool complete in collected) if (!complete) return false;
        active = false;
        ClearFalling();
        flow.CollectionComplete(mistakes);
        return true;
    }

    private void SpawnIngredient()
    {
        IngredientType type;
        if (randomSpawns >= 2 || Random.value < 0.58f)
        {
            List<IngredientType> needed = new List<IngredientType>();
            for (int i = 0; i < recipe.ingredients.Length; i++) if (!collected[i]) needed.Add(recipe.ingredients[i]);
            type = needed[Random.Range(0, needed.Count)];
            randomSpawns = 0;
        }
        else
        {
            type = (IngredientType)Random.Range(0, 4);
            randomSpawns++;
        }

        GameObject itemObject = new GameObject("Falling " + type, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        itemObject.transform.SetParent(fallingRoot, false);
        RectTransform rect = itemObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(74f, 74f);
        rect.anchoredPosition = new Vector2(Random.Range(-535f, 535f), 330f);
        Image image = itemObject.GetComponent<Image>();
        image.sprite = ingredientSprites[(int)type];
        image.preserveAspect = true;
        falling.Add(new FallingItem { type = type, rect = rect, gameObject = itemObject, speed = Random.Range(190f, 245f) });
    }

    private void RefreshStatus()
    {
        string line = "BASKET:  ";
        for (int i = 0; i < recipe.ingredients.Length; i++)
            line += (collected[i] ? "✓ " : "○ ") + KitchenFlowManager.IngredientName(recipe.ingredients[i]) + "    ";
        statusText.text = line + "\nWRONG CATCHES: " + mistakes + "   (each lowers quality)";
    }

    private void RemoveItem(int index)
    {
        if (falling[index].gameObject != null) Destroy(falling[index].gameObject);
        falling.RemoveAt(index);
    }

    private void ClearFalling()
    {
        foreach (FallingItem item in falling) if (item.gameObject != null) Destroy(item.gameObject);
        falling.Clear();
    }
}
