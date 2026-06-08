using System;
using CSI._01.Script.Enemy;
using UnityEngine;
using UnityEngine.Serialization;

public class FireBird : Enemy
{
    [SerializeField] private GameObject _fireBallPrefab;

    public Vector2 attackDir;
    protected override void Awake(){
        base.Awake();
        foreach (EnemyStateType stateType in Enum.GetValues(typeof(EnemyStateType)))
        {
            try
            {
                string enumName = stateType.ToString();
                Type t = Type.GetType($"FireBird{enumName}State");
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
        if (_fireBallPrefab != null && Player != null)
        {
            Vector2 baseDir;

            if (Mathf.Abs(attackDir.x) > Mathf.Abs(attackDir.y) )
                baseDir = attackDir.x > 0 ? Vector2.right : Vector2.left;    
            else
                baseDir = attackDir.y > 0 ? Vector2.up : Vector2.down;

            float[] angles = { -15f, 0f, 15f };

            foreach (float angle in angles)
            {
                Quaternion rot = Quaternion.AngleAxis(angle, Vector3.forward);
                Vector2 rotatedDir = rot * baseDir;
                GameObject fireballObj = Instantiate(_fireBallPrefab, transform.position, Quaternion.identity);
                var fireball = fireballObj.GetComponent<SJ._01.Code.Enemies.FireBall>();
                if (fireball != null)
                {
                    fireball.Launch(rotatedDir);
                }
            }
        }
    }
}