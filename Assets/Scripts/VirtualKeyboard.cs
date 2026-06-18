using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VirtualKeyboard : MonoBehaviour
{
    public static VirtualKeyboard instance;

    private void Awake()
    {
        instance = this;
    }

    public void PressLetter(string letter)
    {
        if (GameManager.instance._Gamelevel == gamelevel.easy)
        {
            CrosswordManager.instance.EnterLetter(letter);
        }
        else if (GameManager.instance._Gamelevel == gamelevel.medium)
        {
            CrosswordManagerM.instance.EnterLetter(letter);
        }
        else if (GameManager.instance._Gamelevel == gamelevel.hard)
        {
            CrosswordManagerH.instance.EnterLetter(letter);
        }
    }

    public void Backspace()
    {
        if (GameManager.instance._Gamelevel == gamelevel.easy)
        {
            CrosswordManager.instance.DeleteLetter();
        }
        else if (GameManager.instance._Gamelevel == gamelevel.medium)
        {
            CrosswordManagerM.instance.DeleteLetter();
        }
        else if (GameManager.instance._Gamelevel == gamelevel.hard)
        {
            CrosswordManagerH.instance.DeleteLetter();
        }
    }
}
