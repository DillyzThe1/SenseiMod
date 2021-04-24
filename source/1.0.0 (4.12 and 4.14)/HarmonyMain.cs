using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using DillyzRolesAPI.Roles;
using HarmonyLib;
using Reactor;
using Reactor.Networking;
using UnityEngine;

namespace SenseiReworked
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    public class SenseiReworked : BasePlugin
    {
        public const string Id = "gg.reactor.roleapitestdillyz";

        public Harmony Harmony { get; } = new Harmony(Id);

        public static RoleGenerator sensei = new RoleGenerator();

        public override void Load()
        {
            Harmony.PatchAll();

            AssetLoader.BundleLoad();

            RegisterInIl2CppAttribute.Register();
            RegisterCustomRpcAttribute.Register(this);

            staticvars.noColorYoinking = true;

            sensei.NameOfRole = "Sensei";
            sensei.RoleColor = new Color(0.51f, 0.27f, 0.79f, 1f);
            sensei.IntroText = "Seal The Impostors in your demonic sword.";
            sensei.EjectionText = "was The Sensei.";
            sensei.isEnabled = true;
            sensei.canVent = false;
            sensei.Awake();

            NewRole.pingText.Add("Sensei mod V1.0.0 \n[3AA3D9]github.com/DillyzThe1[]");
            NewRole.modsText.Add("Sensei Mod <#F6FF00>1.0.0</color> by <#3AA3D9>DillyzThe1</color>.");
        }
    }
}
