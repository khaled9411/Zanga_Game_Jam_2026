using UnityEngine;

[System.Serializable]
public class EnemyData
{
    public string enemyName;
    public int maxHits = 2;
    public float moveSpeed = 1.5f;
    public float aggroRange = 4f;
    public bool isBig = false;
}