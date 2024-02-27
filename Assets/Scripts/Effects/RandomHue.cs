﻿using UnityEngine;

namespace BoatAttack
{
    /// <summary>
    /// Simple scripts sets a random HUE on a shader with the property '_Hue'
    /// </summary>
    public class RandomHue : MonoBehaviour
    {
        public MeshRenderer[] renderers;
        private static readonly int _hue = Shader.PropertyToID("_Hue");

        private void OnEnable()
        {
            RandomizeHue();
        }

        void RandomizeHue()
        {
            var hue = Random.Range(0f, 1f);

            if (renderers == null || renderers.Length <= 0) return;

            foreach (var t in renderers)
            {
                if (t == null) continue;

                var mat = new Material(t.sharedMaterial);
                mat.SetFloat(_hue, hue);
                t.material = mat;
            }
        }
    }
}