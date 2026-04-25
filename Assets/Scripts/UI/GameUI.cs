using System.Linq;
using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI recipeText;

    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p1CauldronText;
    public TextMeshProUGUI p1ToggleText;

    public TextMeshProUGUI p2ScoreText;
    public TextMeshProUGUI p2CauldronText;
    public TextMeshProUGUI p2ToggleText;

    private bool _subscribed;

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (!_subscribed)
        {
            GameManager.Instance.OnPotionLoaded += RefreshRecipe;
            RefreshRecipe();
            _subscribed = true;
        }

        p1ScoreText.text    = $"P1 Score: {GameManager.Instance.GetScore(0)}";
        p1CauldronText.text = "P1: " + ListToString(GameManager.Instance.cauldron1.Ingredients);
        p1ToggleText.text   = GameManager.Instance.player1.isTargetingOpponent ? "Targeting: OPPONENT" : "Targeting: SELF";

        p2ScoreText.text    = $"P2 Score: {GameManager.Instance.GetScore(1)}";
        p2CauldronText.text = "P2: " + ListToString(GameManager.Instance.cauldron2.Ingredients);
        p2ToggleText.text   = GameManager.Instance.player2.isTargetingOpponent ? "Targeting: OPPONENT" : "Targeting: SELF";
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPotionLoaded -= RefreshRecipe;
    }

    private void RefreshRecipe()
    {
        var potion = GameManager.Instance.CurrentPotion;
        if (potion == null) return;
        recipeText.text = $"Recipe ({potion.potionName}): {string.Join(" → ", potion.recipe)}";
    }

    private string ListToString(System.Collections.Generic.IReadOnlyList<IngredientType> list)
        => list.Count == 0 ? "—" : string.Join(", ", list.Select(i => i.ToString()));
}
