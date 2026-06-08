using System;
using CSI._01.Script.Enemy;
using UnityEngine;
using UnityEngine.Serialization;

public class CaptainSkeleton : Enemy
{
    [SerializeField] private GameObject _slashPrefab;

    public Vector2 attackDir;
    
    protected override void Awake(){
        base.Awake();
        foreach (EnemyStateType stateType in Enum.GetValues(typeof(EnemyStateType)))
        {
            try
            {
                string enumName = stateType.ToString();
                Type t = Type.GetType($"CaptainSkeleton{enumName}State");
                EnemyState state = Activator.CreateInstance(t, new object[] { this }) as EnemyState;
                state.stateName = enumName;
                StateEnum.Add(stateType, state);
            }
            catch
            {
                print("Error: " + stateType + " state not found");
            }
        }
    }
    
    public override void InitSlash()
    {
        if (_slashPrefab != null && Player != null)
        {
            GameObject slashObj = Instantiate(_slashPrefab, transform.position, Quaternion.identity);
            var slash = slashObj.GetComponent<SJ._01.Code.Enemies.Slash>();
            if (slash != null)
            {
                slash.Launch(attackDir);
            }
        }
    }
}
