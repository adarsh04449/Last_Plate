using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores and displays ingredients currently carried by the chef. Wrong food is recoverable:
/// the player can tap/hold Space to tilt and discard the newest slot.
/// </summary>
public class PlateInventory : MonoBehaviour
{
    private sealed class PlateItem
    {
        public IngredientType type;
        public bool golden;
        public Sprite sprite;
        public GameObject visual;
    }

    public int maxSlots = 3;
    public int Count => items.Count;
    public float LoadRatio => maxSlots == 0 ? 0f : items.Count / (float)maxSlots;
    public IReadOnlyList<IngredientType> CurrentTypes => GetTypes();

    private readonly List<PlateItem> items = new List<PlateItem>();
    private readonly List<IngredientType> typeView = new List<IngredientType>();
    private Transform displayRoot;

    private void Awake()
    {
        displayRoot = new GameObject("PlateContents").transform;
        displayRoot.SetParent(transform.parent, false);
        displayRoot.localPosition = transform.localPosition + new Vector3(0f, 0.42f, -0.2f);
    }

    public void TryCatch(FallingIngredient ingredient)
    {
        if (ingredient.ingredientType == IngredientType.BurntFood)
        {
            GameManager.Instance.LoseLife("BURNT FOOD!");
            FindAnyObjectByType<AudioManager>().PlayLifeLost();
            return;
        }

        if (items.Count >= maxSlots)
        {
            FindAnyObjectByType<UIManager>().ShowCatchFeedback("PLATE FULL — TILT!", false);
            FindAnyObjectByType<AudioManager>().PlayWrongIngredient();
            return;
        }

        SpriteRenderer sourceRenderer = ingredient.GetComponent<SpriteRenderer>();
        PlateItem item = new PlateItem
        {
            type = ingredient.ingredientType,
            golden = ingredient.isGolden,
            sprite = sourceRenderer.sprite
        };
        item.visual = CreateVisual(item);
        items.Add(item);
        RefreshVisualPositions();

        FindAnyObjectByType<OrderManager>().ProcessPlateCatch(GetTypes(), item.golden);
    }

    public void DiscardLast()
    {
        if (items.Count == 0) return;
        int last = items.Count - 1;
        Destroy(items[last].visual);
        items.RemoveAt(last);
        RefreshVisualPositions();
        FindAnyObjectByType<OrderManager>().SyncPlate(GetTypes());
        FindAnyObjectByType<UIManager>().ShowCatchFeedback("DISCARDED", false);
    }

    public void Clear()
    {
        foreach (PlateItem item in items)
            if (item.visual != null) Destroy(item.visual);
        items.Clear();
        typeView.Clear();
        FindAnyObjectByType<OrderManager>()?.SyncPlate(typeView);
    }

    private GameObject CreateVisual(PlateItem item)
    {
        GameObject visual = new GameObject(item.golden ? "Golden Plate Item" : "Plate Item");
        visual.transform.SetParent(displayRoot, false);
        visual.transform.localScale = Vector3.one * 0.38f;
        SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
        renderer.sprite = item.sprite;
        renderer.sortingOrder = 6;
        if (item.golden) renderer.color = new Color(1f, 0.8f, 0.2f);
        return visual;
    }

    private void RefreshVisualPositions()
    {
        float spacing = 0.58f;
        float startX = -(items.Count - 1) * spacing * 0.5f;
        for (int i = 0; i < items.Count; i++)
        {
            items[i].visual.transform.localPosition = new Vector3(startX + i * spacing, i * 0.08f, -i * 0.01f);
            items[i].visual.transform.localRotation = Quaternion.Euler(0f, 0f, (i - 1) * 7f);
        }
    }

    private IReadOnlyList<IngredientType> GetTypes()
    {
        typeView.Clear();
        foreach (PlateItem item in items) typeView.Add(item.type);
        return typeView;
    }
}
