using UnityEngine;
using Zenject;

public class DatabaseInstaller : MonoInstaller
{
    [SerializeField]
    private DatabaseAccessService _instance;

    public override void InstallBindings()
    {
        Container.Bind<DatabaseAccessService>().FromComponentInNewPrefab(_instance).AsSingle().NonLazy();
    }
}
