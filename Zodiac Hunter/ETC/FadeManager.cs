using System;
using System.Collections;
using Code.CoreSystem.GameEvents;
using Code.Events;
using Code.SaveSystem;
using GondrLib.Events;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Work.PSJ.Code.ETC
{
    public class FadeManager : MonoBehaviour
    {
        [SerializeField] private SaveManager saveManager;
        [SerializeField] private Material fadeMat;
        private Coroutine _fadeCoroutine;

        private void Awake()
        {
            Bus<FadeEvent>.onEvent += HandleFadeEvent;
        }

        private void OnDestroy()
        {
            Bus<FadeEvent>.onEvent -= HandleFadeEvent;
        }

        private void HandleFadeEvent(FadeEvent evt)
        {
            float targetAlpha = evt.isFadeIn ? 1f : 0f;
            StartFade(evt.fadeDuration, targetAlpha, evt.endCallback);
        }

        private void Start()
        {
            if (fadeMat != null)
            {
                fadeMat.SetFloat("_FadeAmount", 1f);
                StartFade(1f, 0f);
            }
        }

        private void StartFade(float duration, float target, Action onComplete = null)
        {
            Time.timeScale = 0;
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(FadeRoutine(duration, target, onComplete));
        }

        private IEnumerator FadeRoutine(float duration, float target, Action onComplete)
        {
            float start = fadeMat.GetFloat("_FadeAmount");

            if (Mathf.Approximately(target, 1f))
                saveManager.SaveGameToFile();

            for (float time = 0f; time < duration; time += Time.unscaledDeltaTime)
            {
                fadeMat.SetFloat("_FadeAmount", Mathf.Lerp(start, target, time / duration));
                yield return null;
            }

            fadeMat.SetFloat("_FadeAmount", target);
            onComplete?.Invoke();

            Time.timeScale = 1;
            _fadeCoroutine = null;
        }
    }
}
