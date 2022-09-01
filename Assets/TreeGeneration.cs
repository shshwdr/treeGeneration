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

    public void growTrunk()
    {
        widthScale *= 1.2f;
    }


    float maxX = 0;
    float minX = 0;
    float maxY = 0.28f;
    float minY = 0;
    public void updatePosition(Vector3 p)
    {
        maxY = Mathf.Max(p.y, maxY);
        minY = Mathf.Min(p.y, minY);
        maxX = Mathf.Max(p.x, maxX);
        minX = Mathf.Min(p.x, minX);
        Camera.main.transform.position = new Vector3((maxX + minX) / 2f, (maxY + minY) / 2, Camera.main.transform.position.z);
        Camera.main.orthographicSize = (maxY - minY) / 2;
    }

    public void grow()
    {
        if(root == null)
        {

            var go = Instantiate(treeRootPrefab, treeParent);
            root = go.transform;
           // root.transform.Rotate(new Vector3(0, 0, 90));
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
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }


}
