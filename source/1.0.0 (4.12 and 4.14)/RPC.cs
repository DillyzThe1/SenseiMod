using System;
using System.Collections.Generic;
using Hazel;
using Reactor;
using Reactor.Networking;
using UnityEngine;

namespace SenseiReworked
{
    public enum CustomRpcCalls : uint
    {
        bypassKill,
        moveSword,
        createSword,
        breakSword
    }
    [RegisterCustomRpc((uint)CustomRpcCalls.bypassKill)]
    public class BypassKill : PlayerCustomRpc<SenseiReworked, (int, int)> //killer id, player id
    {
        public BypassKill(SenseiReworked plugin, uint id) : base(plugin, id)
        { }
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
        public override void Write(MessageWriter writer, (int, int) data)
        {
            writer.Write(data.Item1);
            writer.Write(data.Item2);
        }
        public override (int, int) Read(MessageReader reader)
        {
            int item1 = reader.ReadInt32();
            int item2 = reader.ReadInt32();
            return (item1, item2);
        }
        public override void Handle(PlayerControl innerNetObject, (int, int) data)
        {
            MurderBypass.Murder(data.Item1.getPlayerById(), data.Item2.getPlayerById());
        }
    }
    [RegisterCustomRpc((uint)CustomRpcCalls.moveSword)]
    public class MoveSword : PlayerCustomRpc<SenseiReworked, (float, float, float, string)> //killer id, player id
    {
        public MoveSword(SenseiReworked plugin, uint id) : base(plugin, id)
        { }
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
        public override void Write(MessageWriter writer, (float, float, float, string) data)
        {
            writer.Write(data.Item1);
            writer.Write(data.Item2);
            writer.Write(data.Item3);
            writer.Write(data.Item4);
        }
        public override (float, float, float, string) Read(MessageReader reader)
        {
            float item1 = reader.ReadSingle();
            float item2 = reader.ReadSingle();
            float item3 = reader.ReadSingle();
            string item4 = reader.ReadString();
            return (item1, item2, item3, item4);
        }
        public override void Handle(PlayerControl innerNetObject, (float, float, float, string) data)
        {
            var vectorToTarget = new Vector3(data.Item1, data.Item2, data.Item3);
            float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * (180 / (float)Math.PI);
            GameObject.Find(data.Item4).transform.Find("senseiSword(Clone)").transform.Find("katana").gameObject.GetComponent<SpriteRenderer>().flipY = angle < -90 || angle > 90;
            Quaternion q = Quaternion.Euler(0, 0, angle);
            GameObject.Find(data.Item4).transform.Find("senseiSword(Clone)").transform.localRotation = q;
        }
    }
    [RegisterCustomRpc((uint)CustomRpcCalls.createSword)]
    public class CreateSword : PlayerCustomRpc<SenseiReworked, (int, int)> //killer id, player id
    {
        public CreateSword(SenseiReworked plugin, uint id) : base(plugin, id)
        { }
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
        public override void Write(MessageWriter writer, (int, int) data)
        {
            writer.Write(data.Item1);
        }
        public override (int, int) Read(MessageReader reader)
        {
            int item1 = reader.ReadInt32();
            return (item1, 0);
        }
        public override void Handle(PlayerControl innerNetObject, (int, int) data)
        {
            GameObject newsword = GameObject.Instantiate(AssetLoader.sword); newsword.transform.Find("katana").transform.Find("hitbox").gameObject.AddComponent<SwordMono>().owner = data.Item1.getPlayerById();
            newsword.transform.Find("katana").transform.Find("hitbox").gameObject.GetComponent<SwordMono>().Attach();
        }
    }
    [RegisterCustomRpc((uint)CustomRpcCalls.breakSword)]
    public class BreakSword : PlayerCustomRpc<SenseiReworked, (int, int)> //killer id, player id
    {
        public BreakSword(SenseiReworked plugin, uint id) : base(plugin, id)
        { }
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
        public override void Write(MessageWriter writer, (int, int) data)
        {
            writer.Write(data.Item1);
        }
        public override (int, int) Read(MessageReader reader)
        {
            int item1 = reader.ReadInt32();
            return (item1, 0);
        }
        public override void Handle(PlayerControl innerNetObject, (int, int) data)
        {
            GameObject.Find(data.Item1.getPlayerById().gameObject.name).transform.Find("senseiSword(Clone)").transform.Find("katana").transform.Find("hitbox").GetComponent<SwordMono>().destoryMe = true;
        }
    }
    public static class Mechanics
    {
        public static PlayerControl getPlayerById(this int plrID)
        {
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                if (player.PlayerId == plrID)
                    return player;
            return null;
        }
    }
}