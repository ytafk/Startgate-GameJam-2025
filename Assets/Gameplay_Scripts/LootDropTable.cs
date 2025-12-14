using UnityEngine;

[CreateAssetMenu(menuName = "Loot/Drop Table", fileName = "LootDropTable")]
public class LootDropTable : ScriptableObject
{
    [System.Serializable]
    public class DropItem
    {
        public string id;                 // "Health", "Ammo" gibi (debug için)
        public GameObject prefab;         // düþecek pickup prefab
        [Range(0f, 1f)] public float chance = 0.2f;  // 0-1 arasý olasýlýk
        public int minCount = 1;
        public int maxCount = 1;
    }

    public DropItem[] items;
}
