using UnityEngine;

public class AirWall : MonoBehaviour
{
    public GameObject cubeMeshRender;
    public BoxCollider boxCollider;
    public Vector3 pos;
    public Vector3 forward;
    public Vector3 scaleW;


    public void Init(float width)
    {
        boxCollider = gameObject.GetComponent<BoxCollider>();
        cubeMeshRender = gameObject.GetComponentInChildren<MeshRenderer>().gameObject;
        boxCollider.size = new Vector3(width, 10, 1.0f);
        cubeMeshRender.transform.localScale = new Vector3(width, 3, 1);
    }
}