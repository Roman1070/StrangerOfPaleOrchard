using UnityEngine;
using Zenject;

public class CoroutineExecutorInstaller : MonoInstaller<CoroutineExecutorInstaller>
{
    [SerializeField]
    private CoroutineExecutor _coroutineExecutor;

    public override void InstallBindings()
    {
        Container.Bind<CoroutineExecutor>().FromComponentInNewPrefab(_coroutineExecutor).AsSingle();
    }
}