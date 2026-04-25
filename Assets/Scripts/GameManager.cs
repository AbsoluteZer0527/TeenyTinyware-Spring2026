using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event System.Action OnPotionLoaded;

    [Header("Setup")]
    public Player player1;
    public Player player2;
    public Cauldron cauldron1;
    public Cauldron cauldron2;

    public PotionData CurrentPotion { get; private set; }

    private readonly int[] _totalScores = new int[2];
    private bool _evaluating;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        player1.OwnCauldron = cauldron1;
        player1.OpponentCauldron = cauldron2;
        player2.OwnCauldron = cauldron2;
        player2.OpponentCauldron = cauldron1;
        cauldron1.ownerIndex = 0;
        cauldron2.ownerIndex = 1;
        LoadNewPotion();
    }

    public void OnCauldronFull(Cauldron _)
    {
        if (_evaluating) return;
        _evaluating = true;
        EvaluateRound();
    }

    private void EvaluateRound()
    {
        int score1 = RecipeEvaluator.Evaluate(cauldron1.Ingredients, CurrentPotion.recipe);
        int score2 = RecipeEvaluator.Evaluate(cauldron2.Ingredients, CurrentPotion.recipe);
        int winnerIndex = score2 > score1 ? 1 : 0;
        _totalScores[winnerIndex] += CurrentPotion.scoreValue;
        LoadNewPotion();
    }

    private void LoadNewPotion()
    {
        cauldron1.Clear();
        cauldron2.Clear();
        CurrentPotion = PotionDatabase.Instance.GetNextPotion();
        _evaluating = false;
        OnPotionLoaded?.Invoke();
    }

    public int GetScore(int playerIndex) => _totalScores[playerIndex];
}
