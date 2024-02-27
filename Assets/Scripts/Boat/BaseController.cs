using UnityEngine;

namespace BoatAttack
{
    public class BaseController : MonoBehaviour
    {
        protected Boat controller;
        protected Engine engine;

        public virtual void OnEnable()
        {
            if (TryGetComponent(out controller))
                engine = controller.engine;
        }
    }
}