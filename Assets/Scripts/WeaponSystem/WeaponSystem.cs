using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject depletionParticlePrefab;
    [SerializeField] private GameObject attackConeVisualPrefab;
    [SerializeField] private bool showConeVisual = false; // toggle back on later once art/positioning is ready
    [SerializeField] private PlayerController playerController;

    private WeaponData currentWeapon;
    private int currentCharges;
    private bool hasWeapon = false;

    private GameObject coneVisualInstance;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (attackConeVisualPrefab != null)
        {
            coneVisualInstance = Instantiate(attackConeVisualPrefab, transform);
            coneVisualInstance.transform.localPosition = Vector3.zero;
            coneVisualInstance.SetActive(false);
        }
    }

    private void Update()
    {
        if (!hasWeapon) return;

        UpdateConeRotation();

        if (Input.GetKeyDown(KeyCode.Space))
            Attack();
    }

    private void UpdateConeRotation()
    {
        if (coneVisualInstance == null || playerController == null) return;

        Vector2 facing = playerController.GetFacingDirection();
        float angle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
        coneVisualInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void EquipWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;
        currentCharges = weapon.maxCharges;
        hasWeapon = true;

        if (coneVisualInstance != null && showConeVisual)
            coneVisualInstance.SetActive(true);

        Debug.Log($"Equipped {weapon.weaponName}, charges: {currentCharges}");
    }

    private void Attack()
    {
        if (currentCharges <= 0) return;

        Vector2 facing = playerController != null ? playerController.GetFacingDirection() : Vector2.down;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentWeapon.attackRange, enemyLayer);
        HashSet<EnemyAI> alreadyHit = new HashSet<EnemyAI>();
        int hitCount = 0;

        foreach (var hit in hits)
        {
            Vector2 dirToEnemy = (hit.transform.position - transform.position).normalized;
            float angle = Vector2.Angle(facing, dirToEnemy);

            if (angle <= currentWeapon.attackConeAngle / 2f)
            {
                EnemyAI enemy = hit.GetComponent<EnemyAI>();
                if (enemy != null && alreadyHit.Add(enemy))
                {
                    enemy.TakeHit(dirToEnemy);
                    hitCount++;
                }
            }
        }

        Debug.Log($"Attack fired — hit {hitCount} enemies (cone {currentWeapon.attackConeAngle}°, range {currentWeapon.attackRange})");
        GameEvents.TriggerWeaponAttackUsed();

        currentCharges--;

        if (currentCharges <= 0)
        {
            if (depletionParticlePrefab != null)
                Instantiate(depletionParticlePrefab, transform.position, Quaternion.identity);

            hasWeapon = false;
            currentWeapon = null;

            if (coneVisualInstance != null)
                coneVisualInstance.SetActive(false);

            GameEvents.TriggerWeaponDepleted();
        }
    }
}