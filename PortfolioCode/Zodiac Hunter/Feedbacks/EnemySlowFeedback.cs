using System;
using System.Collections.Generic;
using Code.Effects;
using Code.Feedback;
using ObjectPool.RunTime;
using UnityEngine;
using Work.JES._01.Scripts.Abilitys.SlowSys;

namespace Work.PSJ.Code.Feedbacks
{
    public class EnemySlowFeedback : Feedback
    {
        [SerializeField] protected GameObject _slowEffectPrefab;

        private void Awake()
        {
            _slowEffectPrefab.SetActive(false);
        }

        public void SlowChanage()
        {
        }
        
        public override void CreateFeedback()
        {
           _slowEffectPrefab.SetActive(true);
        }

        public override void StopFeedback()
        {
            _slowEffectPrefab.SetActive(false);
        }
    }
}