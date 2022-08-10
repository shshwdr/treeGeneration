using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawDragon : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Button buttonSplit = default;
    [SerializeField] UnityEngine.UI.Button buttonMerge = default;
    [SerializeField] UnityEngine.UI.Text labelIteration = default;
    [SerializeField] LineGenerator lineGenerator = default;
    [SerializeField] float initRadius = 300f;

    private DragonCurve dragonCurve = null;

    struct CurveData
    {
        public Vector3[] line;
        public Vector3[] curve;
        public float width;

        public CurveData(Vector3[] line, Vector3[] curve, float width)
        {
            this.line = line;
            this.curve = curve;
            this.width = width;
        }
    }

    private Dictionary<int, CurveData> curveTable = new Dictionary<int, CurveData>();

    private int currentIteration = 1;
    public readonly int kMaxIteration = 2;
    void Start()
    {
        buttonSplit.onClick.AddListener(() =>
        {
            if (currentIteration >= kMaxIteration || lineGenerator.IsInFade) return;
            currentIteration++;
            UpdateIterationLabel();
            var curveData = curveTable[currentIteration];
            lineGenerator.UpdateLine(curveData.curve, curveData.width);
        });
        buttonMerge.onClick.AddListener(() =>
        {
            if (currentIteration <= 1 || lineGenerator.IsInFade) return;
            var curveData = curveTable[currentIteration];
            lineGenerator.UpdateLine(curveData.line, curveData.width);
            currentIteration--;
            UpdateIterationLabel();
        });

        dragonCurve = new DragonCurve(false);

        // pre calculate curves for all iteration
        float lineWidth = 0.5f;
        //List<Vector3> curve = new List<Vector3>();
        //curve.Add(new Vector3(-initRadius, initRadius / 2, 0));
        //curve.Add(new Vector3(0, -initRadius / 2, 0));
        //curve.Add(new Vector3(initRadius, initRadius / 2, 0));

        //curveTable[1] = new CurveData(curve.ToArray(), curve.ToArray(), lineWidth);

        //for (int i = 2; i <= kMaxIteration; i++)
        //{
        //    List<Vector3> splitLine = dragonCurve.SplitByLine(curve);
        //    List<Vector3> splitCurve = dragonCurve.SplitByCurve(curve);
        //    lineWidth = lineWidth / 1.4f;

        //    curveTable[i] = new CurveData(splitLine.ToArray(), splitCurve.ToArray(), lineWidth);
        //    curve = splitCurve;
        //}


        List<char> res = new List<char>(){ 'F'};

        int test = 0;
        for (int i = 0; i < kMaxIteration; i++)
        {
            List<char> newRes = new List<char>();
            for (int j = 0; j < res.Count; j++)
            {
                switch (res[j])
                {
                    case 'F':
                        newRes.Add('F');
                        newRes.Add('+');
                        newRes.Add('G');
                        break;
                    case 'G':

                        newRes.Add('F');
                        newRes.Add('-');
                        newRes.Add('G');
                        break;
                }
                test++;
                if (test > 1000)
                {
                    break;
                }
            }
            res = newRes;
            if (test > 1000)
            {
                break;
            }
        }
        currentIteration = 1;
       // UpdateIterationLabel();
       // lineGenerator.UpdateLine(curveTable[1].curve, curveTable[1].width);
    }

    void UpdateIterationLabel()
    {
        labelIteration.text = "Iteration : " + currentIteration.ToString();
    }
}
