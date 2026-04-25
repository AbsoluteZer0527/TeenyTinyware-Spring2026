using System.Collections.Generic;
using UnityEngine;

public class PotionDatabase : MonoBehaviour
{
    public static PotionDatabase Instance { get; private set; }

    public PotionData[] templates;
    public int seed = 42;

    private readonly Queue<PotionData> _queue = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GenerateQueue();
    }

    private void GenerateQueue()
    {
        var rng = new System.Random(seed);
        var values = (IngredientType[])System.Enum.GetValues(typeof(IngredientType));
        int n = values.Length;

        var combos = new List<IngredientType[]>();
        for (int a = 0; a < n; a++)
        for (int b = 0; b < n; b++)
        for (int c = 0; c < n; c++)
        for (int d = 0; d < n; d++)
            combos.Add(new[] { values[a], values[b], values[c], values[d] });

        for (int i = combos.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (combos[i], combos[j]) = (combos[j], combos[i]);
        }

        _queue.Clear();
        foreach (var combo in combos)
        {
            var t = templates[rng.Next(templates.Length)];
            var potion = ScriptableObject.CreateInstance<PotionData>();
            potion.potionName = t.potionName;
            potion.recipe = combo;
            potion.scoreValue = t.scoreValue;
            _queue.Enqueue(potion);
        }
    }

    public PotionData GetNextPotion()
    {
        if (_queue.Count == 0) GenerateQueue();
        return _queue.Dequeue();
    }
}
