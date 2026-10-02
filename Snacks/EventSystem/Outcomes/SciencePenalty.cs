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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    /// <summary>
    /// This outcome disrupts science experiments aboard a vessel.
    /// Example definition:
    /// OUTCOME 
    /// {
    ///     name  = SciencePenalty
    /// }
    /// </summary>   

    public class SciencePenalty : BaseOutcome
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.SciencePenalty"/> class.
        /// </summary>
        /// <param name="node">A ConfigNode containing initialization parameters. Parameters in the
        /// <see cref="T:Snacks.BaseOutcome"/> class also apply.</param>
        public SciencePenalty(ConfigNode node) : base (node)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.SciencePenalty"/> class.
        /// </summary>
        /// <param name="canBeRandom">If set to <c>true</c> it can be randomly selected from the outcomes list.</param>
        public SciencePenalty(bool canBeRandom) : base(canBeRandom)
        {

        }
        #endregion


        #region Overrides
        public override bool IsEnabled()
        {
            return SnacksProperties.LoseScienceWhenHungry;
        }

        public override void ApplyOutcome(Vessel vessel, SnacksProcessorResult result)
        {
            if ((HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) && SnacksProperties.LoseScienceWhenHungry)
            {
                //If the vessel is loaded, apply the penalties.
                if (vessel.loaded)
                {
                    int count = vessel.vesselModules.Count;
                    SnacksVesselModule snacksVesselModule;
                    int sciencePenalties = 0;
                    for (int index = 0; index < count; index++)
                    {
                        if (vessel.vesselModules[index] is SnacksVesselModule)
                        {
                            snacksVesselModule = (SnacksVesselModule)vessel.vesselModules[index];
                            sciencePenalties = result.affectedKerbalCount + snacksVesselModule.sciencePenalties;
                            break;
                        }
                    }

                    //Apply Science penalties
                    for (int index = 0; index < sciencePenalties; index++)
                        applySciencePenalties(vessel);
                }

                //Not loaded, keep track of how many penalties we acquire
                else
                {
                    ScreenMessages.PostScreenMessage("Kerbals have ruined some science aboard the " + vessel.vesselName + "! Check the vessel for details.", 5f, ScreenMessageStyle.UPPER_LEFT);

                    ConfigNode node = vessel.protoVessel.vesselModules;
                    if (node.HasNode(SnacksVesselModule.SnacksVesselModuleNode))
                    {
                        node = node.GetNode(SnacksVesselModule.SnacksVesselModuleNode);

                        int sciencePenalties = 0;
                        if (node.HasValue(SnacksVesselModule.ValueSciencePenalties))
                        {
                            int.TryParse(node.GetValue(SnacksVesselModule.ValueSciencePenalties), out sciencePenalties);
                            sciencePenalties += result.affectedKerbalCount;
                            node.SetValue(SnacksVesselModule.ValueSciencePenalties, sciencePenalties);
                        }
                        else
                        {
                            sciencePenalties = result.affectedKerbalCount;
                            node.AddValue(SnacksVesselModule.ValueSciencePenalties, sciencePenalties);
                        }
                    }
                }
            }

            //Call the base class
            base.ApplyOutcome(vessel, result);
        }
        #endregion

        #region Helpers
        public static void CheckSciencePenalties(Vessel vessel)
        {
            //Apply science loss
            if ((HighLogic.CurrentGame.Mode == Game.Modes.CAREER || HighLogic.CurrentGame.Mode == Game.Modes.SCIENCE_SANDBOX) && SnacksProperties.LoseScienceWhenHungry)
            {
                if (vessel.loaded)
                {
                    int count = vessel.vesselModules.Count;
                    SnacksVesselModule snacksVesselModule;
                    int sciencePenalties = 0;
                    for (int index = 0; index < count; index++)
                    {
                        if (vessel.vesselModules[index] is SnacksVesselModule)
                        {
                            snacksVesselModule = (SnacksVesselModule)vessel.vesselModules[index];
                            sciencePenalties = snacksVesselModule.sciencePenalties;
                            snacksVesselModule.sciencePenalties = 0;
                            break;
                        }
                    }

                    //Apply all the penalties we acquired
                    for (int index = 0; index < sciencePenalties; index++)
                        applySciencePenalties(vessel);
                }
            }
        }

        protected static void applySciencePenalties(Vessel vessel)
        {
            List<ModuleScienceContainer> scienceContainers = vessel.FindPartModulesImplementing<ModuleScienceContainer>();
            List<ModuleScienceLab> scienceLabs = vessel.FindPartModulesImplementing<ModuleScienceLab>();
            List<ModuleScienceExperiment> scienceExperiments = vessel.FindPartModulesImplementing<ModuleScienceExperiment>();

            //If we have a science lab aboard, see if it has any data. If so then lose some data.
            if (scienceLabs.Count > 0)
            {
                ModuleScienceLab[] labs = scienceLabs.ToArray();
                for (int index = 0; index < labs.Length; index++)
                {
                    if (labs[index].dataStored > 0.001)
                    {
                        float dataLoss = labs[index].dataStored * SnacksProperties.DataLostWhenHungry;
                        labs[index].dataStored -= labs[index].dataStored - dataLoss;
                        ScreenMessages.PostScreenMessage(string.Format("Kerbals fat-fingered ongoing research and lost {0:f3} data in the {1:s}", dataLoss, labs[index].part.partInfo.title),
                            5, ScreenMessageStyle.UPPER_LEFT);
                        return;
                    }
                }
            }

            //If we have containers aboard, see if they have any stored experiment results. If so, lose one.
            if (scienceContainers.Count > 0)
            {
                ModuleScienceContainer[] containers = scienceContainers.ToArray();
                ScienceData[] dataEntries;
                string title;
                for (int index = 0; index < containers.Length; index++)
                {
                    dataEntries = containers[index].GetData();
                    if (dataEntries != null && dataEntries.Length > 0)
                    {
                        title = dataEntries[0].title;
                        containers[index].DumpData(dataEntries[0]);
                        ScreenMessages.PostScreenMessage("Kerbals fat-fingered the controls and lost data from the " + title + " experiment in the " + scienceContainers[index].part.partInfo.title,
                            5, ScreenMessageStyle.UPPER_LEFT);
                        return;
                    }
                }
            }

            //If there is a science experiment aboard that has data, then dump one of the results.
            if (scienceExperiments.Count > 0)
            {
                ModuleScienceExperiment[] experiments = scienceExperiments.ToArray();
                ScienceData[] dataEntries;
                for (int index = 0; index < experiments.Length; index++)
                {
                    if (experiments[index].Deployed)
                    {
                        dataEntries = experiments[index].GetData();
                        for (int dataIndex = 0; dataIndex < dataEntries.Length; dataIndex++)
                            experiments[index].DumpData(dataEntries[dataIndex]);
                        ScreenMessages.PostScreenMessage("Kerbals fat-fingered the controls and lost all data from the " + experiments[index].part.partInfo.title,
                            5, ScreenMessageStyle.UPPER_LEFT);
                    }
                }
            }
        }
        #endregion
    }
}
