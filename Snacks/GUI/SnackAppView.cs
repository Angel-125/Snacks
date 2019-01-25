using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP.UI.Screens;

namespace Snacks
{
    public class SnackAppView : Window<SnackAppView>
    {
        private Vector2 scrollPos = new Vector2();
        public string exemptKerbals = "Ted";

        public SnackAppView() :
        base("Snack Supply", 300, 300)
        {
            Resizable = false;
        }

        public override void SetVisible(bool newValue)
        {
            base.SetVisible(newValue);

            if (newValue)
            {
                exemptKerbals = SnacksScenario.Instance.exemptKerbals;
                SnacksScenario.Instance.SetExemptCrew(exemptKerbals);
            }

            else
            {
                SnacksScenario.Instance.exemptKerbals = exemptKerbals;

            }
        }

        protected double DaysPerYear
        {
            get
            {
                //if (GameSettings.KERBIN_TIME)
                //    return 426.08f;
                //else
                //    return 365f;
                return (KSPUtil.dateTimeFormatter.Year / KSPUtil.dateTimeFormatter.Day);
            }
        }

        protected double DaysPerMonth
        {
            get
            {
                if (GameSettings.KERBIN_TIME)
                    return 6.43f;
                else
                    return 30.41666666666667f;
            }
        }

        double HoursPerDay
        {
            get
            {
                //if (GameSettings.KERBIN_TIME)
                //    return 6f;
                //else
                //    return 24f;
                return (KSPUtil.dateTimeFormatter.Day / KSPUtil.dateTimeFormatter.Hour);
            }
        }

        protected string timeFormat(int days)
        {
            StringBuilder timeBuilder = new StringBuilder();
            double timeDays = days;
            double years;

            years = Math.Floor(timeDays / DaysPerYear);
            if (years >= 1.0)
            {
                timeDays -= years * DaysPerYear;
                if (years > 1.0)
                    timeBuilder.AppendFormat("{0:f0} years", years);
                else
                    timeBuilder.Append("1 year");
                timeBuilder.Append(", ");
            }
            else
            {
                years = 0;
            }

            if (timeDays >= 1.0f)
            {
                int intDays = Convert.ToInt32(timeDays);
                timeBuilder.Append(intDays.ToString() + " days");
            }

            return timeBuilder.ToString();
        }

        protected override void DrawWindowContents(int windowId)
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER)
                drawSpaceCenterWindow();
            else if (HighLogic.LoadedSceneIsEditor == false)
                drawFlightWindow();
            else
                drawEditorWindow();
        }

        public void drawEditorWindow()
        {
            ShipSupply supply = SnackSnapshot.Instance().TakeEditorSnapshot();

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300), GUILayout.Width(300));

            if (!GameSettings.KERBIN_TIME)
                GUILayout.Label("<color=lightblue>Time format: 24hr/day, 365 day/year</color>");

            if (SnacksProperties.EnableRandomSnacking || SnacksProperties.RecyclersEnabled)
                GUILayout.Label("<color=yellow>The following are estimates</color>");

            GUILayout.Label("<color=white>Snacks: " + supply.SnackAmount + "/" + supply.SnackMaxAmount + "</color>");
            //GUILayout.Label("<color=white>Days (Cur Crew): " + supply.DayEstimate + "</color>");
            //GUILayout.Label("<color=white>Days (Max Crew): " + supply.MaxDayEstimate + "</color>");

            if (supply.DayEstimate > 0)
            {
                GUILayout.Label("<color=white>Current Crew: " + supply.CrewCount + "</color>");
                GUILayout.Label("<color=white>Duration: " + timeFormat(supply.DayEstimate) + "</color>");
            }
            else
            {
                GUILayout.Label("<color=white>Current Crew: " + supply.CrewCount + "</color>");
                GUILayout.Label("<color=white>Duration: Indefinite</color>");
            }

            if (supply.MaxDayEstimate > 0)
            {
                GUILayout.Label("<color=white>Max Crew: " + supply.MaxCrewCount + "</color>");
                GUILayout.Label("<color=white>Duration: " + timeFormat(supply.MaxDayEstimate) + "</color>");
            }
            else
            {
                GUILayout.Label("<color=white>Max Crew: " + supply.MaxCrewCount + "</color>");
                GUILayout.Label("<color=white>Duration: Indefinite</color>");
            }


            GUILayout.EndScrollView();
        }

        public void drawSpaceCenterWindow()
        {
            GUILayout.Label("<color=white><b>Exempt Kerbals:</b> separate names by semicolon, first name only</color>");
            GUILayout.Label("<color=yellow>These kerbals won't consume Snacks and won't suffer penalties from a lack of Snacks.</color>");
            if (string.IsNullOrEmpty(exemptKerbals))
                exemptKerbals = string.Empty;
            exemptKerbals = GUILayout.TextField(exemptKerbals);

            if (SnackController.debugMode)
            {
                if (GUILayout.Button("Snack Time!"))
                    SnackController.Instance.EatSnacks();
            }

            drawFlightWindow();
        }

        public void drawFlightWindow()
        {
            Dictionary<int, List<ShipSupply>> snapshot = SnackSnapshot.Instance().TakeSnapshot();
            var keys = snapshot.Keys.ToList();
            List<ShipSupply> supplies;

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(300), GUILayout.Width(300));
            keys.Sort();
            foreach (int planet in keys)
            {
                if (!snapshot.TryGetValue(planet, out supplies))
                {
                    GUILayout.Label("Can't seem to get supplies");
                    GUILayout.EndScrollView();
                }

                if (!GameSettings.KERBIN_TIME)
                    GUILayout.Label("<color=lightblue>Time format: 24hr/day, 365 day/year</color>");

                if (SnacksProperties.EnableRandomSnacking)
                {
                    GUILayout.Label("<color=yellow>The following are estimates</color>");
                }

                GUILayout.Label("<color=lightblue><b>" + supplies.First().BodyName + ":</b></color>");
                foreach (ShipSupply supply in supplies)
                {
                    if (supply.DayEstimate < 0)
                    {
                        GUILayout.Label("<color=white><b>" + supply.VesselName + "</b></color>");
                        GUILayout.Label("<color=white> Crew: " + supply.CrewCount + ", Snacks: " + supply.SnackAmount + "/" + supply.SnackMaxAmount + "</color>");
                        GUILayout.Label("<color=white> Duration: Indefinite</color>");
                    }
                    else if (supply.Percent > 50)
                    {
                        GUILayout.Label("<color=white><b>" + supply.VesselName + "</b></color>");
                        GUILayout.Label("<color=white> Crew: " + supply.CrewCount + ", Snacks: " + supply.SnackAmount + "/" + supply.SnackMaxAmount + "</color>");
                        GUILayout.Label("<color=white> Duration: " + timeFormat(supply.DayEstimate) + "</color>");
                    }
                    else if (supply.Percent > 25)
                    {
                        GUILayout.Label("<color=yellow><b>" + supply.VesselName + "</b></color>");
                        GUILayout.Label("<color=yellow> Crew: " + supply.CrewCount + ", Snacks: " + supply.SnackAmount + "/" + supply.SnackMaxAmount + "</color>");
                        GUILayout.Label("<color=yellow> Duration: " + timeFormat(supply.DayEstimate) + "</color>");
                    }
                    else
                    {
                        GUILayout.Label("<color=red><b>" + supply.VesselName + "</b></color>");
                        GUILayout.Label("<color=red> Crew: " + supply.CrewCount + ", Snacks: " + supply.SnackAmount + "/" + supply.SnackMaxAmount + "</color>");
                        GUILayout.Label("<color=red> Duration: " + timeFormat(supply.DayEstimate) + "</color>");
                    }
                }
            }

            GUILayout.EndScrollView();
        }
    }
}
