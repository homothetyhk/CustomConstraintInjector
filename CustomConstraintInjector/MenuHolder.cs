using MenuChanger;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using MenuChanger.Extensions;

namespace CustomConstraintInjector
{
    public class MenuHolder
    {
        public static MenuHolder Instance { get; private set; }

        public MenuPage ConnectionsPage;
        public MenuPage MainPage;
        public SmallButton JumpButton;
        public MultiGridItemPanel Panel;
        public IMenuElement[] PackToggles;
        public Dictionary<string, ToggleButton> PackToggleLookup = new();
        public SmallButton? RestoreLocalPacks;

        public static void OnExitMenu()
        {
            Instance = null;
        }

        public static void ConstructMenu(MenuPage connectionsPage)
        {
            Instance ??= new();
            Instance.OnConstructMenuFirstTime(connectionsPage);
            Instance.OnMenuConstruction();
        }

        public void ReconstructMenu()
        {
            UnityEngine.Object.Destroy(MainPage.self);
            JumpButton.ClearOnClick();
            OnMenuConstruction();
        }

        public void OnConstructMenuFirstTime(MenuPage connectionsPage)
        {
            ConnectionsPage = connectionsPage;
            JumpButton = new(connectionsPage, "Custom Constraint Injection");
            connectionsPage.BeforeShow += () => JumpButton.Text.color = CustomConstraintInjectorMod.GS.ActivePacks.Count != 0 ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
        }

        public void OnMenuConstruction()
        {
            MainPage = new("Custom Constraint Injector Main Menu", ConnectionsPage);
            JumpButton.AddHideAndShowEvent(MainPage);
            PackToggles = CustomConstraintInjectorMod.Packs.Values
                .Select(def => (IMenuElement)CreatePackToggle(MainPage, def)).ToArray();
            Panel = new(MainPage, 5, 3, 60f, 650f, new(0, 300), PackToggles);
        }

        public void CreateRestoreLocalPacksButton()
        {
            RestoreLocalPacks = new SmallButton(MainPage, "Restore Local Packs");
            RestoreLocalPacks.OnClick += () =>
            {
                MainPage.Hide();
                CustomConstraintInjectorMod.LoadFiles();
                ReconstructMenu();
                MainPage.Show();
            };
            RestoreLocalPacks.MoveTo(new(0f, -300f));
            RestoreLocalPacks.SymSetNeighbor(Neighbor.Up, Panel);
            RestoreLocalPacks.SymSetNeighbor(Neighbor.Down, MainPage.backButton);
        }

        public ToggleButton CreatePackToggle(MenuPage page, ConstraintPack def)
        {
            ToggleButton button = new(page, def.Name);
            button.SetValue(CustomConstraintInjectorMod.GS.ActivePacks.Contains(def.Name));
            button.ValueChanged += b =>
            {
                if (b)
                {
                    CustomConstraintInjectorMod.GS.ActivePacks.Add(def.Name);
                }
                else
                {
                    CustomConstraintInjectorMod.GS.ActivePacks.Remove(def.Name);
                }
            };
            PackToggleLookup[def.Name] = button;
            if (Panel is not null)
            {
                Panel.Add(button);
            }
            return button;
        }

        public static bool TryGetMenuButton(MenuPage connectionsPage, out SmallButton button)
        {
            return Instance.TryGetJumpButton(connectionsPage, out button);
        }

        public bool TryGetJumpButton(MenuPage connectionsPage, out SmallButton button)
        {
            button = JumpButton;
            return true;
        }

        public bool TryGetPackToggle(string name, out ToggleButton button)
        {
            return PackToggleLookup.TryGetValue(name, out button);
        }

        public void ToggleAllOff()
        {
            foreach (ToggleButton b in PackToggleLookup.Values) b.SetValue(false);
        }
    }
}
