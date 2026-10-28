using UnityEngine;

/// <summary>The plate can phase out catches while tilted, but only while energy remains.</summary>
public class PlateController : MonoBehaviour
{
    public Sprite flatSprite;
    public Sprite tiltedSprite;
    public float maxTiltEnergy = 2.5f;
    public float rechargePerSecond = 1.1f;
    public bool IsTilted { get; private set; }
    public float TiltEnergy { get; private set; }
    public float EnergyNormalized => TiltEnergy / maxTiltEnergy;

    private SpriteRenderer spriteRenderer;
    private bool wasTilted;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        TiltEnergy = maxTiltEnergy;
    }

    private void Update()
    {
        bool playing = GameManager.Instance != null && GameManager.Instance.State == GameState.Playing;
        IsTilted = playing && Input.GetKey(KeyCode.Space) && TiltEnergy > 0.03f;
        TiltEnergy = Mathf.Clamp(TiltEnergy + (IsTilted ? -1f : rechargePerSecond) * Time.deltaTime, 0f, maxTiltEnergy);

        if (IsTilted && !wasTilted)
        {
            FindAnyObjectByType<AudioManager>().PlayTilt();
            GetComponent<PlateInventory>().DiscardLast();
        }
        wasTilted = IsTilted;
        if (tiltedSprite != null && flatSprite != null) spriteRenderer.sprite = IsTilted ? tiltedSprite : flatSprite;
        spriteRenderer.color = IsTilted ? new Color(0.55f, 0.9f, 1f) : Color.white;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0f, 0f, IsTilted ? -22f : 0f), 18f * Time.deltaTime);
    }

    public void ResetEnergy() { TiltEnergy = maxTiltEnergy; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTilted || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        FallingIngredient ingredient = other.GetComponent<FallingIngredient>();
        if (ingredient != null) ingredient.CaughtBy(this);
    }
}
