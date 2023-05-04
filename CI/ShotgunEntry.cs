using System;
using ComputerInterface.Interfaces;

namespace GorillaShotgun.CI
{
    class ShotgunEntry : IComputerModEntry
    {
        public string EntryName => "Gorilla Shotgun";

        public Type EntryViewType => typeof(ShotgunView);
    }
}
