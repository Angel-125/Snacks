using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP.UI.Screens;

/**
The MIT License (MIT)
Copyright (c) 2014-2019 by Michael Billard
 

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 * */
namespace Snacks
{
    [KSPAddon(KSPAddon.Startup.Flight | KSPAddon.Startup.EveryScene, false)]
    public class SnackApp : MonoBehaviour
    {
        static protected Texture2D appIcon = null;
        static protected ApplicationLauncherButton appLauncherButton = null;
        static protected SnackAppView snackView;

        public void Awake()
        {
            snackView = new SnackAppView();
            appIcon = GameDatabase.Instance.GetTexture("WildBlueIndustries/Snacks/Textures/snacks", false);
            GameEvents.onGUIApplicationLauncherReady.Add(SetupGUI);
        }

        public void OnDestroy()
        {
            snackView.SetVisible(false);
            GameEvents.onGUIApplicationLauncherReady.Remove(SetupGUI);
        }

        public void OnGUI()
        {
            if (snackView.IsVisible())
                snackView.DrawWindow();
        }

        private void SetupGUI()
        {
            if (HighLogic.LoadedSceneIsFlight || HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedSceneIsEditor)
            {
                if (appLauncherButton == null)
                    appLauncherButton = ApplicationLauncher.Instance.AddModApplication(ToggleGUI, ToggleGUI, null, null, null, null, ApplicationLauncher.AppScenes.ALWAYS, appIcon);
            }
            else if (appLauncherButton != null)
                ApplicationLauncher.Instance.RemoveModApplication(appLauncherButton);
        }

        private void ToggleGUI()
        {
            snackView.SetVisible(!snackView.IsVisible());
        }
    }
}
