using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ScrollRectFix : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect _parentScrollRect;

    private void Awake()
    {
        _parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnBeginDrag(PointerEventData eventData)
        => _parentScrollRect?.OnBeginDrag(eventData);

    public void OnDrag(PointerEventData eventData)
        => _parentScrollRect?.OnDrag(eventData);

    public void OnEndDrag(PointerEventData eventData)
        => _parentScrollRect?.OnEndDrag(eventData);
}