using UnityEngine;

public class StagePickup : MonoBehaviour
{
    public enum Kind { Coin, Relic, Heart, Checkpoint, Exit, Spikes }
    public Kind kind;
    public Sprite[] frames;
    public Vector3 origin;
    public bool activated;
    SpriteRenderer visual;
    void Start() { origin = transform.position; visual = GetComponent<SpriteRenderer>(); }
    void Update()
    {
        var stage = StageDirector.Instance;
        if (stage == null || stage.Player == null || stage.InputBlocked) return;
        if (frames != null && frames.Length > 0 && visual != null) visual.sprite = frames[(int)(Time.time * 10) % frames.Length];
        if (kind == Kind.Coin || kind == Kind.Relic || kind == Kind.Heart)
            transform.position = origin + Vector3.up * Mathf.Sin(Time.time * 3 + origin.x) * .12f;
        Vector2 delta = stage.Player.transform.position - transform.position;
        if (kind == Kind.Spikes)
        { if(Mathf.Abs(delta.x) < 1.45f && delta.y < 1.1f && delta.y > -.6f) stage.Player.TakeDamage(1, transform.position); return; }
        if (delta.magnitude > (kind == Kind.Checkpoint || kind == Kind.Exit ? 1.8f : .9f)) return;
        if (kind == Kind.Exit)
        { if(stage.BossDefeated) stage.Win(); else if(!activated) { activated = true; stage.Announce("EL PORTAL ESTA SELLADO", "Derrota al guardian para encender el ultimo faro.", 3); } return; }
        if (kind == Kind.Checkpoint)
        {
            if(!activated) { activated=true; stage.ActivateCheckpoint(origin + Vector3.up * .8f); if(visual!=null)visual.color=new Color(.5f,1,.8f); }
            return;
        }
        if (kind == Kind.Coin) stage.CollectCoin();
        if (kind == Kind.Relic) stage.CollectRelic();
        if (kind == Kind.Heart) stage.Player.Heal();
        StageDirector.Fx(transform.position, kind == Kind.Relic ? Color.cyan : new Color(1,.8f,.3f), 7);
        Destroy(gameObject);
    }
}



