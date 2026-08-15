using UnityEngine;

[System.Serializable]
public class DreamSkin
{
    public int dreamIndex;      // 1-4
    public Sprite skinSprite;
}

[System.Serializable]
public class EnemyData
{
    public string enemyName;
    public int maxHits = 2;              // 2 for mini, 6 for big
    public float moveSpeed = 1.5f;
    public float aggroRange = 4f;        // big enemies: idle until player enters this
    public bool isBig = false;
    public DreamSkin[] skinsPerDream;    // assign 4 sprites, one per dream
}