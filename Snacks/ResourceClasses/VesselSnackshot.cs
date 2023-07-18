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

using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Snacks
{
    public class VesselSnackshot
    {
        /// <summary>
        /// ID of the celestial body where the vessel is located.
        /// </summary>
        public int bodyID;

        /// <summary>
        /// Name of the vessel
        /// </summary>
        public string vesselName;

        /// <summary>
        /// Current number of crew in the vessel
        /// </summary>
        public int crewCount;

        /// <summary>
        /// Max number of crew in the vessel
        /// </summary>
        public int maxCrewCount;

        /// <summary>
        /// Reference to the vessel itself
        /// </summary>
        public Vessel vessel;

        /// <summary>
        /// List of resource snapshots
        /// </summary>
        public List<Snackshot> snackshots;

        /// <summary>
        /// Flag to indicate that the simulator couldn't determine if the converters were active, so it assumed that they were.
        /// </summary>
        public bool convertersAssumedActive;

        /// <summary>
        /// Returns the status of the vessel and its resources.
        /// </summary>
        /// <param name="showCrewView">A flag to indicate whether to show crew status instead of vessel resource status.</param>
        /// <returns>A string containing the vessel's status.</returns>
        public virtual string GetStatusDisplay(bool showCrewView = false)
        {
            StringBuilder status = new StringBuilder();

            //Vessel name
            status.AppendLine("<color=white><b>" + vesselName + "</b></color>");

            //Crew capacity
            if (!vessel.isEVA)
                status.AppendLine("<color=white> "+Localizer.Format("#LOC_GUI_CREW") + crewCount + "/" + maxCrewCount + "</color>");

            //Resource snapshots
            if (!showCrewView)
            {
                int count = snackshots.Count;
                for (int snapshotIndex = 0; snapshotIndex < count; snapshotIndex++)
                {
                    status.AppendLine(snackshots[snapshotIndex].GetStatusDisplay());
                }
            }
            else
            {
                ProtoCrewMember[] astronauts;
                if (vessel.loaded)
                    astronauts = vessel.GetVesselCrew().ToArray();
                else
                    astronauts = vessel.protoVessel.GetVesselCrew().ToArray();
                AstronautData astronautData;
                string conditionSummary;

                for (int index = 0; index < astronauts.Length; index++)
                {
                    astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[index]);

                    status.AppendLine("<color=orange><i>" + astronauts[index].name + "</i></color>");
                    if (!string.IsNullOrEmpty(astronautData.conditionSummary))
                        conditionSummary = astronautData.conditionSummary;
                    else
                        conditionSummary = Localizer.Format("#LOC_AOK");//#LOC_AOK=A-OK
                    status.AppendLine("<color=white> "+Localizer.Format("#LOC_GUI_STATUS") + conditionSummary + "</color>");

                    string[] keys = astronautData.rosterResources.Keys.ToArray();
                    for (int rosterIndex = 0; rosterIndex < keys.Length; rosterIndex++)
                    {
                        if (astronautData.rosterResources[keys[rosterIndex]].showInSnapshot)
                        {
                            conditionSummary = astronautData.rosterResources[keys[rosterIndex]].GetStatusDisplay();
                            if (!string.IsNullOrEmpty(conditionSummary))
                                status.AppendLine(conditionSummary);
                        }
                    }
                }
            }

            return status.ToString();
        }

        public VesselSnackshot()
        {
            snackshots = new List<Snackshot>();
        }
    }
}
