using UnityEngine;

public class StageParticle : MonoBehaviour
{
    Vector3 velocity; float life, duration; SpriteRenderer visual;
    public void Init(Vector3 speed, float seconds) { velocity=speed; duration=life=seconds; visual=GetComponent<SpriteRenderer>(); }
    void Update()
    { transform.position+=velocity*Time.deltaTime; velocity+=Vector3.down*6*Time.deltaTime;life-=Time.deltaTime;transform.localScale=Vector3.one*.12f*Mathf.Max(0,life/duration);if(life<=0)Destroy(gameObject); }
}



