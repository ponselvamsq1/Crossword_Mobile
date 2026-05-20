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


   public GameObject playagainbtn, pausepanel, tryagain, levelselection, startpage, levelcomplete, finalpanel;

   public GameObject easypanel, mediumpanel, hardpanel;//level1animation,level2animation,level3animation;
   public Button hintbutton1,hintbutton2,hintbutton3;
   
   public CrosswordManager crosswordManager;
   public CrosswordManagerM crosswordManagerM;
   public CrosswordManagerH crosswordManagerH;

   public TextMeshProUGUI level1txtx, level2txtx, level3txtxt;

   public Image s1, s2, s3;
   public Sprite emptystar,filledstar;
   //public GameObject worm1, worm2, worm3,spiderdead;
   //[Header("Animations")]
  // public GameObject pauseThink, levelcompletedance, tryagiansad, finaldance;
   private void Start()
   {

      Tween myTween;
      instance = this;
   }
   
   public GameObject timeuppanel;
   public TextMeshProUGUI timerText,timerTextM,timerTextH; 
    float timeRemaining;
   public bool timerRunning = false;
   
   void Update()
   {
      if (timerRunning && timeRemaining > 0)
      {
         timeRemaining -= Time.deltaTime;
         UpdateTimerText();
      }
      else if (timeRemaining <= 0 && timerRunning)
      {
         timerRunning = false;
         timerText.text = "Time's Up!";
         timerTextM.text = "Time's Up!";
         timerTextH.text = "Time's Up!";

         if (_Gamelevel == gamelevel.easy)
         {
           // worm3.SetActive(false);
            ///easylevelbreathing.SetActive(false);
            //wormdead.SetActive(true);
            
            Invoke("EnableTimeupPanel",5.2f);
         }
         else if (_Gamelevel == gamelevel.medium)
         {
            
            //Vector3 pos = smokepos.position;
           // GameObject obj = Instantiate(smoke,pos,quaternion.identity);
          //  Invoke("DisableBreathAnimation",0.3f);
          //  Invoke("enabledeadcharmedium",0.3f);
            Invoke("EnableTimeupPanel",3.5f);
         }
         else if (_Gamelevel == gamelevel.hard)
         {
            StartCoroutine("enableBug");
         }
         
         

      }
   }

  
    void EnableTimeupPanel()
   {
      timeuppanel.SetActive(true);
      
   }

   IEnumerator enableBug()
   {
     
   //  hardlevelbreathing.SetActive(false);
   //  bugloop.SetActive(false);
   //  bugdead.SetActive(true);
     yield return new WaitForSeconds(4.8f);
      EnableTimeupPanel();
   }

  

   void UpdateTimerText()
   {
      int minutes = Mathf.FloorToInt(timeRemaining / 60);
      int seconds = Mathf.FloorToInt(timeRemaining % 60);
      timerText.text ="Time left "  + string.Format("{0:00}:{1:00}", minutes, seconds) ;
      timerTextM.text ="Time left "  + string.Format("{0:00}:{1:00}", minutes, seconds) ;
      timerTextH.text ="Time left "  + string.Format("{0:00}:{1:00}", minutes, seconds) ;
   }

   public void RetryGame()
   {
      timeuppanel.SetActive(false);
      //tryagiansad.SetActive(false);
      
      levelcomplete.SetActive(false);
      //levelcompletedance.SetActive(false);

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
      
     // pauseThink.SetActive(false);
     // tryagiansad.SetActive(false);
     // finaldance.SetActive(false);
    //  levelcompletedance.SetActive(false);
      
      hintbutton1.interactable = true;
      hintbutton2.interactable = true;
      hintbutton3.interactable = true;

   }

   void Resetwormanimation()
   {
     // easylevelbreathing.SetActive(true);
    //  wormdead.SetActive(false);
      
//      mediumlevelbreathing.SetActive(true);
     // spiderroundanimation.SetActive(true);
      //spiderdead.SetActive(false);
      
    //  hardlevelbreathing.SetActive(true);
    //  bugloop.SetActive(true);
     // bugdead.SetActive(false);
      
      CancelInvoke();
     // DOTween.Kill(worm1.transform);
     // DOTween.Kill(worm2.transform);
     // DOTween.Kill(worm3.transform);
     // DOTween.KillAll();
      
     // worm1.transform.localPosition = new Vector3(10, -8,9);
     // worm2.transform.localPosition = new Vector3(1, -8, 9);
     // worm3.transform.localPosition = new Vector3(-7, -8,9);
      
     // worm1.SetActive(true);
     // worm2.SetActive(false);
     // worm3.SetActive(false);
   }

   public void PlayAgain()
   {
      SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex );
   }

   public void EnableEasyWinpanel()
   {
      levelcomplete.SetActive(true);
      timerRunning = false;
      //levelcompletedance.SetActive(true);
      
      Debug.Log(timeRemaining);
      // star given condition 
      if (_Gamelevel == gamelevel.easy && timeRemaining <= 120)
      {
         Debug.Log(timeRemaining);
         Onestar();
      }
      else if (_Gamelevel == gamelevel.easy && timeRemaining > 120 &&  timeRemaining < 180)
      {
         Debug.Log(timeRemaining);
         Twostar();
      }
      else if (_Gamelevel == gamelevel.easy && timeRemaining >= 180)
      {
         Debug.Log(timeRemaining);
         Threestar();
      }
      
      
      
      
      
      //medium
      if (_Gamelevel == gamelevel.medium && timeRemaining <= 240)
      {
         Debug.Log(timeRemaining);
         Onestar();
      }
      else if (_Gamelevel == gamelevel.medium && timeRemaining > 240 &&  timeRemaining < 360)
      {
         Debug.Log(timeRemaining);
         Twostar();
      }
      else if (_Gamelevel == gamelevel.medium && timeRemaining >= 360)
      {
         Debug.Log(timeRemaining);
         Threestar();
      }
      
      // hard
      if (_Gamelevel == gamelevel.hard && timeRemaining <= 240)
      {
         Debug.Log(timeRemaining);
         Onestar();
      }
      else if (_Gamelevel == gamelevel.hard && timeRemaining > 240 &&  timeRemaining < 360)
      {
         Debug.Log(timeRemaining);
         Twostar();
      }
      else if (_Gamelevel == gamelevel.hard && timeRemaining >= 360)
      {
         Debug.Log(timeRemaining);
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
      //pauseThink.SetActive(true);
   }

   public void Resumegame()
   {
      Time.timeScale = 1;
      timerRunning = true;
      pausepanel.SetActive(false);
      //pauseThink.SetActive(false);
   }

   public void EnableTryagain()
   {
      tryagain.SetActive(true);
      //tryagiansad.SetActive(true);
   }

   public void Levelshow()
   {
      timerRunning = false;
      levelselection.SetActive(true);
      
      startpage.SetActive(false);
      //startpageanimation.SetActive(false);
      
      levelcomplete.SetActive(false);
      //levelcompletedance.SetActive(false);
      
      easypanel.SetActive(false);
      mediumpanel.SetActive(false);
      hardpanel.SetActive(false);
      
      // level1animation.SetActive(false);
      // level2animation.SetActive(false);
      // level3animation.SetActive(false);
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
      
      // level1animation.SetActive(true);
      // level2animation.SetActive(false);
      // level3animation.SetActive(false);
      
      timeRemaining = 300;
      //timeRemaining = 10;
      
     
      // worm1.transform.DOLocalMoveX(1, 100).SetEase(Ease.InOutSine);
       Invoke("wormtwo",100);
      
   }

   void wormtwo()
   {
     // worm1.SetActive(false);
     // worm2.SetActive(true);
     // worm2.transform.DOLocalMoveX(-7, 100).SetEase(Ease.InOutSine);
      Invoke("wormthree",100);
   }
   
   void wormthree()
   {
     // worm1.SetActive(false);
    //  worm2.SetActive(false);
    //  worm3.SetActive(true);
    //  worm3.transform.DOLocalMoveX(-10, 100).SetEase(Ease.InOutSine);
   }

   public void MediumLevel()
   {
     
      hintbutton2.interactable = true;
      
      level2txtx.text = "Level 2";
      _Gamelevel = gamelevel.medium;
      
      easypanel.SetActive(false);
      mediumpanel.SetActive(true);
      hardpanel.SetActive(false);
      
     // level1animation.SetActive(false);
     // level2animation.SetActive(true);
    //  level3animation.SetActive(false);
      
      timeRemaining = 480;
     //timeRemaining = 10;
      //Debug.Log("medium");
      
   }

   public void HardLevel()
   {
      hintbutton3.interactable = true;
      
      level3txtxt.text = "Level 3";
      
      _Gamelevel = gamelevel.hard;
      
      easypanel.SetActive(false);
      mediumpanel.SetActive(false);
      hardpanel.SetActive(true);
      
      //level1animation.SetActive(false);
      //level2animation.SetActive(false);
      //level3animation.SetActive(true);

      timeRemaining = 600;
     // timeRemaining = 10;
      
     // Debug.Log("hard");
      
   }

   public void NextLevel()
   {
      if (_Gamelevel == gamelevel.easy)
      {
         crosswordManagerM.StartMediumlevel();
         levelcomplete.SetActive(false);
         //levelcompletedance.SetActive(false);
      }
      else if (_Gamelevel == gamelevel.medium)
      {
         crosswordManagerH.StartHardlevel();
         levelcomplete.SetActive(false);
         //levelcompletedance.SetActive(false);
      }
      else if(_Gamelevel == gamelevel.hard)
      {
         finalpanel.SetActive(true);
         timerRunning = false;
         //finaldance.SetActive(true);
         //levelcompletedance.SetActive(false);
      }
      
   }

   public void OnClick_Exit()
   {
      //Application.OpenURL("https://beta.skillhub.sq1.security/course/view.php?id=33");
     // Application.OpenURL("http://192.168.8.59/course/view.php?id=33");
     
     Application.Quit();
   }
   
}
   public enum gamelevel
   {
      easy,
      medium,
      hard
   };



