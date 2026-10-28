using UnityEngine;

/// <summary>Prepares the marked recipe ingredient at the left-side chopping board.</summary>
public class ChoppingStation : MonoBehaviour
{
    public Transform chef;
    public float interactionDistance = 1.45f;

    private UIManager ui;
    private PlateInventory plate;

    private void Start()
    {
        ui = FindAnyObjectByType<UIManager>();
        plate = FindAnyObjectByType<PlateInventory>();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Playing || chef == null)
            return;

        bool nearby = Mathf.Abs(chef.position.x - transform.position.x) <= interactionDistance;
        bool canChop = plate != null && plate.CanChopRequiredIngredient();

        if (ui != null)
        {
            if (nearby && canChop) ui.SetChoppingPrompt("PRESS C TO CHOP", true);
            else if (canChop) ui.SetChoppingPrompt("←  CHOP ITEM", true);
            else if (nearby) ui.SetChoppingPrompt("CATCH CHOP ITEM FIRST", false);
            else ui.SetChoppingPrompt("←  CHOPPING BOARD", false);
        }

        if (nearby && Input.GetKeyDown(KeyCode.C) && plate != null)
            plate.ChopRequiredIngredient();
    }
}
