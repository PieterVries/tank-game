using UnityEngine;
using System.Collections.Generic;

public class HESH : MonoBehaviour
{
    [Header("Flight")]
    public float gravityScale = 1f;
    public float rotationSpeed = 720f;
    public float rotationOffset;

    public float explosionRadius = 1.5f;
    public float explosionForce = 5f;
    public float damage = 25f;
    private bool hasHit;
    private Rigidbody2D body;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        if (body != null)
        {
            body.gravityScale = gravityScale;
        }
    }

    private void FixedUpdate()
    {
        if (body == null || body.linearVelocity.sqrMagnitude < 0.01f)
        {
            return;
        }

        float targetAngle = Mathf.Atan2(body.linearVelocity.y, body.linearVelocity.x)
            * Mathf.Rad2Deg + rotationOffset;
        float nextAngle = Mathf.MoveTowardsAngle(
            body.rotation,
            targetAngle,
            rotationSpeed * Time.fixedDeltaTime
        );
        body.MoveRotation(nextAngle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHit)
        {
            return;
        }

        BoemBoemGround hitGround =
            collision.collider.GetComponentInParent<BoemBoemGround>();
        Tankcontroller hitTank =
            collision.collider.GetComponentInParent<Tankcontroller>();

        if (hitGround == null && hitTank == null)
        {
            return;
        }

        hasHit = true;
        Vector2 impactPosition = collision.GetContact(0).point;

        if (hitGround != null)
        {
            hitGround.MakeCrater(impactPosition);
        }

        if (hitTank != null)
        {
            hitTank.TakeDamage(damage);
        }

        Explode(impactPosition);
    }

    private void Explode(Vector2 explosionPosition)
    {
        Collider2D[] objects = Physics2D.OverlapCircleAll(
            explosionPosition,
            explosionRadius
        );
        HashSet<Rigidbody2D> affectedBodies = new HashSet<Rigidbody2D>();

        foreach (Collider2D obj in objects)
        {
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();

            if (rb == null || !affectedBodies.Add(rb))
            {
                continue;
            }

            Tankcontroller tank = obj.GetComponentInParent<Tankcontroller>();
            if (tank != null)
            {
                tank.ApplyExplosionForce(explosionPosition, explosionForce);
            }
            else
            {
                Vector2 direction = (rb.position - explosionPosition).normalized;
                rb.AddForce(direction * explosionForce, ForceMode2D.Impulse);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }
}
