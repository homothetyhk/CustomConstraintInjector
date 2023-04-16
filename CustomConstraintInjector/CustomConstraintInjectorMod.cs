using MenuChanger;
using Modding;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RandomizerCore.Randomization;
using RandomizerMod.RC;

namespace CustomConstraintInjector
{
    public class CustomConstraintInjectorMod : Mod, IGlobalSettings<GlobalSettings>
    {
        public static string ModDirectory { get; }
        public static GlobalSettings GS { get; private set; } = new();
        public static readonly Dictionary<string, ConstraintPack> Packs = new();

        public CustomConstraintInjectorMod()
        {
            LoadFiles();
        }

        public override void Initialize()
        {
            MenuChangerMod.OnExitMainMenu += MenuHolder.OnExitMenu;
            RandomizerMod.Menu.RandomizerMenuAPI.AddMenuPage(MenuHolder.ConstructMenu, MenuHolder.TryGetMenuButton);
            RandomizerMod.Logging.SettingsLog.AfterLogSettings += LogSettings;
            SettingsInterop.Setup(this);
            RequestBuilder.OnUpdate.Subscribe(300f, ApplyConstraints);
            RequestBuilder.OnUpdate.Subscribe(300f, ApplyPriorityEdits);
        }

        public override string GetVersion()
        {
            Version v = GetType().Assembly.GetName().Version;
            return $"{v.Major}.{v.Minor}.{v.Build}";
        }

        public static void LoadFiles()
        {
            Packs.Clear();
            DirectoryInfo main = new(ModDirectory);

            foreach (var f in main.EnumerateFiles("*.json"))
            {
                using FileStream fs = f.OpenRead();
                using StreamReader sr = new(fs);
                using JsonTextReader jtr = new(sr);
                JsonSerializer serializer = new()
                {
                    DefaultValueHandling = DefaultValueHandling.Include,
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto,
                };
                serializer.Converters.Add(new StringEnumConverter());
                ConstraintPack def = serializer.Deserialize<ConstraintPack>(jtr);
                Packs.Add(def.Name, def);
            }
            GS.ActivePacks.RemoveWhere(s => !Packs.ContainsKey(s));
        }

        private static void LogSettings(RandomizerMod.Logging.LogArguments arg1, TextWriter tw)
        {
            tw.WriteLine("Logging CustomConstraintInjector settings:");
            using JsonTextWriter jtw = new(tw) { CloseOutput = false, };
            RandomizerMod.RandomizerData.JsonUtil._js.Serialize(jtw, GS);
            tw.WriteLine();
        }

        public static void ApplyConstraints(RequestBuilder rb)
        {
            if (GS.ActivePacks.Count == 0) return;
            ConstraintHandler ch = new(rb, Packs.Values.Where(p => GS.ActivePacks.Contains(p.Name)));
            foreach (GroupBuilder gb in rb.Stages.SelectMany(sb => sb.Groups)) if (gb.strategy is DefaultGroupPlacementStrategy dgps) dgps.ConstraintList.Add(ch);
        }

        public static void ApplyPriorityEdits(RequestBuilder rb)
        {
            if (GS.ActivePacks.Count == 0) return;
            PriorityHandler ph = new(rb, Packs.Values.Where(p => GS.ActivePacks.Contains(p.Name)));
            foreach (GroupBuilder gb in rb.Stages.SelectMany(sb => sb.Groups)) gb.onPermute += ph.OnPermute;
        }

        void IGlobalSettings<GlobalSettings>.OnLoadGlobal(GlobalSettings s)
        {
            GS = s ?? new();
        }

        GlobalSettings IGlobalSettings<GlobalSettings>.OnSaveGlobal()
        {
            return GS;
        }

        static CustomConstraintInjectorMod()
        {
            ModDirectory = Path.GetDirectoryName(typeof(CustomConstraintInjectorMod).Assembly.Location);
        }
    }
}
