using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Newtonsoft.Json;
using VRCLinuxAssistant.Classes;
using VRCLinuxAssistant.Libs;

namespace VRCLinuxAssistant.Pages
{
    public class Mods : UserControl
    {
        
        private static readonly ModListItem.CategoryInfo BrokenCategory = new("Broken", "These mods were broken by a game update. They will be temporarily removed and restored once they are updated for the current game version");
        private static readonly ModListItem.CategoryInfo RetiredCategory = new("Retired", "These mods are either no longer needed due to VRChat updates or are no longer being maintained");
        private static readonly ModListItem.CategoryInfo UncategorizedCategory = new("Uncategorized", "Mods without a category assigned");
        private static readonly ModListItem.CategoryInfo UnknownCategory = new("Unknown/Unverified", "Mods not coming from VRCMG. Potentially dangerous.");

        public List<string> DefaultMods = new List<string>() { "UI Expansion Kit", "Finitizer", "VRCModUpdater.Loader", "VRChatUtilityKit", "Final IK Sanity", "ActionMenuApi" };
        public Mod[] AllModsList;
        public List<Mod> UnknownMods = new List<Mod>();
        public DataGrid ModsList { get; set; }
        public Grid NoModsGrid { get; set; }
        //public CollectionView view;
        public bool PendingChanges;
        public bool HaveInstalledMods;
        
        public string ModBody { get; set; }

        private readonly SemaphoreSlim _modsLoadSem = new SemaphoreSlim(1, 1);

        public List<ModListItem> ModList { get; set; }
        
        public Mods()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        public async void InstallMods()
        {
            MainWindow.InstallButton.IsEnabled = false;

            if (!InstallHandlers.IsMelonLoaderInstalled())
                await InstallHandlers.InstallMelonLoader();

            foreach (ModListItem modListItem in ModsList.SelectedItems)
            {
                Mod mod = modListItem.ModInfo;
                // Ignore mods that are newer than installed version or up-to-date
                if (mod.ListItem.GetVersionComparison >= 0 && mod.installedInBrokenDir == mod.versions[0].IsBroken && mod.installedInRetiredDir == mod.versions[0].IsRetired) continue;


                    MainWindow.MainTextBlock.Text = $"Installing {mod.versions[0].name}...";
                    await InstallHandlers.InstallMod(mod);
                    MainWindow.MainTextBlock.Text = $"{mod.versions[0].name} Installed.";
                
            }

            MainWindow.MainTextBlock.Text = "Finished installing mods.";
            MainWindow.InstallButton.IsEnabled = true;
            LoadMods();
        }
        
        public async Task LoadMods()
        {
            await _modsLoadSem.WaitAsync();

            try
            {
                MainWindow.InstallButton.IsEnabled = false;
                MainWindow.InfoButton.IsEnabled = false;

                AllModsList = null;

                ModList = new List<ModListItem>();
                UnknownMods.Clear();
                HaveInstalledMods = false;

                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    //ModsList.IsVisible = false;
                    MainWindow.MainTextBlock.Text = $"Checking Installed Mods...";
                });
                await CheckInstalledMods();
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    MainWindow.MainTextBlock.Text = $"Loading Mods...";
                });
                await PopulateModsList();
                
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.DataContext = this;

                    //RefreshModsList();

                    MainWindow.MainTextBlock.Text = $"Finished Loading Mods.";
                    MainWindow.InstallButton.IsEnabled = ModList.Count != 0;
                    Task.Run(() =>
                    {
                        while (ModsList == null) { }

                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            ModsList.IsVisible = ModList.Count == 0 ? false : true;
                            NoModsGrid.IsVisible = ModList.Count == 0 ? true : false;
                            ModsList.AutoGenerateColumns = false;
                            ModsList.Items = ModList;
                        });
                    });
                });
                //view = (CollectionView)CollectionViewSource.GetDefaultView(ModsListView.ItemsSource);
                //PropertyGroupDescription groupDescription = new PropertyGroupDescription("Category");
                //view.GroupDescriptions.Add(groupDescription);

                
            }
            finally
            {
                _modsLoadSem.Release();
            }
        }
        
        public async Task PopulateModsList()
        {
            foreach (Mod mod in AllModsList)
                AddModToList(mod);

            foreach (var mod in UnknownMods)
                AddModToList(mod, UnknownCategory);
        }
        
        private void AddModToList(Mod mod, ModListItem.CategoryInfo categoryOverride = null)
        {
            bool preSelected = false;

            var latestVersion = mod.versions[0];

            if (DefaultMods.Contains(latestVersion.name) && !HaveInstalledMods || mod.installedFilePath != null)
            {
                preSelected = true;
            }

            ModListItem.CategoryInfo GetCategory(Mod mod)
            {
                if (mod.category == null) return UncategorizedCategory;
                return new ModListItem.CategoryInfo(mod.category,
                    HardcodedCategories.GetCategoryDescription(mod.category));
            }

            ModListItem ListItem = new ModListItem()
            {
                IsSelected = preSelected,
                IsEnabled = true,
                ModName = latestVersion.name,
                ModVersion = latestVersion.modVersion,
                ModAuthor = HardcodedCategories.FixupAuthor(latestVersion.author),
                ModDescription = latestVersion.description.Replace("\r\n", " ").Replace("\n", " "),
                ModInfo = mod,
                IsInstalled = mod.installedFilePath != null,
                InstalledVersion = mod.installedVersion,
                InstalledModInfo = mod,
                Category = categoryOverride ?? (latestVersion.IsBroken ? BrokenCategory : (latestVersion.IsRetired ? RetiredCategory : GetCategory(mod)))
            };

            foreach (var promo in Promotions.ActivePromotions)
            {
                if (latestVersion.name == promo.ModName)
                {
                    ListItem.PromotionText = promo.Text;
                    ListItem.PromotionLink = promo.Link;
                }
            }

            mod.ListItem = ListItem;

            ModList.Add(ListItem);
        }
        
        public async Task CheckInstalledMods()
        {
            await GetAllMods();

            await Task.Run(() =>
            {
                CheckInstallDir("Plugins");
                CheckInstallDir("Mods");
                CheckInstallDir("Plugins/Broken", isBrokenDir: true);
                CheckInstallDir("Mods/Broken", isBrokenDir: true);
                CheckInstallDir("Plugins/Retired", isRetiredDir: true);
                CheckInstallDir("Mods/Retired", isRetiredDir: true);
            });
        }
        
        public async Task GetAllMods()
        {
            try
            {
                //var client = new HttpClient();
                //var resp = await client.GetAsync(VLAUtils.Constants.VRCMGModsJson);
                //var body = await resp.Content.ReadAsStringAsync();
                //AllModsList = JsonSerializer.Deserialize<Mod[]>(body);
                if (ModBody == null)
                {
                    var client = new WebClient();
                    ModBody = client.DownloadString(VLAUtils.Constants.VRCMGModsJson);   
                }
                AllModsList = JsonConvert.DeserializeObject<Mod[]>(ModBody);
                
                foreach (var mod in AllModsList)
                {
                    try
                    {
                        mod.category = HardcodedCategories.GetCategoryFor(mod) ?? "Uncategorized";
                    }
                    catch (Exception e)
                    {
                        mod.category = "Uncategorized";
                    }
                }

                Array.Sort(AllModsList, (a, b) =>
                {
                    try
                    {
                        var categoryCompare = String.Compare(a.category, b.category, StringComparison.Ordinal);
                        if (categoryCompare != 0) return categoryCompare;
                        return String.Compare(a.versions[0].name, b.versions[0].name, StringComparison.Ordinal);
                    }
                    catch (Exception e)
                    {
                        return 0;
                    }
                });
            }
            catch (Exception e)
            {
                MainWindow.MainTextBlock.Text = $"Error getting mods: {e.Message}";
            }
        }
        
        private void CheckInstallDir(string directory, bool isBrokenDir = false, bool isRetiredDir = false)
        {
            if (!Directory.Exists(Path.Combine(App.VRChatInstallDirectory, directory)))
            {
                return;
            }

            foreach (string file in Directory.GetFileSystemEntries(Path.Combine(App.VRChatInstallDirectory, directory), "*.dll", SearchOption.TopDirectoryOnly))
            {
                if (!File.Exists(file) || Path.GetExtension(file) != ".dll") continue;

                var modInfo = ExtractModVersions(file);
                if (modInfo.Item1 != null && modInfo.Item2 != null)
                {
                    var haveFoundMod = false;

                    foreach (var mod in AllModsList)
                    {
                        if (!mod.aliases.Contains(modInfo.ModName) && mod.versions.All(it => it.name != modInfo.ModName)) continue;

                        HaveInstalledMods = true;
                        haveFoundMod = true;
                        mod.installedFilePath = file;
                        mod.installedVersion = modInfo.ModVersion;
                        mod.installedInBrokenDir = isBrokenDir;
                        mod.installedInRetiredDir = isRetiredDir;
                        break;
                    }

                    if (!haveFoundMod)
                    {
                        var mod = new Mod()
                        {
                            installedFilePath = file,
                            installedVersion = modInfo.ModVersion,
                            installedInBrokenDir = isBrokenDir,
                            installedInRetiredDir = isRetiredDir,
                            versions = new []
                            {
                                new Mod.ModVersion()
                                {
                                    name = modInfo.ModName,
                                    modVersion = modInfo.ModVersion,
                                    author = modInfo.ModAuthor,
                                    description = ""
                                }
                            }
                        };
                        UnknownMods.Add(mod);
                    }
                }
            }
        }
        
        private (string ModName, string ModVersion, string ModAuthor) ExtractModVersions(string dllPath)
        {
            try
            {
                using var asmdef = Mono.Cecil.AssemblyDefinition.ReadAssembly(dllPath);
                foreach (var attr in asmdef.CustomAttributes)
                    if (attr.AttributeType.Name == "MelonInfoAttribute" ||
                        attr.AttributeType.Name == "MelonModInfoAttribute")
                        return ((string) attr.ConstructorArguments[1].Value,
                            (string) attr.ConstructorArguments[2].Value, (string) attr.ConstructorArguments[3].Value);
            }
            catch (Exception ex)
            {
                //TODO: Implement MessageBoxes
            }

            return (null, null, null);
        }
        
        public class Category
        {
            public string CategoryName { get; set; }
            public List<ModListItem> Mods = new List<ModListItem>();
        }
        public class ModListItem
        {
            public string ModName { get; set; }
            public string ModVersion { get; set; }
            public string ModAuthor { get; set; }
            public string ModDescription { get; set; }
            public bool PreviousState { get; set; }

            public bool IsEnabled { get; set; }
            public bool IsSelected { get; set; }
            public Mod ModInfo { get; set; }
            public CategoryInfo Category { get; set; }

            public Mod InstalledModInfo { get; set; }
            public bool IsInstalled { get; set; }
            private SemVersion _installedVersion { get; set; }
            public string InstalledVersion
            {
                get
                {
                    if (!IsInstalled || _installedVersion == null) return "-";
                    return _installedVersion.ToString();
                }
                set
                {
                    if (SemVersion.TryParse(value, out SemVersion tempInstalledVersion))
                    {
                        _installedVersion = tempInstalledVersion;
                    }
                    else
                    {
                        _installedVersion = null;
                    }
                }
            }

            public string GetVersionColor
            {
                get
                {
                    if (!IsInstalled || _installedVersion == null) return "Black";
                    return _installedVersion >= ModVersion ? "Green" : "Red";
                }
            }

            public string GetVersionDecoration
            {
                get
                {
                    if (!IsInstalled || _installedVersion == null) return "None";
                    return _installedVersion >= ModVersion ? "None" : "Strikethrough";
                }
            }

            public int GetVersionComparison
            {
                get
                {
                    if (!IsInstalled || _installedVersion == null || _installedVersion < ModVersion) return -1;
                    if (_installedVersion > ModVersion) return 1;
                    return 0;
                }
            }

            public bool CanDelete => IsInstalled;

            public string CanSeeDelete => IsInstalled ? "Visible" : "Hidden";

            public string PromotionText { get; set; }
            public string PromotionLink { get; set; }
            public string PromotionMargin
            {
                get
                {
                    if (string.IsNullOrEmpty(PromotionText)) return "0";
                    return "0,0,5,0";
                }
            }

            public bool PromotionVisibility => string.IsNullOrEmpty(PromotionText) ? false : true;

            public record CategoryInfo(string Name, string Description)
            {
                public string Name { get; } = Name;
                public string Description { get; } = Description;
            }
        }

        private void ModGrid_OnInitialized(object? sender, EventArgs e)
        {
            ModsList = sender as DataGrid;
        }

        private void NoModsGrid_OnInitialized(object? sender, EventArgs e)
        {
            NoModsGrid = sender as Grid;
        }

        private void Uninstall_OnClick(object? sender, RoutedEventArgs e)
        {
            Mod mod = (sender as Button).Tag as Mod;
            UninstallModFromList(mod);
        }
        
        private void UninstallModFromList(Mod mod)
        {
            UninstallMod(mod.ListItem.InstalledModInfo);
            mod.ListItem.IsInstalled = false;
            mod.ListItem.InstalledVersion = null;
            LoadMods();
        }
        
        public void UninstallMod(Mod mod)
        {
            try
            {
                File.Delete(mod.installedFilePath);
            }
            catch (Exception ex)
            {
                MainWindow.MainTextBlock.Text = $"Failed to Uninstall {mod.versions[0].name}";
            }
        }
    }
}