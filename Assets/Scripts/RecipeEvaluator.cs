using System.Collections.Generic;

public static class RecipeEvaluator
{
    public static int Evaluate(IReadOnlyList<IngredientType> submitted, IngredientType[] recipe)
    {
        int score = 0;
        bool[] recipeMatched = new bool[recipe.Length];
        bool[] submittedMatched = new bool[submitted.Count];

        // Pass 1: Greens — correct ingredient in correct slot
        for (int i = 0; i < recipe.Length && i < submitted.Count; i++)
        {
            if (submitted[i] == recipe[i])
            {
                score += 2;
                recipeMatched[i] = true;
                submittedMatched[i] = true;
            }
        }

        // Pass 2: Yellows — correct ingredient in wrong slot
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
}
