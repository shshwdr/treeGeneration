using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineGenerator : MonoBehaviour
{
    [SerializeField] float minWidth = 0.018f;
    [SerializeField] float fadeTime = 0.4f;

    private LineRenderer line = default;
    private bool isInFade = false;

    private Vector3[] startCurve;
    private Vector3[] endCurve;
    private float lineWidth;

    public bool IsInFade => isInFade;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
    }

    public void UpdateLine(Vector3[] curve, float lineWidth)
    {
        line.positionCount = curve.Length;
        line.SetPositions(curve);
        line.widthMultiplier = Mathf.Max(minWidth, lineWidth);
    }

    public void UpdateLine(Vector3[] startCurve, Vector3[] endCurve, float lineWidth)
    {
        this.startCurve = startCurve;
        this.endCurve = endCurve;
        this.lineWidth = lineWidth;

        isInFade = true;
        StartCoroutine(ProcUpdateLine());
    }

    IEnumerator ProcUpdateLine()
    {
        int size = startCurve.Length;
        line.positionCount = size;
        line.SetPositions(startCurve);

        Vector3[] curve = new Vector3[size];
        System.Array.Copy(startCurve, curve, size);
        float w = 0f;
        int iterations = (size - 1) / 2;

        while (w < 1f)
        {
            int index = 1;
            for (int i = 0; i < iterations; i++)
            {
                curve[index] = Vector3.Lerp(startCurve[index], endCurve[index], w);
                index += 2;
            }

            line.SetPositions(curve);
            yield return null;
            w += Time.deltaTime / fadeTime;
        }

        line.positionCount = endCurve.Length;
        line.SetPositions(endCurve);
        line.widthMultiplier = Mathf.Max(minWidth, lineWidth);
        isInFade = false;
    }
}
