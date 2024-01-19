using BepInEx;
using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Artifacts;
using System.Collections.Generic;
using System.Security;
using System.Security.Permissions;
using UnityEngine;

[module: UnverifiableCode]
#pragma warning disable CS0618 // Type or member is obsolete
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
#pragma warning restore CS0618 // Type or member is obsolete

namespace RoR2VoidExplosionResult
{
    [BepInPlugin("com.DestroyedClone.VoidDeathResult", "Void Death Result", "0.0.0")]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    public class Plugin : BaseUnityPlugin
    {
        public static ConfigEntry<bool> cfgPerformInvasion;

        public static Dictionary<string, string> transformPairs = new Dictionary<string, string>()
        {
            { "CommandoBody", "VoidSurvivorBody" }
        };
        public const string tokenTransform = "PLAYER_DEATH_QUOTE_VOIDDEATH_TRANSFORM";
        public const string tokenExtraLife = "PLAYER_DEATH_QUOTE_VOIDDEATH_EXTRALIFE";
        //public const string tokenPreviouslyExtraLife = "PLAYER_DEATH_QUOTE_VOIDDEATH_EXTRALIFE";

        public void Start()
        {
            cfgPerformInvasion = Config.Bind("", "Perform Invasion on Transform", true, "If true, then your Commando Umbra will spawn when transformed by this mod.");

            LanguageAPI.Add(tokenTransform+"_2P", "You have been escaped your imprisonment, but things are different.");
            LanguageAPI.Add(tokenTransform, "{0} has returned as a different form.");
            LanguageAPI.Add(tokenExtraLife+"_2P", "You have escaped your imprisonment, giving up something else.");
            LanguageAPI.Add(tokenExtraLife, "{0} has avoided imprisonment.");
            On.RoR2.Chat.SendBroadcastChat_ChatMessageBase += Chat_SendBroadcastChat_ChatMessageBase;
        }

        private void Chat_SendBroadcastChat_ChatMessageBase(On.RoR2.Chat.orig_SendBroadcastChat_ChatMessageBase orig, RoR2.ChatMessageBase message)
        {
            if (message is RoR2.Chat.PlayerDeathChatMessage deathMessage && deathMessage.subjectAsNetworkUser)
            {
                var hasExtraLife = deathMessage.subjectAsNetworkUser.master.inventory.GetItemCount(RoR2Content.Items.ExtraLife) > 0 || deathMessage.subjectAsNetworkUser.master.inventory.GetItemCount(DLC1Content.Items.ExtraLifeVoid) > 0;
                var isVoidDeath = deathMessage.baseToken == "PLAYER_DEATH_QUOTE_VOIDDEATH";
                if (isVoidDeath)
                {
                    if (hasExtraLife)
                    {
                        deathMessage.baseToken = tokenExtraLife;
                    } else
                    {
                        if (deathMessage.subjectAsNetworkUser.NetworkbodyIndexPreference == BodyCatalog.FindBodyIndex("CommandoBody"))
                        {
                            if (cfgPerformInvasion.Value)
                            {
                                var currentInvasionCycle = Mathf.FloorToInt(Run.instance.GetRunStopwatch() / 600);
                                DoppelgangerInvasionManager.CreateDoppelganger(deathMessage.subjectAsNetworkUser.master, new Xoroshiro128Plus(Run.instance.seed + (ulong)(long)currentInvasionCycle));
                            }
                            deathMessage.subjectAsNetworkUser.master.TransformBody("VoidSurvivorBody");
                            deathMessage.baseToken = tokenTransform;
                        }
                    }
                }
            }
            orig(message);
        }
    }
}