using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

public class QRController : MonoBehaviour
{
    private QRScanner scanner;          // assign your existing scanner (on AR Camera)

    Renderer wallRenderer;             // set by WallVideoSurface when the wall spawns
    string videoTexProp = "_MainTex";  // auto-switches to _BaseMap for URP

    void Awake()
    {
        scanner = GetComponent<QRScanner>();
        if (!scanner) scanner = GetComponent<QRScanner>() ?? FindObjectOfType<QRScanner>();
    }

    void OnEnable()  { if (scanner) scanner.OnQrFound += Handle; }
    void OnDisable() { if (scanner) scanner.OnQrFound -= Handle; }

    // Called by WallVideoSurface on the spawned wall
    public void RegisterWall(Renderer r)
    {
        wallRenderer = r;
        EnsureVideoMaterial();
    }

    // -------- QR handling --------
    void Handle(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) { scanner.ResumeScanning(); return; }
        var t = text.Trim();

        if (t.StartsWith("sa:", StringComparison.OrdinalIgnoreCase))
        {
            var file = t.Substring(3).Trim();
            StartCoroutine(ResolveSaToPlayableUrl(file, url => StartOrQueue(url)));
            return;
        }
        scanner.ResumeScanning();
    }

    void StartOrQueue(string url)
    {
        if (wallRenderer) 
            PlayOnWall(url);
    }

    // -------- Video playback on wall --------
    void PlayOnWall(string url)
    {
        var go = wallRenderer.gameObject;
        var vp = go.GetComponent<VideoPlayer>() ?? go.AddComponent<VideoPlayer>();
        var audio = go.GetComponent<AudioSource>() ?? go.AddComponent<AudioSource>();

        vp.playOnAwake = false;
        vp.renderMode = VideoRenderMode.MaterialOverride;
        vp.targetMaterialRenderer = wallRenderer;
        vp.targetMaterialProperty = videoTexProp;   // _BaseMap on URP, fallback _MainTex

        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        audio.playOnAwake = false;
        vp.SetTargetAudioSource(0, audio);

        vp.isLooping = false;
        vp.errorReceived    += (p, m) => { Debug.LogError("Video error: " + m); scanner.ResumeScanning(); };
        vp.prepareCompleted += _ => { audio.Play(); vp.Play(); };
        vp.loopPointReached += _ => { audio.Stop(); scanner.ResumeScanning(); };

        vp.source = VideoSource.Url;
        vp.url = url;
        vp.Prepare();
    }

    bool EnsureVideoMaterial()
    {
        if (!wallRenderer) return false;
            
        var mat = wallRenderer.sharedMaterial;
        if (mat == null || (!mat.HasProperty("_BaseMap") && !mat.HasProperty("_MainTex")))
        {
            Debug.LogError("I NEED NEW MATERIAL");
            // ensure a texture-capable material
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Unlit/Texture");
            wallRenderer.material = new Material(shader);
            mat = wallRenderer.sharedMaterial;
        }

        videoTexProp = mat.HasProperty("_BaseMap") ? "_BaseMap" : "_MainTex";
        return true;
    }

    // -------- StreamingAssets helper for sa: --------
    IEnumerator ResolveSaToPlayableUrl(string filename, Action<string> onReady)
    {
        string saPath = Path.Combine(Application.streamingAssetsPath, filename);

#if UNITY_ANDROID
        // Copy once from APK to a normal path
        string dst = Path.Combine(Application.persistentDataPath, filename);
        if (!File.Exists(dst))
        {
            using (var req = UnityWebRequest.Get(saPath))
            {
                yield return req.SendWebRequest();
                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError("StreamingAssets copy failed: " + req.error);
                    yield break;
                }
                File.WriteAllBytes(dst, req.downloadHandler.data);
            }
        }
        onReady?.Invoke("file://" + dst);
#else
        if (!saPath.StartsWith("file://")) saPath = "file://" + saPath;
        onReady?.Invoke(saPath);
        yield return null;
#endif
    }
}
