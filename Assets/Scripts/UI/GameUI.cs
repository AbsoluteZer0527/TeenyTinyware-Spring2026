using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Ingredient sprites — Eye, Mushroom, Root, Crystal")]
    public Sprite[] ingredientSprites;

    [Header("P1 cauldron slots (slot 1-4)")]
    public Image[] p1Slots;

    [Header("P2 cauldron slots (slot 1-4)")]
    public Image[] p2Slots;

    [Header("Text")]
    public TextMeshProUGUI recipeText;
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

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

        UpdateSlots(p1Slots, GameManager.Instance.cauldron1.Ingredients);
        UpdateSlots(p2Slots, GameManager.Instance.cauldron2.Ingredients);

        if (p1ScoreText != null) p1ScoreText.text = $"P1: {GameManager.Instance.GetScore(0)}";
        if (p2ScoreText != null) p2ScoreText.text = $"P2: {GameManager.Instance.GetScore(1)}";
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPotionLoaded -= RefreshRecipe;
    }

    private void UpdateSlots(Image[] slots, System.Collections.Generic.IReadOnlyList<IngredientType> ingredients)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < ingredients.Count)
            {
                slots[i].sprite = ingredientSprites[(int)ingredients[i]];
                slots[i].enabled = true;
            }
            else
            {
                slots[i].enabled = false;
            }
        }
    }

    private void RefreshRecipe()
    {
        if (recipeText == null) return;
        var potion = GameManager.Instance.CurrentPotion;
        if (potion == null) return;
        recipeText.text = $"Recipe: {string.Join(" → ", potion.recipe.Select(r => r.ToString()))}";
    }
}
