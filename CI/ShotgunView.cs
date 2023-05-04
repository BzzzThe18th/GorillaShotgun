using System;
using ComputerInterface;
using ComputerInterface.ViewLib;
using UnityEngine;

namespace GorillaShotgun.CI
{
    public class ShotgunView : ComputerView
    {
        public static ShotgunView instance;
        private readonly UISelectionHandler selectionHandler;
        const string yellow = "ffff00";

        public ShotgunView()
        {
            instance = this;

            selectionHandler = new UISelectionHandler(EKeyboardKey.Up, EKeyboardKey.Down, EKeyboardKey.Enter);
            selectionHandler.MaxIdx = 3;
            selectionHandler.OnSelected += OnEntrySelected;
            selectionHandler.ConfigureSelectionIndicator($"<color=#{yellow}>></color> ", "", "  ", "");
        }

        public override void OnShow(object[] args)
        {
            base.OnShow(args);
            ViewUpdate();
        }

        public void ViewUpdate()
        {
            SetText(s =>
            {
                s.BeginCenter();
                s.MakeBar('=', SCREEN_WIDTH, 0, "ffff0010");
                s.AppendClr("Gorilla Shotgun", yellow).EndColor().AppendLine();
                s.Append("Made by Buzz Bzzz bzz BZZZ The 18th#0431\nand ojsauce#4400\n");
                s.MakeBar('=', SCREEN_WIDTH, 0, "ffff0010");
                s.EndAlign().AppendLines(1);

                s.AppendLine(selectionHandler.GetIndicatedText(0, Config.ShotgunConfig.enabled.Value ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>"));
                s.AppendLines(1);

                s.AppendClr("Hand Mode", yellow).EndColor().AppendLine();
                s.AppendLine(selectionHandler.GetIndicatedText(1, Config.ShotgunConfig.isLeft.Value ? "LEFT" : "RIGHT"));

                s.AppendClr("Force", yellow).EndColor().AppendLine();
                if (Config.ShotgunConfig.force.Value == 1) s.AppendLine(selectionHandler.GetIndicatedText(2, Config.ShotgunConfig.force.Value.ToString() + " (DEFAULT)"));
                else s.AppendLine(selectionHandler.GetIndicatedText(2, Config.ShotgunConfig.force.Value.ToString()));

                s.AppendClr("Volume", yellow).EndColor().AppendLine();
                if (Config.ShotgunConfig.volume.Value == 5) s.AppendLine(selectionHandler.GetIndicatedText(3, Config.ShotgunConfig.volume.Value.ToString() + " (DEFAULT)"));
                else s.AppendLine(selectionHandler.GetIndicatedText(3, Config.ShotgunConfig.volume.Value.ToString()));
            });
        }

        private void OnEntrySelected(int index)
        {
            try
            {
                switch (index)
                {
                    case 0:
                        Config.ShotgunConfig.enabled.Value = !Config.ShotgunConfig.enabled.Value;
                        if (Config.ShotgunConfig.enabled.Value)
                            Plugin.shotgunParent.SetActive(true);
                        if (!Config.ShotgunConfig.enabled.Value)
                            Plugin.shotgunParent.SetActive(false);
                        ViewUpdate();
                        break;
                    case 1:
                        Config.ShotgunConfig.enabled.Value = !Config.ShotgunConfig.enabled.Value;
                        if (Config.ShotgunConfig.enabled.Value)
                            Plugin.shotgunParent.SetActive(true);
                        if (!Config.ShotgunConfig.enabled.Value)
                            Plugin.shotgunParent.SetActive(false);
                        ViewUpdate();
                        break;

                }
            }
            catch(Exception e) { Debug.Log(e); }
        }

        public override void OnKeyPressed(EKeyboardKey key)
        {
            if (selectionHandler.HandleKeypress(key))
            {
                ViewUpdate();
                return;
            }

            switch (selectionHandler.CurrentSelectionIndex)
            {
                case 1:
                    switch(key)
                    {
                        case EKeyboardKey.Left:
                            Config.ShotgunConfig.isLeft.Value = !Config.ShotgunConfig.isLeft.Value;
                            Behaviours.ShotgunManager.instance?.UpdateInfo();
                            break;
                        case EKeyboardKey.Right:
                            Config.ShotgunConfig.isLeft.Value = !Config.ShotgunConfig.isLeft.Value;
                            Behaviours.ShotgunManager.instance?.UpdateInfo();
                            break;
                    }
                    ViewUpdate();
                    break;
                case 2:
                    switch(key)
                    {
                        case EKeyboardKey.Left:
                            Behaviours.ShotgunManager.instance.ChangeForce(true);
                            break;
                        case EKeyboardKey.Right:
                            Behaviours.ShotgunManager.instance.ChangeForce(false);
                            break;
                    }
                    ViewUpdate();
                    break;
                case 3:
                    switch (key)
                    {
                        case EKeyboardKey.Left:
                            Behaviours.ShotgunManager.instance.ChangeVolume(true);
                            break;
                        case EKeyboardKey.Right:
                            Behaviours.ShotgunManager.instance.ChangeVolume(false);
                            break;
                    }
                    ViewUpdate();
                    break;
            }

            switch (key)
            {
                case EKeyboardKey.Back:
                    ReturnToMainMenu();
                    break;
                case EKeyboardKey.Up:
                    selectionHandler.MoveSelectionUp();
                    ViewUpdate();
                    break;
                case EKeyboardKey.Down:
                    selectionHandler.MoveSelectionDown();
                    ViewUpdate();
                    break;
            }
        }
    }
}
