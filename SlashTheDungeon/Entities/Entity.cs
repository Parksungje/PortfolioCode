using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool CanChangeState { get; protected set; } = true;
    public bool IsDead { get; protected set; }
    public Animator Anim { get; protected set; }

    protected Dictionary<Type, IEntityComponent> _components = new Dictionary<Type, IEntityComponent>();

    protected virtual void Awake()
    {
        GetComponentsInChildren<IEntityComponent>().ToList().ForEach(compo =>
        {
            _components.Add(compo.GetType(), compo);
        });

        _components.Values.ToList().ForEach(compo => compo.Initialize(this));

        Anim = GetComponentInChildren<Animator>();
    }

    //public virtual void SetDead(bool dead)
    //{
    //    IsDead = dead;
    //    CanChangeState = !dead;

    //    if (!dead) GetCompo<Health>().ResetHealth();
    //}

    public T GetCompo<T>(bool checkInheritance = true) where T : class
    {
        Type targetType = typeof(T);

        if (checkInheritance)
        {
            foreach (Type type in _components.Keys)
            {
                if (targetType.IsAssignableFrom(type))
                {
                    return _components.GetValueOrDefault(type) as T;
                }
            }
        }

        return _components.GetValueOrDefault(targetType) as T;
    }
}
