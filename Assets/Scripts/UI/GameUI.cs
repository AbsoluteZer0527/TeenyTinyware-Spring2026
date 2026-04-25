using System.Collections;
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

    [Header("Recipe slots (4 images)")]
    public Image[] recipeSlots;
    [Tooltip("How fast sprites cycle during spin (seconds per frame)")]
    public float spinInterval = 0.07f;
    [Tooltip("When each slot stops, left to right")]
    public float[] slotStopTimes = { 0.7f, 1.0f, 1.3f, 1.6f };

    [Header("P1 Cauldron slots (slot 1-4)")]
    public Image[] p1CauldronSlots;

    [Header("P2 Cauldron slots (slot 1-4)")]
    public Image[] p2CauldronSlots;

    [Header("P1 Inventory")]
    public Image[] p1InventorySlots;
    public Image   p1ToggleSlot;
    public Image   p1CooldownBar;

    [Header("P2 Inventory")]
    public Image[] p2InventorySlots;
    public Image   p2ToggleSlot;
    public Image   p2CooldownBar;

    [Header("Text")]
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    private bool _subscribed;
    private Coroutine _spinCoroutine;

    private void Start()
    {
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
            GameManager.Instance.OnPotionLoaded += OnPotionLoaded;
            OnPotionLoaded();
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
            GameManager.Instance.OnPotionLoaded -= OnPotionLoaded;
    }

    private void OnPotionLoaded()
    {
        var potion = GameManager.Instance.CurrentPotion;
        if (potion == null) return;

        if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
        _spinCoroutine = StartCoroutine(SpinRecipe(potion));
    }

    private IEnumerator SpinRecipe(PotionData potion)
    {
        bool[] stopped = new bool[recipeSlots.Length];
        float elapsed = 0f;
        float nextFlip = 0f;

        while (!stopped[recipeSlots.Length - 1])
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < recipeSlots.Length; i++)
            {
                if (!stopped[i] && elapsed >= slotStopTimes[i])
                {
                    stopped[i] = true;
                    recipeSlots[i].sprite = ingredientSprites[(int)potion.recipe[i]];
                    recipeSlots[i].enabled = true;
                }
            }

            if (elapsed >= nextFlip)
            {
                nextFlip += spinInterval;
                for (int i = 0; i < recipeSlots.Length; i++)
                {
                    if (!stopped[i])
                        recipeSlots[i].sprite = ingredientSprites[Random.Range(0, ingredientSprites.Length)];
                }
            }

            yield return null;
        }
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
}
