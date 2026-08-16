using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string weaponName;
    public Sprite weaponSprite;
    public int maxCharges = 6;
    public float attackRange = 1.5f;
    [Range(1, 359)] public float attackConeAngle = 90f;
    public AudioClip fireSound;
    public RuntimeAnimatorController animatorController; // this weapon's own Fire animation controller
}