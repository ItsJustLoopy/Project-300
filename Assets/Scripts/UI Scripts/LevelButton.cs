using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class LevelButton : MonoBehaviour, IPointerClickHandler
{
    public TMP_Text txtLevelName;
    public Action OnFloorSelected;

    public void OnPointerClick(PointerEventData eventData)
    {
        OnFloorSelected?.Invoke();   
    }
}
