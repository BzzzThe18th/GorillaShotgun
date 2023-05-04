using ComputerInterface.Interfaces;
using Zenject;

namespace GorillaShotgun.CI
{
    class ShotgunInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<IComputerModEntry>().To<ShotgunEntry>().AsSingle();
        }
    }
}
