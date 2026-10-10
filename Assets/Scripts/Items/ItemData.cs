using UnityEngine;

namespace Chisanuphong
{
    public enum ItemType
    {
        TimeBonus,      // +3 seconds
        TimePenalty,    // -5 seconds
        ScoreBonus,     // +500 points
        ScorePenalty    // -500 points
    }

    [CreateAssetMenu(fileName = "NewItemData", menuName = "Chisanuphong/Item Data")]
    public class ItemData : ScriptableObject
    {
        [Header("Item Info")]
        public string itemName = "Item";
        public ItemType itemType = ItemType.ScoreBonus;

        [Header("Modifiers")]
        public int scoreModifier = 0;
        public float timeModifier = 0f;

        [Header("Appearance & Audio")]
        public Color itemColor = Color.yellow;
        public AudioClip sfxClip;
    }
}
