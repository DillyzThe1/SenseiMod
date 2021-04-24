using System;
using System.Collections.Generic;
using System.Text;
using Reactor;
using Reactor.Extensions;
using Reactor.Networking;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SenseiReworked
{
    [RegisterInIl2Cpp]
    public class SwordMono : MonoBehaviour
    {
        public PlayerControl triggerPlayer;
        public PlayerControl owner;
        public bool destoryMe;
        private void OnTriggerEnter2D(Collider2D tag)
        {
            PlayerControl targetobject = tag.gameObject.GetComponent<PlayerControl>();
            if (targetobject != null && !targetobject.Data.IsDead && !targetobject.inVent)
                triggerPlayer = tag.gameObject.GetComponent<PlayerControl>();
        }
        private void FixedUpdate()
        {
            if (this.owner == PlayerControl.LocalPlayer && this.triggerPlayer != null && this.triggerPlayer != this.owner)
            {
                MurderBypass.RpcMurder(this.owner, this.triggerPlayer);
                this.triggerPlayer = null;
            }
            if (this.owner.Data.IsDead || MeetingHud.Instance != null || destoryMe)
            {
                //GameObject.Find(this.owner.gameObject.name).transform.Find("swordRotate(Clone)").gameObject.Destroy();
                this.transform.parent.transform.parent.gameObject.Destroy();
            }
            if (this.owner == PlayerControl.LocalPlayer)
            {
                var vectorToTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition) - this.transform.parent.transform.parent.transform.position;
                float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * (180 / (float)Math.PI);
                this.transform.parent.gameObject.GetComponent<SpriteRenderer>().flipY = angle < -90 || angle > 90;
                Quaternion q = Quaternion.Euler(0, 0, angle);
                this.transform.parent.transform.parent.transform.localRotation = q;
                Rpc<MoveSword>.Instance.Send((vectorToTarget.x, vectorToTarget.y, vectorToTarget.z, this.owner.gameObject.name));
            }
        }
        public void Attach()
        {
            this.transform.parent.transform.parent.SetParent(this.owner.transform);
            this.transform.parent.transform.parent.transform.localPosition = Vector3.zero;
        }
        public SwordMono(IntPtr ptr) : base(ptr)
        {
        }
    }
}
