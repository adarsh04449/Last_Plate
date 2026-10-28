using UnityEngine;

public class FallingIngredient : MonoBehaviour
{
    public IngredientType ingredientType;
    public float fallSpeed = 3f;
    public bool isGolden;
    private bool resolved;
    private float wobbleOffset;

    private void Start() { wobbleOffset = Random.Range(0f, 10f); }

    private void Update()
    {
        float sway = Mathf.Sin(Time.time * 4f + wobbleOffset) * 0.45f;
        transform.Translate(new Vector3(sway, -fallSpeed, 0f) * Time.deltaTime, Space.World);
        transform.Rotate(0f, 0f, (isGolden ? 110f : 45f) * Time.deltaTime);
        if (isGolden)
        {
            float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.08f;
            transform.localScale = Vector3.one * pulse;
        }
        if (transform.position.y < -6.5f) Destroy(gameObject);
    }

    public void CaughtBy(PlateController plate)
    {
        if (resolved) return;
        resolved = true;
        FindAnyObjectByType<OrderManager>().ProcessIngredient(ingredientType, isGolden);
        Destroy(gameObject);
    }
}
