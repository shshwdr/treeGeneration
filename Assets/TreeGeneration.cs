using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreeGeneration : Singleton<TreeGeneration>
{ 
    public GameObject treeRootPrefab;
    public Transform treeParent;

    Transform root;

    public float widthScale = 1;

    float maxWidth = 5;

    public void growTrunk()
    {
        widthScale *= 1.25f;
        widthScale = Mathf.Min(widthScale, maxWidth);
        foreach(var growth in GameObject.FindObjectsOfType<BranchGrowth>())
        {
            growth.grow();
        }
    }


    float maxX = 0;
    float minX = 0;
    float maxY = 30f;
    float minY = 0;
    public void updatePosition(Vector3 p)
    {
        maxY = Mathf.Max(p.y, maxY);
        minY = Mathf.Min(p.y, minY);
        maxX = Mathf.Max(p.x, maxX);
        minX = Mathf.Min(p.x, minX);
        Camera.main.transform.position = new Vector3(0, (maxY + minY) / 2, Camera.main.transform.position.z);
        Camera.main.orthographicSize = (maxY - minY) / 2 + 0.2f;
    }

    public void grow()
    {
        if(root == null)
        {

            var go = Instantiate(treeRootPrefab, treeParent);
            root = go.transform;
            root.transform.Rotate(new Vector3(0, 0, 90));
            //root.GetComponent<TreeRoot>().isTrueRoot = true;
        }
        else
        {
            float width = 0;
            var trans = root.GetComponent<TreeRoot>().getNode(ref width);
            if (trans)
            {

                var go = Instantiate(treeRootPrefab, trans.position, trans.rotation, trans);
                go.GetComponent<TreeRoot>().startWidth = width;
            }
        }


    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            grow();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            widthScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }


}
