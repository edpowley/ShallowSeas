using UnityEngine;
using System.Collections;

public class GearItem : MonoBehaviour
{
    public float CastDuration;
    public int MaxCatch;
    public MeshRenderer Renderer;

    internal bool IsCast { get; private set; }

    internal float Progress { get; private set; }
    private float m_progressPerSecond;

    // Use this for initialization
    void Start()
    {
        
    }

    internal void Cast()
    {
        IsCast = true;
        Progress = 0;
        m_progressPerSecond = 1.0f / CastDuration;
    }
    
    // Update is called once per frame
    void Update()
    {
        Renderer.enabled = IsCast;

        if (IsCast)
        {
            Progress += m_progressPerSecond * Time.deltaTime;
            if (Progress >= 1.0f)
                IsCast = false;
        }
    }
}
