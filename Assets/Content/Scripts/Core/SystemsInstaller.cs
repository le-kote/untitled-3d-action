using UnityEngine;
using Zenject;

public class SystemsInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IEventSystem>().To<GameEventSystem>().AsSingle();
    }
}
