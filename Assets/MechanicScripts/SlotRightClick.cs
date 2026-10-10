using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class SlotRightClick : MonoBehaviour, IPointerClickHandler
{
    public Action onLeftClick;
    public Action onRightClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            onLeftClick?.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            onRightClick?.Invoke();
        }
    }
}
