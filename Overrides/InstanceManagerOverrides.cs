using Klyte.Commons.Extensions;
using Klyte.Commons.Utils;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace Klyte.Addresses.Overrides
{
    public class InstanceManagerOverrides : MonoBehaviour, IRedirectable
    {
        public Redirector RedirectorInstance { get; } = new Redirector();


        #region Events
        public delegate void OnBuildingNameChanged(ushort buildingID);
        public static event OnBuildingNameChanged EventOnBuildingRenamed;

#pragma warning disable IDE0051 // Remover membros privados não utilizados
        private static void OnInstanceRenamed(ref InstanceID id)
#pragma warning restore IDE0051 // Remover membros privados não utilizados
        {
            if (id.Building > 0)
            {
                CallBuildRenamedEvent(id.Building);
            }

        }
        #endregion

        #region Hooking

        public void Awake()
        {
            LogUtils.DoLog("Loading Instance Manager Overrides");
            #region Release Line Hooks
            MethodInfo posRename = typeof(InstanceManagerOverrides).GetMethod("OnInstanceRenamed", RedirectorUtils.allFlags);

            RedirectorInstance.AddRedirect(typeof(InstanceManager).GetMethod("SetName", RedirectorUtils.allFlags), null, posRename);
            #endregion

        }
        #endregion


        public static void CallBuildRenamedEvent(ushort building) => BuildingManager.instance.StartCoroutine(CallBuildRenamedEvent_impl(building));
        private static IEnumerator CallBuildRenamedEvent_impl(ushort building)
        {

            //returning 0 will make it wait 1 frame
            yield return new WaitForSeconds(1);


            //code goes here

            EventOnBuildingRenamed?.Invoke(building);
        }

    }
}
