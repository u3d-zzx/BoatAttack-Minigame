using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;
    [HideInInspector] public int boatNum = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetBG()
    {
        return 1.0f;
    }

    public float GetSE()
    {
        return 1.0f;
    }

    public void SetBG(float v)
    {
    }

    public void SetSE(float v)
    {
    }

    public int SetBoatNum()
    {
        return boatNum++;
    }

    public void ResetBoatNum()
    {
        boatNum = 0;
    }
}