using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;
using UnityEngine;
using Reactor.Extensions;
using InnerNet;
using Reactor.Networking;
using DillyzRolesAPI.Roles;
using UnityEngine.SceneManagement;
using TMPro;
using Hazel.Udp;

namespace SenseiReworked
{
    [HarmonyPatch(typeof(ShipStatus), "CalculateLightRadius")]
    public static class lightLowerPatch
    {
        public static bool Prefix([HarmonyArgument(0)] GameData.PlayerInfo player, ShipStatus __instance, ref float __result)
        {
            if (SenseiReworked.sensei.containedPlayerIds.Contains(PlayerControl.LocalPlayer.PlayerId))
            {
                __result = 10f;
                return false;
            }
            return true;
        }
    }
    public static class AssetLoader
    {
        public static AssetBundle bundle;

        public static Sprite hideSprite;
        public static Sprite revealSprite;

        public static GameObject sword;
        public static void BundleLoad()
        {
            if (bundle == null)
            {
                byte[] array = Properties.Resources.items;
                bundle = AssetBundle.LoadFromMemory(array);
            }
            hideSprite = bundle.LoadAsset<Sprite>("hidesword").DontUnload();
            revealSprite = bundle.LoadAsset<Sprite>("revealsword").DontUnload();
            sword = bundle.LoadAsset<GameObject>("senseiSword").DontUnload();
        }
    }
    [HarmonyPatch(typeof(HudManager), nameof(HudManager.Start))]
    public static class ButtonGen
    {
        private static CooldownButton swordButton;
        public static bool isReveal = true;
        

        public static void Postfix(HudManager __instance)
        {
            swordButton = new CooldownButton(
                () =>
                {
                    if (isReveal)
                    {
                        swordButton.Image = AssetLoader.hideSprite;
                        isReveal = false;
                        GameObject newsword = GameObject.Instantiate(AssetLoader.sword); newsword.transform.Find("katana").transform.Find("hitbox").gameObject.AddComponent<SwordMono>().owner = PlayerControl.LocalPlayer;
                        newsword.transform.Find("katana").transform.Find("hitbox").gameObject.GetComponent<SwordMono>().Attach();
                        Rpc<CreateSword>.Instance.Send((PlayerControl.LocalPlayer.PlayerId,0));
                    }
                    else
                    {
                        swordButton.Image = AssetLoader.revealSprite;
                        isReveal = true;
                        GameObject.Find(PlayerControl.LocalPlayer.gameObject.name).transform.Find("senseiSword(Clone)").transform.Find("katana").transform.Find("hitbox").GetComponent<SwordMono>().destoryMe = true;
                        Rpc<BreakSword>.Instance.Send((PlayerControl.LocalPlayer.PlayerId, 0));
                    }
                },
                1f,
                AssetLoader.revealSprite,
                Vector2.zero,
                () =>
                {
                    return !PlayerControl.LocalPlayer.Data.IsDead && huduppatch.localIsSensei && MeetingHud.Instance == null && (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started || AmongUsClient.Instance.GameMode == GameModes.FreePlay);
                },
                __instance
            );
        }
    }

    [HarmonyPatch(typeof(HudManager), (nameof(HudManager.Update)))]
    class huduppatch
    {
        public static bool localIsSensei;
        public static void Postfix() 
        { 
            try { localIsSensei = SenseiReworked.sensei.containedPlayerIds.Contains(PlayerControl.LocalPlayer.PlayerId); } catch {  } 
        }
    }

    /*[HarmonyPatch(typeof(FindGameButton),nameof(FindGameButton.OnClick))]
    public class FindGameDisablePatch
    {
        public static void Prefix(FindGameButton __instance)
        {
            __instance.gameObject.SetActive(false);
            //Application.OpenURL("https://youtu.be/dQw4w9WgXcQ");
            AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.Custom;
            AmongUsClient.Instance.LastCustomDisconnect = "Cheating on regular servers is <#FF6A00>STRICTLY</color> prohibited.\n<#FF0000>Don't</color> attempt it.";
            AmongUsClient.Instance.HandleDisconnect(AmongUsClient.Instance.LastDisconnectReason, AmongUsClient.Instance.LastCustomDisconnect);
        }
    }
    [HarmonyPatch(typeof(JoinGameButton), nameof(JoinGameButton.OnClick))]
    public class JoinGameCodePatchLmao
    {
        public static void Prefix(JoinGameButton __instance)
        {
            if (SceneManager.GetActiveScene().name == "MMOnline")
                if (__instance.transform.Find("GameIdText").transform.Find("Text_TMP").GetComponent<TextMeshPro>().m_text.Contains("SUS"))
                {
                    Application.OpenURL("https://youtu.be/dQw4w9WgXcQ");
                    AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.Custom;
                    AmongUsClient.Instance.LastCustomDisconnect = "When The <#FF0000>Impostor</color> is sus!\n(lmao get rickrolled)";
                    AmongUsClient.Instance.HandleDisconnect(AmongUsClient.Instance.LastDisconnectReason, AmongUsClient.Instance.LastCustomDisconnect);
                }
            //__instance.gameObject.SetActive(false);
            //Application.OpenURL("https://youtu.be/dQw4w9WgXcQ");
        }
    }*/ // moves to the api

    // yoinked from https://github.com/slushiegoose/Town-Of-Us/tree/master/source/Patches
    public static class MurderBypass
    {
        public static void RpcMurder(PlayerControl killer, PlayerControl victim)
        {
            Murder(killer, victim);
            Rpc<BypassKill>.Instance.Send((killer.PlayerId, victim.PlayerId));
        }
        public static void Murder(PlayerControl killer, PlayerControl victim)
        {
            GameData.PlayerInfo data = victim.Data;
            if (data != null && !data.IsDead)
            {
                if (killer == PlayerControl.LocalPlayer)
                    SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 0.8f);
                victim.gameObject.layer = LayerMask.NameToLayer("Ghost");
                if (victim.AmOwner)
                {
                    try
                    {
                        if (Minigame.Instance)
                        {
                            Minigame.Instance.Close();
                            Minigame.Instance.Close();
                        }

                        if (MapBehaviour.Instance)
                        {
                            MapBehaviour.Instance.Close();
                            MapBehaviour.Instance.Close();
                        }
                    }catch { }
                    DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowOne(killer.Data, data);
                    DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
                    victim.nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
                    victim.RpcSetScanner(false);
                    ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
                    importantTextTask.transform.SetParent(AmongUsClient.Instance.transform, false);
                    if (!PlayerControl.GameOptions.GhostsDoTasks)
                    {
                        for (int i = 0; i < victim.myTasks.Count; i++)
                        {
                            PlayerTask playerTask = victim.myTasks.ToArray()[i];
                            playerTask.OnRemove();
                            UnityEngine.Object.Destroy(playerTask.gameObject);
                        }
                        victim.myTasks.Clear();
                        importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(StringNames.GhostIgnoreTasks, new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Object>(0));
                    }
                    else
                    {
                        importantTextTask.Text = DestroyableSingleton<TranslationController>.Instance.GetString(
                            StringNames.GhostDoTasks,
                            new UnhollowerBaseLib.Il2CppReferenceArray<Il2CppSystem.Object>(0));
                    }
                    victim.myTasks.Insert(0, importantTextTask);
                }
                killer.MyPhysics.StartCoroutine(killer.KillAnimations.Random<KillAnimation>().CoPerformKill(killer, victim));
                var deadBody = new DeadPlayer
                {
                    PlayerId = victim.PlayerId,
                    KillerId = killer.PlayerId,
                    KillTime = DateTime.UtcNow,
                };
            }
        }
    }
    [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
    public static class FastBoiUpdate
    {
        public static void Postfix(PlayerPhysics __instance)
        {
            if (isFast(__instance.myPlayer))
                if (__instance.AmOwner && GameData.Instance && __instance.myPlayer.CanMove)
                    __instance.body.velocity *= 2f;
        }
        public static bool isFast(PlayerControl playerRequest)
        {
            if (ShipStatus.Instance != null)
            {
                PlayerControl closeBoi = null;
                if (!SenseiReworked.sensei.containedPlayerIds.Contains(playerRequest.PlayerId) || PlayerControl.LocalPlayer.Data.IsDead)
                    return false;
                double mindist = 5.5;
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    Vector3 refpos = playerRequest.GetTruePosition();
                    Vector3 playerpos = player.GetTruePosition();
                    double dist = Math.Sqrt((refpos.x - playerpos.x) * (refpos.x - playerpos.x) + (refpos.y - playerpos.y) * (refpos.y - playerpos.y));
                    if (player == playerRequest)
                        continue;
                    if (dist <= mindist)
                    {
                        mindist = dist;
                        closeBoi = player;
                    }
                }
                if (closeBoi == null || closeBoi.Data.IsDead)
                    return true;
            }
            return false;
        }
    }
    [HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.FixedUpdate))]
    public static class NetworkFastBoiUpdate
    {
        public static void Postfix(CustomNetworkTransform __instance)
        {
            if (!__instance.AmOwner)
                if (__instance.interpolateMovement != 0f)
                    if (FastBoiUpdate.isFast(__instance.gameObject.GetComponent<PlayerControl>()))
                        __instance.body.velocity *= 2f;
        }
    }
    //GameStartManager.Instance.transform.Find("MakePublicButton").gameObject.SetActive(false);
    /*[HarmonyPatch(typeof(GameStartManager),nameof(GameStartManager.Start))]
    public class gameStartManagePatch
    {
        public static void Postfix(GameStartManager __instance)
        {
            if (SceneManager.GetActiveScene().name == "OnlineGame")
            {
                __instance.GameRoomName.transform.position = __instance.MakePublicButton.transform.position;
                __instance.MakePublicButton.gameObject.SetActive(false);
                __instance.StartButton.transform.position += new Vector3(0f, -0.9f, 0f);
                __instance.PlayerCounter.transform.position += new Vector3(0.1f, 0f, 0f);
                __instance.GameStartText.transform.position += new Vector3(0f, -0.9f, 0f);
                gameStartUpdatePatch.originalCode = __instance.GameRoomName.text;
            }
        }
    }
    [HarmonyPatch(typeof(GameStartManager), nameof(GameStartManager.Update))]
    public class gameStartUpdatePatch
    {
        public static string originalCode;
        public static void Postfix(GameStartManager __instance)
        {
            if (SceneManager.GetActiveScene().name == "OnlineGame" && originalCode.Contains("Code"))
                if (AmongUsClient.Instance.AmHost)
                    if (Input.GetKey("c"))
                        __instance.GameRoomName.text = originalCode;
                    else
                        __instance.GameRoomName.text = "Hold C!";
                else
                    __instance.GameRoomName.text = "Requires host!";
        }
    }*/ //moved to the api
    public class DeadPlayer
    {
        public byte KillerId { get; set; }
        public byte PlayerId { get; set; }
        public DateTime KillTime { get; set; }
        public DeathReason DeathReason { get; set; }
    }

    [HarmonyPatch(typeof(ShipStatus), (nameof(ShipStatus.Awake)))]
    public class shipstatusAwake
    {
        public static void Postfix()
        {
            ButtonGen.isReveal = true;
        }
    }
}
