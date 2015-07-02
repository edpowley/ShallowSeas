using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GearItem : MonoBehaviour
{
    public float CastDuration;
    public int MaxCatch;
    public int[] CatchMultiplier = new int[] { 1, 1, 1 };
    public MeshRenderer Renderer;
    
    internal bool IsCast { get; private set; }

    internal float Progress { get; private set; }

    // Use this for initialization
    void Start()
    {
        
    }

    internal void Cast()
    {
        StartCoroutine(castCoroutine());
    }

    private IEnumerator castCoroutine()
    {
        IsCast = true;
        Progress = 0;
        float progressPerSecond = 1.0f / CastDuration;
        int totalFishCaught = 0;
        List<int> fishCaught = new List<int>();
        
        List<float> density = GameManager.Instance.CurrentCellFishDensity;
        for (int i=0; i<density.Count; i++)
        {
            fishCaught.Add(0);
        }

        while (Progress < 1.0f)
        {
            Progress += progressPerSecond * Time.deltaTime;

            if (totalFishCaught < MaxCatch)
            {
                int fishIndex = Random.Range(0, density.Count);
                
                if (Random.Range(0.0f, 1.0f) < density[fishIndex] * Time.deltaTime * CatchMultiplier[fishIndex])
                {
                    fishCaught[fishIndex]++;
                    totalFishCaught++;
                }
            }

            yield return null;
        }

        IsCast = false;
        GameManager.Instance.AddCatch(fishCaught);
    }
    
    // Update is called once per frame
    void Update()
    {
        Renderer.enabled = IsCast;
    }
}
