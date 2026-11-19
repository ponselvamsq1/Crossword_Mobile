using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellData : MonoBehaviour
{
    public TextMeshProUGUI letterText;
    public TextMeshProUGUI numberText;
    public Image bgImage;
    public Vector2Int gridPosition;
    private Tween blinkTween;
    public bool isCorrect = false;
    public bool IsSelected { get; private set; }
    public void Setup(Vector2Int pos)
    {
        gridPosition = pos;
        letterText.text = "";
        numberText.text = "";
        letterText.color = CyberTheme.CellText;
        DisableCellColor();
    }

    public void SetLetter(string letter)
    {
        letterText.text = letter;
    }

    public void SetNumber(string number)
    {
        numberText.text = number;
    }

    public void EnableCell()
    {
        GetComponent<Button>().interactable = true;
        bgImage.color = CyberTheme.CellNormal;
    }
    public void DisableCell()
    {
        GetComponent<Button>().interactable = false;
        DisableCellColor();
    }
    public void DisableCellColor()
    {
        bgImage.color = CyberTheme.CellBlocked;
    }

    public void SelectCellColor()
    {
       StopBlink();
       IsSelected = true;
       bgImage.color = CyberTheme.CellSelected;

       blinkTween = transform
           .DOScale(1.15f, 0.4f)
           .SetLoops(-1, LoopType.Yoyo);
       
    }

    public void NormalColor()
    {
        bgImage.color = CyberTheme.CellNormal;
    }
    public void CorrectColor()
    {
        isCorrect = true;
        
        bgImage.color = CyberTheme.CellCorrect;

        letterText.color = Color.white;
    }
    public void ClickCell()
    {
        if (GameManager.instance._Gamelevel == gamelevel.easy)
        {
            CrosswordManager.instance.SelectCell(this);
        }
        else if (GameManager.instance._Gamelevel == gamelevel.medium)
        {
            CrosswordManagerM.instance.SelectCell(this);
        }
        else if (GameManager.instance._Gamelevel == gamelevel.hard)
        {
            CrosswordManagerH.instance.SelectCell(this);
        }
    }
    public void StopBlink()
    {
        if (blinkTween != null)
        {
            blinkTween.Kill();
            blinkTween = null;
        }
        transform.localScale = Vector3.one;
        IsSelected = false;
        bgImage.color = CyberTheme.CellActiveWord;
    }
}
