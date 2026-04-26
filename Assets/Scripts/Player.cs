using UnityEngine;

public class Player : MonoBehaviour
{
    public float baseCooldown = 1f;
    public bool isTargetingOpponent;

    public Cauldron OwnCauldron      { get; set; }
    public Cauldron OpponentCauldron { get; set; }

    public int[]  IngredientOrder       { get; private set; } = { 0, 1, 2, 3 };
    public bool   IsScrambled           => ScrambleRoundsLeft > 0;
    public int    ScrambleRoundsLeft    { get; private set; }
    public float  CooldownMultiplier    { get; private set; } = 1f;
    public bool   HasCooldownBoost      => CooldownBoostRoundsLeft > 0;
    public int    CooldownBoostRoundsLeft { get; private set; }

    private float _cooldownTimer;

    public bool  IsOnCooldown    => _cooldownTimer > 0f;
    public float CooldownFraction => IsOnCooldown ? _cooldownTimer / baseCooldown : 0f;

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;
    }

    public void ToggleTarget()
    {
        isTargetingOpponent = !isTargetingOpponent;
        AudioManager.Instance?.PlaySwitch();
    }


    public void TryAddIngredient(IngredientType ingredient)
    {
        if (_cooldownTimer > 0f) return;
        Cauldron target = isTargetingOpponent ? OpponentCauldron : OwnCauldron;
        target.AddIngredient(ingredient);
        _cooldownTimer = baseCooldown * CooldownMultiplier;
    }

    public void ApplyScramble(int rounds)
    {
        ScrambleRoundsLeft = rounds;
        IngredientOrder = new int[] { 0, 1, 2, 3 };
        Shuffle();
    }

    public void ApplyCooldownBoost(float multiplier, int rounds)
    {
        CooldownMultiplier      = multiplier;
        CooldownBoostRoundsLeft = rounds;
    }

    public void OnRoundStart()
    {
        if (ScrambleRoundsLeft > 0)
        {
            --ScrambleRoundsLeft;
            if (ScrambleRoundsLeft == 0)
                IngredientOrder = new int[] { 0, 1, 2, 3 };
            else
                Shuffle();
        }
        if (CooldownBoostRoundsLeft > 0 && --CooldownBoostRoundsLeft == 0)
            CooldownMultiplier = 1f;
    }

    private void Shuffle()
    {
        for (int i = 3; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (IngredientOrder[i], IngredientOrder[j]) = (IngredientOrder[j], IngredientOrder[i]);
        }
    }
}
