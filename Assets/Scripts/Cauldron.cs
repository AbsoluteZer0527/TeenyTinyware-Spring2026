using System.Collections.Generic;
using UnityEngine;

public class Cauldron : MonoBehaviour
{
    public int ownerIndex; // 0 = P1, 1 = P2

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip   plopClip;
    public AudioClip   plop2Clip;
    public AudioClip   plop3Clip;

    private int _plopIndex;
    private readonly List<IngredientType> _ingredients = new();
    public IReadOnlyList<IngredientType> Ingredients => _ingredients;

    public void AddIngredient(IngredientType ingredient)
    {
        PotionData current = GameManager.Instance.CurrentPotion;
        if (current == null || _ingredients.Count >= current.recipe.Length)
            return;

        _ingredients.Add(ingredient);
        AudioClip[] plops = { plopClip, plop2Clip, plop3Clip };
        AudioClip clip = plops[_plopIndex % plops.Length] ?? plopClip;
        _plopIndex++;
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);

        if (_ingredients.Count == current.recipe.Length)
            GameManager.Instance.OnCauldronFull(this);
    }

    public void Clear() => _ingredients.Clear();
}
