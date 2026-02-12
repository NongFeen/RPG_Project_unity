using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragInventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI")]
    public Image image;
    [SerializeField] private InputReader inputReader;
    private Transform parentAfterDrag;
    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = inputReader.AimPosition;
        // transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }

    public Transform GetParentAfterDrag()
    {
        return this.parentAfterDrag;
    }
    public void SetParentAfterDrag(Transform newParent)
    {
        this.parentAfterDrag = newParent;
    }
}
