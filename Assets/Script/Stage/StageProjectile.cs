using UnityEngine;

public class StageProjectile : MonoBehaviour
{
    Vector2 velocity; bool friendly; float life = 5;
    public static void Spawn(Vector2 position, Vector2 direction, bool reflected)
    {
        var go = new GameObject("Brasa"); go.transform.position = position;
        var renderer = go.AddComponent<SpriteRenderer>(); renderer.sprite = StageDirector.Instance.coinFrames[0];
        renderer.color = reflected ? Color.cyan : new Color(1, .32f, .12f); renderer.sortingOrder = 110;
        go.transform.localScale = Vector3.one * .6f;
        var shot = go.AddComponent<StageProjectile>(); shot.velocity = direction; shot.friendly = reflected;
        var collider = go.AddComponent<CircleCollider2D>(); collider.isTrigger = true; collider.radius = .35f;
    }
    void Update()
    {
        if (StageDirector.Instance.InputBlocked) return;
        transform.position += (Vector3)velocity * Time.deltaTime; life -= Time.deltaTime;
        transform.Rotate(0, 0, 180 * Time.deltaTime);
        if (life <= 0) Destroy(gameObject);
        if (!friendly && Vector2.Distance(transform.position, StageDirector.Instance.Player.transform.position) < .7f)
        { StageDirector.Instance.Player.TakeDamage(1, transform.position); Destroy(gameObject); }
        if (friendly)
            foreach (var hit in Physics2D.OverlapCircleAll(transform.position, .45f))
            {
                var enemy = hit.GetComponent<StageEnemy>();
                if (enemy != null && !enemy.Dead) { enemy.TakeHit(2, velocity.normalized); Destroy(gameObject); break; }
            }
    }
    public void Reflect(Vector2 direction) { if(friendly) return; friendly = true; velocity = direction * 12; life = 3; GetComponent<SpriteRenderer>().color = Color.cyan; }
}



