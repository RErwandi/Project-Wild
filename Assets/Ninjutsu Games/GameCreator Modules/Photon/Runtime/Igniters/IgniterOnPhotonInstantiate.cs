namespace NJG.PUN
{
    using GameCreator.Core;
    using GameCreator.Variables;
    using Photon.Pun;
    using Photon.Realtime;
    using System.Collections.Generic;
    using UnityEngine;

    [AddComponentMenu("")]
    public class IgniterOnPhotonInstantiate : Igniter, IPunInstantiateMagicCallback
    {
#if UNITY_EDITOR
        public new static string NAME = "Photon/On Photon Instantiate";
        public new static bool REQUIRES_COLLIDER = false;
        public new static string ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Igniters/";
        public const string CUSTOM_ICON_PATH = "Assets/Ninjutsu Games/GameCreator Modules/Photon/Icons/Igniters/";
#endif
        [HideInInspector]
        public bool executed = false;

        [Space]
        [VariableFilter(Variable.DataType.GameObject)]
        public VariableProperty storeOwner = new VariableProperty(Variable.VarType.GlobalVariable);

        public void ManualExecute(GameObject invoker, PhotonMessageInfo info)
        {
            if (!executed)
            {
                if (storeOwner != null) storeOwner.Set((GameObject)info.Sender.TagObject, gameObject);
                ExecuteTrigger(invoker == null ? gameObject : invoker);
                executed = true;
            }
        }

        public void OnPhotonInstantiate(PhotonMessageInfo info)
        {
            if (storeOwner != null) storeOwner.Set((GameObject)info.Sender.TagObject, gameObject);
            ExecuteTrigger(gameObject);
            executed = true;
        }
    }
}