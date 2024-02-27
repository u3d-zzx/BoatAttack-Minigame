using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowFps : MonoBehaviour
{
    TextMeshProUGUI textmeshPro;
    const int WINDOW_SIZE = 60;

    float[] msArr = new float[WINDOW_SIZE];

    // Start is called before the first frame update
    void Start()
    {
        textmeshPro = GetComponent<TextMeshProUGUI>();
        for (int i = 0; i < WINDOW_SIZE; ++i)
            msArr[i] = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        msArr[Time.frameCount % WINDOW_SIZE] = Time.deltaTime * 1000.0f;
        // float maxDuration = float.MinValue, minDuration = float.MaxValue;
        float totalTime = 0.0f;
        int validFrameCnt = 0;
        for (int i = 0; i < WINDOW_SIZE; ++i)
        {
            if (msArr[i] < 1.0f)
                continue;

            ++validFrameCnt;
            totalTime += msArr[i];

            //if (msArr[i] > maxDuration)
            //    maxDuration = msArr[i];
            //if (msArr[i] < minDuration)
            //    minDuration = msArr[i];
        }

        //textmeshPro.text = string.Format("ST [{0}, {1}, {2}]",
        //    (int)totalTime / validFrameCnt, (int)minDuration, (int)maxDuration);

        textmeshPro.text = string.Format("FPS: {0}", (float)(1000.0f * validFrameCnt / totalTime));

        //GUI.TextArea(new Rect(100, 100, 130, 50),
        //    string.Format("FPS: {0}", (float)(1000.0f * validFrameCnt / totalTime)));
    }

    //void OnGUI()
    //{
    //    msArr[Time.frameCount % WINDOW_SIZE] = Time.deltaTime * 1000.0f;
    //    // float maxDuration = float.MinValue, minDuration = float.MaxValue;
    //    float totalTime = 0.0f;
    //    int validFrameCnt = 0;
    //    for (int i = 0; i < WINDOW_SIZE; ++i)
    //    {
    //        if (msArr[i] < 1.0f)
    //            continue;

    //        ++validFrameCnt;
    //        totalTime += msArr[i];

    //        //if (msArr[i] > maxDuration)
    //        //    maxDuration = msArr[i];
    //        //if (msArr[i] < minDuration)
    //        //    minDuration = msArr[i];
    //    }

    //    //textmeshPro.text = string.Format("ST [{0}, {1}, {2}]",
    //    //    (int)totalTime / validFrameCnt, (int)minDuration, (int)maxDuration);

    //    textmeshPro.text = string.Format("FPS: {0}", (float)(1000.0f * validFrameCnt / totalTime));

    //    GUI.TextArea(new Rect(100, 100, 130, 50),
    //        string.Format("FPS: {0}", (float)(1000.0f * validFrameCnt / totalTime)));
    //}

}
