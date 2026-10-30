using UnityEngine;

public class ChefReactionController : MonoBehaviour
{
    public Sprite readySprite;
    public Sprite worriedSprite;
    public Sprite celebrationSprite;
    public float worriedDuration = 0.8f;
    public float celebrationDuration = 1.05f;

    private SpriteRenderer spriteRenderer;
    private float reactionEndsAt;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (reactionEndsAt > 0f && Time.unscaledTime >= reactionEndsAt)
        {
            reactionEndsAt = 0f;
            SetSprite(readySprite);
        }
    }

    public void ShowWorried()
    {
        SetSprite(worriedSprite);
        reactionEndsAt = Time.unscaledTime + worriedDuration;
    }

    public void ShowCelebration()
    {
        SetSprite(celebrationSprite);
        reactionEndsAt = Time.unscaledTime + celebrationDuration;
    }

    public void ResetPose()
    {
        reactionEndsAt = 0f;
        SetSprite(readySprite);
    }

    private void SetSprite(Sprite sprite)
    {
        if (spriteRenderer != null && sprite != null) spriteRenderer.sprite = sprite;
    }
}
