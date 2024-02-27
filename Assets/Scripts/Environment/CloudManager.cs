using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class CloudManager : MonoBehaviour
{
    public float scale = 0.1f;
    public Material material;
    public LayerMask layer;
    private Cloud[] _clouds;

    private void OnValidate()
    {
        Init();
    }

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += CloudAlign;
        Init();
    }

    private void Init()
    {
        transform.localScale = Vector3.one * scale;

        _clouds = new Cloud[transform.childCount];

        for (int i = 0; i < _clouds.Length; i++)
        {
            var cloud = new Cloud();
            cloud.transform = transform.GetChild(i);
            cloud.matrix = cloud.transform.localToWorldMatrix;
            cloud.mesh = cloud.transform.GetComponent<MeshFilter>().sharedMesh;
            cloud.transform.GetComponent<Renderer>().enabled = false;
            _clouds[i] = cloud;
        }
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= CloudAlign;
    }

    void CloudAlign(ScriptableRenderContext context, Camera camera)
    {
        if (camera.cameraType != CameraType.Preview)
        {
            var t = camera.transform;
            var position = t.position;
            position -= position * scale;
            transform.position = position;

            Debug.Log($"Rendering {_clouds.Length} clouds for camera:{camera.name}");
            foreach (var cloud in _clouds)
            {
                Graphics.DrawMesh(cloud.mesh, cloud.transform.localToWorldMatrix, material, 8);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Vector3.zero, 750f);
    }

    private class Cloud
    {
        public Transform transform;
        public Matrix4x4 matrix;
        public Mesh mesh;
    }
}