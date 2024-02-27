using UnityEngine;
using TMPro;
using BoatAttack.UI;

public class CountDown : MonoBehaviour
{
    private TMP_Text _text;
    private float _countDown;
    private RaceUI _raceUI;

    void Start()
    {
        _raceUI = gameObject.GetComponentInParent<RaceUI>();
        _text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        _countDown = 3f;
    }

    void Update()
    {
        if (_countDown > 0)
        {
            _countDown -= Time.deltaTime;
            _text.text = Mathf.CeilToInt(_countDown).ToString();
            if (_countDown < 0)
            {
                BoatAttack.HumanController.canTurn = true;
                _raceUI.engineToggle.isOn = true;
                gameObject.SetActive(false);
            }
        }
    }
}