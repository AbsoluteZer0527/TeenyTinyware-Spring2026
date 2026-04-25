using System.Collections.Generic;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
    public int ownerIndex; // 0 = P1, 1 = P2

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip   plopClip;

    private readonly List<IngredientType> _ingredients = new();
    public IReadOnlyList<IngredientType> Ingredients => _ingredients;

    public void AddIngredient(IngredientType ingredient)
    {
        PotionData current = GameManager.Instance.CurrentPotion;
        if (current == null || _ingredients.Count >= current.recipe.Length)
            return;

        _ingredients.Add(ingredient);
        if (audioSource != null && plopClip != null)
            audioSource.PlayOneShot(plopClip);

        if (_ingredients.Count == current.recipe.Length)
            GameManager.Instance.OnCauldronFull(this);
    }

    public void Clear() => _ingredients.Clear();
}
