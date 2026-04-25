using UnityEngine;

public class Player : MonoBehaviour
{
    public float baseCooldown = 1f;
    public bool isTargetingOpponent;

    public Cauldron OwnCauldron { get; set; }
    public Cauldron OpponentCauldron { get; set; }

    private float _cooldownTimer;

    public bool IsOnCooldown => _cooldownTimer > 0f;
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
        _cooldownTimer = baseCooldown;
    }
}
