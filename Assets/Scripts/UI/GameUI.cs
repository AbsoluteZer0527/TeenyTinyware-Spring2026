using System.Collections;
using System.Linq;
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

    [Header("Potion slot")]
    public Image   potionSlot;
    public Sprite[] allPotionSprites;
    public float   potionSpinDuration = 1.4f;

    [Header("Recipe slots")]
    public Image[] recipeSlots;
    public float   spinInterval  = 0.07f;
    public float[] slotStopTimes = { 0.7f, 1.0f, 1.3f, 1.6f };

    [Header("Round display")]
    public Image[]  roundDigitSlots;
    public Sprite[] roundNumberSprites; // 0-9 from 'Round numbers.png'

    [Header("P1 Score")]
    public Image    p1ScoreSign;        // hidden when positive, shows minus when negative
    public Image[]  p1ScoreDigitSlots;
    public Image    p1DeltaSign;
    public Image[]  p1DeltaDigits;

    [Header("P2 Score")]
    public Image    p2ScoreSign;
    public Image[]  p2ScoreDigitSlots;
    public Image    p2DeltaSign;
    public Image[]  p2DeltaDigits;

    [Header("Delta sign sprites")]
    public Sprite plusSprite;
    public Sprite minusSprite;

    [Header("Score number sprites (0-9 from 'Numbers.png')")]
    public Sprite[] scoreNumberSprites;

    [Header("Score counter")]
    public float counterSpeed     = 60f;  // digits per second
    public float deltaFadeIn      = 0.2f;
    public float deltaHold        = 1.2f;
    public float deltaFadeOut     = 0.5f;

    [Header("Wordle slot flash")]
    public float flashDuration = 0.4f;

    [Header("P1")]
    public Cauldron p1Cauldron;
    public Image[]  p1CauldronSlots;
    public Image[]  p1ScoreSlots;
    public Image[]  p1InventorySlots;
    public Image    p1ToggleSlot;
    public Image    p1CooldownBar;

    [Header("P2")]
    public Cauldron p2Cauldron;
    public Image[]  p2CauldronSlots;
    public Image[]  p2ScoreSlots;
    public Image[]  p2InventorySlots;
    public Image    p2ToggleSlot;
    public Image    p2CooldownBar;

    private bool _subscribed;
    private Coroutine _spinCoroutine;
    private Coroutine _potionSpinCoroutine;

    private float _displayedScore1;
    private float _displayedScore2;

    private void Start()
    {
        for (int i = 0; i < ingredientSprites.Length; i++)
        {
            if (i < p1InventorySlots.Length) p1InventorySlots[i].sprite = ingredientSprites[i];
            if (i < p2InventorySlots.Length) p2InventorySlots[i].sprite = ingredientSprites[i];
        }
        ResetScoreSlots(p1ScoreSlots);
        ResetScoreSlots(p2ScoreSlots);
        SetDeltaGroupAlpha(p1DeltaSign, p1DeltaDigits, 0f);
        SetDeltaGroupAlpha(p2DeltaSign, p2DeltaDigits, 0f);
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        if (!_subscribed)
        {
            GameManager.Instance.OnPotionLoaded   += OnPotionLoaded;
            GameManager.Instance.OnRoundEvaluated += OnRoundEvaluated;
            OnPotionLoaded();
            _subscribed = true;
        }

        UpdateCauldronSlots(p1CauldronSlots, p1Cauldron.Ingredients);
        UpdateCauldronSlots(p2CauldronSlots, p2Cauldron.Ingredients);
        UpdateInventory(GameManager.Instance.player1, p1ToggleSlot, p1CooldownBar, false);
        UpdateInventory(GameManager.Instance.player2, p2ToggleSlot, p2CooldownBar, true);

        // Animate score counters toward actual values
        int actual1 = GameManager.Instance.GetScore(0);
        int actual2 = GameManager.Instance.GetScore(1);
        _displayedScore1 = Mathf.MoveTowards(_displayedScore1, actual1, counterSpeed * Time.deltaTime);
        _displayedScore2 = Mathf.MoveTowards(_displayedScore2, actual2, counterSpeed * Time.deltaTime);
        int rounded1 = Mathf.RoundToInt(_displayedScore1);
        int rounded2 = Mathf.RoundToInt(_displayedScore2);
        DisplayNumber(p1ScoreDigitSlots, scoreNumberSprites, rounded1);
        DisplayNumber(p2ScoreDigitSlots, scoreNumberSprites, rounded2);
        if (p1ScoreSign != null) { p1ScoreSign.sprite = minusSprite; p1ScoreSign.enabled = rounded1 < 0; }
        if (p2ScoreSign != null) { p2ScoreSign.sprite = minusSprite; p2ScoreSign.enabled = rounded2 < 0; }
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

        DisplayNumber(roundDigitSlots, roundNumberSprites, GameManager.Instance.CurrentRound);
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

        int delta1 = GameManager.Instance.LastDeltas[0];
        int delta2 = GameManager.Instance.LastDeltas[1];
        if (delta1 != 0) StartCoroutine(ShowDelta(p1DeltaSign, p1DeltaDigits, delta1));
        if (delta2 != 0) StartCoroutine(ShowDelta(p2DeltaSign, p2DeltaDigits, delta2));
    }

    // ── Score / round display ────────────────────────────────────────────

    private void DisplayNumber(Image[] slots, Sprite[] sprites, int number)
    {
        if (slots == null || slots.Length == 0 || sprites == null || sprites.Length < 10) return;
        string digits = Mathf.Abs(number).ToString().PadLeft(slots.Length, '0');
        // Take only the rightmost N digits if number overflows slot count
        if (digits.Length > slots.Length) digits = digits.Substring(digits.Length - slots.Length);
        for (int i = 0; i < slots.Length; i++)
            slots[i].sprite = sprites[digits[i] - '0'];
    }

    private IEnumerator ShowDelta(Image sign, Image[] digits, int delta)
    {
        if (sign == null) yield break;
        sign.sprite = delta >= 0 ? plusSprite : minusSprite;
        DisplayNumber(digits, scoreNumberSprites, Mathf.Abs(delta));

        float t = 0f;
        while (t < deltaFadeIn)  { t += Time.deltaTime; SetDeltaGroupAlpha(sign, digits, t / deltaFadeIn); yield return null; }
        SetDeltaGroupAlpha(sign, digits, 1f);

        yield return new WaitForSeconds(deltaHold);

        t = 0f;
        while (t < deltaFadeOut) { t += Time.deltaTime; SetDeltaGroupAlpha(sign, digits, 1f - t / deltaFadeOut); yield return null; }
        SetDeltaGroupAlpha(sign, digits, 0f);
    }

    private void SetDeltaGroupAlpha(Image sign, Image[] digits, float a)
    {
        SetImageAlpha(sign, a);
        if (digits != null) foreach (var d in digits) SetImageAlpha(d, a);
    }

    private static void SetImageAlpha(Image img, float a)
    {
        if (img == null) return;
        var c = img.color; c.a = a; img.color = c;
    }

    // ── Wordle slot coloring ─────────────────────────────────────────────

    private void ApplySlotColors(Image[] slots, SlotResult[] results)
    {
        if (results == null) return;
        for (int i = 0; i < slots.Length && i < results.Length; i++)
        {
            if (results[i] == SlotResult.None) continue;
            StartCoroutine(FlashSlot(slots[i], results[i] == SlotResult.Green ? ColorGreen : ColorYellow));
        }
    }

    private IEnumerator FlashSlot(Image slot, Color target)
    {
        slot.enabled = true;
        Color bright  = new Color(target.r, target.g, target.b, 1f);
        float elapsed = 0f;
        while (elapsed < flashDuration) { elapsed += Time.deltaTime; slot.color = Color.Lerp(bright, target, elapsed / flashDuration); yield return null; }
        slot.color = target;
    }

    private void ResetScoreSlots(Image[] slots) { foreach (var s in slots) s.enabled = false; }

    // ── Spin animations ──────────────────────────────────────────────────

    private IEnumerator SpinPotion(PotionData potion)
    {
        if (potionSlot == null || allPotionSprites.Length == 0) yield break;
        float elapsed = 0f, nextFlip = 0f;
        while (elapsed < potionSpinDuration)
        {
            elapsed += Time.deltaTime;
            if (elapsed >= nextFlip) { nextFlip += spinInterval; potionSlot.sprite = allPotionSprites[Random.Range(0, allPotionSprites.Length)]; }
            yield return null;
        }
        if (potion.potionSprite != null) potionSlot.sprite = potion.potionSprite;
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
            if (elapsed >= nextFlip) { nextFlip += spinInterval; for (int i = 0; i < recipeSlots.Length; i++) if (!stopped[i]) recipeSlots[i].sprite = ingredientSprites[Random.Range(0, ingredientSprites.Length)]; }
            yield return null;
        }
    }

    // ── Per-frame slot updates ───────────────────────────────────────────

    private void UpdateCauldronSlots(Image[] slots, System.Collections.Generic.IReadOnlyList<IngredientType> ingredients)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < ingredients.Count) { slots[i].sprite = ingredientSprites[(int)ingredients[i]]; slots[i].enabled = true; }
            else slots[i].enabled = false;
        }
    }

    private void UpdateInventory(Player player, Image toggleSlot, Image cooldownBar, bool invertToggle = false)
    {
        if (toggleSlot != null && toggleSprites.Length >= 2)
        {
            bool targeting = invertToggle ? !player.isTargetingOpponent : player.isTargetingOpponent;
            toggleSlot.sprite = targeting ? toggleSprites[1] : toggleSprites[0];
        }
        if (cooldownBar != null)
            cooldownBar.fillAmount = player.CooldownFraction;
    }
}
