using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateByTime : MonoBehaviour
{
    public float change = 10f;
    public Vector3 m_from = new Vector3(0.0F, 45.0F, 0.0F);
    public Vector3 m_to = new Vector3(0.0F, -45.0F, 0.0F);
    [SerializeField] protected float m_frequency = 1.0F;
    private void Start()
    {
        m_from = transform.localRotation.eulerAngles - new Vector3(0, 0, change);
        m_to = transform.localRotation.eulerAngles + new Vector3(0, 0, change);
    }

    protected virtual void Update()
    {
        
        Quaternion from = Quaternion.Euler(this.m_from);
        Quaternion to = Quaternion.Euler(this.m_to);

        float lerp = 0.5F * (1.0F + Mathf.Sin(Mathf.PI * Time.realtimeSinceStartup * this.m_frequency));
        this.transform.localRotation = Quaternion.Lerp(from, to, lerp);
    }
}
