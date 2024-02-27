using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace BoatAttack.UI
{
    public class RaceStatsPlayer : MonoBehaviour
    {
        public TextMeshProUGUI place;
        public TextMeshProUGUI playerName;
        public TextMeshProUGUI boatType;
        public TextMeshProUGUI bestLap;
        public TextMeshProUGUI time;
        public Boat boat;
        public int placeNum = -1;
        private bool _update = true;

        public void Setup(BoatData _boatData)
        {
            boat = _boatData.boat;
            playerName.text = boat.name;
            boatType.text = _boatData.boatType;
        }

        private void Update()
        {
            if (!_update) return;
            UpdateStats();
        }

        private void LateUpdate()
        {
            if (boat)
            {
                _update = !boat.matchComplete;
            }
        }

        public void UpdateStats()
        {
            if (boat)
            {
                placeNum = boat.place;
                place.text = RaceUI.OrdinalNumber(boat.place);

                var bestLapTime = RaceUI.BestLapFromSplitTimes(boat.splitTimes);
                bestLap.text = bestLapTime > 0 ? RaceUI.FormatRaceTime(bestLapTime) : "N/A";

                var totalTime = boat.matchComplete ? boat.splitTimes.Last() : RaceManager.raceTime;
                time.text = RaceUI.FormatRaceTime(totalTime);
            }
        }
    }
}