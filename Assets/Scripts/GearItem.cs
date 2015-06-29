using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GearItem : MonoBehaviour
{
    public float CastDuration;
    public int MaxCatch;
    public MeshRenderer Renderer;

    internal bool IsCast { get; private set; }

    internal float Progress { get; private set; }
    private float m_progressPerSecond;
    private int m_fishCaught;

    // Use this for initialization
    void Start()
    {
        
    }

    internal void Cast()
    {
        IsCast = true;
        Progress = 0;
        m_progressPerSecond = 1.0f / CastDuration;
        m_fishCaught = 0;
    }
    
    // Update is called once per frame
    void Update()
    {
        Renderer.enabled = IsCast;

        if (IsCast)
        {
            Progress += m_progressPerSecond * Time.deltaTime;
            if (Progress >= 1.0f)
            {
                IsCast = false;
            }
            else if (m_fishCaught < MaxCatch)
            {
                List<float> density = GameManager.Instance.CurrentCellFishDensity;
                int fishIndex = Random.Range(0, density.Count);

                if (Random.Range(0.0f, 1.0f) < density[fishIndex] * Time.deltaTime * 10)
                {
                    GameManager.Instance.PlayerBoat.m_currentCatch[fishIndex]++;
                    m_fishCaught++;
                }
            }
        }
    }
}
