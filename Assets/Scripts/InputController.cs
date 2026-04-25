using UnityEngine;

public class InputController : MonoBehaviour
{
    public Player player1;
    public Player player2;

    // Maps Player 1's ingredient keys (1-4) to IngredientType values 0-3
    private static readonly IngredientType[] _ingredientOrder =
    {
        IngredientType.Eye,
        IngredientType.Mushroom,
        IngredientType.Root,
        IngredientType.Crystal
    };

    private void Update()
    {
        // Player 1: keys 1-4 = ingredients, 5 = toggle target
        if (Input.GetKeyDown(KeyCode.Alpha1)) player1.TryAddIngredient(_ingredientOrder[0]);
        if (Input.GetKeyDown(KeyCode.Alpha2)) player1.TryAddIngredient(_ingredientOrder[1]);
        if (Input.GetKeyDown(KeyCode.Alpha3)) player1.TryAddIngredient(_ingredientOrder[2]);
        if (Input.GetKeyDown(KeyCode.Alpha4)) player1.TryAddIngredient(_ingredientOrder[3]);
        if (Input.GetKeyDown(KeyCode.Alpha5)) player1.ToggleTarget();

        // Player 2: keys 6-9 = ingredients, 0 = toggle target
        if (Input.GetKeyDown(KeyCode.Alpha6)) player2.TryAddIngredient(_ingredientOrder[0]);
        if (Input.GetKeyDown(KeyCode.Alpha7)) player2.TryAddIngredient(_ingredientOrder[1]);
        if (Input.GetKeyDown(KeyCode.Alpha8)) player2.TryAddIngredient(_ingredientOrder[2]);
        if (Input.GetKeyDown(KeyCode.Alpha9)) player2.TryAddIngredient(_ingredientOrder[3]);
        if (Input.GetKeyDown(KeyCode.Alpha0)) player2.ToggleTarget();
    }
}
