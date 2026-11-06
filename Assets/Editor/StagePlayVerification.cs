using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// End-to-end checks in a real Play Mode session, isolated from the shipping build.
[InitializeOnLoad]
public static class StagePlayVerification
{
    static double next;
    static int phase;
    static StageEnemy target;
    static int targetHealth;
    static StagePlayVerification(){EditorApplication.update+=Tick;}
    public static void Run()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/BrasasDelBosque.unity");
        SessionState.SetBool("Brasas.Verify",true);SessionState.SetFloat("Brasas.Start",(float)EditorApplication.timeSinceStartup);
        EditorApplication.isPlaying=true;
    }
    static void Require(bool ok,string message){if(!ok)throw new Exception("STAGE_TEST_FAILED: "+message);Debug.Log("STAGE_CHECK_OK: "+message);}
    static void Tick()
    {
        if(!SessionState.GetBool("Brasas.Verify",false))return;
        try
        {
            if(EditorApplication.timeSinceStartup-SessionState.GetFloat("Brasas.Start",0)>90)throw new Exception("Play Mode verification timed out.");
            if(!EditorApplication.isPlaying || StageDirector.Instance==null)return;
            var stage=StageDirector.Instance;var player=stage.Player;
            if(EditorApplication.timeSinceStartup<next)return;
            if(phase==0)
            {
                Require(stage.InputBlocked,"la introduccion bloquea el movimiento");stage.Begin();
                Require(!stage.InputBlocked && player.Health==5,"el stage inicia con cinco vidas");
                player.QueueJump();next=EditorApplication.timeSinceStartup+.15;phase=10;
            }
            else if(phase==10)
            {
                Require(player.transform.position.y>1.4f && player.GetComponent<Rigidbody2D>().velocity.y>0,"el salto eleva al personaje usando fisica real");
                player.QueueJump();next=EditorApplication.timeSinceStartup+.15;phase=11;
            }
            else if(phase==11)
            {
                Require(player.AirJumps==0 && player.GetComponent<Rigidbody2D>().velocity.y>0,"el segundo salto consume la reserva aerea");
                Require(player.TryDash(Vector2.right),"el dash puede activarse en el aire");
                Require(!player.TryDash(Vector2.right),"el dash no puede repetirse inmediatamente");
                next=EditorApplication.timeSinceStartup+.24;phase=12;
            }
            else if(phase==12)
            {
                Require(!player.hacerRodar && !player.TryDash(Vector2.right),"el dash termina y respeta su recarga");
                player.RestoreAt(new Vector3(2,1,0));next=EditorApplication.timeSinceStartup+2.3;phase=1;
            }
            else if(phase==1)
            {
                player.TakeDamage(1,(Vector2)player.transform.position+Vector2.right);
                Require(player.Health==4,"el contacto resta vida");player.TakeDamage(1,player.transform.position);
                Require(player.Health==4,"la invulnerabilidad evita dano repetido");
                stage.CollectCoin();stage.CollectRelic();int deaths=stage.Deaths;stage.Respawn();
                Require(player.Health==5 && stage.Deaths==deaths+1 && stage.Coins==1 && stage.Relics==1,"reaparecer restaura vida y conserva coleccionables");
                target=UnityEngine.Object.FindObjectsOfType<StageEnemy>().First(e=>e.species==StageEnemy.Species.Slime);
                targetHealth=target.Health;
                player.RestoreAt(target.transform.position+new Vector3(-1,.3f,0));Physics2D.SyncTransforms();player.SendMessage("Attack");
                Require(target.Health==targetHealth-1,"la espada realmente alcanza y dana un enemigo");
                next=EditorApplication.timeSinceStartup+.6;phase=2;
            }
            else if(phase==2)
            {
                int kills=stage.Kills;target.TakeHit(99,Vector2.right);
                Require(target.Dead && stage.Kills==kills+1,"la muerte de un enemigo aumenta el marcador");
                int coins=stage.Coins;
                for(int i=0;i<19;i++)stage.CollectCoin();
                Require(stage.Coins==coins+19,"las monedas se acumulan");
                var boss=stage.Boss;boss.TakeHit(2,Vector2.left);boss.ResetBoss();
                Require(boss.Health==boss.maxHealth,"el encuentro del jefe se reinicia al reaparecer");
                boss.TakeHit(999,Vector2.right);
                Require(stage.BossDefeated,"derrotar al jefe abre la salida");
                player.RestoreAt(new Vector3(149,1.2f,0));
                next=EditorApplication.timeSinceStartup+.5;phase=3;
            }
            else if(phase==3)
            {
                Require(stage.InputBlocked,"el ultimo faro completa el stage");
                File.WriteAllText(".tools/stage-verification.txt","PASS: intro, salud, invulnerabilidad, checkpoints, coleccionables, combate, enemigos, reinicio del jefe y victoria.\n");
                Debug.Log("STAGE_PLAY_VERIFICATION_OK");SessionState.SetBool("Brasas.Verify",false);EditorApplication.Exit(0);
            }
        }
        catch(Exception e){SessionState.SetBool("Brasas.Verify",false);Debug.LogException(e);EditorApplication.Exit(1);}
    }
}
