using UnityEngine;

// Input is sampled in Update; all movement is applied in FixedUpdate.
[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    public float velocidadDeMovimiento = 7.8f;
    public float fuerzaDeSalto = 12.5f;
    public float velocidadRodar = 20f;
    public bool puedeMover = true, enSuelo, puedeRodar = true, hacerRodar;
    public bool tocadoPido, haciendoShake, isAtacando;
    public Vector2 abajo = new Vector2(0, -0.73f);
    public float radioColision = .16f;
    public LayerMask layerPiso = 64;
    public int Health { get; private set; } = 5;
    public int MaxHealth { get { return 5; } }
    public bool Invulnerable { get { return invincible > 0 || hacerRodar; } }
    public float DashReady { get { return Mathf.Clamp01(1 - dashCooldown / .65f); } }
    public int Facing { get; private set; } = 1;
    public int AirJumps { get { return airJump ? 1 : 0; } }
    Rigidbody2D body;
    CapsuleCollider2D capsule;
    SpriteRenderer visual;
    Animator animator;
    float horizontal, vertical, coyote, jumpBuffer, dashTime, dashCooldown;
    float attackTime, invincible, knockback, comboWindow;
    bool airJump = true, airDash = true, jumpReleased;
    int combo;
    Vector2 dashDirection;
    readonly RaycastHit2D[] groundHits = new RaycastHit2D[8];

    void Awake()
    {
        body = GetComponent<Rigidbody2D>(); capsule = GetComponent<CapsuleCollider2D>();
        visual = GetComponent<SpriteRenderer>(); animator = GetComponent<Animator>();
        body.freezeRotation = true; body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.interpolation = RigidbodyInterpolation2D.Interpolate; body.gravityScale = 2.7f;
        // The original scene had a zero-radius probe. Use the character's actual collider instead.
        if (radioColision < .05f) radioColision = .16f;
        Health = MaxHealth;
    }

    void Update()
    {
        if (StageDirector.Instance != null && StageDirector.Instance.InputBlocked) return;
        horizontal = Input.GetAxisRaw("Horizontal"); vertical = Input.GetAxisRaw("Vertical");
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C)) QueueJump();
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C)) jumpReleased = true;
        dashCooldown -= Time.deltaTime; invincible -= Time.deltaTime;
        attackTime -= Time.deltaTime; comboWindow -= Time.deltaTime;
        if (attackTime <= 0) FinalizarAtaque();
        if (Mathf.Abs(horizontal) > .1f && !hacerRodar && knockback <= 0)
        {
            Facing = horizontal > 0 ? 1 : -1;
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * Facing, transform.localScale.y, 1);
        }
        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift)) TryDash(new Vector2(horizontal, vertical));
        if ((Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)) && attackTime <= 0 && !hacerRodar)
            Attack();
        if (visual != null) visual.color = invincible > 0 && Mathf.Sin(Time.time * 35) > 0 ? new Color(1, 1, 1, .35f) : Color.white;
        if (animator != null)
        {
            animator.SetBool("Caminar", enSuelo && Mathf.Abs(horizontal) > .1f);
            animator.SetBool("Saltar", !enSuelo);
            animator.SetFloat("VelocidadVertical", body.velocity.y > .1f ? 1 : -1);
            animator.SetBool("Rodar", hacerRodar);
        }
        if (transform.position.y < -8 && StageDirector.Instance != null) Die();
    }

    public void QueueJump() { jumpBuffer = .14f; }
    public bool TryDash(Vector2 direction)
    {
        if (dashCooldown > 0 || !airDash || hacerRodar || (StageDirector.Instance != null && StageDirector.Instance.InputBlocked)) return false;
        dashDirection = direction.sqrMagnitude < .1f ? Vector2.right * Facing : direction.normalized;
        dashTime = .16f; dashCooldown = .65f; airDash = false;
        hacerRodar = true; attackTime = 0; FinalizarAtaque();
        StageDirector.Fx(transform.position, new Color(.35f, 1, .86f), 9);
        return true;
    }

    void FixedUpdate()
    {
        if (StageDirector.Instance != null && StageDirector.Instance.InputBlocked) { body.velocity = Vector2.zero; body.gravityScale = 0; return; }
        if (hacerRodar)
        {
            dashTime -= Time.fixedDeltaTime; body.gravityScale = 0; body.velocity = dashDirection * velocidadRodar;
            if (dashTime <= 0) { hacerRodar = false; body.gravityScale = 2.7f; body.velocity *= .45f; }
            return;
        }
        var filter = new ContactFilter2D(); filter.SetLayerMask(layerPiso); filter.useTriggers = false;
        int count = capsule.Cast(Vector2.down, filter, groundHits, .10f);
        enSuelo = false;
        for (int i = 0; i < count; i++) if (groundHits[i].normal.y > .6f && body.velocity.y <= .2f) enSuelo = true;
        coyote = enSuelo ? .12f : coyote - Time.fixedDeltaTime;
        if (enSuelo) { airJump = true; airDash = true; }
        jumpBuffer -= Time.fixedDeltaTime; knockback -= Time.fixedDeltaTime;
        if (jumpBuffer > 0 && (coyote > 0 || airJump))
        {
            if (coyote <= 0) { airJump = false; StageDirector.Fx(transform.position + Vector3.down * .5f, new Color(.5f, 1, .85f), 7); }
            body.velocity = new Vector2(body.velocity.x, fuerzaDeSalto);
            jumpBuffer = 0; coyote = 0; enSuelo = false;
        }
        if (jumpReleased && body.velocity.y > 0) body.velocity = new Vector2(body.velocity.x, body.velocity.y * .48f);
        jumpReleased = false;
        body.gravityScale = body.velocity.y < 0 ? 3.7f : 2.7f;
        if (puedeMover && knockback <= 0)
        {
            float target = horizontal * velocidadDeMovimiento;
            float acceleration = enSuelo ? (horizontal == 0 ? 70 : 65) : 38;
            body.velocity = new Vector2(Mathf.MoveTowards(body.velocity.x, target, acceleration * Time.fixedDeltaTime), Mathf.Max(body.velocity.y, -23));
        }
    }

    void Attack()
    {
        combo = comboWindow > 0 ? (combo + 1) % 3 : 0; comboWindow = .8f;
        attackTime = combo == 2 ? .4f : .25f; isAtacando = true;
        Vector2 direction = Mathf.Abs(vertical) > .1f ? new Vector2(0, Mathf.Sign(vertical)) : Vector2.right * Facing;
        Vector2 center = (Vector2)transform.position + direction * 1.05f;
        Vector2 size = Mathf.Abs(direction.y) > 0 ? new Vector2(1.6f, 1.9f) : new Vector2(2.25f, 1.8f);
        var struck = new System.Collections.Generic.HashSet<StageEnemy>();
        foreach (var hit in Physics2D.OverlapBoxAll(center, size, 0))
        {
            StageEnemy enemy = hit.GetComponentInParent<StageEnemy>();
            if (enemy != null && struck.Add(enemy))
            {
                enemy.TakeHit(combo == 2 ? 2 : 1, direction);
                if (direction.y < 0) { body.velocity = new Vector2(body.velocity.x, 11); airJump = true; airDash = true; }
            }
            StageProjectile projectile = hit.GetComponent<StageProjectile>();
            if (projectile != null) projectile.Reflect(direction);
        }
        StageSlash.Show(center, direction, combo == 2);
        if (animator != null) { animator.SetFloat("AtaqueX", direction.x); animator.SetFloat("AtaqueY", direction.y); animator.SetBool("Atacar", true); }
    }

    public void TakeDamage(int amount, Vector2 source)
    {
        if (Invulnerable || (StageDirector.Instance != null && StageDirector.Instance.InputBlocked)) return;
        Health -= amount; invincible = 1.25f; knockback = .2f;
        body.velocity = new Vector2(transform.position.x < source.x ? -8 : 8, 7);
        StageDirector.Fx(transform.position, new Color(1, .3f, .25f), 12);
        if (StageDirector.Instance != null) StageDirector.Instance.Shake(.22f);
        if (Health <= 0) Die();
    }
    void Die() { if (StageDirector.Instance != null) StageDirector.Instance.Respawn(); }
    public void RestoreAt(Vector3 position)
    {
        transform.position = position; body.position = position; body.velocity = Vector2.zero;
        Health = MaxHealth; invincible = 1.8f; knockback = 0; dashCooldown = 0; jumpBuffer = 0;
        airJump = airDash = true; hacerRodar = false; body.gravityScale = 2.7f; FinalizarAtaque();
    }
    public void Heal() { Health = Mathf.Min(MaxHealth, Health + 1); }
    public void FinalizarAtaque() { if (attackTime > .05f) return; isAtacando = false; if (animator != null) animator.SetBool("Atacar", false); }
    public void FinalizarSalto() { }
    public void FinalizarRodar() { }
}
