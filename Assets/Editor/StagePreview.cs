using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;

// Offline camera renders for level-art review; no desktop input or screen capture.
public static class StagePreview
{
    public static void GenerateBuildAndRender(){StageBuild.Generate();BuildAndRender();}
    public static void BuildAndRender()
    {
        StageBuild.Build();
        EditorSceneManager.OpenScene("Assets/Scenes/BrasasDelBosque.unity");
        var camera=Camera.main;var follow=camera.GetComponent<StageCamera>();follow.SendMessage("Start");
        float[] positions={2,55,86,138};
        foreach(float x in positions)
        {
            follow.target.position=new Vector3(x,1,0);follow.Snap();follow.SendMessage("LateUpdate");
            var target=new RenderTexture(1280,720,24);camera.targetTexture=target;camera.Render();
            var previous=RenderTexture.active;RenderTexture.active=target;
            var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();
            File.WriteAllBytes(".tools/stage-preview-"+x+".png",texture.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=previous;
            Object.DestroyImmediate(texture);Object.DestroyImmediate(target);
        }
        Debug.Log("STAGE_PREVIEWS_OK");
    }
}
