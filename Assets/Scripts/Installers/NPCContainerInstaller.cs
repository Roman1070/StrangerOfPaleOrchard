using UnityEngine;
using Zenject;

public class NPCContainerInstaller : MonoInstaller
{
    [SerializeField]
    private NPCContainer _instance;
    public override void InstallBindings()
    {
        Container.Bind<NPCContainer>().FromInstance(_instance).AsSingle().NonLazy();
    }
}