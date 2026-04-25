using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Ingredient sprites — Eye, Mushroom, Root, Crystal")]
    public Sprite[] ingredientSprites;

    [Header("Toggle sprites — [0] self, [1] opponent")]
    public Sprite[] toggleSprites;

    [Header("P1 Cauldron slots (slot 1-4)")]
    public Image[] p1CauldronSlots;

    [Header("P2 Cauldron slots (slot 1-4)")]
    public Image[] p2CauldronSlots;

    [Header("P1 Inventory")]
    public Image[] p1InventorySlots;   // 4 ingredient slots, fixed sprites
    public Image   p1ToggleSlot;       // 5th slot, alternates sprite
    public Image   p1CooldownBar;      // filled image overlaying all 4 slots

    [Header("P2 Inventory")]
    public Image[] p2InventorySlots;
    public Image   p2ToggleSlot;
    public Image   p2CooldownBar;

    [Header("Text")]
    public TextMeshProUGUI recipeText;
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    private bool _subscribed;

    private void Start()
    {
        // Inventory slots always show the same sprite — set once
        for (int i = 0; i < ingredientSprites.Length; i++)
        {
            if (i < p1InventorySlots.Length) p1InventorySlots[i].sprite = ingredientSprites[i];
            if (i < p2InventorySlots.Length) p2InventorySlots[i].sprite = ingredientSprites[i];
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (!_subscribed)
        {
            GameManager.Instance.OnPotionLoaded += RefreshRecipe;
            RefreshRecipe();
            _subscribed = true;
        }

        UpdateCauldronSlots(p1CauldronSlots, GameManager.Instance.cauldron1.Ingredients);
        UpdateCauldronSlots(p2CauldronSlots, GameManager.Instance.cauldron2.Ingredients);

        UpdateInventory(GameManager.Instance.player1, p1ToggleSlot, p1CooldownBar);
        UpdateInventory(GameManager.Instance.player2, p2ToggleSlot, p2CooldownBar);

        if (p1ScoreText != null) p1ScoreText.text = $"P1: {GameManager.Instance.GetScore(0)}";
        if (p2ScoreText != null) p2ScoreText.text = $"P2: {GameManager.Instance.GetScore(1)}";
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnPotionLoaded -= RefreshRecipe;
    }

    private void UpdateCauldronSlots(Image[] slots, System.Collections.Generic.IReadOnlyList<IngredientType> ingredients)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < ingredients.Count)
            {
                slots[i].sprite  = ingredientSprites[(int)ingredients[i]];
                slots[i].enabled = true;
            }
            else
            {
                slots[i].enabled = false;
            }
        }
    }

    private void UpdateInventory(Player player, Image toggleSlot, Image cooldownBar)
    {
        if (toggleSlot != null && toggleSprites.Length >= 2)
            toggleSlot.sprite = player.isTargetingOpponent ? toggleSprites[1] : toggleSprites[0];

        if (cooldownBar != null)
            cooldownBar.fillAmount = player.CooldownFraction;
    }

    private void RefreshRecipe()
    {
        if (recipeText == null) return;
        var potion = GameManager.Instance.CurrentPotion;
        if (potion == null) return;
        recipeText.text = $"Recipe: {string.Join(" → ", potion.recipe.Select(r => r.ToString()))}";
    }
}
