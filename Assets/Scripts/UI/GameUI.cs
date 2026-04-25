using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private static readonly Color ColorGreen  = new Color(0.18f, 0.80f, 0.44f, 0.6f);
    private static readonly Color ColorYellow = new Color(0.95f, 0.77f, 0.06f, 0.6f);

    [Header("Ingredient sprites — Eye, Mushroom, Root, Crystal")]
    public Sprite[] ingredientSprites;

    [Header("Toggle sprites — [0] self, [1] opponent")]
    public Sprite[] toggleSprites;

    [Header("Potion slot (1 image)")]
    public Image potionSlot;
    public Sprite[] allPotionSprites;
    public float potionSpinDuration = 1.4f;

    [Header("Recipe slots (4 images)")]
    public Image[] recipeSlots;
    public float spinInterval = 0.07f;
    public float[] slotStopTimes = { 0.7f, 1.0f, 1.3f, 1.6f };

    [Header("Score slot flash")]
    public float flashDuration = 0.4f;

    [Header("P1")]
    public Image[] p1CauldronSlots;
    public Image[] p1ScoreSlots;
    public Image[] p1InventorySlots;
    public Image   p1ToggleSlot;
    public Image   p1CooldownBar;

    [Header("P2")]
    public Image[] p2CauldronSlots;
    public Image[] p2ScoreSlots;
    public Image[] p2InventorySlots;
    public Image   p2ToggleSlot;
    public Image   p2CooldownBar;

    [Header("Text")]
    public TextMeshProUGUI p1ScoreText;
    public TextMeshProUGUI p2ScoreText;

    private bool _subscribed;
    private Coroutine _spinCoroutine;
    private Coroutine _potionSpinCoroutine;

    private void Start()
    {
        for (int i = 0; i < ingredientSprites.Length; i++)
        {
            if (i < p1InventorySlots.Length) p1InventorySlots[i].sprite = ingredientSprites[i];
            if (i < p2InventorySlots.Length) p2InventorySlots[i].sprite = ingredientSprites[i];
        }

        ResetScoreSlots(p1ScoreSlots);
        ResetScoreSlots(p2ScoreSlots);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (!_subscribed)
        {
            GameManager.Instance.OnPotionLoaded    += OnPotionLoaded;
            GameManager.Instance.OnRoundEvaluated  += OnRoundEvaluated;
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
        if (GameManager.Instance == null) return;
        GameManager.Instance.OnPotionLoaded   -= OnPotionLoaded;
        GameManager.Instance.OnRoundEvaluated -= OnRoundEvaluated;
    }

    private void OnPotionLoaded()
    {
        var potion = GameManager.Instance.CurrentPotion;
        if (potion == null) return;

        ResetScoreSlots(p1ScoreSlots);
        ResetScoreSlots(p2ScoreSlots);

        if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
        _spinCoroutine = StartCoroutine(SpinRecipe(potion));

        if (_potionSpinCoroutine != null) StopCoroutine(_potionSpinCoroutine);
        _potionSpinCoroutine = StartCoroutine(SpinPotion(potion));
    }

    private void OnRoundEvaluated()
    {
        ApplySlotColors(p1ScoreSlots, GameManager.Instance.LastResult1);
        ApplySlotColors(p2ScoreSlots, GameManager.Instance.LastResult2);
    }

    private void ApplySlotColors(Image[] slots, SlotResult[] results)
    {
        if (results == null) return;
        for (int i = 0; i < slots.Length && i < results.Length; i++)
        {
            if (results[i] == SlotResult.None) continue;
            Color target = results[i] == SlotResult.Green ? ColorGreen : ColorYellow;
            StartCoroutine(FlashSlot(slots[i], target));
        }
    }

    private IEnumerator FlashSlot(Image slot, Color target)
    {
        slot.enabled = true;
        Color bright = new Color(target.r, target.g, target.b, 1f);
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            slot.color = Color.Lerp(bright, target, elapsed / flashDuration);
            yield return null;
        }
        slot.color = target;
    }

    private void ResetScoreSlots(Image[] slots)
    {
        foreach (var s in slots) s.enabled = false;
    }

    private IEnumerator SpinPotion(PotionData potion)
    {
        if (potionSlot == null || allPotionSprites.Length == 0) yield break;

        float elapsed = 0f, nextFlip = 0f;
        while (elapsed < potionSpinDuration)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= nextFlip)
            {
                nextFlip += spinInterval;
                potionSlot.sprite = allPotionSprites[Random.Range(0, allPotionSprites.Length)];
            }
            yield return null;
        }
        if (potion.potionSprite != null)
            potionSlot.sprite = potion.potionSprite;
    }

    private IEnumerator SpinRecipe(PotionData potion)
    {
        bool[] stopped = new bool[recipeSlots.Length];
        float elapsed = 0f, nextFlip = 0f;

        while (!stopped[recipeSlots.Length - 1])
        {
            elapsed += Time.deltaTime;

            for (int i = 0; i < recipeSlots.Length; i++)
            {
                if (!stopped[i] && elapsed >= slotStopTimes[i])
                {
                    stopped[i] = true;
                    recipeSlots[i].sprite  = ingredientSprites[(int)potion.recipe[i]];
                    recipeSlots[i].enabled = true;
                }
            }

            if (elapsed >= nextFlip)
            {
                nextFlip += spinInterval;
                for (int i = 0; i < recipeSlots.Length; i++)
                    if (!stopped[i])
                        recipeSlots[i].sprite = ingredientSprites[Random.Range(0, ingredientSprites.Length)];
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
