using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using Unity.Mathematics;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
   public static GameManager instance;
   public gamelevel _Gamelevel;
   public GameObject playagainbtn, pausepanel, tryagain, levelselection, startpage, levelcomplete, finalpanel,quitpanel;
   public GameObject easypanel, mediumpanel, hardpanel;
   public Button hintbutton1,hintbutton2,hintbutton3;
   public CrosswordManager crosswordManager;
   public CrosswordManagerM crosswordManagerM;
   public CrosswordManagerH crosswordManagerH;
   public TextMeshProUGUI level1txtx, level2txtx, level3txtxt;
   public Image s1, s2, s3;
   public Sprite emptystar,filledstar;
   private float saveTimer;
   
   private void Start()
   {
      Tween myTween;
      instance = this;

      if (PlayerPrefs.HasKey("CurrentLevel"))
      {
         int savedLevel =
            PlayerPrefs.GetInt("CurrentLevel");

         _Gamelevel = (gamelevel)savedLevel;

         if (_Gamelevel == gamelevel.easy)
            crosswordManager.StartEasylevel();

         else if (_Gamelevel == gamelevel.medium)
            crosswordManagerM.StartMediumlevel();

         else if (_Gamelevel == gamelevel.hard)
            crosswordManagerH.StartHardlevel();
      }
   }
   public GameObject timeuppanel;
   public TextMeshProUGUI timerText,timerTextM,timerTextH; 
    float timeRemaining;
   public bool timerRunning = false;
   public TextMeshProUGUI tryAgainText;
   void Update()
   {
      if (timerRunning && timeRemaining > 0)
      {
         timeRemaining -= Time.deltaTime;
         UpdateTimerText();
         saveTimer += Time.deltaTime;
         if (saveTimer >= 2f)
         {
            SaveGameData();
            saveTimer = 0;
         }
      }
      else if (timeRemaining <= 0 && timerRunning)
      {
         timerRunning = false;
         timerText.text = "Time's Up!";
         timerTextM.text = "Time's Up!";
         timerTextH.text = "Time's Up!";

         if (_Gamelevel == gamelevel.easy)
         {
            EnableTimeupPanel();
         }
         else if (_Gamelevel == gamelevel.medium)
         {
            EnableTimeupPanel();
         }
         else if (_Gamelevel == gamelevel.hard)
         {
            EnableTimeupPanel();
         }
         
      }
   }
   
   
    void EnableTimeupPanel()
   {
      timeuppanel.SetActive(true);
      
   }
    
   public void SaveGameData()
   {
      PlayerPrefs.SetInt("CurrentLevel", (int)_Gamelevel);

      if (_Gamelevel == gamelevel.easy) PlayerPrefs.SetFloat("EasyTime", timeRemaining);

      else if (_Gamelevel == gamelevel.medium) PlayerPrefs.SetFloat("MediumTime", timeRemaining);

      else if (_Gamelevel == gamelevel.hard) PlayerPrefs.SetFloat("HardTime", timeRemaining);

      PlayerPrefs.Save();
   }
    
    
   void UpdateTimerText()
   {
      int minutes = Mathf.FloorToInt(timeRemaining / 60);
      int seconds = Mathf.FloorToInt(timeRemaining % 60);
      timerText.text = "Time Left:" +string.Format("{0:00}:{1:00}", minutes, seconds) ;
      timerTextM.text = "Time Left:" +string.Format("{0:00}:{1:00}", minutes, seconds) ;
      timerTextH.text = "Time Left:" +string.Format("{0:00}:{1:00}", minutes, seconds) ;
   }

   public void RetryGame()
   {
      timeuppanel.SetActive(false);
      
      levelcomplete.SetActive(false);
      Resetwormanimation();
      
      TimerLoad();
      if (_Gamelevel == gamelevel.easy)
      {
         crosswordManager.ClearGrid();
         crosswordManager.StartEasylevel();
      }
      else if (_Gamelevel == gamelevel.medium)
      {
         crosswordManagerM.ClearGrid();
         crosswordManagerM.StartMediumlevel();
      }
      else if (_Gamelevel == gamelevel.hard)
      {
         crosswordManagerH.ClearGrid();
         crosswordManagerH.StartHardlevel();
      }
      tryagain.SetActive(false);
      hintbutton1.interactable = true;
      hintbutton2.interactable = true;
      hintbutton3.interactable = true;
   }
   void Resetwormanimation()
   {
      CancelInvoke();
   }
   public void PlayAgain()
   {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex );
   }

   public void EnableEasyWinpanel()
   {
      levelcomplete.SetActive(true);
      timerRunning = false;
      
      if (_Gamelevel == gamelevel.easy && timeRemaining <= 120)
      {
         
         Onestar();
      }
      else if (_Gamelevel == gamelevel.easy && timeRemaining > 120 &&  timeRemaining < 180)
      {
         Twostar();
      }
      else if (_Gamelevel == gamelevel.easy && timeRemaining >= 180)
      {
         Threestar();
      }
      
      
      //medium
      if (_Gamelevel == gamelevel.medium && timeRemaining <= 240)
      {
         Onestar();
      }
      else if (_Gamelevel == gamelevel.medium && timeRemaining > 240 &&  timeRemaining < 360)
      {
         Twostar();
      }
      else if (_Gamelevel == gamelevel.medium && timeRemaining >= 360)
      {
         Threestar();
      }
      // hard
      if (_Gamelevel == gamelevel.hard && timeRemaining <= 240)
      {
         Onestar();
      }
      else if (_Gamelevel == gamelevel.hard && timeRemaining > 240 &&  timeRemaining < 360)
      {
         Twostar();
      }
      else if (_Gamelevel == gamelevel.hard && timeRemaining >= 360)
      {
         Threestar();
      }
      TimerLoad();
   }

   void Onestar()
   {
      s1.sprite = filledstar;
      s2.sprite = emptystar;
      s3.sprite = emptystar;
      
      s1.gameObject.GetComponent<Animator>().SetTrigger("starzoom");
     
   }

   void Twostar()
   {
      s1.sprite = filledstar;
      s2.sprite = filledstar;
      s3.sprite = emptystar;
      
      s1.gameObject.GetComponent<Animator>().SetTrigger("starzoom");
      s2.gameObject.GetComponent<Animator>().SetTrigger("starzoom");
      
   }

   void Threestar()
   {
      s1.sprite = filledstar;
      s2.sprite = filledstar;
      s3.sprite = filledstar;
      
      s1.gameObject.GetComponent<Animator>().SetTrigger("starzoom");
      s2.gameObject.GetComponent<Animator>().SetTrigger("starzoom");
      s3.gameObject.GetComponent<Animator>().SetTrigger("starzoom");
   }

   void TimerLoad()
   {
      if (_Gamelevel == gamelevel.easy)
      {
         timeRemaining = 300;
      }
      else if(_Gamelevel == gamelevel.medium)
      {
         timeRemaining = 480;
      }
      else if(_Gamelevel==gamelevel.hard)
      {
         timeRemaining = 600;
      }
   }

   public void Pausegame()
   {
      Time.timeScale = 0;
      timerRunning = false;
      pausepanel.SetActive(true);
     
   }

   public void Resumegame()
   {
      Time.timeScale = 1;
      timerRunning = true;
      pausepanel.SetActive(false);
      
   }

   public void EnableTryagain()
   {
      tryagain.SetActive(true);
      
   }

   public void Levelshow()
   {
      timerRunning = false;
      levelselection.SetActive(true);
      startpage.SetActive(false);
      levelcomplete.SetActive(false);
      easypanel.SetActive(false);
      mediumpanel.SetActive(false);
      hardpanel.SetActive(false);
      
   }

   public void Homepage()
   {
      SceneManager.LoadScene(0);
   }
   

   ///game level
   public void EasyLevel()
   {
      Resetwormanimation();
      hintbutton1.interactable = true;
      level1txtx.text = "Level 1";
      
      _Gamelevel = gamelevel.easy;
      
      easypanel.SetActive(true);
      mediumpanel.SetActive(false);
      hardpanel.SetActive(false);
      
      timeRemaining = PlayerPrefs.GetFloat("EasyTime", 300);
      
    
      
   }
   public void MediumLevel()
   {
     
       hintbutton2.interactable = true;
      
       level2txtx.text = "Level 2";
       _Gamelevel = gamelevel.medium;
      
       easypanel.SetActive(false);
       mediumpanel.SetActive(true);
       hardpanel.SetActive(false);
      
       timeRemaining = PlayerPrefs.GetFloat("MediumTime", 480);
      
     
   }
   public void HardLevel()
   {
      hintbutton3.interactable = true;
      level3txtxt.text = "Level 3";
      _Gamelevel = gamelevel.hard;
      easypanel.SetActive(false);
      mediumpanel.SetActive(false);
      hardpanel.SetActive(true);
      timeRemaining = PlayerPrefs.GetFloat("HardTime", 600);
     
   }

   public void NextLevel()
   {
      if (_Gamelevel == gamelevel.easy)
      {
         crosswordManagerM.StartMediumlevel();
         levelcomplete.SetActive(false);
         
      }
      else if (_Gamelevel == gamelevel.medium)
      {
         crosswordManagerH.StartHardlevel();
         levelcomplete.SetActive(false);
         
      }
      else if(_Gamelevel == gamelevel.hard)
      {
         finalpanel.SetActive(true);
         timerRunning = false;
      }
   }
   public void OnClick_Exit()
   {
      Time.timeScale = 0;
     quitpanel.SetActive(true);
   }

   public void OnClick_ExitConfirm()
   {
      Time.timeScale = 1;
      Application.Quit();
   }
   public void OnCLick_NoExit()
   {
      Time.timeScale = 1;
      quitpanel.SetActive(false);
   }
   public void OnClick_ClearWord()
   {
      if (_Gamelevel == gamelevel.easy)
      {
         crosswordManager.ClearAllWords();
      }
      else if (_Gamelevel == gamelevel.medium)
      {
         crosswordManagerM.ClearAllWords();
      }
      else if (_Gamelevel == gamelevel.hard)
      {
         crosswordManagerH.ClearAllWords();
      }
   }
   
   private void OnApplicationPause(bool pause)
   {
      if (pause)
         SaveGameData();
   }

   private void OnApplicationQuit()
   {
      SaveGameData();
   }
}
   public enum gamelevel
   {
      easy,
      medium,
      hard
   };



