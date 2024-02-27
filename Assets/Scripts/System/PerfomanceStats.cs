using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class PerfomanceStats : MonoBehaviour
{
    // Frame time stats
    private List<float> _samples = new List<float>();

    private int _totalSamples = 250;

    // UI display
    public Text frametimeDisplay;

    void Update()
    {
        frametimeDisplay.text = "";
        // sample frametime
        // add sample at the start
        _samples.Insert(0, Time.deltaTime);
        if (_samples.Count >= _totalSamples - 1)
        {
            _samples.RemoveAt(_totalSamples);
        }

        UpdateFrametime();

        long totalMem = Profiler.GetTotalAllocatedMemoryLong();
        frametimeDisplay.text += string.Format("Total Memory:{0}Mbs\n", ((float)totalMem / 1000000).ToString("##.00"));
        long gpuMem = Profiler.GetAllocatedMemoryForGraphicsDriver();
        frametimeDisplay.text += string.Format("GPU Memory:{0}Mbs\n", ((float)gpuMem / 1000000).ToString("##.00"));
    }

    void UpdateFrametime()
    {
        float avgFrametime = 0f;
        float sampleDivision = 1f / _samples.Count;
        for (var i = 0; i < _samples.Count; i++)
        {
            avgFrametime += _samples[i] * sampleDivision;
        }

        frametimeDisplay.text += string.Format("Total time:{0}ms fps:{1}\n", (avgFrametime * 1000).ToString("00.00"),
            (1f / avgFrametime).ToString("###.00"));
    }
}