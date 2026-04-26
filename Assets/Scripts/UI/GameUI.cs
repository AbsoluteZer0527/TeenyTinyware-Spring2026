using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    private static readonly Color ColorGreen  = new(0.18f, 0.80f, 0.44f, 0.6f);
    private static readonly Color ColorYellow = new(0.95f, 0.77f, 0.06f, 0.6f);

    [Header("Ingredient sprites — Eye, Mushroom, Root, Crystal")]
    public Sprite[] ingredientSprites;

    [Header("Toggle sprites — [0] self, [1] opponent")]
    public Sprite[] toggleSprites;

    [Header("Potion slot")]
    public Image    potionSlot;
    public Sprite[] allPotionSprites;
    public float    potionSpinDuration = 1.4f;

    [Header("Recipe slots")]
    public Image[] recipeSlots;
    public float   spinInterval  = 0.07f;
    public float[] slotStopTimes = { 0.7f, 1.0f, 1.3f, 1.6f };

    [Header("Round display")]
    public Image[]  roundDigitSlots;
    public Sprite[] roundNumberSprites;
    public float    roundDigitSpacing = 30f;

    [Header("P1 Score")]
    public Image   p1ScoreSign;
    public Image[] p1ScoreDigitSlots;
    public Image   p1DeltaSign;
    public Image[] p1DeltaDigits;

    [Header("P2 Score")]
    public Image   p2ScoreSign;
    public Image[] p2ScoreDigitSlots;
    public Image   p2DeltaSign;
    public Image[] p2DeltaDigits;

    [Header("Delta sign sprites")]
    public Sprite plusSprite;
    public Sprite minusSprite;

    [Header("Score number sprites (0-9)")]
    public Sprite[] scoreNumberSprites;

    [Header("Score counter")]
    public float counterSpeed = 60f;
    public float deltaFadeIn  = 0.2f;
    public float deltaHold    = 1.2f;
    public float deltaFadeOut = 0.5f;

    [Header("Wordle slot flash")]
    public float flashDuration = 0.4f;

    [Header("P1")]
    public Cauldron p1Cauldron;
    public Image[]  p1CauldronSlots;
    public Image[]  p1ScoreSlots;
    public Image[]  p1InventorySlots;
    public Image    p1ToggleSlot;
    public Image    p1CooldownBar;
    public Image    p1ScrambleIcon;
    public Image    p1CooldownIcon;

    [Header("P2")]
    public Cauldron p2Cauldron;
    public Image[]  p2CauldronSlots;
    public Image[]  p2ScoreSlots;
    public Image[]  p2InventorySlots;
    public Image    p2ToggleSlot;
    public Image    p2CooldownBar;
    public Image    p2ScrambleIcon;
    public Image    p2CooldownIcon;

    [Header("Status effect strobe")]
    public float strobeSpeed = 4f;

    [Header("Homepage")]
    public GameObject homepagePanel;

    private static readonly int BirdLoseHash = Animator.StringToHash("Bird_lose");
    private static readonly int CatLoseHash  = Animator.StringToHash("Cat_lose");

    private float _roundCenterX;
    private float _roundSlotsY;
    private bool      _subscribed;
    private Coroutine _spinCoroutine;
    private Coroutine _potionSpinCoroutine;
    private float     _displayedScore1;
    private float     _displayedScore2;

    private void Start()
    {
        if (homepagePanel != null) homepagePanel.SetActive(true);
        ResetScoreSlots(p1ScoreSlots);
        ResetScoreSlots(p2ScoreSlots);
        SetDeltaGroupAlpha(p1DeltaSign, p1DeltaDigits, 0f);
        SetDeltaGroupAlpha(p2DeltaSign, p2DeltaDigits, 0f);

        if (roundDigitSlots != null && roundDigitSlots.Length >= 2)
        {
            float x0 = roundDigitSlots[0].rectTransform.anchoredPosition.x;
            float x1 = roundDigitSlots[1].rectTransform.anchoredPosition.x;
            _roundCenterX = (x0 + x1) * 0.5f;
            _roundSlotsY  = roundDigitSlots[0].rectTransform.anchoredPosition.y;
        }
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
        UpdateInventory(GameManager.Instance.player1, p1InventorySlots, p1ToggleSlot, p1CooldownBar, false);
        UpdateInventory(GameManager.Instance.player2, p2InventorySlots, p2ToggleSlot, p2CooldownBar, true);
        UpdateStatusIcons(GameManager.Instance.player1, p1ScrambleIcon, p1CooldownIcon);
        UpdateStatusIcons(GameManager.Instance.player2, p2ScrambleIcon, p2CooldownIcon);

        int actual1 = GameManager.Instance.GetScore(0);
        int actual2 = GameManager.Instance.GetScore(1);
        _displayedScore1 = Mathf.MoveTowards(_displayedScore1, actual1, counterSpeed * Time.deltaTime);
        _displayedScore2 = Mathf.MoveTowards(_displayedScore2, actual2, counterSpeed * Time.deltaTime);
        int rounded1 = Mathf.RoundToInt(_displayedScore1);
        int rounded2 = Mathf.RoundToInt(_displayedScore2);
        DisplayScore(p1ScoreDigitSlots, scoreNumberSprites, rounded1);
        DisplayScore(p2ScoreDigitSlots, scoreNumberSprites, rounded2);
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

        DisplayRound(GameManager.Instance.CurrentRound);
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

        if (GameManager.Instance.CurrentPotion.effectType == PotionEffect.SubtractScore)
        {
            if (delta1 != 0)
            {
                var anim1 = GameManager.Instance.player1.GetComponent<Animator>();
                if (anim1 != null) anim1.SetTrigger(BirdLoseHash);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.p1LoseClip);
            }
            if (delta2 != 0)
            {
                var anim2 = GameManager.Instance.player2.GetComponent<Animator>();
                if (anim2 != null) anim2.SetTrigger(CatLoseHash);
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.p2LoseClip);
            }
        }
    }

    public void OnPlayPressed()
    {
        if (homepagePanel != null) homepagePanel.SetActive(false);
        GameManager.Instance.StartGame();
    }

    // ── Score / round display ────────────────────────────────────────────

    private void DisplayRound(int round)
    {
        if (roundDigitSlots == null || roundDigitSlots.Length < 2 || roundNumberSprites == null) return;
        bool twoDigit = round >= 10;
        float half = roundDigitSpacing * 0.5f;

        roundDigitSlots[0].enabled = twoDigit;
        roundDigitSlots[0].sprite  = roundNumberSprites[round / 10 % 10];
        roundDigitSlots[0].rectTransform.anchoredPosition = new Vector2(twoDigit ? _roundCenterX - half : _roundCenterX, _roundSlotsY);

        roundDigitSlots[1].enabled = true;
        roundDigitSlots[1].sprite  = roundNumberSprites[round % 10];
        roundDigitSlots[1].rectTransform.anchoredPosition = new Vector2(twoDigit ? _roundCenterX + half : _roundCenterX, _roundSlotsY);
    }

    private void DisplayScore(Image[] slots, Sprite[] sprites, int number)
    {
        if (slots == null || slots.Length == 0 || sprites == null || sprites.Length < 10) return;
        string digits = Mathf.Abs(number).ToString().PadLeft(slots.Length, '0');
        if (digits.Length > slots.Length) digits = digits[^slots.Length..];
        bool significant = false;
        for (int i = 0; i < slots.Length; i++)
        {
            bool lastSlot     = i == slots.Length - 1;
            bool leadingZero  = !significant && digits[i] == '0' && !lastSlot;
            if (leadingZero) { slots[i].enabled = false; continue; }
            significant      = true;
            slots[i].enabled = true;
            slots[i].sprite  = sprites[digits[i] - '0'];
        }
    }

    private void DisplayNumber(Image[] slots, Sprite[] sprites, int number)
    {
        if (slots == null || slots.Length == 0 || sprites == null || sprites.Length < 10) return;
        string digits = Mathf.Abs(number).ToString().PadLeft(slots.Length, '0');
        if (digits.Length > slots.Length) digits = digits[^slots.Length..];
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
        Color bright  = new(target.r, target.g, target.b, 1f);
        float elapsed = 0f;
        while (elapsed < flashDuration) { elapsed += Time.deltaTime; slot.color = Color.Lerp(bright, target, elapsed / flashDuration); yield return null; }
        slot.color = target;
    }

    private static void ResetScoreSlots(Image[] slots) { foreach (var s in slots) s.enabled = false; }

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
            if (elapsed >= nextFlip)
            {
                nextFlip += spinInterval;
                for (int i = 0; i < recipeSlots.Length; i++)
                    if (!stopped[i]) recipeSlots[i].sprite = ingredientSprites[Random.Range(0, ingredientSprites.Length)];
            }
            yield return null;
        }
    }

    // ── Per-frame slot updates ───────────────────────────────────────────

    private void UpdateCauldronSlots(Image[] slots, IReadOnlyList<IngredientType> ingredients)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < ingredients.Count) { slots[i].sprite = ingredientSprites[(int)ingredients[i]]; slots[i].enabled = true; }
            else slots[i].enabled = false;
        }
    }

    private void UpdateInventory(Player player, Image[] inventorySlots, Image toggleSlot, Image cooldownBar, bool invertToggle = false)
    {
        for (int i = 0; i < inventorySlots.Length && i < ingredientSprites.Length; i++)
            inventorySlots[i].sprite = ingredientSprites[player.IngredientOrder[i]];

        if (toggleSlot != null && toggleSprites.Length >= 2)
        {
            bool targeting = invertToggle ? !player.isTargetingOpponent : player.isTargetingOpponent;
            toggleSlot.sprite = targeting ? toggleSprites[1] : toggleSprites[0];
        }
        if (cooldownBar != null)
            cooldownBar.fillAmount = player.CooldownFraction;
    }

    private void UpdateStatusIcons(Player player, Image scrambleIcon, Image cooldownIcon)
    {
        if (scrambleIcon != null)
        {
            scrambleIcon.enabled = player.IsScrambled;
            if (player.IsScrambled)
            {
                float alpha = (Mathf.Sin(Time.time * strobeSpeed) + 1f) * 0.5f;
                var c = scrambleIcon.color; c.a = alpha; scrambleIcon.color = c;
            }
        }
        if (cooldownIcon != null)
        {
            cooldownIcon.enabled = player.HasCooldownBoost;
            if (player.HasCooldownBoost)
            {
                float alpha = (Mathf.Sin(Time.time * strobeSpeed) + 1f) * 0.5f;
                var c = cooldownIcon.color; c.a = alpha; cooldownIcon.color = c;
            }
        }
    }
}
