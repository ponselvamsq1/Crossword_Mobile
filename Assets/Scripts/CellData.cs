using System.Collections;
using System.Collections.Generic;
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
        Debug.Log("SET LETTER : " + letter);

        letterText.text = letter;

        //letterText.color = Color.red;
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
        bgImage.color = Color.yellow;
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
}
