using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkPointAnima : MonoBehaviour
{
    private float _timevalue;
    private float _timestep = 0f;
    private Material _material;

    private void Start()
    {
        _material = gameObject.GetComponent<MeshRenderer>().material;
    }

    private void OnEnable()
    {
        _timevalue = 0f;
        _material = gameObject.GetComponent<MeshRenderer>().material;
        StartCoroutine(Shutdown());
    }

    void Update()
    {
        _timevalue = Mathf.Lerp(0, 1f, _timestep);
        _timestep += Time.deltaTime;
        _material.SetFloat("Vector1_BBD96EF2", _timevalue);
    }

    IEnumerator Shutdown()
    {
        yield return new WaitForSeconds(2.5f);
        gameObject.SetActive(false);
    }
}