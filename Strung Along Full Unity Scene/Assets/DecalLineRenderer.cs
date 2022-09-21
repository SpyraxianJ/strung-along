using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DecalLineRenderer : MonoBehaviour
{

    public Transform start;
    public Transform end;

    [Space]

    public DecalProjector projector;

    [Space]

    public float decalHeight = 20;
    public float lineThickness = 0.2f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PositionUpdate();
    }

    public void SetPosition(Transform s, Transform e)
    {
        start = s;
        end = e;
        PositionUpdate();
    }

    void PositionUpdate()
    {
        transform.position = (start.position + end.position) / 2;
        transform.position = new Vector3(transform.position.x, decalHeight, transform.position.z);

        float rot = Mathf.Atan2((start.position - end.position).x, (start.position - end.position).z) * Mathf.Rad2Deg; // issue

        transform.rotation = Quaternion.Euler(new Vector3(0, rot, 0));

        projector.size = new Vector3(lineThickness, ignoreYDistance(start.position, end.position), decalHeight * 2f);
    }

    float ignoreYDistance(Vector3 a, Vector3 b)
    {
        return Vector3.Distance(new Vector3(a.x, 0, a.z), new Vector3(b.x, 0, b.z));
    }
}
