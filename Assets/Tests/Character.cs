using System.Collections;
using System.Collections.Generic;
using LDtkVania;
using UnityEngine;

public class Character : MonoBehaviour, MV_ILevelSpawnSubject
{
    [SerializeField] private GameObjectProvider _characterProvider;

    private void Awake()
    {
        _characterProvider.Register(gameObject);
    }

    private void OnDestroy()
    {
        _characterProvider.Unregister();
    }

    public void Spawn(Vector2 position, int directionSign)
    {
        transform.position = position;
    }
}
