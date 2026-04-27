using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event System.Action OnPotionLoaded;
    public event System.Action OnRoundEvaluated;
    public event System.Action OnGameOver;

    [Header("Setup")]
    public Player player1;
    public Player player2;
    public Cauldron cauldron1;
    public Cauldron cauldron2;

    [Header("Rounds")]
    public int maxRounds = 5;

    [Header("Timing")]
    public float roundEndDelay = 4f;

    public PotionData CurrentPotion  { get; private set; }
    public int        CurrentRound   { get; private set; } = 1;
    public SlotResult[] LastResult1  { get; private set; }
    public SlotResult[] LastResult2  { get; private set; }
    public int[]      LastDeltas     { get; private set; } = new int[2];

    private readonly int[] _totalScores = new int[2];
    private int _filledCount;
    private int _firstFilledOwner = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool IsPlaying { get; private set; }

    private void Start()
    {
        player1.OwnCauldron      = cauldron1;
        player1.OpponentCauldron = cauldron2;
        player2.OwnCauldron      = cauldron2;
        player2.OpponentCauldron = cauldron1;
        cauldron1.ownerIndex = 0;
        cauldron2.ownerIndex = 1;
    }

    public void StartGame()
    {
        if (IsPlaying) return;
        IsPlaying = true;
        LoadNewPotion();
    }

    public void OnCauldronFull(Cauldron c)
    {
        if (_filledCount == 0) _firstFilledOwner = c.ownerIndex;
        _filledCount++;
        if (_filledCount >= 2)
            StartCoroutine(EvaluateRound());
    }

    private IEnumerator EvaluateRound()
    {
        player1.OnRoundStart();
        player2.OnRoundStart();

        int score1 = RecipeEvaluator.Evaluate(cauldron1.Ingredients, CurrentPotion.recipe);
        int score2 = RecipeEvaluator.Evaluate(cauldron2.Ingredients, CurrentPotion.recipe);
        int winnerIndex = score1 > score2 ? 0
                        : score2 > score1 ? 1
                        : _firstFilledOwner; // tie → faster cauldron wins

        int scoreChange = CurrentPotion.effectType == PotionEffect.SubtractScore
            ? -CurrentPotion.scoreValue
            :  CurrentPotion.scoreValue;

        LastDeltas[0] = winnerIndex == 0 ? scoreChange : 0;
        LastDeltas[1] = winnerIndex == 1 ? scoreChange : 0;
        _totalScores[0] += LastDeltas[0];
        _totalScores[1] += LastDeltas[1];

        Player winner = winnerIndex == 0 ? player1 : player2;
        switch (CurrentPotion.effectType)
        {
            case PotionEffect.ScrambleInputs:
                winner.ApplyScramble(CurrentPotion.effectDuration);
                break;
            case PotionEffect.ReduceCooldown:
                winner.ApplyCooldownBoost(1f - CurrentPotion.effectStrength, CurrentPotion.effectDuration);
                break;
        }

        LastResult1 = RecipeEvaluator.EvaluateSlots(cauldron1.Ingredients, CurrentPotion.recipe);
        LastResult2 = RecipeEvaluator.EvaluateSlots(cauldron2.Ingredients, CurrentPotion.recipe);
        AudioManager.Instance?.PlayScore(LastDeltas[0] != 0 ? LastDeltas[0] : LastDeltas[1]);
        OnRoundEvaluated?.Invoke();

        yield return new WaitForSeconds(roundEndDelay);

        if (CurrentRound >= maxRounds)
        {
            IsPlaying = false;
            OnGameOver?.Invoke();
            yield break;
        }

        CurrentRound++;
        LoadNewPotion();
    }

    private void LoadNewPotion()
    {
        _filledCount      = 0;
        _firstFilledOwner = -1;
        cauldron1.Clear();
        cauldron2.Clear();
        CurrentPotion = PotionDatabase.Instance.GetNextPotion();
        AudioManager.Instance?.PlaySpin();
        OnPotionLoaded?.Invoke();
    }

    public int GetScore(int playerIndex) => _totalScores[playerIndex];

    public void Rematch()
    {
        StopAllCoroutines();
        _totalScores[0] = _totalScores[1] = 0;
        CurrentRound    = 1;
        _filledCount      = 0;
        _firstFilledOwner = -1;
        LastDeltas[0]     = LastDeltas[1] = 0;
        LastResult1     = null;
        LastResult2     = null;
        player1.ResetEffects();
        player2.ResetEffects();
        cauldron1.Clear();
        cauldron2.Clear();
        PotionDatabase.Instance.Regenerate();
    }
}
