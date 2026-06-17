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
    

    public void Setup(Vector2Int pos)
    {
        gridPosition = pos;

        letterText.text = "";
        numberText.text = "";

        letterText.color = Color.black;

        DisableCellColor();
    }

    public void SetLetter(string letter)
    {
       // Debug.Log("SET LETTER : " + letter);

        letterText.text = letter;

      
    }

    public void SetNumber(string number)
    {
        numberText.text = number;
    }

    public void EnableCell()
    {
        GetComponent<Button>().interactable = true;

        bgImage.color = Color.white;
    }

    public void DisableCell()
    {
        GetComponent<Button>().interactable = false;

        DisableCellColor();
    }

    public void DisableCellColor()
    {
        bgImage.color = new Color(0.7f, 0.7f, 0.7f);
    }

    public void SelectCellColor()
    {
       // bgImage.color = Color.yellow;
      
       StopBlink();

       bgImage.color = new Color(1f, 0.8f, 0f);

       blinkTween = transform
           .DOScale(1.15f, 0.4f)
           .SetLoops(-1, LoopType.Yoyo);
       
    }

    public void NormalColor()
    {
        bgImage.color = Color.white;
    }

    public void CorrectColor()
    {
        bgImage.color = Color.green;

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

        // Return to highlighted-word color
        bgImage.color = new Color(1f, 0.92f, 0.6f);
    }
}
