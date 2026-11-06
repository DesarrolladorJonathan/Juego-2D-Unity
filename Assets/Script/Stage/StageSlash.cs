using UnityEngine;

public class StageSlash : MonoBehaviour
{
    LineRenderer line; float life = .18f;
    public static void Show(Vector2 center, Vector2 direction, bool heavy)
    {
        var go=new GameObject("Estela de espada"); go.transform.position=center;
        var slash=go.AddComponent<StageSlash>(); slash.line=go.AddComponent<LineRenderer>();
        slash.line.material=new Material(Shader.Find("Sprites/Default")); slash.line.useWorldSpace=false;
        slash.line.positionCount=15; slash.line.startWidth=heavy? .2f:.12f;slash.line.endWidth=.015f;slash.line.sortingOrder=150;
        slash.line.startColor=new Color(.6f,1,.85f);slash.line.endColor=new Color(1,.9f,.4f,.2f);
        float angle=Mathf.Atan2(direction.y,direction.x);
        for(int i=0;i<15;i++){float a=angle+Mathf.Lerp(-1.2f,1.2f,i/14f);slash.line.SetPosition(i,new Vector3(Mathf.Cos(a),Mathf.Sin(a),0)*(heavy?1.1f:.85f));}
    }
    void Update(){life-=Time.deltaTime;if(life<=0){Destroy(line.material);Destroy(gameObject);}}
}


