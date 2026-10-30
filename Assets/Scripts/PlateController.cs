using UnityEngine;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LastPlate.PlayModeTests")]

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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        TiltEnergy = maxTiltEnergy;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        AdvanceTilt(Input.GetKey(KeyCode.Space), Time.deltaTime);
    }

    internal void AdvanceTilt(bool held, float deltaTime)
    {
        bool wasTilted = IsTilted;
        IsTilted = held && TiltEnergy > 0f;
        if (IsTilted && !wasTilted)
        {
            FindAnyObjectByType<AudioManager>().PlayTilt();
            GetComponent<PlateInventory>().DiscardLast();
        }
        // Holding an empty plate must not recharge it and repeatedly restart tilt.
        TiltEnergy = Mathf.Clamp(TiltEnergy + (held ? -1f : rechargePerSecond) * deltaTime, 0f, maxTiltEnergy);
        if (TiltEnergy == 0f) IsTilted = false;
        if (tiltedSprite != null && flatSprite != null) spriteRenderer.sprite = IsTilted ? tiltedSprite : flatSprite;
        spriteRenderer.color = IsTilted ? new Color(0.55f, 0.9f, 1f) : Color.white;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0f, 0f, IsTilted ? -22f : 0f), 18f * deltaTime);
    }

    public void ResetEnergy()
    {
        TiltEnergy = maxTiltEnergy;
        IsTilted = false;
        transform.localRotation = Quaternion.identity;
        spriteRenderer.sprite = flatSprite;
        spriteRenderer.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsTilted || GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        FallingIngredient ingredient = other.GetComponent<FallingIngredient>();
        if (ingredient != null) ingredient.CaughtBy(this);
    }
}
