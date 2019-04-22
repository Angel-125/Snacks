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
    public class SoilRecycler : SnackProcessor
    {
        [KSPField(isPersistant = true)]
        public int RecyclerCapacity;

        protected double baseInputEfficiency = 1.0f;

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Fields["dailyOutput"].guiName = "Max Recycling";
        }

        protected override void updateSettings()
        {
            //Recyclers are calibrated for 1 snack per meal, 1 meal per day, at 100% efficiency.
            //For resource converters, Efficiency is a production rate multiplier.
            //We want the total recycler output, which is based on snacks per meal, meals per day, and recycler capacity.
            updateProductionEfficiency();

            dailyOutput = string.Format("{0:f2} Soil/day", GetDailySnacksOutput());
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
            int specialistBonus = HasSpecialist(this.ExperienceEffect);
            float crewEfficiencyBonus = 1.0f;

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
