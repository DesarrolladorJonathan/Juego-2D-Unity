using UnityEngine;
using UnityEngine.SceneManagement;

public class StageDirector : MonoBehaviour
{
    public static StageDirector Instance { get; private set; }
    public PlayerController Player;
    public Sprite[] coinFrames, fireFrames;
    public StageEnemy Boss;
    public bool BossDefeated { get; private set; }
    public bool InputBlocked { get { return intro || paused || won; } }
    public int Coins { get; private set; }
    public int Relics { get; private set; }
    public int Kills { get; private set; }
    public int Deaths { get; private set; }
    public float RunTime { get; private set; }
    bool intro = true, paused, won, muted;
    Vector3 checkpoint = new Vector3(2,1,0);
    string banner, subtitle;
    float bannerUntil;
    StageCamera follow;
    GUIStyle text, title, small, button;
    AudioSource sfx, music;
    AudioClip coinSound, hitSound, checkpointSound;
    static readonly Color Ink = new Color(.035f,.065f,.095f,.92f);
    static readonly Color Mint = new Color(.48f,1,.77f);
    static readonly Color Gold = new Color(1,.79f,.36f);

    void Awake()
    {
        Instance=this; Time.timeScale=1;
        Physics2D.IgnoreLayerCollision(8,8,true);
        follow=Camera.main.GetComponent<StageCamera>();
        sfx=gameObject.AddComponent<AudioSource>();sfx.volume=.22f;
        coinSound=Tone(880,.12f);hitSound=Tone(170,.14f);checkpointSound=Tone(660,.4f);
        music=gameObject.AddComponent<AudioSource>();music.clip=Melody();music.loop=true;music.volume=.055f;music.Play();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) {muted=!muted; AudioListener.volume=muted?0:1;}
        if(intro && (Input.GetKeyDown(KeyCode.Return)||Input.GetKeyDown(KeyCode.Space))) Begin();
        else if(Input.GetKeyDown(KeyCode.Escape) && !intro && !won){paused=!paused;Time.timeScale=paused?0:1;}
        if((won || paused) && Input.GetKeyDown(KeyCode.R)) Restart();
        if(!InputBlocked)RunTime+=Time.deltaTime;
        if(!intro && !won && !paused && Input.GetKeyDown(KeyCode.R)) Respawn();
    }
    public void Begin(){intro=false;Player.RestoreAt(checkpoint);Announce("EL SENDERO DESPIERTA", "Sigue las brasas. Los faros guardan tu progreso.",3.5f);}
    public void Announce(string heading,string detail,float duration){banner=heading;subtitle=detail;bannerUntil=Time.unscaledTime+duration;}
    public void ActivateCheckpoint(Vector3 position)
    {
        checkpoint=position; Player.RestoreAt(checkpoint);sfx.PlayOneShot(checkpointSound);
        Announce("FARO ENCENDIDO", "Vida recuperada. Tu punto de retorno esta aqui.",2.7f);Fx(position,Mint,18);
    }
    public void Respawn()
    {
        if(InputBlocked)return; Deaths++;Player.RestoreAt(checkpoint);
        if(Boss!=null)Boss.ResetBoss();
        foreach(var projectile in FindObjectsOfType<StageProjectile>())Destroy(projectile.gameObject);
        if(follow!=null)follow.Snap();
        Announce("UNA BRASA NUNCA SE APAGA", "De vuelta al ultimo faro. Las monedas y reliquias se conservan.",2.8f);
    }
    public void CollectCoin(){Coins++;sfx.PlayOneShot(coinSound,.55f);if(Coins%20==0){Player.Heal();Announce("20 MONEDAS · +1 VIDA", "Explorar tiene recompensa.",2);}}
    public void CollectRelic(){Relics++;sfx.PlayOneShot(checkpointSound);Announce("RELIQUIA "+Relics+" / 3", "Un fragmento del antiguo bosque.",2.5f);}
    public void EnemyDefeated(StageEnemy enemy)
    {
        Kills++;sfx.PlayOneShot(hitSound);Shake(enemy.IsBoss? .4f:.1f);
        if(enemy.IsBoss){BossDefeated=true;Announce("EL GUARDIAN HA CAIDO", "El portal del este esta abierto. Termina el recorrido.",4);}
        else
        {
            for(int i=0;i<3;i++)
            {
                var go=new GameObject("Moneda de enemigo");go.transform.position=enemy.transform.position+new Vector3((i-1)*.45f,.5f,0);
                var sprite=go.AddComponent<SpriteRenderer>();sprite.sprite=coinFrames[0];sprite.sortingOrder=95;go.transform.localScale=Vector3.one*.5f;
                var pickup=go.AddComponent<StagePickup>();pickup.kind=StagePickup.Kind.Coin;pickup.frames=coinFrames;
            }
        }
    }
    public void Shake(float strength){if(follow!=null)follow.shake=Mathf.Max(follow.shake,strength);}
    public void Win(){if(won)return;won=true;sfx.PlayOneShot(checkpointSound);Fx(Player.transform.position,Gold,40);}
    void Restart(){Time.timeScale=1;SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);}
    public static void Fx(Vector3 position,Color color,int count)
    {
        if(Instance==null || Instance.coinFrames.Length==0)return;
        for(int i=0;i<count;i++)
        {
            var go=new GameObject("Chispa");go.transform.position=position;
            var visual=go.AddComponent<SpriteRenderer>();visual.sprite=Instance.coinFrames[0];visual.color=color;visual.sortingOrder=145;
            go.AddComponent<StageParticle>().Init(new Vector3(Random.Range(-4f,4f),Random.Range(1f,6f),0),Random.Range(.3f,.7f));
        }
    }
    AudioClip Tone(float frequency,float seconds)
    {
        int rate=22050;var data=new float[(int)(rate*seconds)];
        for(int i=0;i<data.Length;i++){float t=i/(float)rate;data[i]=Mathf.Sin(t*frequency*6.28318f)*Mathf.Pow(1-i/(float)data.Length,2)*.4f;}
        var clip=AudioClip.Create("Brasa",data.Length,1,rate,false);clip.SetData(data,0);return clip;
    }
    AudioClip Melody()
    {
        int rate=22050;int[] notes={57,64,69,72,67,64,60,64,53,60,65,69,64,60,57,60,55,62,67,71,65,62,59,62,52,59,64,67,62,59,55,59};
        float beat=.4f;var data=new float[(int)(notes.Length*beat*rate)];
        for(int i=0;i<data.Length;i++)
        {
            float t=i/(float)rate;int n=(int)(t/beat);float phase=t%beat;
            float f=440*Mathf.Pow(2,(notes[Mathf.Min(n,notes.Length-1)]-69)/12f);
            data[i]=(Mathf.Sin(t*f*6.28318f)+.3f*Mathf.Sin(t*f*12.566f))*Mathf.Exp(-phase*9)*.4f;
        }
        var clip=AudioClip.Create("El bosque de las brasas",data.Length,1,rate,false);clip.SetData(data,0);return clip;
    }
    void Styles()
    {
        if(text!=null)return;
        var font=Resources.GetBuiltinResource<Font>("Arial.ttf");
        text=new GUIStyle {font=font,fontSize=18,normal={textColor=Color.white}};
        small=new GUIStyle(text){fontSize=13}; title=new GUIStyle(text){fontSize=38,fontStyle=FontStyle.Bold};
        button=new GUIStyle(GUI.skin.button){font=font,fontSize=17,fontStyle=FontStyle.Bold,padding=new RectOffset(18,18,12,12)};
    }
    void Box(Rect rect,Color color){GUI.color=color;GUI.DrawTexture(rect,Texture2D.whiteTexture);GUI.color=Color.white;}
    void Label(float x,float y,float w,string content,GUIStyle style,Color color){var old=style.normal.textColor;style.normal.textColor=color;GUI.Label(new Rect(x,y,w,55),content,style);style.normal.textColor=old;}
    string Region(float x){return x<27?"01  /  EL POBLADO":x<73?"02  /  COPAS DEL BOSQUE":x<123?"03  /  EL CLARO ROTO":"04  /  SANTUARIO DE LA BRASA";}
    void OnGUI()
    {
        Styles();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
        if(Player==null)return;
        Box(new Rect(24,22,310,94),Ink);Box(new Rect(24,22,4,94),Mint);
        Label(44,33,270,"BRASAS DEL BOSQUE",small,Mint);
        for(int i=0;i<Player.MaxHealth;i++){Box(new Rect(44+i*37,62,29,16),i<Player.Health?new Color(1,.4f,.32f):new Color(.2f,.25f,.28f));}
        Label(44,91,270,"DASH",small,Color.white);Box(new Rect(95,94,176,5),new Color(.17f,.25f,.27f));Box(new Rect(95,94,176*Player.DashReady,5),Mint);
        Box(new Rect(960,22,296,94),Ink);Label(979,40,260,Coins.ToString("00")+"  MONEDAS     "+Relics+" / 3  RELIQUIAS",small,Gold);
        Label(979,76,260,Region(Player.transform.position.x),small,Color.white);
        Box(new Rect(364,29,552,3),new Color(.09f,.18f,.2f));Box(new Rect(364,29,552*Mathf.Clamp01(Player.transform.position.x/148f),3),Mint);
        Label(366,43,550,"EL ULTIMO FARO  /  Un solo viaje. Un bosque por recuperar.",small,new Color(.8f,.94f,.87f));
        if(Time.unscaledTime<bannerUntil && !intro && !won)
        {
            Box(new Rect(305,131,670,72),Ink);Label(324,141,630,banner,text,Gold);Label(324,172,630,subtitle,small,Color.white);
        }
        if(Boss!=null && !Boss.Dead && Boss.Engaged && !intro)
        {
            Box(new Rect(352,619,576,62),Ink);Label(372,628,540,"GUARDIAN DE LA BRASA",small,Gold);
            Box(new Rect(372,655,536,8),new Color(.25f,.14f,.14f));Box(new Rect(372,655,536*Boss.Health/Boss.maxHealth,8),new Color(1,.35f,.21f));
        }
        if(!intro && !won && !paused)
        {
            Box(new Rect(24,671,700,28),Ink);
            Label(38,676,690,"A/D  mover    ESPACIO  doble salto    Z / clic  atacar    X / SHIFT  dash    ESC  pausa    M  sonido",small,new Color(.8f,.9f,.86f));
            if(Player.transform.position.x>144 && !BossDefeated)Label(820,676,430,"El portal espera la derrota del guardian.",small,Gold);
        }
        if(intro || paused || won)
        {
            Box(new Rect(0,0,1280,720),new Color(.015f,.035f,.055f,.78f));
            Box(new Rect(270,150,740,427),Ink);Box(new Rect(270,150,740,4),Mint);
            Label(312,183,660,intro?"UN VIAJE · CUATRO CLAROS · UN GUARDIAN":won?"STAGE COMPLETADO":"RESPIRA. EL BOSQUE PUEDE ESPERAR.",small,Mint);
            Label(310,222,690,intro?"EL BOSQUE DE LAS BRASAS":won?"LA BRASA SIGUE VIVA":"EN PAUSA",title,Color.white);
            string detail=intro?"Llega al ultimo faro y derrota a su guardian.\nExplora las alturas para encontrar las tres reliquias.":won?"El bosque vuelve a respirar. Gracias por recorrerlo.":"Continua tu viaje o vuelve a comenzar desde el poblado.";
            var wrapped=new GUIStyle(text){wordWrap=true};GUI.Label(new Rect(312,290,650,72),detail,wrapped);
            Label(312,374,660,intro?"ESPACIO dos veces: doble salto. X: dash, incluso en el aire.":"MONEDAS  "+Coins+"     RELIQUIAS  "+Relics+"/3     ENEMIGOS  "+Kills,small,Gold);
            Label(312,404,660,intro?"Z + abajo en el aire: rebota sobre enemigos. Z devuelve proyectiles.":"TIEMPO  "+System.TimeSpan.FromSeconds(RunTime).ToString(@"mm\:ss")+"     CAIDAS  "+Deaths,small,Color.white);
            Label(312,435,660,intro?"Los faros recuperan tu vida. Cada 20 monedas ganas una vida.":"R: reiniciar     M: sonido     Alt + F4: salir",small,new Color(.72f,.8f,.78f));
            if(GUI.Button(new Rect(312,489,310,49),intro?"ENTRAR AL BOSQUE  [ENTER]":won?"OTRO VIAJE  [R]":"CONTINUAR  [ESC]",button))
            {if(intro)Begin();else if(won)Restart();else{paused=false;Time.timeScale=1;}}
            if(!intro && !won && GUI.Button(new Rect(647,489,270,49),"REINICIAR STAGE",button))Restart();
        }
    }
    void OnDestroy(){Time.timeScale=1;if(Instance==this)Instance=null;AudioListener.volume=1;}
}

