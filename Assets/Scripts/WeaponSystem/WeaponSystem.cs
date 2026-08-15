using System.Collections.Generic;
using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject depletionParticlePrefab;
    [SerializeField] private GameObject attackConeVisualPrefab; // spawned once, kept alive, rotates with player
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
            coneVisualInstance.SetActive(false); // hidden until a weapon is equipped
        }
    }

    private void Update()
    {
        if (!hasWeapon) return;

        if (Input.GetKeyDown(KeyCode.Space))
            Attack();
    }

    public void EquipWeapon(WeaponData weapon)
    {
        currentWeapon = weapon;
        currentCharges = weapon.maxCharges;
        hasWeapon = true;

        if (coneVisualInstance != null)
            coneVisualInstance.SetActive(true);

        Debug.Log($"[WeaponSystem] Equipped {weapon.weaponName}, charges: {currentCharges}");
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

        Debug.Log($"[WeaponSystem] Attack fired — hit {hitCount} enemies. Charges left: {currentCharges - 1}");
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