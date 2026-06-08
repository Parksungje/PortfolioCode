using Code.Events;
using GondrLib.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Work.PSJ.Code.UI
{
    public class PlayerDeadCanvas : MonoBehaviour
    {
        [SerializeField] private GameObject DeathCanvas;
        private void OnEnable()
        {
            Bus<PlayerDeathEvent>.onEvent += HandleDeathEvent;
        }

        private void HandleDeathEvent(PlayerDeathEvent evt)
        {
            DeathCanvas.SetActive(true);
            Time.timeScale = 0f;
        }

        private void OnDisable()
        {
            Bus<PlayerDeathEvent>.onEvent -= HandleDeathEvent;
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        public void RestartBtn()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainScene");
        }
        
        public void TitleScneeBtn()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("TitleScene");
        }
    }
}