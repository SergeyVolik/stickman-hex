using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prototype
{
    public class PlayerResourceUIItem : MonoBehaviour
    {
        public TMPro.TextMeshProUGUI itemText;
        public Image spriteImage;
        public void SetValue(int value)
        {
            itemText.text = value.ToString();
        }
        public void SetSprite(Sprite sprite, Color color)
        {
            spriteImage.color = color;
            spriteImage.sprite = sprite;
        }
    }
}
