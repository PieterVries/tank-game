using UnityEngine;

public class Tankcontroller : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float moveAcceleration = 20f;
    public float maximumDriveableSlope = 85f;

    [Header("Stability")]
    public float maximumTiltAngle = 55f;
    public float rotationDamping = 2.5f;
    [Range(0f, 1f)]
    public float hitVerticalForceMultiplier = 0.1f;

    [Header("Health")]
    public float maxHealth = 100f;
    public int playerNumber = 1;
    private float currentHealth;

    [Header("Turret")]
    public Transform turret;
    public Transform turretPivot;
    public float turretSpeed = 100f;
    public float minimumTurretAngle = -10f;
    public float maximumTurretAngle = 35f;

    [Header("Shooting")]
    public Transform firePoint;
    public GameObject projectilePrefab;
    public float minimumProjectileForce = 5f;
    public float maximumProjectileForce = 30f;
    public float chargeTime = 2f;

    [Header("Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode aimUpKey = KeyCode.W;
    public KeyCode aimDownKey = KeyCode.S;
    public KeyCode fireKey = KeyCode.Space;

    private Rigidbody2D rb;
    private float shotCharge;
    private Vector2 groundNormal = Vector2.up;
    private bool hasGroundContact;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (rb != null)
        {
            rb.angularVelocity = 0f;
            rb.angularDamping = Mathf.Max(0f, rotationDamping);
        }

        if (turretPivot != null && turret != null && turret.parent != turretPivot)
        {
            turret.SetParent(turretPivot, true);
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= Mathf.Max(0f, damage);

        if (currentHealth <= 0f)
        {
            if (GameManeger.Instance != null)
            {
                GameManeger.Instance.TankDefeated(playerNumber);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public void ApplyExplosionForce(Vector2 explosionPosition, float force)
    {
        if (rb == null)
        {
            return;
        }

        Vector2 direction = (rb.position - explosionPosition).normalized;
        direction.y *= Mathf.Clamp01(hitVerticalForceMultiplier);
        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsThisPlayersTurn())
        {
            return;
        }

        Aim();
        Shoot();
    }

    void FixedUpdate()
    {
        PreventTankFlip();

        if (!IsThisPlayersTurn())
        {
            StopMovement();
            hasGroundContact = false;
            return;
        }

        Move();
        hasGroundContact = false;
    }

    bool IsThisPlayersTurn()
    {
        return GameManeger.Instance == null ||
            GameManeger.Instance.IsPlayerTurn(playerNumber);
    }

    void Move()
    {
        if (rb == null)
        {
            return;
        }

        float movement = 0f;

        if (Input.GetKey(leftKey))
            movement = -1f;

        if (Input.GetKey(rightKey))
            movement = 1f;

        float acceleration = Mathf.Max(0f, moveAcceleration);
        float targetSpeed = movement * moveSpeed;

        if (hasGroundContact)
        {
            Vector2 slopeDirection = new Vector2(groundNormal.y, -groundNormal.x).normalized;
            if (slopeDirection.x < 0f)
            {
                slopeDirection = -slopeDirection;
            }

            float slopeSpeed = Vector2.Dot(rb.linearVelocity, slopeDirection);
            float adjustedSlopeSpeed = Mathf.MoveTowards(
                slopeSpeed,
                targetSpeed,
                acceleration * Time.fixedDeltaTime
            );
            rb.linearVelocity += slopeDirection * (adjustedSlopeSpeed - slopeSpeed);
            return;
        }

        float horizontalSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            acceleration * Time.fixedDeltaTime
        );
        rb.linearVelocity = new Vector2(horizontalSpeed, rb.linearVelocity.y);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        float minimumNormalY = Mathf.Cos(
            Mathf.Clamp(maximumDriveableSlope, 0f, 89f) * Mathf.Deg2Rad
        );
        Vector2 normalSum = Vector2.zero;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector2 normal = collision.GetContact(i).normal;
            Vector2 toTank = rb.worldCenterOfMass - collision.GetContact(i).point;
            if (Vector2.Dot(normal, toTank) < 0f)
            {
                normal = -normal;
            }

            if (normal.y >= minimumNormalY)
            {
                normalSum += normal;
            }
        }

        if (normalSum.sqrMagnitude > 0f)
        {
            groundNormal = normalSum.normalized;
            hasGroundContact = true;
        }
    }

    void PreventTankFlip()
    {
        if (rb == null)
        {
            return;
        }

        float tiltAngle = Mathf.DeltaAngle(0f, rb.rotation);
        float tiltLimit = Mathf.Clamp(maximumTiltAngle, 0f, 89f);
        if (Mathf.Abs(tiltAngle) > tiltLimit)
        {
            rb.SetRotation(Mathf.Clamp(tiltAngle, -tiltLimit, tiltLimit));
            rb.angularVelocity = 0f;
        }
    }

    void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    void Aim()
    {
        Transform aimPivot = turretPivot != null ? turretPivot : turret;

        if (aimPivot == null)
        {
            Debug.LogWarning($"{name} needs a Turret or Turret Pivot assigned.", this);
            return;
        }

        float currentAngle = aimPivot.localEulerAngles.z;
        if (currentAngle > 180f)
            currentAngle -= 360f;

        float aimInput = 0f;
        if (Input.GetKey(aimUpKey))
            aimInput += 1f;
        if (Input.GetKey(aimDownKey))
            aimInput -= 1f;

        float targetAngle = Mathf.Clamp(
            currentAngle + aimInput * turretSpeed * Time.deltaTime,
            minimumTurretAngle,
            maximumTurretAngle
        );

        float nextAngle = Mathf.MoveTowardsAngle(
            currentAngle,
            targetAngle,
            turretSpeed * Time.deltaTime
        );

        aimPivot.localRotation = Quaternion.Euler(0f, 0f, nextAngle);
    }

    void Shoot()
    {
        if (Input.GetKey(fireKey))
        {
            shotCharge = Mathf.Min(shotCharge + Time.deltaTime, chargeTime);
        }

        if (Input.GetKeyUp(fireKey))
        {
            if (FireChargedShot())
            {
                if (GameManeger.Instance != null)
                {
                    GameManeger.Instance.EndTurn(playerNumber);
                }
            }

            shotCharge = 0f;
        }
    }

    bool FireChargedShot()
    {
        if (projectilePrefab == null || firePoint == null)
        {
            return false;
        }

        GameObject projectile = Instantiate(
            projectilePrefab,
            firePoint.position,
            firePoint.rotation
        );

        Rigidbody2D projectileRb = projectile.GetComponent<Rigidbody2D>();
        if (projectileRb == null)
        {
            Destroy(projectile);
            return false;
        }

        float chargePercent = chargeTime <= 0f
            ? 1f
            : shotCharge / chargeTime;
        float shotForce = Mathf.Lerp(
            minimumProjectileForce,
            maximumProjectileForce,
            chargePercent
        );

        projectileRb.AddForce(
            firePoint.right * shotForce,
            ForceMode2D.Impulse
        );

        return true;
    }
}
