using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace BoatAttack.UI
{
    public class PlayerMapMarker : MonoBehaviour
    {
        public Image primary;
        public Image secondary;

        private RectTransform _rect;
        private BoatData _boatData;
        private Boat _boat;
        private Transform _boatTransform;
        private float _scale;
        private int _playerCount;

        private float _reScale;
        private GameObject _mapSizePointU;
        private GameObject _mapSizePointB;
        private GameObject _mapSizePointL;
        private GameObject _mapSizePointR;
        private float _centerPointZ;
        private float _centerPointX;

        private void OnEnable()
        {
            RenderPipelineManager.beginFrameRendering += UpdatePosition;

            _mapSizePointU = GameObject.Find("MapSizePointU");
            _mapSizePointB = GameObject.Find("MapSizePointB");
            _mapSizePointL = GameObject.Find("MapSizePointL");
            _mapSizePointR = GameObject.Find("MapSizePointR");
            _centerPointZ = (_mapSizePointU.transform.position.z + _mapSizePointB.transform.position.z) / 2;
            _centerPointX = (_mapSizePointL.transform.position.x + _mapSizePointR.transform.position.x) / 2;

            Scene scene = SceneManager.GetActiveScene();
            if (scene.buildIndex == 3 || scene.buildIndex == 6)
            {
                _reScale = 0.00125f;
            }
            else
            {
                _reScale = 0.0028f;
            }
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginFrameRendering -= UpdatePosition;
        }

        public void Setup(BoatData boat, float scale = 0.0028f)
        {
            _boatData = boat;
            _boat = boat.boat;
            _boatTransform = boat.boat.transform;
            _rect = transform as RectTransform;
            _scale = scale;

            var p = _boatData.livery.primaryColor;
            p.a = 1f;
            primary.color = p;
            var t = _boatData.livery.trimColor;
            t.a = 1f;
            secondary.color = t;

            _playerCount = RaceManager.raceData.boatCount;
        }

        private void UpdatePosition(ScriptableRenderContext context, Camera[] cameras)
        {
            // if no boat or camera, the player marker cannot work
            if (_boatData == null || Camera.main == null) return;

            var position = _boatTransform.position;
            _rect.anchorMin = _rect.anchorMax = Vector2.one * 0.5f +
                                                new Vector2(position.x - _centerPointX, position.z - _centerPointZ) *
                                                _reScale;
            _rect.SetSiblingIndex(_playerCount - _boat.place + 1);
        }
    }
}