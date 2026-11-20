using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[DisallowMultipleComponent]
public class WallVideoSurface : MonoBehaviour
{
    private QRController controller;  // optional: drag AR Camera here
    private GameObject camera;
    void Awake()
    {
        if (!controller)
        {
            camera = GameObject.Find("Main Camera");
            controller = camera.GetComponent<QRController>();
        }
        var rend = GetComponent<Renderer>();
        if (controller && rend)
            controller.RegisterWall(rend);
        else
            Debug.LogError("WallVideoSurface: QRController or Renderer missing.");
    }
}