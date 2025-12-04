using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PrimeTween;

public class Card : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    public Sprite hiddenIconSprite;
    public Vector2 hiddenIconSize;  // size for hidden icon

    public Sprite iconSprite;
    public Vector2 iconSize;        // size for icon sprite

    public bool isSelected;

    public CardsController controller;

    private RectTransform iconRect;

    private void Awake()
    {
        iconRect = iconImage.GetComponent<RectTransform>();

        // Set initial hidden icon
        iconImage.sprite = hiddenIconSprite;
        iconRect.sizeDelta = hiddenIconSize;
    }

    public void OnCardClick()
    {
        controller.SetSelected(this);
    }

    public void SetIconSprite(Sprite sp, Vector2 size)
    {
        iconSprite = sp;
        iconSize = size;
    }

    public void Show()
    {
        Tween.Rotation(transform,     // the target
            new Vector3(0f, 180f, 0f), // 180 degrees in y axis
            0.2f);                     // duration

        Tween.Delay(0.1f, () =>
        {
            iconImage.sprite = iconSprite;
            iconRect.sizeDelta = iconSize;
        });

        isSelected = true;
    }

    public void Hide()
    {
        Tween.Rotation(transform,
            new Vector3(0f, 0f, 0f),
            0.2f);

        Tween.Delay(0.1f, () =>
        {
            iconImage.sprite = hiddenIconSprite;
            iconRect.sizeDelta = hiddenIconSize;
            isSelected = false;
        });
    }
}
