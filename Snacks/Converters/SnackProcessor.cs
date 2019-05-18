﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

/**
The MIT License (MIT)
Copyright (c) 2014-2019 by Michael Billard
Original concept by Troy Gruetzmacher

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
    public class SnackProcessor : SnacksConverter
    {
        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Max Production")]
        public string dailyOutput = string.Empty;

        [KSPField(isPersistant = true)]
        protected double originalSnacksRatio;

        protected double productionEfficiency = 0f;

        public virtual double GetDailySnacksOutput()
        {
            updateProductionEfficiency();

            return originalSnacksRatio * productionEfficiency * SnacksScenario.GetHoursPerDay();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);
            GameEvents.OnGameSettingsApplied.Add(updateSettings);

            ResourceRatio ratio;
            for (int index = 0; index < outputList.Count; index++)
            {
                ratio = outputList[index];

                if (ratio.ResourceName == SnacksProperties.SnacksResourceName)
                    originalSnacksRatio = ratio.Ratio;
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

            dailyOutput = string.Format("{0:f2} Snacks/day", GetDailySnacksOutput());
        }

        protected override void PreProcessing()
        {
            int specialistBonus = HasSpecialist(this.ExperienceEffect);
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
                infoBuilder.AppendLine("<color=#7FFF00><b>Inputs</b></color>");
                for (int index = 0; index < resourceCount; index++)
                {
                    resourceName = definitions[inputList[index].ResourceName].displayName;
                    infoBuilder.AppendLine(" -" + resourceName);
                }
            }

            resourceCount = outputList.Count;
            if (resourceCount > 0)
            {
                infoBuilder.AppendLine("<color=#7FFF00><b>Outputs</b></color>");
                for (int index = 0; index < resourceCount; index++)
                {
                    resourceName = definitions[outputList[index].ResourceName].displayName;
                    infoBuilder.AppendLine(" -" + resourceName);
                }
            }
            infoBuilder.AppendLine(" ");
            infoBuilder.AppendLine("<b>Note: </b> Production rates vary depending upon game settings. Consult the Part Action Window for details.");

            if (!string.IsNullOrEmpty(ExperienceEffect))
            {
                List<string> traits;
                Experience.ExperienceSystemConfig config = new Experience.ExperienceSystemConfig();
                config.LoadTraitConfigs();
                traits = config.GetTraitsWithEffect(ExperienceEffect);

                if (traits != null)
                {
                    int traitCount = traits.Count;
                    StringBuilder traitBuilder = new StringBuilder();

                    for (int index = 0; index < traitCount; index++)
                        traitBuilder.Append(traits[index] + ",");
                    char[] charsToTrim = {','};
                    string traitList = traitBuilder.ToString().TrimEnd(charsToTrim);

                    infoBuilder.AppendLine(" ");
                    infoBuilder.AppendLine("<b>Kerbal(s) that improve production: </b>");
                    infoBuilder.AppendLine(traitList);
                }
            }

            return infoBuilder.ToString();
        }
    }
}