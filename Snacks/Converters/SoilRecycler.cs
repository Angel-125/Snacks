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
    /// The SoilRecycler is designed to recycle Soil into Snacks. It is derived from SnacksProcessor (<see cref="T:Snacks.ClearCondition"/>),
    /// which is derived from SnacksConverter. SoilRecycler config nodes should be calibrated
    /// to turn 1 Soil into 1 Snacks; game settings will adjust the recycler based on desired difficulty.
    /// </summary>
    public class SoilRecycler : SnackProcessor
    {
        /// <summary>
        /// The number of kerbals that the recycler supports.
        /// </summary>
        [KSPField(isPersistant = true)]
        public int RecyclerCapacity;

        protected double baseInputEfficiency = 1.0f;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Fields["dailyOutput"].guiName = Localizer.Format("#LOC_GUI_MAXRECY");//#LOC_GUI_MAXRECY="Max Recycling"
        }

        public override string GetInfo()
        {
            StringBuilder infoBuilder = new StringBuilder();

            infoBuilder.AppendLine(base.GetInfo());
            infoBuilder.AppendLine(" ");
            infoBuilder.AppendLine(Localizer.Format("#LOC_INFO_SOILRECYCAPA", RecyclerCapacity));//#LOC_INFO_SOILRECYCAPA=<b>Recycler Capacity: <<1>> kerbals

            return infoBuilder.ToString();
        }

        protected override void updateSettings()
        {
            //Recyclers are calibrated for 1 snack per meal, 1 meal per day, at 100% efficiency.
            //For resource converters, Efficiency is a production rate multiplier.
            //We want the total recycler output, which is based on snacks per meal, meals per day, and recycler capacity.
            updateProductionEfficiency();

            dailyOutput = string.Format(Localizer.Format("#LOC_INFO_SOILPERDAY"), GetDailySnacksOutput());//#LOC_INFO_SOILPERDAY={0:f2} Soil/day
        }

        protected override void updateProductionEfficiency()
        {
            //Recyclers are calibrated for 1 snack per meal, 1 meal per day, at 100% efficiency.
            //For resource converters, we want the total recycler output, which is based on snacks per meal, meals per day, recycler capacity and recycler efficiency.
            baseInputEfficiency = SnacksProperties.SnacksPerMeal * SnacksProperties.MealsPerDay * RecyclerCapacity;
            productionEfficiency = baseInputEfficiency * (SnacksProperties.RecyclerEfficiency / 100.0f);
        }

        protected override void PreProcessing()
        {
            int specialistBonus = 0;
            if (HighLogic.LoadedSceneIsFlight)
                specialistBonus = HasSpecialist(this.ExperienceEffect);

            if (specialistBonus > 0)
                crewEfficiencyBonus = 1.0f + (SpecialistBonusBase + (1.0f + (float)specialistBonus) * SpecialistEfficiencyFactor);

            //Update the inputEfficiency and outputEfficiency here.
            if (productionEfficiency <= 0)
                updateProductionEfficiency();

            inputEfficiency = baseInputEfficiency * crewEfficiencyBonus;
            outputEfficiency = productionEfficiency * crewEfficiencyBonus;
        }
    }
}
