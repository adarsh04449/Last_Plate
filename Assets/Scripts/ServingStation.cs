using UnityEngine;

/// <summary>Delivers a completed plate when the chef reaches the right-side window.</summary>
public class ServingStation : MonoBehaviour
{
    public Transform chef;
    public float interactionDistance = 1.45f;

    private UIManager ui;
    private OrderManager orders;
    private PlateInventory plate;

    private void Start()
    {
        ui = FindAnyObjectByType<UIManager>();
        orders = FindAnyObjectByType<OrderManager>();
        plate = FindAnyObjectByType<PlateInventory>();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing || chef == null)
            return;

        bool nearby = Mathf.Abs(chef.position.x - transform.position.x) <= interactionDistance;
        bool ready = orders != null && orders.IsPlateReady;

        if (ui != null)
        {
            if (nearby && ready) ui.SetServingPrompt("PRESS E TO SERVE", true);
            else if (ready) ui.SetServingPrompt("ORDER READY  →", true);
            else if (nearby) ui.SetServingPrompt("FINISH ORDER FIRST", false);
            else ui.SetServingPrompt("SERVING WINDOW  →", false);
        }

        if (nearby && Input.GetKeyDown(KeyCode.E) && orders != null && plate != null)
        {
            if (orders.TryServe(plate.CurrentTypes, plate.CurrentPrepared)) plate.Clear();
        }
    }
}
