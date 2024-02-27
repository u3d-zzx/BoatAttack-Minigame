using BoatAttack;
using BoatAttack.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceCtrl : MonoBehaviour
{
    private RaceUI _raceUI;
    public bool engineon;
    private HumanController _humanController;

    private void Awake()
    {
        _raceUI = gameObject.GetComponent<RaceUI>();
        _humanController = GameObject.FindObjectOfType<HumanController>();
    }

    private void OnEnable()
    {
        if (!PlayerPrefs.HasKey(ConstantData.ctrlToggle))
        {
            PlayerPrefs.SetInt(ConstantData.ctrlToggle, 1);
            _raceUI.ToggleFirstSet();
        }
        else
        {
            _raceUI.ToggleReset();
        }

        _raceUI.gasIndexImage.fillAmount = PlayerPrefs.GetFloat("InitGas");
        _raceUI.SetGaspercent(_raceUI.gasIndexImage.fillAmount);
        _raceUI.ToggleChange();
        Scene scene = SceneManager.GetActiveScene();
        _raceUI.mapIB = transform.Find("Gameplay/Map").GetComponent<Image>();
        _raceUI.mapIU = transform.Find("Gameplay/Map/Map").GetComponent<Image>();
        if (scene.buildIndex == 2)
        {
            Sprite spriteB = Sprite.Create(_raceUI.mapB2, new Rect(0, 0, _raceUI.mapB2.width, _raceUI.mapB2.height),
                new Vector2(0.5f, 0.5f));
            _raceUI.mapIB.overrideSprite = spriteB;
            Sprite spriteU = Sprite.Create(_raceUI.mapU2, new Rect(0, 0, _raceUI.mapU2.width, _raceUI.mapU2.height),
                new Vector2(0.5f, 0.5f));
            _raceUI.mapIU.overrideSprite = spriteU;
        }
        else if (scene.buildIndex == 3)
        {
            Sprite spriteB = Sprite.Create(_raceUI.mapB3, new Rect(0, 0, _raceUI.mapB2.width, _raceUI.mapB2.height),
                new Vector2(0.5f, 0.5f));
            _raceUI.mapIB.overrideSprite = spriteB;
            Sprite spriteU = Sprite.Create(_raceUI.mapU3, new Rect(0, 0, _raceUI.mapU2.width, _raceUI.mapU2.height),
                new Vector2(0.5f, 0.5f));
            _raceUI.mapIU.overrideSprite = spriteU;
        }
        else if (scene.buildIndex == 4)
        {
            Sprite spriteB = Sprite.Create(_raceUI.mapB4, new Rect(0, 0, _raceUI.mapB2.width, _raceUI.mapB2.height),
                new Vector2(0.5f, 0.5f));
            _raceUI.mapIB.overrideSprite = spriteB;
            Sprite spriteU = Sprite.Create(_raceUI.mapU4, new Rect(0, 0, _raceUI.mapU2.width, _raceUI.mapU2.height),
                new Vector2(0.5f, 0.5f));
            _raceUI.mapIU.overrideSprite = spriteU;
        }
        else if (scene.buildIndex == 5)
        {
            Sprite spriteB = Sprite.Create(_raceUI.mapB5, new Rect(0, 0, _raceUI.mapB2.width, _raceUI.mapB2.height),
                new Vector2(0.5f, 0.5f));
            _raceUI.mapIB.overrideSprite = spriteB;
            Sprite spriteU = Sprite.Create(_raceUI.mapU5, new Rect(0, 0, _raceUI.mapU2.width, _raceUI.mapU2.height),
                new Vector2(0.5f, 0.5f));
            _raceUI.mapIU.overrideSprite = spriteU;
        }

        _raceUI.addButton.onClick.AddListener(_raceUI.AddinRace);
        Invoke("ToggleChange", 1);
    }

    public void ToggleChange()
    {
        _raceUI.ToggleChange();
    }
}