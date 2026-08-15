using UnityEngine;

/// <summary>
/// Plain data class describing a weapon type. No ScriptableObject needed for a jam —
/// just fill these in directly on the WeaponSystem or WeaponPickup in the Inspector.
/// </summary>
[System.Serializable]
public class WeaponData
{
    public string weaponName;
    public Sprite weaponSprite;
    public int maxCharges = 6;
    public float attackRange = 1.5f;
    [Range(1, 359)] public float attackConeAngle = 90f;
}