using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BoatAttack.UI
{
    public class MainMenuHelper : MonoBehaviour
    {
        [Header("Level Selection")] public EnumSelector levelSelector;
        public EnumSelector lapSelector;
        public EnumSelector reverseSelector;

        [Header("Boat Selection")] public GameObject[] boatMeshes;
        public TextMeshProUGUI boatName;
        public EnumSelector boatHullSelector;
        public ColorSelector boatPrimaryColorSelector;
        public ColorSelector boatTrimColorSelector;
        public bool isfirst;
        private Color _priColor;
        private Color _trimClolor;
        private const string _tryLevel = "trylevel_v01";

        private void OnEnable()
        {
            // level stuff
            levelSelector.updateVal += SetLevel;
            lapSelector.updateVal += SetLaps;
            reverseSelector.updateVal += SetReverse;
            // boat stuff
            boatHullSelector.updateVal += UpdateBoat;
            boatPrimaryColorSelector.updateVal += UpdatePrimaryColor;
            boatTrimColorSelector.updateVal += UpdateTrimColor;
            ABManager.manifestLoaded += CreatFirstBoat;
        }

        private void OnDisable()
        {
            ABManager.manifestLoaded -= CreatFirstBoat;
        }

        public void CreatFirstBoat()
        {
            StartCoroutine(CreateandShowBoat(0));
        }

        private void SetupDefaults()
        {
            // level stuff
            SetLevel(levelSelector.CurrentOption);
            SetLaps(lapSelector.CurrentOption);
            SetReverse(reverseSelector.CurrentOption);
            // boat stuff
            SetSinglePlayerName(boatName.text);
        }

        private void UpdateBoat(int index)
        {
            StartCoroutine(CreateandShowBoat(index));
        }

        IEnumerator CreateandShowBoat(int index)
        {
            isfirst = true;
            RaceManager.playerBoatType = index;
            if (boatMeshes[index] == null)
            {
                Debug.Log("BoatMeshNull");
                yield return ABFactory.instance.CreateFromABAsync(ABManager.instance.GetBoatABNameFromIndex(index),
                    ABManager.instance.GetBoatPrefabNameFromIndex(index, false),
                    (boatfromAB) => { boatMeshes[index] = boatfromAB; });
                boatMeshes[index]?.transform.SetParent(gameObject.transform.Find("BoatRoom"), false);
            }

            if (index == 0 || index == 1)
            {
                SetColor(_priColor, _trimClolor, true);
                SetColor(_priColor, _trimClolor, false);
            }

            for (var i = 0; i < boatMeshes.Length; i++)
            {
                if (boatMeshes[i] != null)
                {
                    boatMeshes[i].SetActive(i == index);
                    boatMeshes[i].transform.SetParent(gameObject.transform.Find("BoatRoom"), false);
                }
                else
                    continue;
            }
        }

        public void SetupSingleplayerGame()
        {
            RaceManager.SetGameType(RaceManager.GameType.Singleplayer);
            UpdateBoatColor(boatPrimaryColorSelector.CurrentOption, true);
            UpdateBoatColor(boatTrimColorSelector.CurrentOption, false);
            _priColor = RaceManager.raceData.boats[0].livery.primaryColor;
            _trimClolor = RaceManager.raceData.boats[0].livery.trimColor;
            if (!isfirst)
                RaceManager.playerBoatType = 0;
            SetupDefaults();
        }

        public void SetupSpectatorGame()
        {
            RaceManager.SetGameType(RaceManager.GameType.Spectator);
            SetupDefaults();
        }

        private static void SetLevel(int index)
        {
            RaceManager.SetLevel(index);
        }

        private static void SetLaps(int index)
        {
            RaceManager.raceData.laps = ConstantData.laps[index];
        }

        private static void SetReverse(int reverse)
        {
            RaceManager.raceData.reversed = (reverse == 1);
        }

        public void StartRace()
        {
            RaceManager.FixSetPlayerBoat();
            RaceManager.FixPlayerBoatType();
            RaceManager.FixSetAIBoat();
            RaceManager.LoadGame();
        }

        public void SetSinglePlayerName(string playerName) => RaceManager.raceData.boats[0].boatName = playerName;

        private void UpdatePrimaryColor(int index) => UpdateBoatColor(index, true);

        private void UpdateTrimColor(int index) => UpdateBoatColor(index, false);


        public void SetColor(Color PriColor, Color TrimColor, bool primary)
        {
            foreach (var t in boatMeshes)
            {
                if (t == null)
                    continue;
                else
                {
                    var renderers = t.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var rend in renderers)
                    {
                        rend.material.SetColor(primary ? "_Color1" : "_Color2", primary ? PriColor : TrimColor);
                    }
                }
            }
        }

        private void UpdateBoatColor(int index, bool primary)
        {
            // update racedata
            if (primary)
            {
                RaceManager.raceData.boats[0].livery.primaryColor = ConstantData.GetRandomPaletteColor;
            }
            else
            {
                RaceManager.raceData.boats[0].livery.trimColor = ConstantData.GetRandomPaletteColor;
            }

            // update menu boats
            foreach (var t in boatMeshes)
            {
                if (t == null)
                    continue;
                else
                {
                    var renderers = t.GetComponentsInChildren<MeshRenderer>(true);
                    foreach (var rend in renderers)
                    {
                        rend.material.SetColor(primary ? "_Color1" : "_Color2",
                            primary
                                ? RaceManager.raceData.boats[0].livery.primaryColor
                                : RaceManager.raceData.boats[0].livery.trimColor);
                    }
                }
            }
        }

        public static void SetTryLevel(int currentOption)
        {
            int trylevel = PlayerPrefs.HasKey(_tryLevel) ? PlayerPrefs.GetInt(_tryLevel) : 1;
            trylevel = trylevel | (1 << currentOption);
            PlayerPrefs.SetInt(_tryLevel, trylevel);
        }

        public static bool CheckTryLevel(int currentOption)
        {
            int trylevel = PlayerPrefs.HasKey(_tryLevel) ? PlayerPrefs.GetInt(_tryLevel) : 1;
            int optionflag = 1 << currentOption;
            if (0 != (trylevel & optionflag))
                return true;
            return false;
        }
    }
}