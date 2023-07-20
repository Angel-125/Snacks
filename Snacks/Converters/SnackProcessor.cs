using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;
using KSP.Localization;

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
    /// <summary>
    /// The SnacksProcessor grinds out Snacks from Ore. It is derived from the SnacksConverter. The output of the
    /// processor is affected by the game settings.
    /// </summary>
    public class SnackProcessor : SnacksConverter
    {
        /// <summary>
        /// A status field showing the daily output of Snacks.
        /// </summary>
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max Production")]
        public string dailyOutput = string.Empty;

        /// <summary>
        /// Helper field describing the original output ratio of Snacks.
        /// </summary>
        [KSPField(isPersistant = true)]
        public double originalSnacksRatio;

        /// <summary>
        /// Helper field to describe the original input ratio of Ore
        /// </summary>
        [KSPField(isPersistant = true)]
        public double sourceInputRatio;

        protected double productionEfficiency = 0f;

        /// <summary>
        /// Gets the daily snacks output.
        /// </summary>
        /// <returns>The amount of Snacks produced daily, subjected to game settings.</returns>
        public virtual double GetDailySnacksOutput()
        {
            updateProductionEfficiency();

            return originalSnacksRatio * productionEfficiency * SnacksScenario.GetSecondsPerDay();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            GameEvents.OnGameSettingsApplied.Add(updateSettings);
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;

            ResourceRatio ratio;
            for (int index = 0; index < outputList.Count; index++)
            {
                ratio = outputList[index];

                if (ratio.ResourceName == SnacksProperties.SnacksResourceName)
                {
                    originalSnacksRatio = ratio.Ratio;
                    break;
                }
            }
            for (int index = 0; index < inputList.Count; index++)
            {
                ratio = inputList[index];

                if (ratio.ResourceName == "Ore")
                {
                    sourceInputRatio = ratio.Ratio;
                    break;
                }
                else if (!definitions.Contains(ratio.ResourceName))
                {
                    DisableModule();
                    return;
                }
            }

            //Now set up the processor
            updateSettings();
        }

        public virtual void Destroy()
        {
            GameEvents.OnGameSettingsApplied.Remove(updateSettings);
        }

        protected virtual void updateProductionEfficiency()
        {
            productionEfficiency = SnacksProperties.ProductionEfficiency / 100.0f;
        }

        protected virtual void updateSettings()
        {
            updateProductionEfficiency();

            dailyOutput = Localizer.Format("#LOC_GUI_SNACKPERDAY", GetDailySnacksOutput().ToString("f2"));
        }

        protected override void PreProcessing()
        {
            int specialistBonus = 0;
            if (HighLogic.LoadedSceneIsFlight)
                specialistBonus = HasSpecialist(this.ExperienceEffect);

            float crewEfficiencyBonus = 1.0f;

            if (specialistBonus > 0)
                crewEfficiencyBonus = 1.0f + (SpecialistBonusBase + (1.0f + (float)specialistBonus) * SpecialistEfficiencyFactor);

            //Update the inputEfficiency and outputEfficiency here.
            if (productionEfficiency <= 0)
                updateProductionEfficiency();
            inputEfficiency = crewEfficiencyBonus;
            outputEfficiency = productionEfficiency * crewEfficiencyBonus;
        }

        public override string GetInfo()
        {
            StringBuilder infoBuilder = new StringBuilder();
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            string resourceName;

            int resourceCount = inputList.Count;
            if (resourceCount > 0)
            {
                infoBuilder.AppendLine(Localizer.Format("#LOC_INFO_INPUTS"));//#LOC_INFO_INPUTS="<color=#7FFF00><b>Inputs</b></color>"
                for (int index = 0; index < resourceCount; index++)
                {
                    if (!definitions.Contains(inputList[index].ResourceName))
                        continue;
                    resourceName = definitions[inputList[index].ResourceName].displayName;
                    infoBuilder.AppendLine(" -" + resourceName);
                }
            }

            resourceCount = outputList.Count;
            if (resourceCount > 0)
            {
                infoBuilder.AppendLine(Localizer.Format("#LOC_INFO_OUTPUTS"));//#LOC_INFO_OUTPUTS="<color=#7FFF00><b>Outputs</b></color>"
                for (int index = 0; index < resourceCount; index++)
                {
                    if (!definitions.Contains(outputList[index].ResourceName))
                        continue;
                    resourceName = definitions[outputList[index].ResourceName].displayName;
                    infoBuilder.AppendLine(" -" + resourceName);
                }
            }
            infoBuilder.AppendLine(" ");
            infoBuilder.AppendLine(Localizer.Format("#LOC_INFO_NOTEPRODUCTIONRATES"));
            //#LOC_INFO_NOTEPRODUCTIONRATES="<b>Note: </b> Production rates vary depending upon game settings. Consult the Part Action Window for details."

            if (!string.IsNullOrEmpty(ExperienceEffect))
            {
                List<string> traits;
                traits = GameDatabase.Instance.ExperienceConfigs.GetTraitsWithEffect(ExperienceEffect);

                if (traits != null)
                {
                    int traitCount = traits.Count;
                    StringBuilder traitBuilder = new StringBuilder();

                    for (int index = 0; index < traitCount; index++)
                        traitBuilder.Append(traits[index] + ",");
                    char[] charsToTrim = {','};
                    string traitList = traitBuilder.ToString().TrimEnd(charsToTrim);

                    infoBuilder.AppendLine(" ");
                    infoBuilder.AppendLine(Localizer.Format("#LOC_INFO_NOTEIMPROVE"));//#LOC_INFO_NOTEIMPROVE="<b>Kerbal(s) that improve production: </b>"
                    infoBuilder.AppendLine(traitList);
                }
            }

            return infoBuilder.ToString();
        }
    }
}
