using UnityEngine;

[CreateAssetMenu(fileName = "NewPotion", menuName = "Potion Data")]
public class PotionData : ScriptableObject
{
    public string potionName;
    public IngredientType[] recipe;
    public int scoreValue;
    public PotionEffect effectType;
    public Sprite potionSprite;
}
