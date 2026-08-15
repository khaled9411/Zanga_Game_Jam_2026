using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private GameObject depletionParticlePrefab;

    private WeaponData currentWeapon;
    private int currentCharges;
    private bool hasWeapon = false;

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
    }

    private void Attack()
    {
        if (currentCharges <= 0) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, currentWeapon.attackRange, enemyLayer);

        foreach (var hit in hits)
        {
            // hit.GetComponent<EnemyAI>()?.TakeHit();
        }

        currentCharges--;

        if (currentCharges <= 0)
        {
            if (depletionParticlePrefab != null)
                Instantiate(depletionParticlePrefab, transform.position, Quaternion.identity);

            hasWeapon = false;
            currentWeapon = null;
            GameEvents.TriggerWeaponDepleted();
        }
    }

    public bool HasWeapon => hasWeapon;
    public int CurrentCharges => currentCharges;
}