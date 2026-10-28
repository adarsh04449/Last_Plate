using UnityEngine;

public class ChefController : MonoBehaviour
{
    public float moveSpeed = 8f;
    public float horizontalLimit = 8.3f;
    private float velocity;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing) return;
        float input = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) input = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input = 1f;
        velocity = Mathf.MoveTowards(velocity, input * moveSpeed, 28f * Time.deltaTime);
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x + velocity * Time.deltaTime, -horizontalLimit, horizontalLimit);
        transform.position = position;
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0f, 0f, -velocity * 1.2f), 10f * Time.deltaTime);
    }
}
