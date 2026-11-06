using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StageBuild
{
    const string ScenePath="Assets/Scenes/BrasasDelBosque.unity";
    static Transform world, terrain, decoration, encounters, treasure;
    static Sprite top, soil, ledge, spikes;
    static StageDirector director;
    static readonly float[,] islands={{-8,22,0},{26,44,0},{48,68,1},{73,94,0},{99,119,0},{123,154,0}};

    [MenuItem("Brasas/Crear o reconstruir el stage")]
    public static void Generate()
    {
        // Work in a new scene. The tutorial's original scene stays available for reference.
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity",OpenSceneMode.Single);
        var original=GameObject.Find("Player");
        if(original==null)throw new Exception("No se encontro el personaje original.");
        var player=UnityEngine.Object.Instantiate(original);player.name="Player";
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        SceneManager.MoveGameObjectToScene(player,scene);
        var source=SceneManager.GetSceneByPath("Assets/Scenes/SampleScene.unity");
        EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(scene);
        foreach(Transform child in player.transform.Cast<Transform>().ToArray())UnityEngine.Object.DestroyImmediate(child.gameObject);
        player.transform.position=new Vector3(2,1,0);player.transform.localScale=Vector3.one;
        player.tag="Player";player.layer=0;
        var controller=player.GetComponent<PlayerController>();
        controller.velocidadDeMovimiento=7.8f;controller.fuerzaDeSalto=12.5f;controller.velocidadRodar=20;
        controller.abajo=new Vector2(0,-.73f);controller.radioColision=.16f;controller.layerPiso=64;
        controller.puedeMover=true;controller.hacerRodar=false;controller.isAtacando=false;
        var capsule=player.GetComponent<CapsuleCollider2D>();capsule.size=new Vector2(.75f,1.42f);capsule.offset=new Vector2(0,-.015f);
        var friction=new PhysicsMaterial2D("Movimiento sin friccion"){friction=0,bounciness=0};
        string materialPath="Assets/Stage/Movimiento.physicsMaterial2D";
        if(AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath)==null)AssetDatabase.CreateAsset(friction,materialPath);
        else UnityEngine.Object.DestroyImmediate(friction);
        capsule.sharedMaterial=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(materialPath);
        var body=player.GetComponent<Rigidbody2D>();body.gravityScale=2.7f;body.freezeRotation=true;body.interpolation=RigidbodyInterpolation2D.Interpolate;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        var sr=player.GetComponent<SpriteRenderer>();sr.sortingOrder=100;
        var stage=new GameObject("Stage - El bosque de las brasas");director=stage.AddComponent<StageDirector>();director.Player=controller;
        director.coinFrames=Frames("Assets/Sprites/Animations/Coin");director.fireFrames=Frames("Assets/Sprites/Animations/Campfire");
        var camera=new GameObject("Main Camera");camera.tag="MainCamera";camera.transform.position=new Vector3(4,3.1f,-10);
        var cam=camera.AddComponent<Camera>();cam.orthographic=true;cam.orthographicSize=6.3f;cam.backgroundColor=new Color(.035f,.13f,.16f);camera.AddComponent<AudioListener>();
        var follow=camera.AddComponent<StageCamera>();follow.target=player.transform;
        follow.backgrounds=Enumerable.Range(1,5).Select(i=>SpriteAt("Assets/Sprites/Forest_Background/bg_forest_"+i+".png")).ToArray();
        world=new GameObject("STAGE 01 - El ultimo faro").transform;
        terrain=Group("01 - Suelo y plataformas");decoration=Group("02 - Poblado y vegetacion");encounters=Group("03 - Enemigos");treasure=Group("04 - Monedas, reliquias y faros");
        Sprite[] tiles=AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Tileset/tileset _1.png").OfType<Sprite>().ToArray();
        top=tiles.First(s=>s.name=="tileset _1_3");
        const string soilPath="Assets/Stage/Tierra.asset";
        soil=AssetDatabase.LoadAssetAtPath<Sprite>(soilPath);
        if(soil==null)
        {
            var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Tileset/tileset _1.png");
            // The importer scales the 4096px source atlas down on this target.
            float atlasScale=atlas.width/4096f;
            soil=Sprite.Create(atlas,new Rect(2240*atlasScale,1280*atlasScale,128*atlasScale,128*atlasScale),new Vector2(.5f,.5f),128*atlasScale,0,SpriteMeshType.FullRect);
            soil.name="Tierra oscura";AssetDatabase.CreateAsset(soil,soilPath);
        }
        ledge=tiles.OrderBy(s=>Vector2.Distance(s.rect.center,new Vector2(2400,960))).First();
        const string spikePath="Assets/Stage/Espinas.asset";
        spikes=AssetDatabase.LoadAssetAtPath<Sprite>(spikePath);
        if(spikes==null)
        {
            var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Tileset/tileset _1.png");
            float scale=atlas.width/4096f;
            spikes=Sprite.Create(atlas,new Rect(640*scale,1088*scale,128*scale,128*scale),new Vector2(.5f,.5f),128*scale,0,SpriteMeshType.FullRect);
            spikes.name="Espinas";AssetDatabase.CreateAsset(spikes,spikePath);
        }
        for(int i=0;i<islands.GetLength(0);i++)Ground(islands[i,0],islands[i,1],islands[i,2]);
        Platform(15,2.6f,4);Platform(23,1.25f,2);Platform(32,2.6f,4);Platform(37,4.6f,3);
        Platform(44.5f,2.2f,2);Platform(53,3.6f,4);Platform(60,5,4);Platform(65,6.5f,3);Platform(70,3,3);
        Platform(80,2.7f,4);Platform(88,4.3f,3);Platform(95,1.8f,2);Platform(103,2.5f,4);
        Platform(110,4.7f,4);Platform(117,2.8f,3);Platform(121,1.5f,2);
        // Coins outline safe trajectories and invite short detours into the canopy.
        for(int x=8;x<146;x+=3)
        {
            float y=GroundAt(x);if(float.IsNaN(y))continue;
            Pickup(StagePickup.Kind.Coin,new Vector2(x,y+1.15f),director.coinFrames,.55f);
        }
        float[,] trails={{15,3.7f},{23,2.3f},{32,3.8f},{37,5.7f},{53,4.8f},{60,6.2f},{65,7.6f},{80,3.9f},{88,5.5f},{103,3.7f},{110,5.9f},{117,4}};
        for(int i=0;i<trails.GetLength(0);i++)for(int j=-1;j<=1;j++)Pickup(StagePickup.Kind.Coin,new Vector2(trails[i,0]+j*.7f,trails[i,1]),director.coinFrames,.55f);
        Sprite[] relic=Frames("Assets/Sprites/Animations/Key");
        Pickup(StagePickup.Kind.Relic,new Vector2(37,6.5f),relic,.75f).GetComponent<SpriteRenderer>().color=new Color(.5f,1,1);
        Pickup(StagePickup.Kind.Relic,new Vector2(65,8.45f),relic,.75f).GetComponent<SpriteRenderer>().color=new Color(.5f,1,1);
        Pickup(StagePickup.Kind.Relic,new Vector2(110,6.6f),relic,.75f).GetComponent<SpriteRenderer>().color=new Color(.5f,1,1);
        foreach(float x in new[]{4f,51f,101f,125f})
        {
            float y=GroundAt(x);
            Pickup(StagePickup.Kind.Checkpoint,new Vector2(x,y+.38f),director.fireFrames,.85f);
            Decor("Assets/Sprites/Objects/Vegetations/stone_1.png",x-.6f,y,.8f);
        }
        Enemy(StageEnemy.Species.Slime,13,0,9,19);Enemy(StageEnemy.Species.Slime,31,0,27,41);
        Enemy(StageEnemy.Species.Skeleton,40,0,33,43);Enemy(StageEnemy.Species.Bat,38,3.5f,31,43);
        Enemy(StageEnemy.Species.Slime,56,1,50,66);Enemy(StageEnemy.Species.Bat,62,4.5f,54,68);
        Enemy(StageEnemy.Species.Skeleton,79,0,74,92);Enemy(StageEnemy.Species.Slime,90,0,85,93);
        Enemy(StageEnemy.Species.Bat,87,4,79,94);Enemy(StageEnemy.Species.Skeleton,108,0,100,118);
        Enemy(StageEnemy.Species.Bat,115,4.5f,105,118);
        Enemy(StageEnemy.Species.Guardian,138,0,129,146);
        Hazard(58,1);Hazard(85,0);Hazard(113,0);
        // Safe resting places break up the combat and guide the eye toward the next area.
        Decor("Assets/Sprites/Objects/Buildings/house_3.png",0,0,1);
        Decor("Assets/Sprites/Objects/Buildings/house_4.png",49,1,.8f);
        Decor("Assets/Sprites/Objects/Buildings/house_1.png",99,0,.8f);
        for(int i=0;i<24;i++)
        {
            float x=-4+i*6.5f;float ground=GroundAt(x);if(float.IsNaN(ground))continue;
            Decor("Assets/Sprites/Objects/Vegetations/tree_"+(i%3+1)+".png",x,ground,1.05f+i%3*.15f);
        }
        for(int x=-5;x<153;x+=2)
        {
            float ground=GroundAt(x);if(float.IsNaN(ground))continue;
            string file=x%3==0?"bush_2.png":x%3==1?"grass_2.png":"mushroom_1.png";
            Decor("Assets/Sprites/Objects/Vegetations/"+file,x+.3f,ground,.7f);
        }
        var exit=Pickup(StagePickup.Kind.Exit,new Vector2(149,1.2f),director.fireFrames,2.4f);
        exit.name="El ultimo faro - salida";exit.GetComponent<SpriteRenderer>().color=new Color(.45f,1,.85f);
        Decor("Assets/Sprites/Objects/Vegetations/stone_1.png",149,0,3);
        // Keep the player inside the single stage at its ends.
        Wall(-8,0,20);Wall(155,0,20);
        PlayerSettings.productName="Brasas del Bosque";PlayerSettings.companyName="Jonathan";
        PlayerSettings.defaultIsFullScreen=true;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
        PlayerSettings.resizableWindow=true;PlayerSettings.runInBackground=false;
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene,ScenePath);
        AssetDatabase.ImportAsset(ScenePath,ImportAssetOptions.ForceSynchronousImport);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        ValidateScene();
        Debug.Log("STAGE_GENERATED: 162 unidades, 6 islas, 17 plataformas, 12 enemigos, 3 reliquias, 4 faros.");
    }
    static Transform Group(string name){var go=new GameObject(name);go.transform.parent=world;return go.transform;}
    static Sprite SpriteAt(string path)
    {var result=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault();if(result==null)throw new Exception("Sprite faltante: "+path);return result;}
    static Sprite[] Frames(string folder)
    {var files=Directory.GetFiles(folder,"*.png").OrderBy(f=>f,StringComparer.Ordinal).ToArray();if(files.Length==0)throw new Exception("Animacion vacia: "+folder);return files.Select(f=>SpriteAt(f.Replace('\\','/'))).ToArray();}
    static GameObject Visual(string name,Sprite sprite,Vector2 position,Transform parent,int order)
    {var go=new GameObject(name);go.transform.parent=parent;go.transform.position=position;var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;return go;}
    static void Ground(float left,float right,float height)
    {
        var go=new GameObject("Isla "+left+" a "+right);go.transform.parent=terrain;go.layer=6;
        var c=go.AddComponent<BoxCollider2D>();c.size=new Vector2(right-left,6);c.offset=new Vector2((left+right)/2,height-3);
        for(int x=(int)left;x<right;x++)for(int row=0;row<6;row++)Visual("Suelo",row==0?top:soil,new Vector2(x+.5f,height-.5f-row),go.transform,10);
    }
    static void Platform(float x,float y,int width)
    {
        var go=new GameObject("Plataforma "+x);go.transform.parent=terrain;go.transform.position=new Vector2(x,y);go.layer=6;
        var c=go.AddComponent<BoxCollider2D>();c.size=new Vector2(width,.4f);c.offset=new Vector2(0,-.2f);
        var effector=go.AddComponent<PlatformEffector2D>();effector.useOneWay=true;effector.surfaceArc=150;c.usedByEffector=true;
        for(int i=0;i<width;i++)Visual("Cornisa",top,new Vector2(x-width*.5f+i+.5f,y-.5f),go.transform,15);
    }
    static void Wall(float x,float y,float height){var go=new GameObject("Limite del stage");go.transform.parent=terrain;go.transform.position=new Vector2(x,y);go.layer=6;go.AddComponent<BoxCollider2D>().size=new Vector2(1,height);}
    static float GroundAt(float x){for(int i=0;i<islands.GetLength(0);i++)if(x>=islands[i,0] && x<islands[i,1])return islands[i,2];return float.NaN;}
    static void Decor(string path,float x,float y,float scale)
    {var sprite=SpriteAt(path);var go=Visual(Path.GetFileNameWithoutExtension(path),sprite,new Vector2(x,y+sprite.bounds.size.y*scale*.5f-.07f),decoration,5);go.transform.localScale=Vector3.one*scale;}
    static GameObject Pickup(StagePickup.Kind kind,Vector2 position,Sprite[] frames,float scale)
    {var go=Visual(kind.ToString(),frames[0],position,treasure,80);go.transform.localScale=Vector3.one*scale;var pickup=go.AddComponent<StagePickup>();pickup.kind=kind;pickup.frames=frames;return go;}
    static void Hazard(float x,float floor)
    {var go=Pickup(StagePickup.Kind.Spikes,new Vector2(x,floor+.15f),new[]{spikes},1);go.name="Espinas - zona de peligro";go.transform.localScale=new Vector3(2.5f,.85f,1);go.GetComponent<SpriteRenderer>().color=new Color(1,.7f,.5f);}
    static void Enemy(StageEnemy.Species species,float x,float floor,float left,float right)
    {
        string folder="Assets/Sprites/Enemy/"+(species==StageEnemy.Species.Guardian?"Skeleton_Boss":species.ToString());
        bool bat=species==StageEnemy.Species.Bat;bool boss=species==StageEnemy.Species.Guardian;
        float height=boss?2.9f:bat?1.05f:species==StageEnemy.Species.Slime? .95f:1.65f;
        var go=new GameObject(species.ToString());go.transform.parent=encounters;go.transform.position=new Vector2(x,floor+height*.5f+.05f);go.layer=8;
        var enemy=go.AddComponent<StageEnemy>();enemy.species=species;enemy.patrolLeft=left;enemy.patrolRight=right;enemy.floor=floor;enemy.visualHeight=height;enemy.maxHealth=boss?18:species==StageEnemy.Species.Skeleton?3:2;
        enemy.idleFrames=Frames(folder+(bat?"/fly":"/idle"));enemy.moveFrames=Frames(folder+(bat?"/fly":"/run"));
        enemy.attackFrames=species==StageEnemy.Species.Slime?enemy.moveFrames:Frames(folder+(bat?"/attack":"/attack_1"));enemy.deathFrames=Frames(folder+"/die");
        var visual=Visual("Animacion",enemy.idleFrames[0],go.transform.position,go.transform,60);
        visual.transform.localScale=Vector3.one*height/enemy.idleFrames.Max(s=>s.bounds.size.y);
        var c=go.AddComponent<CapsuleCollider2D>();c.size=new Vector2(height*(bat? .8f:.6f),height*.9f);
        var body=go.AddComponent<Rigidbody2D>();body.gravityScale=bat?0:2.7f;body.freezeRotation=true;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        c.sharedMaterial=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Stage/Movimiento.physicsMaterial2D");
        if(boss)director.Boss=enemy;
    }
    public static void ValidateScene()
    {
        var stage=UnityEngine.Object.FindObjectOfType<StageDirector>();
        if(stage==null || stage.Player==null || Camera.main==null)throw new Exception("Falta el stage, personaje o camara.");
        if(stage.coinFrames.Length==0 || stage.fireFrames.Length==0)throw new Exception("Faltan recursos de los objetos.");
        var enemies=UnityEngine.Object.FindObjectsOfType<StageEnemy>();
        if(enemies.Length!=12 || enemies.Count(e=>e.IsBoss)!=1)throw new Exception("Debe haber 12 enemigos y exactamente un jefe.");
        foreach(var e in enemies)if(e.moveFrames.Length==0 || e.deathFrames.Length==0 || e.GetComponent<Collider2D>()==null)throw new Exception("Enemigo incompleto: "+e.name);
        var pickups=UnityEngine.Object.FindObjectsOfType<StagePickup>();
        if(pickups.Count(p=>p.kind==StagePickup.Kind.Relic)!=3 || pickups.Count(p=>p.kind==StagePickup.Kind.Checkpoint)!=4 || pickups.Count(p=>p.kind==StagePickup.Kind.Exit)!=1)throw new Exception("El stage no tiene todos sus objetivos.");
        foreach(var go in SceneManager.GetActiveScene().GetRootGameObjects())ValidateComponents(go);
        Debug.Log("STAGE_VALIDATION_OK: referencias, colisiones, enemigos y objetivos.");
    }
    static void ValidateComponents(GameObject go)
    {if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go)>0)throw new Exception("Script perdido en "+go.name);foreach(Transform child in go.transform)ValidateComponents(child.gameObject);}
    public static void GenerateAndBuild()
    {
        Generate();Build();
    }
    public static void GenerateAndVerify()
    {
        Generate();StagePlayVerification.Run();
    }
    [MenuItem("Brasas/Compilar Windows")]
    public static void Build()
    {
        EditorSceneManager.OpenScene(ScenePath);ValidateScene();Directory.CreateDirectory(".tools/Build-Brasas-Final");
        var report=BuildPipeline.BuildPlayer(new[]{ScenePath},".tools/Build-Brasas-Final/BrasasDelBosque.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Fallo la compilacion: "+report.summary.result);
        Debug.Log("STAGE_BUILD_OK: "+report.summary.totalSize+" bytes");
    }
}

