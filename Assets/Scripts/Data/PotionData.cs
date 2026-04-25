using UnityEngine;

[CreateAssetMenu(fileName = "NewPotion", menuName = "Potion Data")]
public class PotionData : ScriptableObject
{
    public string potionName;
    public IngredientType[] recipe;
    public int scoreValue;
    public PotionEffect effectType;
    public Sprite potionSprite;
    public int   effectDuration; // rounds the effect lasts
    public float effectStrength; // e.g. 0.5 = 50% cooldown reduction
}
