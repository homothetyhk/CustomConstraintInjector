using Modding;
using MonoMod.ModInterop;

namespace CustomConstraintInjector
{
    internal class SettingsInterop
    {
        [ModImportName("RandoSettingsManager")]
        internal static class RSMImport
        {
            public static Action<Mod, Type, Delegate, Delegate>? RegisterConnectionSimple = null;
            static RSMImport() => typeof(RSMImport).ModInterop();
        }

        public class RSMData
        {
            public RSMData() { }
            public RSMData(GlobalSettings gs)
            {
                SharedPacks = CustomConstraintInjectorMod.Packs.Values.Where(p => gs.ActivePacks.Contains(p.Name)).ToList();
            }

            public List<ConstraintPack> SharedPacks;
        }

        internal static void Setup(Mod mod)
        {
            RSMImport.RegisterConnectionSimple?.Invoke(mod, typeof(RSMData), ReceiveSettings, SendSettings);
        }

        internal static void ReceiveSettings(RSMData? data) 
        {
            MenuHolder.Instance.ToggleAllOff();

            if (data is not null)
            {
                CustomConstraintInjectorMod.Packs.Clear();
                foreach (ConstraintPack pack in data.SharedPacks) CustomConstraintInjectorMod.Packs.Add(pack.Name, pack);
                CustomConstraintInjectorMod.GS.ActivePacks.UnionWith(data.SharedPacks.Select(p => p.Name));
                MenuHolder.Instance.ReconstructMenu();
                MenuHolder.Instance.CreateRestoreLocalPacksButton();
            }
        }

        internal static RSMData? SendSettings()
        {
            return CustomConstraintInjectorMod.GS.ActivePacks.Count > 0 ? new(CustomConstraintInjectorMod.GS) : null;
        }
    }
}
