using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MiniInteraction
{
    public class FreeDrag : MonoBehaviour, IDragHandler
    {
        private RectTransform _dragRectTrans;

        [SerializeField] private bool _moveTop;
        
        private void Awake()
        {
            _dragRectTrans = GetComponent<RectTransform>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_moveTop)
            {
                transform.SetSiblingIndex(transform.parent.childCount - 1);
            }
            Camera eventCamera = eventData.pressEventCamera;
            Vector3 originPosition = _dragRectTrans.position;
            Vector2 position = eventCamera.WorldToScreenPoint(originPosition);
            float preserveZ = originPosition.z;
            position += eventData.delta;
            Vector3 newPosition = eventCamera.ScreenToWorldPoint(position);
            newPosition.z = preserveZ;
            _dragRectTrans.position = newPosition;
        }
    }
}
