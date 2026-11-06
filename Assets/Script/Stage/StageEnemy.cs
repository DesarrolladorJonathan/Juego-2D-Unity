using UnityEngine;

public class StageEnemy : MonoBehaviour
{
    public enum Species { Slime, Skeleton, Bat, Guardian }
    public Species species;
    public Sprite[] idleFrames, moveFrames, attackFrames, deathFrames;
    public float patrolLeft, patrolRight, floor;
    public int maxHealth = 3;
    public float visualHeight = 1.6f;
    public int Health { get; private set; }
    public bool Dead { get { return Health <= 0; } }
    public bool IsBoss { get { return species == Species.Guardian; } }
    public bool Engaged { get; private set; }
    public bool WindingUp { get { return windup > 0; } }
    Rigidbody2D body;
    SpriteRenderer visual;
    Vector3 origin;
    float clock, flash, cooldown = 1.3f, windup, rush, stunned, deadTime;
    int facing = -1, attackCount;
    bool attackPending;

    void Start()
    {
        Health = maxHealth; origin = transform.position;
        body = GetComponent<Rigidbody2D>(); visual = GetComponentInChildren<SpriteRenderer>();
        body.freezeRotation = true; body.interpolation = RigidbodyInterpolation2D.Interpolate;
        if (species == Species.Bat) body.gravityScale = 0;
        if (IsBoss && StageDirector.Instance != null) StageDirector.Instance.Boss = this;
    }

    void Update()
    {
        if (StageDirector.Instance == null || StageDirector.Instance.Player == null) return;
        if (StageDirector.Instance.InputBlocked) return;
        clock += Time.deltaTime; flash -= Time.deltaTime; stunned -= Time.deltaTime;
        PlayerController player = StageDirector.Instance.Player;
        Vector2 delta = player.transform.position - transform.position;
        if (Dead)
        {
            deadTime += Time.deltaTime; body.velocity = Vector2.zero;
            Animate(deathFrames, 10, false);
            visual.color = new Color(1, 1, 1, Mathf.Clamp01(1 - deadTime));
            if (deadTime > 1) Destroy(gameObject);
            return;
        }
        if (!IsBoss && (transform.position.y < -7 || Mathf.Abs(delta.x) > 45)) { if(transform.position.y < -7) TakeHit(100,Vector2.up); return; }
        if (IsBoss && !Engaged && delta.magnitude < 13)
        { Engaged = true; StageDirector.Instance.Announce("EL GUARDIAN DE LA BRASA", "Esquiva su carga. Ataca durante el descanso.", 3.5f); }
        cooldown -= Time.deltaTime;
        if (windup > 0)
        {
            windup -= Time.deltaTime;
            if (species != Species.Bat) body.velocity = new Vector2(0, body.velocity.y);
            visual.color = Color.Lerp(Color.white, new Color(1, .35f, .2f), .65f + Mathf.Sin(Time.time * 24) * .3f);
            Animate(attackFrames, 8, true);
            if (windup <= 0 && attackPending) ExecuteAttack(player);
            return;
        }
        if (rush > 0)
        {
            rush -= Time.deltaTime; body.velocity = new Vector2(facing * (IsBoss ? 11 : 5), body.velocity.y);
            if (IsBoss && Mathf.Abs(delta.x) < 2.3f && Mathf.Abs(delta.y) < 2.8f) player.TakeDamage(1,transform.position);
            Animate(moveFrames, 13, true); return;
        }
        float aggro = IsBoss ? 30 : species == Species.Bat ? 9 : 7;
        bool sees = Mathf.Abs(delta.x) < aggro && Mathf.Abs(delta.y) < 4.5f;
        if (sees) facing = delta.x > 0 ? 1 : -1;
        if (!sees && (transform.position.x < patrolLeft || transform.position.x > patrolRight)) facing = transform.position.x < patrolLeft ? 1 : -1;
        if (stunned <= 0)
        {
            if (species == Species.Bat)
            {
                Vector2 target = sees ? (Vector2)player.transform.position + Vector2.up * .25f : (Vector2)origin + new Vector2(Mathf.Sin(clock) * 2, Mathf.Sin(clock * 2) * .6f);
                body.velocity = Vector2.ClampMagnitude((target - (Vector2)transform.position) * 1.7f, 3.6f);
            }
            else
            {
                float speed = IsBoss ? (Engaged ? 2.1f : 0) : species == Species.Slime ? 1.5f : 2.2f;
                bool edge = !Physics2D.Raycast((Vector2)transform.position + new Vector2(facing * .85f, .1f), Vector2.down, visualHeight * .6f + .8f, 64);
                if (edge || transform.position.x < patrolLeft || transform.position.x > patrolRight)
                {
                    facing = transform.position.x <= patrolLeft + .5f ? 1 : transform.position.x >= patrolRight - .5f ? -1 : -facing;
                    speed = 0;
                }
                if (sees && Mathf.Abs(delta.x) < 1.4f) speed = 0;
                body.velocity = new Vector2(facing * speed, body.velocity.y);
                if ((sees || Engaged) && cooldown <= 0 && Mathf.Abs(delta.x) < (IsBoss ? 13 : 2.5f))
                {
                    windup = IsBoss ? .85f : species == Species.Slime ? .55f : .6f;
                    attackPending = true; clock = 0;
                }
            }
        }
        if (delta.magnitude < (IsBoss ? 1.75f : 1.05f)) player.TakeDamage(1, transform.position);
        visual.flipX = facing > 0;
        visual.color = flash > 0 ? new Color(1, .5f, .4f) : Color.white;
        Animate(body.velocity.sqrMagnitude > .1f ? moveFrames : idleFrames, species == Species.Slime ? 9 : 11, true);
    }

    void ExecuteAttack(PlayerController player)
    {
        attackPending = false; attackCount++;
        Vector2 delta = player.transform.position - transform.position;
        if (IsBoss)
        {
            cooldown = Health < maxHealth / 2 ? 1.25f : 1.9f;
            if (attackCount % 2 == 0)
            {
                body.velocity = new Vector2(0, 10);
                for (int i = -1; i <= 1; i++) StageProjectile.Spawn(transform.position + Vector3.up * .3f, new Vector2(facing * 6, i * 2), false);
                StageDirector.Fx(transform.position, new Color(1, .5f, .2f), 14);
            }
            else rush = .65f;
        }
        else
        {
            cooldown = 1.5f;
            if (Mathf.Abs(delta.x) < 2.65f && Mathf.Abs(delta.y) < 2.5f) player.TakeDamage(1, transform.position);
            if (species == Species.Slime) { body.velocity = new Vector2(facing * 4, 6); rush = .3f; }
        }
    }
    void Animate(Sprite[] frames, float fps, bool loop)
    {
        if (frames == null || frames.Length == 0 || visual == null) return;
        int index = (int)((Dead ? deadTime : clock) * fps);
        visual.sprite = frames[loop ? index % frames.Length : Mathf.Min(index, frames.Length - 1)];
    }
    public void TakeHit(int damage, Vector2 direction)
    {
        if (Dead) return;
        Health -= damage; flash = .14f; stunned = .2f;
        if (body != null) body.velocity = new Vector2(direction.x * (IsBoss ? 1.5f : 5), IsBoss ? body.velocity.y : 3);
        StageDirector.Fx(transform.position, new Color(1, .78f, .3f), 8);
        if (Health <= 0)
        {
            GetComponent<Collider2D>().enabled = false; body.gravityScale = 0;
            if (StageDirector.Instance != null) StageDirector.Instance.EnemyDefeated(this);
        }
    }
    public void ResetBoss()
    {
        if (!IsBoss || Dead) return;
        Health = maxHealth; transform.position = origin; body.velocity = Vector2.zero;
        windup = rush = 0; cooldown = 2; Engaged = false; attackPending = false;
    }
}

