using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

namespace BoatAttack.UI
{
    public class PlayerMarker : MonoBehaviour
    {
        public TextMeshProUGUI placeText;
        public TextMeshProUGUI nameText;

        private RectTransform _rect;
        private BoatData _boatData;
        private Boat _boat;
        private int _curPlace = -1;

        private void OnEnable()
        {
            RenderPipelineManager.beginFrameRendering += UpdatePosition;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginFrameRendering -= UpdatePosition;
        }

        public void Setup(BoatData boat)
        {
            _boatData = boat;
            _boat = boat.boat;
            nameText.text = boat.boatName;
            _rect = transform as RectTransform;
        }

        private void LateUpdate()
        {
            UpdatePlace();
        }

        private void UpdatePlace()
        {
            if (!_boat || _curPlace == _boat.place) return;
            _curPlace = _boat.place;
            placeText.text = _curPlace.ToString();
        }

        private void UpdatePosition(ScriptableRenderContext context, Camera[] cameras)
        {
            // if no boat or camera, the player marker cannot work
            if (_boatData == null || Camera.main == null) return;

            var screenPos = Camera.main.WorldToViewportPoint(_boatData.boatObject.transform.position + Vector3.up * 3f);
            if (screenPos.z < 0)
            {
                screenPos = -Vector3.one;
            }

            _rect.anchorMin = _rect.anchorMax = screenPos;
            _rect.anchoredPosition = Vector2.zero;
        }
    }
}