using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalLineVisualizer : MonoBehaviour
{

    public PuppetController puppet;
    public List<LineRenderer> lines;
    public Material mat;

    [Space]

    public Color lineStartColor;
    public Color lineEndColor;
    [Tooltip("The distance that the lines are previewed, making this too high can cause a performance drop")]
    public float linePreviewDistance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //lines[0].colorGradient.SetKeys()

        // Destroy all old lines (very inefficent sorry)

        try
        {
            for (int i = 0; i < lines.Count; i++)
            {
                Destroy(lines[i].gameObject);
                lines.RemoveAt(i);
            }
        }
        catch (System.Exception)
        {
            Debug.Log("Lines not destoryed successfully, probably not an issue");
        }

        // Puppet to point 1

        float distanceLeft = linePreviewDistance - Vector3.Distance(puppet.transform.position, new Vector3(puppet.gridPoint1.transform.position.x, puppet.transform.position.y, puppet.gridPoint1.transform.position.z));
        float distanceLeftBefore = linePreviewDistance;

        CreateLine(puppet.transform.position, puppet.gridPoint1.transform.position, puppet.transform.position.y, distanceLeftBefore, distanceLeft);

        if (distanceLeft > 0)
        {

            // Returns back to this point
            float distanceLock = distanceLeft;

            DoForPoint(distanceLock, puppet.gridPoint1);

        }

        // Puppet to point 2

        distanceLeft = linePreviewDistance - Vector3.Distance(puppet.transform.position, new Vector3(puppet.gridPoint2.transform.position.x, puppet.transform.position.y, puppet.gridPoint2.transform.position.z));
        distanceLeftBefore = linePreviewDistance;

        CreateLine(puppet.transform.position, puppet.gridPoint2.transform.position, puppet.transform.position.y, distanceLeftBefore, distanceLeft);

        if (distanceLeft > 0)
        {

            // Returns back to this point
            float distanceLock = distanceLeft;

            DoForPoint(distanceLock, puppet.gridPoint2);

        }

    }

    public void CreateLine(Vector3 from, Vector3 to, float y, float remainingStart, float remainingEnd)
    {
        GameObject obj = new GameObject();
        lines.Add(obj.AddComponent<LineRenderer>());
        lines[lines.Count - 1].SetPosition(0, new Vector3(from.x, y, from.z));
        lines[lines.Count - 1].SetPosition(1, new Vector3(to.x, y, to.z));
        lines[lines.Count - 1].colorGradient.colorKeys = getGradient(remainingStart, remainingEnd);
        lines[lines.Count - 1].colorGradient.alphaKeys = getAlpha(remainingStart, remainingEnd);
        lines[lines.Count - 1].material = mat;
    }

    public GradientColorKey[] getGradient(float startValue, float endValue)
    {
        GradientColorKey[] output = new GradientColorKey[2];
        output[0] = new GradientColorKey(Color.Lerp(lineStartColor, lineEndColor, startValue), 0);
        output[1] = new GradientColorKey(Color.Lerp(lineStartColor, lineEndColor, endValue), 1);
        return output;
    }

    public GradientAlphaKey[] getAlpha(float startValue, float endValue)
    {
        GradientAlphaKey[] output = new GradientAlphaKey[2];
        output[0] = new GradientAlphaKey(Mathf.Lerp(lineStartColor.a, lineEndColor.a, startValue), 0);
        output[1] = new GradientAlphaKey(Mathf.Lerp(lineStartColor.a, lineEndColor.a, endValue), 1);
        return output;
    }

    public float Distance(Vector3 from, Vector3 to) {
        return (Vector3.Distance(from, new Vector3(to.x, from.y, to.z)));
    }

    public void DoForPoint(float distanceLock, GridPoint current)
    {
        for (int i = 0; i < current.connectedPoints.Count; i++)
        {
            float distanceLeft = distanceLock;
            float distanceLeftBefore = distanceLock;
            distanceLeft = distanceLeft - Distance(current.transform.position, current.connectedPoints[i].transform.position);

            if (distanceLeft < 0)
            {
                CreateLine(current.transform.position, (current.transform.position - current.connectedPoints[i].transform.position).normalized * distanceLeftBefore, puppet.transform.position.y, distanceLeftBefore, distanceLeft);
            }
            else
            {
                CreateLine(current.transform.position, current.connectedPoints[i].transform.position, puppet.transform.position.y, distanceLeftBefore, distanceLeft);
                DoForPoint(distanceLeft, current.connectedPoints[i]);
            }

        }
    }
}
