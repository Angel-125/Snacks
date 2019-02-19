using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using KSP.IO;

namespace Snacks
{
    public enum RepLoss
    {
        Low,
        Medium,
        High
    }

    public enum FaintTime
    {
        OneMinute,
        FiveMinutes,
        TenMinutes,
        ThirtyMinutes,
        OneHour,
        TwoHours,
        OneDay
    }

    public class SnackPenalties : GameParameters.CustomParameterNode
    {
        [GameParameters.CustomParameterUI("Enable random penalties", toolTip = "If enabled, then one of the enabled penalties will be randomly chosen", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public bool enableRandomPenalties = true;

        [GameParameters.CustomParameterUI("Hungry kerbals hurt your reputation.", toolTip = "When kerbals go hungry, you lose Reputation", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public bool loseRepWhenHungry = true;

        [GameParameters.CustomParameterUI("Rep loss per kerbal per meal", toolTip = "How mad your kerbals will be", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public RepLoss repLostWhenHungry = RepLoss.Low;

        [GameParameters.CustomParameterUI("Hungry kerbals hurt your bottom line.", toolTip = "When kerbals go hungry, you lose Funds", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public bool loseFundsWhenHuntry = true;

        [GameParameters.CustomIntParameterUI("Fine per kerbal", maxValue = 50000, minValue = 1000, stepSize = 1000, toolTip = "How much is it gonna cost", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER)]
        public int finePerKerbal = 10000;

        [GameParameters.CustomParameterUI("Hungry kerbals ruin science.", toolTip = "When kerbals go hungry, they ruin stored experiment results and data", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE)]
        public bool loseScienceWhenHungry = true;

        [GameParameters.CustomParameterUI("Data lost", toolTip = "What percentage of lab data is lost", autoPersistance = true, gameMode = GameParameters.GameMode.CAREER | GameParameters.GameMode.SCIENCE)]
        public RepLoss dataLostWhenHungry = RepLoss.Low;

//        [GameParameters.CustomParameterUI("Hungry kerbals can't fly straight.", toolTip = "When kerbals go hungry, ships partialy lose control", autoPersistance = true)]
        public bool partialControlWhenHungry = true;

        [GameParameters.CustomParameterUI("Hungry kerbals can faint.", toolTip = "When kerbals go hungry, they might pass out and can't be controlled.", autoPersistance = true)]
        public bool faintWhenHungry = true;

        [GameParameters.CustomIntParameterUI("Meals before fainting", maxValue = 24, minValue = 1, stepSize = 1, toolTip = "How many meals can a kerbal miss before fainting", autoPersistance = true)]
        public int mealsBeforeFainting = 3;

        [GameParameters.CustomParameterUI("Nap time when fainted", toolTip = "How long will a kerbal nap for when they faint", autoPersistance = true)]
        public FaintTime napTime = FaintTime.OneMinute;

        [GameParameters.CustomParameterUI("Kerbals can starve to death.", toolTip = "If they skip too many meals, kerbals can starve to death.", autoPersistance = true)]
        public bool canStarveToDeath = false;

        [GameParameters.CustomIntParameterUI("Skipped meals before death", maxValue = 42, minValue = 1, stepSize = 1, toolTip = "How many meals can a kerbal miss before dying", autoPersistance = true)]
        public int mealsSkippedBeforeDeath = 42;

        #region CustomParameterNode

        public override string DisplaySection
        {
            get
            {
                return Section;
            }
        }

        public override string Section
        {
            get
            {
                return "Snacks!";
            }
        }

        public override string Title
        {
            get
            {
                return "Penalties";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 2;
            }
        }

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            base.SetDifficultyPreset(preset);
        }

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false;
            }
        }

        public override bool Enabled(System.Reflection.MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "repLoss" && loseRepWhenHungry)
                return true;
            else if (member.Name == "repLoss")
                return false;

            if (member.Name == "finePerKerbal" && loseFundsWhenHuntry)
                return true;
            else if (member.Name == "finePerKerbal")
                return false;

            if ((member.Name == "mealsBeforeFainting" || member.Name == "napTime") && faintWhenHungry)
                return true;
            else if (member.Name == "mealsBeforeFainting" || member.Name == "napTime")
                return false;

            if (member.Name == "mealsSkippedBeforeDeath" && canStarveToDeath)
                return true;
            else if (member.Name == "mealsSkippedBeforeDeath")
                return false;

            return true;
        }
        #endregion
    }

    public class SnacksProperties : GameParameters.CustomParameterNode
    {
        public static string SnacksResourceName = "Snacks";
        public static string SoilResourceName = "Soil";
        private static int defaultSnacksPerCmdPod  = 50;
        private static int defaultSnacksPerCrewPod = 200;

        [GameParameters.CustomIntParameterUI("Snacks per meal", minValue = 1, maxValue = 12, stepSize = 1, toolTip = "How much do kerbals snack on?", autoPersistance = true)]
        public int snacksPerMeal = 1;

        [GameParameters.CustomIntParameterUI("Meals per day", minValue = 1, maxValue = 12, stepSize = 1, toolTip = "How often do kerbals eat?", autoPersistance = true)]
        public int mealsPerDay = 3;

        [GameParameters.CustomParameterUI("Some kerbals eat more, some eat less", toolTip = "At snack time, kerbals eat random amounts of Snacks, and the time between snacking is also random.", autoPersistance = true)]
        public bool enableRandomSnacking = true;

        [GameParameters.CustomParameterUI("Enable recycling", toolTip = "Kerbals produce Soil when snacking. Recyclers convert the Soil back to Snacks at the cost of ElectricCharge.", autoPersistance = true)]
        public bool enableRecyclers = true;

        [GameParameters.CustomIntParameterUI("Recycler Efficiency %", minValue = 10, maxValue = 100, stepSize = 10, toolTip = "How well does the recycler recyle?", autoPersistance = true)]
        public int recyclerEfficiency2 = 40;

        [GameParameters.CustomIntParameterUI("Production Efficiency %", minValue = 10, maxValue = 100, stepSize = 10, toolTip = "How well does the processor produce snacks?", autoPersistance = true)]
        public int processorEfficiency = 100;

        #region Properties

        public static int MealsSkippedBeforeDeath
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.mealsSkippedBeforeDeath;
            }
        }

        public static bool CanStarveToDeath
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.canStarveToDeath;
            }
        }

        public static bool FaintWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.faintWhenHungry;
            }
        }

        public static int MealsBeforeFainting
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.mealsBeforeFainting;
            }
        }

        public static int NapTime
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                
                switch (penalties.napTime)
                {
                    case FaintTime.OneMinute:
                    default:
                        return 1;

                    case FaintTime.FiveMinutes:
                        return 5;

                    case FaintTime.TenMinutes:
                        return 10;

                    case FaintTime.ThirtyMinutes:
                        return 30;

                    case FaintTime.OneHour:
                        return 60;

                    case FaintTime.TwoHours:
                        return 120;

                    case FaintTime.OneDay:
                        //if (GameSettings.KERBIN_TIME)
                        //    return 360;
                        //else
                        //    return 1440;
                        return KSPUtil.dateTimeFormatter.Day;
                }
            }
        }

        public static int SnacksPerCrewModule
        {
            get
            {
                ConfigNode snackConfig = GameDatabase.Instance.GetConfigNode("SNACKS_SETTINGS");
                if (snackConfig == null)
                    return defaultSnacksPerCrewPod;

                if (snackConfig.HasValue("snacksPerCrewPod"))
                    return int.Parse(snackConfig.GetValue("snacksPerCrewPod"));
                else
                    return defaultSnacksPerCrewPod;
            }
        }

        public static int SnacksPerCommand
        {
            get
            {
                ConfigNode snackConfig = GameDatabase.Instance.GetConfigNode("SNACKS_SETTINGS");
                if (snackConfig == null)
                    return defaultSnacksPerCmdPod;

                if (snackConfig.HasValue("snacksPerCmdPod"))
                    return int.Parse(snackConfig.GetValue("snacksPerCmdPod"));
                else
                    return defaultSnacksPerCmdPod;
            }
        }

        public static float ProductionEfficiency
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return (float)snackProperties.processorEfficiency;
            }
        }

        public static int SnackResourceID
        {
            get
            {
                PartResourceDefinition snacksResource = PartResourceLibrary.Instance.GetDefinition(SnacksResourceName);
                return snacksResource.id;
            }
        }

        public static bool RandomPenaltiesEnabled
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.enableRandomPenalties;
            }
        }

        public static bool LoseScienceWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.loseScienceWhenHungry;
            }
        }

        public static float DataLostWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();

                switch (penalties.dataLostWhenHungry)
                {
                    case RepLoss.Low:
                    default:
                        return 0.05f;

                    case RepLoss.Medium:
                        return 0.1f;

                    case RepLoss.High:
                        return 0.2f;
                }
            }
        }

        public static bool PartialControlWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.partialControlWhenHungry;
            }
        }

        public static bool RecyclersEnabled
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.enableRecyclers;
            }
        }

        public static float RecyclerEfficiency
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                if (snackProperties.recyclerEfficiency2 < 10)
                    snackProperties.recyclerEfficiency2 = 40;
                return snackProperties.recyclerEfficiency2 / 100.0f;
            }
        }

        public static bool EnableRandomSnacking
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.enableRandomSnacking;
            }
        }

        public static double SnacksPerMeal
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.snacksPerMeal;
            }
        }

        public static int MealsPerDay
        {
            get
            {
                SnacksProperties snackProperties = HighLogic.CurrentGame.Parameters.CustomParams<SnacksProperties>();
                return snackProperties.mealsPerDay;
            }
        }

        public static bool LoseFundsWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.loseFundsWhenHuntry;
            }
        }

        public static double FinePerKerbal
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.finePerKerbal;
            }
        }

        public static bool LoseRepWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                return penalties.loseRepWhenHungry;
            }
        }

        public static double RepLostWhenHungry
        {
            get
            {
                SnackPenalties penalties = HighLogic.CurrentGame.Parameters.CustomParams<SnackPenalties>();
                switch (penalties.repLostWhenHungry)
                {
                    case RepLoss.Low:
                    default:
                        return 0.0025;

                    case RepLoss.Medium:
                        return 0.005;

                    case RepLoss.High:
                        return 0.01;
                }
            }
        }
        #endregion

        #region CustomParameterNode
        public override string Title
        {
            get
            {
                return "Snacking";
            }
        }

        public override string DisplaySection
        {
            get
            {
                return Section;
            }
        }

        public override string Section
        {
            get
            {
                return "Snacks!";
            }
        }

        public override int SectionOrder
        {
            get
            {
                return 1;
            }
        }

        public override void SetDifficultyPreset(GameParameters.Preset preset)
        {
            base.SetDifficultyPreset(preset);
        }

        public override GameParameters.GameMode GameMode
        {
            get
            {
                return GameParameters.GameMode.ANY;
            }
        }

        public override bool HasPresets
        {
            get
            {
                return false;
            }
        }

        public override bool Enabled(System.Reflection.MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "recyclerEfficiency" && enableRecyclers)
                return true;
            else if (member.Name == "recyclerEfficiency")
                return false;

            return base.Enabled(member, parameters);
        }

        #endregion
    }
}
