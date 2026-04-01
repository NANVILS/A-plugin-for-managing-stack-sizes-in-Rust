using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("StackSizeController", "RustPlugins", "1.0.0")]
    [Description("Controls item stack sizes. NEVER overwrites config on reload.")]
    class StackSizeController : CovalencePlugin
    {
        private Configuration _config;
        private Dictionary<string, int> _vanillaDefaults = new Dictionary<string, int>();
        private bool _configExisted = false;

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Revert to vanilla on unload")]
            public bool RevertOnUnload = true;

            [JsonProperty("Allow stacking items with durability")]
            public bool AllowDurabilityStacking = true;

            [JsonProperty("Stack sizes")]
            public Dictionary<string, int> StackSizes = new Dictionary<string, int>();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null)
                    throw new Exception("Config is null");

                if (_config.StackSizes == null)
                    _config.StackSizes = new Dictionary<string, int>();

                _configExisted = true;
                Puts("Config loaded. " + _config.StackSizes.Count.ToString() + " items.");
            }
            catch
            {
                _configExisted = false;
                _config = new Configuration();
                Puts("No config found, will generate on first start.");
            }
        }

        protected override void LoadDefaultConfig()
        {
            // Do nothing here - we handle it in OnServerInitialized
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        #endregion

        #region Oxide Hooks

        private void OnServerInitialized()
        {
            // Cache vanilla defaults BEFORE applying anything
            foreach (ItemDefinition item in ItemManager.GetItemDefinitions())
            {
                _vanillaDefaults[item.shortname] = item.stackable;
            }

            // ONLY generate config if it did NOT exist before
            if (!_configExisted || _config.StackSizes.Count == 0)
            {
                Puts("First run detected. Generating default config...");
                GenerateDefaultConfig();
                SaveConfig();
                Puts("Config created with " + _config.StackSizes.Count.ToString() + " items. Edit it and use stack.reload");
            }

            // Apply stack sizes
            ApplyStackSizes();

            AddCovalenceCommand("stack.set", nameof(CmdSetStack), "stacksizecontroller.admin");
            AddCovalenceCommand("stack.setall", nameof(CmdSetAll), "stacksizecontroller.admin");
            AddCovalenceCommand("stack.search", nameof(CmdSearch), "stacksizecontroller.admin");
            AddCovalenceCommand("stack.reload", nameof(CmdReload), "stacksizecontroller.admin");

            Puts("Ready. Stack sizes applied.");
        }

        private void Unload()
        {
            if (_config != null && _config.RevertOnUnload)
            {
                foreach (ItemDefinition item in ItemManager.GetItemDefinitions())
                {
                    if (_vanillaDefaults.ContainsKey(item.shortname))
                        item.stackable = _vanillaDefaults[item.shortname];
                }
                Puts("Reverted to vanilla.");
            }
        }

        private object OnMaxStackable(Item item)
        {
            if (item == null || item.info == null)
                return null;

            if (_config != null && _config.StackSizes.ContainsKey(item.info.shortname))
            {
                int size = _config.StackSizes[item.info.shortname];
                if (size > 0)
                    return size;
            }

            return null;
        }

        #endregion

        #region Core

        private void GenerateDefaultConfig()
        {
            // Weapons that stay at stack 1
            HashSet<string> weapons = new HashSet<string>
            {
                "rifle.ak","rifle.ak.diver","rifle.ak.ice","rifle.ak.jungle","rifle.ak.med",
                "rifle.bolt","rifle.l96","rifle.lr300","rifle.lr300.space","rifle.m39",
                "rifle.semiauto","rifle.sks",
                "pistol.eoka","pistol.m92","pistol.python","pistol.revolver",
                "pistol.semiauto","pistol.semiauto.a.m15","pistol.prototype17",
                "pistol.nailgun","pistol.water","revolver.hc",
                "shotgun.double","shotgun.pump","shotgun.m4","shotgun.spas12","shotgun.waterpipe",
                "smg.mp5","smg.thompson","smg.2","t1_smg",
                "lmg.m249","hmlmg","minigun",
                "rocket.launcher","rocket.launcher.dragon","rocket.launcher.rpg7",
                "homingmissile.launcher","multiplegrenadelauncher",
                "bow.hunting","bow.compound","crossbow","legacy bow","minicrossbow",
                "blowpipe","blunderbuss","speargun",
                "flamethrower","military flamethrower",
                "bone.club","knife.bone","knife.bone.obsidian","knife.combat",
                "knife.butcher","knife.skinning","sunken.knife",
                "mace","mace.baseballbat","machete","longsword","salvaged.sword","salvaged.cleaver",
                "spear.stone","spear.wooden","spear.cny",
                "paddle","boomerang","pitchfork","sickle","skull","vampire.stake",
                "candycaneclub","cakefiveyear",
                "krieg.chainsword","krieg.shotgun",
                "improvised.shield","metal.shield","reinforced.wooden.shield","wooden.shield",
                "torch","torch.torch.skull","divertorch",
                "gun.water","paintballgun","snowballgun","toolgun"
            };

            _config.StackSizes.Clear();

            foreach (ItemDefinition item in ItemManager.GetItemDefinitions())
            {
                if (weapons.Contains(item.shortname))
                    _config.StackSizes[item.shortname] = 1;
                else
                    _config.StackSizes[item.shortname] = 999999999;
            }
        }

        private void ApplyStackSizes()
        {
            int count = 0;
            foreach (ItemDefinition item in ItemManager.GetItemDefinitions())
            {
                if (item.condition.enabled && !_config.AllowDurabilityStacking)
                    continue;

                if (_config.StackSizes.ContainsKey(item.shortname))
                {
                    int newStack = Math.Max(1, _config.StackSizes[item.shortname]);
                    item.stackable = newStack;
                    count++;
                }
                // If item is NOT in config - leave it at vanilla, do NOT add to config
            }
            Puts("Applied stacks to " + count.ToString() + " items.");
        }

        #endregion

        #region Commands

        private void CmdSetStack(IPlayer player, string command, string[] args)
        {
            if (args.Length < 2)
            {
                player.Reply("[StackSize] Usage: stack.set <shortname> <amount>");
                return;
            }

            ItemDefinition item = ItemManager.FindItemDefinition(args[0]);
            if (item == null)
            {
                player.Reply("[StackSize] Item not found: " + args[0]);
                return;
            }

            int amount;
            if (!int.TryParse(args[1], out amount) || amount < 1)
            {
                player.Reply("[StackSize] Invalid amount.");
                return;
            }

            _config.StackSizes[item.shortname] = amount;
            SaveConfig();
            ApplyStackSizes();
            player.Reply("[StackSize] " + item.shortname + " = " + amount.ToString());
        }

        private void CmdSetAll(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                player.Reply("[StackSize] Usage: stack.setall <amount>");
                return;
            }

            int amount;
            if (!int.TryParse(args[0], out amount) || amount < 1)
            {
                player.Reply("[StackSize] Invalid amount.");
                return;
            }

            List<string> keys = new List<string>(_config.StackSizes.Keys);
            foreach (string key in keys)
            {
                _config.StackSizes[key] = amount;
            }

            SaveConfig();
            ApplyStackSizes();
            player.Reply("[StackSize] All items set to " + amount.ToString());
        }

        private void CmdSearch(IPlayer player, string command, string[] args)
        {
            if (args.Length < 1)
            {
                player.Reply("[StackSize] Usage: stack.search <name>");
                return;
            }

            string search = args[0].ToLower();
            int found = 0;

            foreach (ItemDefinition item in ItemManager.GetItemDefinitions())
            {
                if (item.shortname.ToLower().Contains(search) ||
                    item.displayName.english.ToLower().Contains(search))
                {
                    int configured = _config.StackSizes.ContainsKey(item.shortname) ? _config.StackSizes[item.shortname] : -1;
                    int active = item.stackable;

                    player.Reply("  " + item.shortname + " | Config: " + configured.ToString() + " | Active: " + active.ToString());
                    found++;
                    if (found >= 20) break;
                }
            }

            if (found == 0)
                player.Reply("[StackSize] Nothing found for: " + search);
        }

        private void CmdReload(IPlayer player, string command, string[] args)
        {
            // Revert to vanilla first
            foreach (ItemDefinition item in ItemManager.GetItemDefinitions())
            {
                if (_vanillaDefaults.ContainsKey(item.shortname))
                    item.stackable = _vanillaDefaults[item.shortname];
            }

            // Re-read config from file (NO modifications to config!)
            try
            {
                _config = Config.ReadObject<Configuration>();
                if (_config == null || _config.StackSizes == null)
                {
                    player.Reply("[StackSize] ERROR: Config is broken! Fix JSON and try again.");
                    return;
                }
            }
            catch (Exception ex)
            {
                player.Reply("[StackSize] ERROR reading config: " + ex.Message);
                return;
            }

            // Apply without touching config file
            ApplyStackSizes();
            player.Reply("[StackSize] Config reloaded from file. " + _config.StackSizes.Count.ToString() + " items. Config NOT modified.");
        }

        #endregion
    }
}