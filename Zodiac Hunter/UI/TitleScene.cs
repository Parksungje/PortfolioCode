using System;
using Code.CoreSystem.GameEvents;
using Code.SaveSystem;
using GameLib.SoundSystem;
using GondrLib.Events;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Work.PSJ.Code.ETC;
using Work.JES._01.Scripts.GameSystem;
using ZodiacHunter.UI;

namespace Work.PSJ.Code.UI
{
    public class TitleScene : MonoBehaviour
    {
        [Inject] private SaveManager _saveManager;
        [Inject] private SoundManager _soundManager;
        [SerializeField] private SoundClipSO titleBGM;
        [SerializeField] private GameManagerSO gameManager;

        private void Awake()
        {
            Bus<PlaySoundEvent>.Raise(new PlaySoundEvent(transform.position,titleBGM));
        }

        public void StartBtn()
       {
           _saveManager.DeleteSaveData();
           if (gameManager != null)
           {
               gameManager.currentBossCnt = 0;
               gameManager.currentStage = 0;
               gameManager.ResetRunStateForNewGame();
           }
           SceneManager.LoadScene("MainScene");
       }
        
        public void ExitBtn()   
        {
            Application.Quit();
        }
    }
}
