using BepInEx;
using HarmonyLib;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using Utilla;
using HoneyUtils = HoneyLib.Utils;
using Bepinject;

namespace GorillaShotgun
{
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.6.7")]
    [BepInDependency("tonimacaroni.computerinterface", "1.5.4")]
    [BepInDependency("com.buzzbzzzbzzbzzzthe18th.gorillatag.HoneyLib", "1.0.4")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool inRoom;
        public static AudioClip fire;
        public static AudioClip load;
        public static GameObject shotgunParent;
        public static MeshRenderer emptyRenderer;

        void Awake()
        {
            Events.GameInitialized += OnGameInitialized;
            Zenjector.Install<CI.ShotgunInstaller>().OnProject();
            new Harmony(PluginInfo.GUID).PatchAll(Assembly.GetExecutingAssembly());
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            GorillaShotgun.Config.ShotgunConfig.LoadConfig();
            AssetBundle bundle = HoneyUtils.EasyAssetLoading.LoadBundle(Assembly.GetExecutingAssembly(), "GorillaShotgun.Assets.gorillashotgun");

            shotgunParent = GameObject.Instantiate(bundle.LoadAsset<GameObject>("beegunparent"));
            emptyRenderer = bundle.LoadAsset<GameObject>("empty").GetComponent<MeshRenderer>();
            fire = bundle.LoadAsset<AudioClip>("shotgunfire");
            load = bundle.LoadAsset<AudioClip>("shotgunload");
            shotgunParent.transform.GetChild(0).gameObject.AddComponent<Behaviours.ShotgunManager>();
        }

        void FixedUpdate()
        {
            HoneyUtils.EasyInput.UpdateInput();

            if (Behaviours.ShotgunManager.instance != null)
            {
                if (GorillaShotgun.Config.ShotgunConfig.enabled.Value)
                {
                    if (GorillaShotgun.Config.ShotgunConfig.isLeft.Value ? HoneyUtils.EasyInput.LeftGrip : HoneyUtils.EasyInput.RightGrip) shotgunParent?.SetActive(true);
                    else if(!Behaviours.ShotgunManager.instance.doingSomething) shotgunParent?.SetActive(false);
                }
                else if (!Behaviours.ShotgunManager.instance.doingSomething) shotgunParent?.SetActive(false);
            }
        }

        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            inRoom = true;
            if (CI.ShotgunView.instance != null)
                CI.ShotgunView.instance.ViewUpdate();
        }

        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            inRoom = false;
            if (CI.ShotgunView.instance != null)
                CI.ShotgunView.instance.ViewUpdate();
        }
    }
}
