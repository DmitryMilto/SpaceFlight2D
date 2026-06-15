using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceFlight2D.Game
{
    public sealed class HoldButton : Button, IPointerDownHandler, IPointerUpHandler
    {
        public bool IsHeld { get; private set; }

        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            IsHeld = true;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            IsHeld = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            IsHeld = false;
        }
    }
}
