using UnityEngine;

[RequireComponent(typeof(Camera))]
public class StageCamera : MonoBehaviour
{
    public Transform target;
    public Sprite[] backgrounds;
    public float shake;
    Vector3 velocity;
    SpriteRenderer[,] layers;
    float[] widths;
    void Start()
    {
        GetComponent<Camera>().orthographic=true;GetComponent<Camera>().orthographicSize=6.3f;
        layers=new SpriteRenderer[backgrounds.Length,3];widths=new float[backgrounds.Length];
        for(int i=0;i<backgrounds.Length;i++)
        {
            if(backgrounds[i]==null)continue;
            float scale=18/backgrounds[i].bounds.size.y; widths[i]=backgrounds[i].bounds.size.x*scale;
            for(int j=0;j<3;j++)
            {
                var go=new GameObject("Bosque - capa "+i);var r=go.AddComponent<SpriteRenderer>();r.sprite=backgrounds[i];r.sortingOrder=-80-i*5;
                go.transform.localScale=Vector3.one*scale;layers[i,j]=r;
            }
        }
        Snap();
    }
    public void Snap(){if(target==null)return;transform.position=Desired();velocity=Vector3.zero;}
    Vector3 Desired(){return new Vector3(Mathf.Clamp(target.position.x+2,2,143),Mathf.Clamp(target.position.y+1.9f,3.1f,6.3f),-10);}
    void LateUpdate()
    {
        if(target==null || layers==null)return;
        transform.position=Vector3.SmoothDamp(transform.position,Desired(),ref velocity,.22f);
        if(shake>0){transform.position+=(Vector3)Random.insideUnitCircle*shake;shake=Mathf.MoveTowards(shake,0,Time.deltaTime*1.8f);}
        for(int i=0;i<backgrounds.Length;i++)for(int j=0;j<3;j++)
        {
            if(layers[i,j]==null)continue;
            float w=widths[i];float offset=transform.position.x*(.42f+i*.13f);
            float tile=Mathf.Floor((transform.position.x-offset)/w);
            layers[i,j].transform.position=new Vector3(offset+(tile+j-1)*w,5.4f,0);
        }
    }
}

