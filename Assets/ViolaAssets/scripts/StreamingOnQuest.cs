using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer), typeof(Renderer))]
public class StreamingOnQuest : MonoBehaviour
{
    [SerializeField] string fileName = "clip.mp4"; // exact name in StreamingAssets
    VideoPlayer vp;

    void Awake()
    {
        vp = GetComponent<VideoPlayer>();
        vp.renderMode = VideoRenderMode.MaterialOverride;
        vp.targetMaterialRenderer = GetComponent<Renderer>();
        vp.targetMaterialProperty = "_BaseMap"; // use "_MainTex" if your shader uses that
        vp.source = VideoSource.Url;
        vp.audioOutputMode = VideoAudioOutputMode.None;
        vp.waitForFirstFrame = true;
        vp.skipOnDrop = true;
        vp.playOnAwake = false;
        vp.isLooping = true;

#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(CopyThenPlay());
#else
        vp.url = Path.Combine(Application.streamingAssetsPath, fileName);
        vp.prepareCompleted += _ => vp.Play();
        vp.Prepare();
#endif
    }

    IEnumerator CopyThenPlay()
    {
        string src = Path.Combine(Application.streamingAssetsPath, fileName);
        string dst = Path.Combine(Application.persistentDataPath, fileName);

        if (!File.Exists(dst))
        {
            using var req = UnityWebRequest.Get(src);
            yield return req.SendWebRequest();
            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Video copy failed: " + req.error);
                yield break;
            }
            File.WriteAllBytes(dst, req.downloadHandler.data);
        }

        vp.url = "file://" + dst;
        vp.prepareCompleted += _ => vp.Play();
        vp.Prepare();
    }
}
