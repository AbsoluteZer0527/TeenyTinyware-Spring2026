using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class PlayerInputActions
{
    public InputAction ingredient1;
    public InputAction ingredient2;
    public InputAction ingredient3;
    public InputAction ingredient4;
    public InputAction toggle;

    public void Enable()
    {
        ingredient1.Enable(); ingredient2.Enable();
        ingredient3.Enable(); ingredient4.Enable();
        toggle.Enable();
    }

    public void Disable()
    {
        ingredient1.Disable(); ingredient2.Disable();
        ingredient3.Disable(); ingredient4.Disable();
        toggle.Disable();
    }
}

public class InputController : MonoBehaviour
{
    public Player player1;
    public PlayerInputActions p1Actions;

    public Player player2;
    public PlayerInputActions p2Actions;

    private static readonly IngredientType[] _ingredientOrder =
    {
        IngredientType.Eye,
        IngredientType.Mushroom,
        IngredientType.Root,
        IngredientType.Crystal
    };

    private void OnEnable()  { p1Actions.Enable();  p2Actions.Enable(); }
    private void OnDisable() { p1Actions.Disable(); p2Actions.Disable(); }

    private void Update()
    {
        HandlePlayer(player1, p1Actions);
        HandlePlayer(player2, p2Actions);
    }

    private void HandlePlayer(Player player, PlayerInputActions actions)
    {
        if (actions.ingredient1.triggered) player.TryAddIngredient(_ingredientOrder[0]);
        if (actions.ingredient2.triggered) player.TryAddIngredient(_ingredientOrder[1]);
        if (actions.ingredient3.triggered) player.TryAddIngredient(_ingredientOrder[2]);
        if (actions.ingredient4.triggered) player.TryAddIngredient(_ingredientOrder[3]);
        if (actions.toggle.triggered)      player.ToggleTarget();
    }
}
