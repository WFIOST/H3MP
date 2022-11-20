using FistVR;
using HarmonyLib;

public static class FVRViveHand_Hooks
{
    [HarmonyPatch(typeof(FVRViveHand), "PollInput")]
    [HarmonyPrefix]
    public static bool PollIfNotPhantom(FVRViveHand __instance)
    {
        return __instance.transform == GM.CurrentPlayerBody.LeftHand &&
               __instance.transform == GM.CurrentPlayerBody.RightHand;
    }
}
