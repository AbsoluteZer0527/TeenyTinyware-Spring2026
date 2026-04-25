using System.Collections.Generic;

public static class RecipeEvaluator
{
    public static int Evaluate(IReadOnlyList<IngredientType> submitted, IngredientType[] recipe)
    {
        int score = 0;
        bool[] recipeMatched = new bool[recipe.Length];
        bool[] submittedMatched = new bool[submitted.Count];

        for (int i = 0; i < recipe.Length && i < submitted.Count; i++)
        {
            if (submitted[i] == recipe[i])
            {
                score += 2;
                recipeMatched[i] = true;
                submittedMatched[i] = true;
            }
        }

        for (int i = 0; i < submitted.Count; i++)
        {
            if (submittedMatched[i]) continue;
            for (int j = 0; j < recipe.Length; j++)
            {
                if (!recipeMatched[j] && submitted[i] == recipe[j])
                {
                    score += 1;
                    recipeMatched[j] = true;
                    submittedMatched[i] = true;
                    break;
                }
            }
        }

        return score;
    }

    public static SlotResult[] EvaluateSlots(IReadOnlyList<IngredientType> submitted, IngredientType[] recipe)
    {
        var results = new SlotResult[recipe.Length];
        bool[] recipeMatched = new bool[recipe.Length];
        bool[] submittedMatched = new bool[submitted.Count];

        for (int i = 0; i < recipe.Length && i < submitted.Count; i++)
        {
            if (submitted[i] == recipe[i])
            {
                results[i] = SlotResult.Green;
                recipeMatched[i] = true;
                submittedMatched[i] = true;
            }
        }

        for (int i = 0; i < submitted.Count; i++)
        {
            if (submittedMatched[i]) continue;
            for (int j = 0; j < recipe.Length; j++)
            {
                if (!recipeMatched[j] && submitted[i] == recipe[j])
                {
                    results[i] = SlotResult.Yellow;
                    recipeMatched[j] = true;
                    submittedMatched[i] = true;
                    break;
                }
            }
        }

        return results;
    }
}
