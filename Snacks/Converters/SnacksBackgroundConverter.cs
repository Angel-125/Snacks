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
using System.Collections;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using KSP.IO;
using KSP.UI.Screens;
using KSP.Localization;

namespace Snacks
{
    #region Enums
    /// <summary>
    /// This enum specifies the diffent types of emails to send during background processing.
    /// </summary>
    public enum SnacksBackroundEmailTypes
    {
        /// <summary>
        /// The processor is missing an input resource.
        /// </summary>
        missingResources,

        /// <summary>
        /// The processor is missing a required resource.
        /// </summary>
        missingRequiredResource,

        /// <summary>
        /// The vessel is out of room.
        /// </summary>
        containerFull,

        /// <summary>
        /// The yield experienced a critical failure.
        /// </summary>
        yieldCriticalFail,

        /// <summary>
        /// The yield has had a critical success.
        /// </summary>
        yieldCriticalSuccess,

        /// <summary>
        /// The yield amount was lower than normal.
        /// </summary>
        yieldLower,

        /// <summary>
        /// The yield amount was normal.
        /// </summary>
        yieldNominal
    }
    #endregion

    /// <summary>
    /// This class runs active converters in the background, consuming inputs, producing outputs, and yielding resources.
    /// </summary>
    public class SnacksBackgroundConverter
    {
        public static string NodeName = "SnacksBackgroundConverter";
        public static string skipReources = ";";
        private const double kHighTimewarpDelta = 360.0;


        #region Properties
        public string converterID;
        public string vesselID;
        double hoursPerCycle = 0.0f;
        float minimumSuccess = 0.0f;
        float criticalSuccess = 0.0f;
        float criticalFail = 0.0f;
        double criticalSuccessMultiplier = 1.0f;
        double failureMultiplier = 1.0f;
        #endregion

        #region Housekeeping
        /// <summary>
        /// Name of the converter
        /// </summary>
        public string ConverterName = string.Empty;

        /// <summary>
        /// Name of the converter part module.
        /// </summary>
        public string moduleName = string.Empty;

        /// <summary>
        /// Flag indicating that the converter is active.
        /// </summary>
        public bool IsActivated = false;

        /// <summary>
        /// Flag indicating that the converter is missing resources.
        /// </summary>
        public bool isMissingResources = false;

        /// <summary>
        /// Flag indicating that the container is full.
        /// </summary>
        public bool isContainerFull = false;

        /// <summary>
        /// The input efficiency.
        /// </summary>
        public double inputEfficiency = 1.0f;

        /// <summary>
        /// The output efficiency.
        /// </summary>
        public double outputEfficiency = 1.0f;

        /// <summary>
        /// Flag to indicate that the player has been emailed. Used so we don't spam the player's inbox.
        /// </summary>
        public bool playerEmailed = false;

        bool UseSpecialistBonus = false;
        float SpecialistBonusBase = 0.05f;
        float SpecialistEfficiencyFactor = 0.1f;
        string ExperienceEffect = string.Empty;
        double cycleStartTime = 0;
        bool isGenerator = false;

        List<ResourceRatio> inputList = new List<ResourceRatio>();
        List<ResourceRatio> outputList = new List<ResourceRatio>();
        List<ResourceRatio> requiredList = new List<ResourceRatio>();
        List<ResourceRatio> yieldsList = new List<ResourceRatio>();
        string inputResourceNames = string.Empty;
        string outputResourceNames = string.Empty;
        string requiredResourceNames = string.Empty;
        string yieldResourceNames = string.Empty;

        ProtoPartSnapshot protoPart;
        ProtoPartModuleSnapshot moduleSnapshot;
        Dictionary<string, List<ProtoPartResourceSnapshot>> protoResources = new Dictionary<string, List<ProtoPartResourceSnapshot>>();
        List<SnacksRosterRatio> rosterInputList = new List<SnacksRosterRatio>();
        List<SnacksRosterRatio> rosterOutputList = new List<SnacksRosterRatio>();
        #endregion

        #region Constructors
        /// <summary>
        /// Parses a vessel to find active converters to run in the background.
        /// </summary>
        /// <returns>A map keyed by Vessel that has a list of running converters to run in the background.</returns>
        public static Dictionary<Vessel, List<SnacksBackgroundConverter>> GetBackgroundConverters()
        {
            string moduleWatchlist = "SnacksConverter;SnacksProcessor;SoilRecycler";
            Dictionary<Vessel, List<SnacksBackgroundConverter>> backgroundConverters = new Dictionary<Vessel, List<SnacksBackgroundConverter>>();
            List<SnacksBackgroundConverter> converters;
            ProtoVessel protoVessel;
            Vessel vessel = null;
            ProtoPartSnapshot protoPart;
            ProtoPartModuleSnapshot protoModule;
            int partCount;
            int moduleCount;
            bool isActivated;
            SnacksBackgroundConverter converter;

            int unloadedCount = FlightGlobals.VesselsUnloaded.Count;
            for (int index = 0; index < unloadedCount; index++)
            {
                vessel = FlightGlobals.VesselsUnloaded[index];
                //Skip vessel types that we're not interested in.
                if (vessel.vesselType == VesselType.Debris ||
                    vessel.vesselType == VesselType.Flag ||
                    vessel.vesselType == VesselType.SpaceObject ||
                    vessel.vesselType == VesselType.Unknown)
                    continue;

                protoVessel = vessel.protoVessel;
                if (protoVessel.GetVesselCrew().Count == 0)
                    continue;

                partCount = protoVessel.protoPartSnapshots.Count;
                for (int partIndex = 0; partIndex < partCount; partIndex++)
                {
                    protoPart = protoVessel.protoPartSnapshots[partIndex];
                    moduleCount = protoPart.modules.Count;
                    for (int moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
                    {
                        protoModule = protoPart.modules[moduleIndex];
                        if (moduleWatchlist.Contains(protoModule.moduleName))
                        {
                            //Skip if not active
                            isActivated = false;
                            if (protoModule.moduleValues.HasValue("IsActivated"))
                            {
                                isActivated = false;
                                bool.TryParse(protoModule.moduleValues.GetValue("IsActivated"), out isActivated);
                                if (isActivated)
                                {
                                    if (!backgroundConverters.ContainsKey(vessel))
                                        backgroundConverters.Add(vessel, new List<SnacksBackgroundConverter>());
                                    converters = backgroundConverters[vessel];

                                    //Create a background converter
                                    converters.Add(new SnacksBackgroundConverter(protoPart, protoModule, moduleIndex));
                                    backgroundConverters[vessel] = converters;
                                }
                            }
                        }

                        //Technically not a background converter, we can still treat a ModuleGenerator as if it were one.
                        else if (protoModule.moduleName == "ModuleGenerator")
                        {
                            if (protoModule.moduleValues.HasValue("isAlwaysActive"))
                            {
                                isActivated = false;
                                bool.TryParse(protoModule.moduleValues.GetValue("isAlwaysActive"), out isActivated);
                                if (isActivated)
                                {
                                    if (!backgroundConverters.ContainsKey(vessel))
                                        backgroundConverters.Add(vessel, new List<SnacksBackgroundConverter>());
                                    converters = backgroundConverters[vessel];

                                    //Create a background converter
                                    converter = new SnacksBackgroundConverter(protoPart, protoModule, moduleIndex);
                                    converter.isGenerator = true;
                                    converters.Insert(0, converter);
                                    backgroundConverters[vessel] = converters;
                                }
                            }
                        }

                        //Solar panels aren't background converters either, but we can treat them like one.
                        else if (protoModule.moduleName == "ModuleDeployableSolarPanel" || protoModule.moduleName == "KopernicusSolarPanel")
                        {
                            if (!protoPart.partInfo.partConfig.HasNode("MODULE"))
                                continue;
                            ConfigNode[] panelNodes = protoPart.partInfo.partConfig.GetNodes("MODULE");
                            ConfigNode panelNode = null;
                            string panelModuleName = null;
                            string deployState = string.Empty;
                            double chargeRate = 0;
                            double solarFlux = PhysicsGlobals.SolarLuminosityAtHome;

                            if (moduleIndex < panelNodes.Length)
                                panelNode = panelNodes[moduleIndex];
                            else
                                continue;
                            if (!panelNode.HasValue("name"))
                                continue;
                            panelModuleName = panelNode.GetValue("name");
                            if (panelModuleName != "ModuleDeployableSolarPanel")
                                continue;

                            //Make sure the array is extended (non-deployable arrays are always extended)
                            if (protoModule.moduleValues.HasValue("deployState"))
                                deployState = protoModule.moduleValues.GetValue("deployState");
                            if (deployState != "EXTENDED")
                                continue;

                            //Get the background converters list
                            if (!backgroundConverters.ContainsKey(vessel))
                                backgroundConverters.Add(vessel, new List<SnacksBackgroundConverter>());
                            converters = backgroundConverters[vessel];

                            //Create a background converter
                            converter = new SnacksBackgroundConverter();
                            converter.protoPart = protoPart;
                            converter.moduleSnapshot = protoModule;
                            converter.isGenerator = true;

                            //Calculate solarFlux                            
                            if (protoVessel.vesselModules.HasNode("SnacksVesselModule"))
                            {
                                ConfigNode vesselNode = protoVessel.vesselModules.GetNode("SnacksVesselModule");
                                if (vesselNode.HasValue("solarFlux"))
                                    double.TryParse(vesselNode.GetValue("solarFlux"), out solarFlux);
                            }

                            //Add EC output
                            ResourceRatio resourceRatio = new ResourceRatio();
                            resourceRatio.FlowMode = ResourceFlowMode.ALL_VESSEL;
                            resourceRatio.ResourceName = panelNode.GetValue("resourceName");
                            double.TryParse(panelNode.GetValue("chargeRate"), out chargeRate);
                            resourceRatio.Ratio = chargeRate / 2;
                            resourceRatio.Ratio *= solarFlux / PhysicsGlobals.SolarLuminosityAtHome;
                            converter.outputList.Add(resourceRatio);

                            converters.Insert(0, converter);
                            backgroundConverters[vessel] = converters;
                        }
                    }
                }

                //Give mods a chance to add custom converters that aren't covered by Snacks.
                SnacksScenario.onBackgroundConvertersCreated.Fire(vessel);
            }

            return backgroundConverters;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.SnacksBackgroundConverter"/> class.
        /// </summary>
        /// <param name="protoPart">The ProtPartSnapshot that hosts the converter.</param>
        /// <param name="protoModule">The ProtoPartModuleSnapshot representing the converter.</param>
        /// <param name="moduleIndex">The module index.</param>
        public SnacksBackgroundConverter(ProtoPartSnapshot protoPart, ProtoPartModuleSnapshot protoModule, int moduleIndex)
        {
            ConfigNode[] moduleNodes;
            ConfigNode node = null;
            int count;

            //Get the config node. Module index must match.
            moduleNodes = protoPart.partInfo.partConfig.GetNodes("MODULE");
            if (moduleIndex <= moduleNodes.Length - 1)
                node = moduleNodes[moduleIndex];
            else
                return;

            //We've got a config node, but is it a converter? if so, get its resource lists.
            if (node.HasValue("ConverterName"))
            {
                this.moduleSnapshot = protoModule;
                this.protoPart = protoPart;
                this.ConverterName = node.GetValue("ConverterName");

                bool.TryParse(protoModule.moduleValues.GetValue("IsActivated"), out IsActivated);
                if (node.HasValue("playerEmailed"))
                    bool.TryParse(moduleSnapshot.moduleValues.GetValue("playerEmailed"), out playerEmailed);

                //Get input resources
                if (node.HasNode("INPUT_RESOURCE"))
                    getConverterResources("INPUT_RESOURCE", inputList, node);
                count = inputList.Count;
                for (int index = 0; index < count; index++)
                    inputResourceNames += inputList[index].ResourceName;

                //Get output resources
                if (node.HasNode("OUTPUT_RESOURCE"))
                    getConverterResources("OUTPUT_RESOURCE", outputList, node);
                count = outputList.Count;
                for (int index = 0; index < count; index++)
                    outputResourceNames += outputList[index].ResourceName;

                //Get required resources
                if (node.HasNode("REQUIRED_RESOURCE"))
                    getConverterResources("YIELD_RESOURCE", requiredList, node);
                count = requiredList.Count;
                for (int index = 0; index < count; index++)
                    requiredResourceNames += requiredList[index].ResourceName;

                //Get roster input resources
                if (node.HasNode(SnacksConverter.ROSTER_INPUT_RESOURCE))
                    getRosterResources(SnacksConverter.ROSTER_INPUT_RESOURCE, rosterInputList, node);
                if (node.HasNode(SnacksConverter.ROSTER_OUTPUT_RESOURCE))
                    getRosterResources(SnacksConverter.ROSTER_OUTPUT_RESOURCE, rosterOutputList, node);

                //Get yield resources
                if (node.HasNode("YIELD_RESOURCE"))
                    getConverterResources("YIELD_RESOURCE", yieldsList, node);
                count = yieldsList.Count;
                for (int index = 0; index < count; index++)
                    yieldResourceNames += yieldsList[index].ResourceName;

                if (node.HasValue("hoursPerCycle"))
                    double.TryParse(node.GetValue("hoursPerCycle"), out hoursPerCycle);

                if (node.HasValue("minimumSuccess"))
                    float.TryParse(node.GetValue("minimumSuccess"), out minimumSuccess);

                if (node.HasValue("criticalSuccess"))
                    float.TryParse(node.GetValue("criticalSuccess"), out criticalSuccess);

                if (node.HasValue("criticalFail"))
                    float.TryParse(node.GetValue("criticalFail"), out criticalFail);

                if (node.HasValue("criticalSuccessMultiplier"))
                    double.TryParse(node.GetValue("criticalSuccessMultiplier"), out criticalSuccessMultiplier);

                if (node.HasValue("failureMultiplier"))
                    double.TryParse(node.GetValue("failureMultiplier"), out failureMultiplier);

                if (node.HasValue("UseSpecialistBonus"))
                    bool.TryParse(node.GetValue("UseSpecialistBonus"), out UseSpecialistBonus);

                if (node.HasValue("SpecialistBonusBase"))
                    float.TryParse(node.GetValue("SpecialistBonusBase"), out SpecialistBonusBase);

                if (node.HasValue("SpecialistEfficiencyFactor"))
                    float.TryParse(node.GetValue("SpecialistEfficiencyFactor"), out SpecialistEfficiencyFactor);

                if (node.HasValue("ExperienceEffect"))
                    ExperienceEffect = node.GetValue("ExperienceEffect");

                if (protoModule.moduleValues.HasValue("cycleStartTime"))
                    double.TryParse(protoModule.moduleValues.GetValue("cycleStartTime"), out cycleStartTime);

                if (protoModule.moduleValues.HasValue("inputEfficiency"))
                    double.TryParse(protoModule.moduleValues.GetValue("inputEfficiency"), out inputEfficiency);

                if (protoModule.moduleValues.HasValue("outputEfficiency"))
                    double.TryParse(protoModule.moduleValues.GetValue("outputEfficiency"), out outputEfficiency);
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Snacks.SnacksBackgroundConverter"/> class.
        /// </summary>
        public SnacksBackgroundConverter()
        {

        }
        #endregion

        #region Converter Operations
        /// <summary>
        /// Checks to be sure the vessel has the required resources.
        /// </summary>
        /// <param name="vessel">The Vessel to check.</param>
        /// <param name="elapsedTime">The seconds that have elapsed.</param>
        public void CheckRequiredResources(ProtoVessel vessel, double elapsedTime)
        {
            int count = requiredList.Count;
            if (count == 0)
                return;

            ResourceRatio resourceRatio;
            double amount = 0;
            for (int index = 0; index < count; index++)
            {
                resourceRatio = requiredList[index];
                amount = getAmount(resourceRatio.ResourceName, resourceRatio.FlowMode);
                if (amount < resourceRatio.Ratio)
                {
                    isMissingResources = true;

                    emailPlayer(resourceRatio.ResourceName, SnacksBackroundEmailTypes.missingRequiredResource);

                    return;
                }
            }
        }

        /// <summary>
        /// Consumes the input resources.
        /// </summary>
        /// <param name="vessel">The Vessel to consume resources from.</param>
        /// <param name="elapsedTime">Converter elapsed time.</param>
        public void ConsumeInputResources(ProtoVessel vessel, double elapsedTime)
        {
            int count = inputList.Count;
            if (count == 0)
                return;
            if (isMissingResources)
                return;
            if (isContainerFull)
                return;

            //Get cheat states
            bool infinitePropellantsEnabled = CheatOptions.InfinitePropellant;
            bool infiniteElectricity = CheatOptions.InfiniteElectricity || !SnacksProperties.ConsumeECInBackground;

            //Check to make sure we have enough resources
            ResourceRatio resourceRatio;
            double amount = 0;
            double demand = 0;
            for (int index = 0; index < count; index++)
            {
                resourceRatio = inputList[index];
                //Skip resource if the appropriate cheat is on
                if ((resourceRatio.ResourceName == "ElectricCharge") || (infinitePropellantsEnabled))
                    continue;

                    demand = resourceRatio.Ratio * inputEfficiency * elapsedTime;
                amount = getAmount(resourceRatio.ResourceName, resourceRatio.FlowMode);
                if (amount < demand)
                {
                    //Set the missing resources flag
                    isMissingResources = true;

                    //Email player
                    emailPlayer(resourceRatio.ResourceName, SnacksBackroundEmailTypes.missingResources);
                    return;
                }
            }

            //Now consume the resources
            for (int index = 0; index < count; index++)
            {
                resourceRatio = inputList[index];
                //Skip resource if the appropriate cheat is on
                if ((resourceRatio.ResourceName == "ElectricCharge") || (infinitePropellantsEnabled))
                    continue;

                //Request resource
                demand = resourceRatio.Ratio * inputEfficiency * elapsedTime;
                requestAmount(resourceRatio.ResourceName, demand, resourceRatio.FlowMode);
            }

            //Now consume roster inputs
            if (protoPart.protoModuleCrew.Count > 0 && rosterInputList.Count > 0)
            {
                SnacksRosterRatio rosterRatio;
                AstronautData astronautData;
                SnacksRosterResource rosterResource;
                ProtoCrewMember[] astronauts = protoPart.protoModuleCrew.ToArray();

                count = rosterInputList.Count;
                for (int index = 0; index < count; index++)
                {
                    rosterRatio = rosterInputList[index];
                    //Skip resource if the appropriate cheat is on
                    if ((rosterRatio.ResourceName == "ElectricCharge" && infiniteElectricity) || (infinitePropellantsEnabled))
                        continue;
                    for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
                    {
                        //Get astronaut data
                        astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[astronautIndex]);
                        if (!astronautData.rosterResources.ContainsKey(rosterRatio.ResourceName))
                            continue;

                        //Get roster resource
                        rosterResource = astronautData.rosterResources[rosterRatio.ResourceName];

                        //Process the input
                        rosterResource.amount -= rosterRatio.AmountPerSecond * elapsedTime;
                        if (rosterResource.amount <= 0)
                            rosterResource.amount = 0;
                        astronautData.rosterResources[rosterRatio.ResourceName] = rosterResource;
                        SnacksScenario.Instance.SetAstronautData(astronautData);

                        if (SnacksProperties.DebugLoggingEnabled)
                            Debug.Log("[" + ConverterName + "] - " + astronautData.name + ": " + rosterResource.GetStatusDisplay());

                        //Fire event
                        SnacksScenario.onRosterResourceUpdated.Fire(vessel.vesselRef, rosterResource, astronautData, astronauts[astronautIndex]);
                    }
                }
            }
        }

        /// <summary>
        /// Produces the output resources.
        /// </summary>
        /// <param name="vessel">The Vessel to add resources to.</param>
        /// <param name="elapsedTime">Converter elapsed time.</param>
        public void ProduceOutputResources(ProtoVessel vessel, double elapsedTime)
        {
            int count = outputList.Count;
            if (count == 0)
                return;
            if (isMissingResources)
                return;
            if (isContainerFull)
                return;

            ResourceRatio resourceRatio;
            double supply = 0;
            for (int index = 0; index < count; index++)
            {
                resourceRatio = outputList[index];
                supply = resourceRatio.Ratio * outputEfficiency * elapsedTime;
                supplyAmount(resourceRatio.ResourceName, supply, resourceRatio.FlowMode, resourceRatio.DumpExcess);
            }

            //Now produce roster outputs
            if (protoPart.protoModuleCrew.Count > 0 && rosterOutputList.Count > 0)
            {
                SnacksRosterRatio rosterRatio;
                AstronautData astronautData;
                SnacksRosterResource rosterResource;
                ProtoCrewMember[] astronauts = protoPart.protoModuleCrew.ToArray();

                count = rosterOutputList.Count;
                for (int index = 0; index < count; index++)
                {
                    rosterRatio = rosterOutputList[index];
                    for (int astronautIndex = 0; astronautIndex < astronauts.Length; astronautIndex++)
                    {
                        //Get astronaut data
                        astronautData = SnacksScenario.Instance.GetAstronautData(astronauts[astronautIndex]);
                        if (!astronautData.rosterResources.ContainsKey(rosterRatio.ResourceName))
                            continue;

                        //Get roster resource
                        rosterResource = astronautData.rosterResources[rosterRatio.ResourceName];

                        //Process the output
                        rosterResource.amount += rosterRatio.AmountPerSecond * elapsedTime;
                        if (rosterResource.amount >= rosterResource.maxAmount)
                            rosterResource.amount = rosterResource.maxAmount;
                        astronautData.rosterResources[rosterRatio.ResourceName] = rosterResource;

                        //Fire event
                        SnacksScenario.onRosterResourceUpdated.Fire(vessel.vesselRef, rosterResource, astronautData, astronauts[astronautIndex]);
                    }
                }
            }
        }

        /// <summary>
        /// Produces the yield resources
        /// </summary>
        /// <param name="vessel">The Vessel to add resources to.</param>
        public void ProduceyieldsList(ProtoVessel vessel)
        {
            int count = yieldsList.Count;
            if (count == 0)
                return;
            if (isMissingResources)
                return;
            if (isContainerFull)
                return;

            //Check cycle start time
            if (cycleStartTime == 0f)
            {
                cycleStartTime = Planetarium.GetUniversalTime();
                return;
            }

            //Calculate elapsed time
            double elapsedTime = Planetarium.GetUniversalTime() - cycleStartTime;
            double secondsPerCycle = hoursPerCycle * 3600;

            //If we've elapsed time cycle then perform the analyis.
            float completionRatio = (float)(elapsedTime / secondsPerCycle);
            if (completionRatio > 1.0f)
            {
                //Reset start time
                cycleStartTime = Planetarium.GetUniversalTime();

                int cyclesSinceLastUpdate = Mathf.RoundToInt(completionRatio);
                int currentCycle;
                for (currentCycle = 0; currentCycle < cyclesSinceLastUpdate; currentCycle++)
                {
                    if (minimumSuccess <= 0)
                    {
                        supplyyieldsList(1.0);
                    }

                    else
                    {
                        //Roll the die
                        float roll = 0.0f;
                        roll = UnityEngine.Random.Range(1, 6);
                        roll += UnityEngine.Random.Range(1, 6);
                        roll += UnityEngine.Random.Range(1, 6);
                        roll *= 5.5556f;

                        if (roll <= criticalFail)
                        {
                            //Deactivate converter
                            IsActivated = false;

                            //Email player
                            emailPlayer(null, SnacksBackroundEmailTypes.yieldCriticalFail);

                            //Done
                            return;
                        }
                        else if (roll >= criticalSuccess)
                        {
                            supplyyieldsList(criticalSuccessMultiplier);
                        }
                        else if (roll >= minimumSuccess)
                        {
                            supplyyieldsList(1.0);
                        }
                        else
                        {
                            supplyyieldsList(failureMultiplier);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Prepares the converter to process.
        /// </summary>
        /// <param name="vessel">The Vessel to check for preparations.</param>
        public void PrepareToProcess(ProtoVessel vessel)
        {
            //Find out proto part and module and resources
            int count = vessel.protoPartSnapshots.Count;
            int resourceCount;
            ProtoPartSnapshot pps;
            ProtoPartResourceSnapshot protoPartResource;
            List<ProtoPartResourceSnapshot> resourceList;

            //Clear our resource map.
            protoResources.Clear();

            for (int index = 0; index < count; index++)
            {
                //Get the proto part snapshot
                pps = vessel.protoPartSnapshots[index];

                //Sort through all the resources and add them to our buckets.
                resourceCount = pps.resources.Count;
                for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                {
                    protoPartResource = pps.resources[resourceIndex];

                    //Inputs
                    if (!string.IsNullOrEmpty(inputResourceNames) && !skipReources.Contains(protoPartResource.resourceName))
                    {
                        if (inputResourceNames.Contains(protoPartResource.resourceName))
                        {
                            if (protoResources.ContainsKey(protoPartResource.resourceName))
                            {
                                resourceList = protoResources[protoPartResource.resourceName];
                            }
                            else
                            {
                                protoResources.Add(protoPartResource.resourceName, new List<ProtoPartResourceSnapshot>());
                                resourceList = protoResources[protoPartResource.resourceName];
                            }

                            resourceList.Add(protoPartResource);
                            protoResources[protoPartResource.resourceName] = resourceList;
                        }
                    }

                    //Outputs
                    if (!string.IsNullOrEmpty(outputResourceNames) && !skipReources.Contains(protoPartResource.resourceName))
                    {
                        if (outputResourceNames.Contains(protoPartResource.resourceName))
                        {
                            if (protoResources.ContainsKey(protoPartResource.resourceName))
                            {
                                resourceList = protoResources[protoPartResource.resourceName];
                            }
                            else
                            {
                                protoResources.Add(protoPartResource.resourceName, new List<ProtoPartResourceSnapshot>());
                                resourceList = protoResources[protoPartResource.resourceName];
                            }

                            resourceList.Add(protoPartResource);
                            protoResources[protoPartResource.resourceName] = resourceList;
                        }
                    }

                    //Required
                    if (!string.IsNullOrEmpty(requiredResourceNames) && !skipReources.Contains(protoPartResource.resourceName))
                    {
                        if (requiredResourceNames.Contains(protoPartResource.resourceName))
                        {
                            if (protoResources.ContainsKey(protoPartResource.resourceName))
                            {
                                resourceList = protoResources[protoPartResource.resourceName];
                            }
                            else
                            {
                                protoResources.Add(protoPartResource.resourceName, new List<ProtoPartResourceSnapshot>());
                                resourceList = protoResources[protoPartResource.resourceName];
                            }

                            resourceList.Add(protoPartResource);
                            protoResources[protoPartResource.resourceName] = resourceList;
                        }
                    }

                    //Yield
                    if (!string.IsNullOrEmpty(yieldResourceNames) && !skipReources.Contains(protoPartResource.resourceName))
                    {
                        if (yieldResourceNames.Contains(protoPartResource.resourceName))
                        {
                            if (protoResources.ContainsKey(protoPartResource.resourceName))
                            {
                                resourceList = protoResources[protoPartResource.resourceName];
                            }
                            else
                            {
                                protoResources.Add(protoPartResource.resourceName, new List<ProtoPartResourceSnapshot>());
                                resourceList = protoResources[protoPartResource.resourceName];
                            }

                            resourceList.Add(protoPartResource);
                            protoResources[protoPartResource.resourceName] = resourceList;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles post process tasks for the converter.
        /// </summary>
        /// <param name="vessel">The Vessel to update.</param>
        public void PostProcess(ProtoVessel vessel)
        {
            if (isGenerator)
                return;

            //Update lastUpdateTime
            moduleSnapshot.moduleValues.SetValue("lastUpdateTime", Planetarium.GetUniversalTime());
            moduleSnapshot.moduleValues.SetValue("cycleStartTime", cycleStartTime);
            moduleSnapshot.moduleValues.SetValue("IsActivated", IsActivated);
        }
        #endregion

        #region Helpers
        protected static void getRosterResources(string nodeName, List<SnacksRosterRatio> rosterResourceList, ConfigNode node)
        {
            ConfigNode rosterNode;
            SnacksRosterRatio rosterRatio;

            //Seconds per day
            double secondsPerDay = 1.0;
            if (HighLogic.LoadedSceneIsFlight ||
                HighLogic.LoadedSceneIsEditor ||
                HighLogic.LoadedScene == GameScenes.SPACECENTER ||
                HighLogic.LoadedScene == GameScenes.TRACKSTATION)
                secondsPerDay = SnacksScenario.GetSecondsPerDay();

            ConfigNode[] nodes = node.GetNodes(nodeName);

            for (int index = 0; index < nodes.Length; index++)
            {
                rosterNode = nodes[index];
                if (rosterNode.HasValue(SnacksConverter.ROSTER_RESOURCE_NAME) && 
                    (rosterNode.HasValue(SnacksConverter.ROSTER_RESOURCE_AMOUNT_PER_DAY) || 
                    rosterNode.HasValue(SnacksConverter.ROSTER_RESOURCE_AMOUNT_PER_SECOND)))
                {
                    rosterRatio = new SnacksRosterRatio();
                    rosterRatio.ResourceName = rosterNode.GetValue(SnacksConverter.ROSTER_RESOURCE_NAME);

                    //AmountPerDay takes precedence over AmountPerSecond
                    if (rosterNode.HasValue(SnacksConverter.ROSTER_RESOURCE_AMOUNT_PER_DAY))
                    {
                        double.TryParse(rosterNode.GetValue(SnacksConverter.ROSTER_RESOURCE_AMOUNT_PER_DAY), out rosterRatio.AmountPerDay);
                        rosterRatio.AmountPerSecond = rosterRatio.AmountPerDay / secondsPerDay;
                    }
                    else
                    {
                        double.TryParse(rosterNode.GetValue(SnacksConverter.ROSTER_RESOURCE_AMOUNT_PER_SECOND), out rosterRatio.AmountPerSecond);
                    }

                    rosterResourceList.Add(rosterRatio);
                }
            }
        }

        protected static void getConverterResources(string nodeName, List<ResourceRatio> resourceList, ConfigNode node)
        {
            ConfigNode[] resourceNodes;
            ConfigNode resourceNode;
            string resourceName;
            ResourceRatio ratio;

            resourceNodes = node.GetNodes(nodeName);
            for (int resourceIndex = 0; resourceIndex < resourceNodes.Length; resourceIndex++)
            {
                //Resource name
                resourceNode = resourceNodes[resourceIndex];
                if (resourceNode.HasValue("ResourceName"))
                    resourceName = resourceNode.GetValue("ResourceName");
                else if (resourceNode.HasValue("name"))
                    resourceName = resourceNode.GetValue("name");
                else
                    resourceName = "";

                //Ratio
                ratio = new ResourceRatio();
                ratio.ResourceName = resourceName;
                if (resourceNode.HasValue("Ratio"))
                    double.TryParse(resourceNode.GetValue("Ratio"), out ratio.Ratio);
                if (resourceNode.HasValue("rate"))
                    double.TryParse(resourceNode.GetValue("rate"), out ratio.Ratio);

                //Flow mode
                if (resourceNode.HasValue("FlowMode"))
                {
                    switch (resourceNode.GetValue("FlowMode"))
                    {
                        case "NO_FLOW":
                        case "NULL":
                            ratio.FlowMode = ResourceFlowMode.NO_FLOW;
                            break;

                        default:
                            ratio.FlowMode = ResourceFlowMode.ALL_VESSEL;
                            break;
                    }
                }

                //Add to the list
                resourceList.Add(ratio);
            }
        }

        protected void emailPlayer(string resourceName, SnacksBackroundEmailTypes emailType)
        {
            StringBuilder resultsMessage = new StringBuilder();
            MessageSystem.Message msg;
            PartResourceDefinition resourceDef = null;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            string titleMessage;

            //Spam check
            if (playerEmailed || !SnacksProperties.EmailConverterResults)
            {
                return;
            }
            else
            {
                playerEmailed = true;
                moduleSnapshot.moduleValues.SetValue("playerEmailed", playerEmailed);
            }

            //From
            resultsMessage.AppendLine("From: " + protoPart.pVesselRef.vesselName);

            switch (emailType)
            {
                case SnacksBackroundEmailTypes.missingResources:
                    resourceDef = definitions[resourceName];
                    titleMessage = Localizer.Format("#LOC_INFO_TITLE1");//#LOC_INFO_TITLE1 = needs more resources
                    resultsMessage.AppendLine(Localizer.Format("#LOC_INFO_MISSINGRESOURCE"));//#LOC_INFO_MISSINGRESOURCE = Subject: Missing Resources
                    resultsMessage.AppendLine(Localizer.Format("#LOC_INFO_PRDUCFAILD1",resourceDef.displayName, ConverterName));
                    //#LOC_INFO_PRDUCFAILD1 = There is no more <<1>> available to continue production. Operations cannot continue with the <<2>> until more resource becomes available.
                    break;

                case SnacksBackroundEmailTypes.missingRequiredResource:
                    resourceDef = definitions[resourceName];
                    titleMessage = Localizer.Format("#LOC_INFO_TITLE2");//#LOC_INFO_TITLE2 = Subject:  needs a resource
                    resultsMessage.AppendLine(Localizer.Format("#LOC_INFO_MISSINGREQRESOURCE"));//#LOC_INFO_MISSINGREQRESOURCE = Subject: Missing Resources
                    resultsMessage.AppendLine(Localizer.Format("#LOC_INFO_PRDUCFAILD2", ConverterName, resourceDef.displayName));
                    //#LOC_INFO_PRDUCFAILD2 = <<1>> needs <<2>> in order to function. Operations halted until the resource becomes available.
                    break;

                case SnacksBackroundEmailTypes.containerFull:
                    resourceDef = definitions[resourceName];
                    titleMessage = Localizer.Format("#LOC_INFO_TITLE3");//#LOC_INFO_TITLE3 = is out of storage space
                    resultsMessage.AppendLine(Localizer.Format("#LOC_INFO_CONTAINERFULL"));//#LOC_INFO_CONTAINERFULL = Subject: Containers Are Full
                    resultsMessage.AppendLine(Localizer.Format("#LOC_INFO_PRDUCFAILD3", resourceDef.displayName, ConverterName));
                    //#LOC_INFO_PRDUCFAILD3 = There is no more storage space available for <<1>> Operations cannot continue with the <<2>> until more space becomes available.
                    break;

                case SnacksBackroundEmailTypes.yieldCriticalFail:
                    titleMessage = Localizer.Format("#LOC_INFO_TITLE4");//#LOC_INFO_TITLE4 = has suffered a critical failure in one of its converters
                    resultsMessage.AppendLine(Localizer.Format("#LOC_INFO_PRDUCFAILD4", ConverterName));
                    //#LOC_INFO_PRDUCFAILD4 = A <<1>> has failed! The production yield has been lost. It must be repaired and/or restarted before it can begin production again.
                    break;

                default:
                    return;
            }

            msg = new MessageSystem.Message(protoPart.pVesselRef.vesselName + titleMessage, resultsMessage.ToString(),
                MessageSystemButton.MessageButtonColor.ORANGE, MessageSystemButton.ButtonIcons.ALERT);
            MessageSystem.Instance.AddMessage(msg);
        }

        protected void supplyyieldsList(double yieldMultiplier)
        {
            int count = yieldsList.Count;
            ResourceRatio resourceRatio;
            double supply = 0;

            for (int index = 0; index < count; index++)
            {
                resourceRatio = yieldsList[index];
                supply = resourceRatio.Ratio * outputEfficiency * yieldMultiplier;
                supplyAmount(resourceRatio.ResourceName, supply, resourceRatio.FlowMode, resourceRatio.DumpExcess);
            }
        }

        protected void supplyAmount(string resourceName, double supply, ResourceFlowMode flowMode, bool dumpExcess)
        {
            int count;
            double currentSupply = supply;
            if (flowMode != ResourceFlowMode.NO_FLOW)
            {
                if (!protoResources.ContainsKey(resourceName))
                    return;
                List<ProtoPartResourceSnapshot> resourceShapshots = protoResources[resourceName];
                count = resourceShapshots.Count;

                //Distribute the resource throughout the resource snapshots.
                //TODO: find a way to evenly distribute the resource.
                for (int index = 0; index < count; index++)
                {
                    //If the current part resource snapshot has enough room, then we can store all of the currentSupply and be done.
                    if (resourceShapshots[index].amount + currentSupply < resourceShapshots[index].maxAmount)
                    {
                        resourceShapshots[index].amount += currentSupply;
                        return;
                    }

                    //The current snapshot can't hold all of the currentSupply, but we can whittle down what we currently have.
                    else
                    {
                        currentSupply -= resourceShapshots[index].maxAmount - resourceShapshots[index].amount;
                        resourceShapshots[index].amount = resourceShapshots[index].maxAmount;
                    }
                }

                //If we have any resource left over, then it means that our containers are full.
                //If we can't dump the excess, then we're done.
                if (currentSupply > 0.0001f && !dumpExcess)
                {
                    isContainerFull = true;

                    //Email player
                    emailPlayer(resourceName, SnacksBackroundEmailTypes.containerFull);

                    //Done
                    return;
                }
            }
        }

        protected double requestAmount(string resourceName, double demand, ResourceFlowMode flowMode)
        {
            double supply = 0;
            int count;

            //Check vessel
            if (flowMode != ResourceFlowMode.NO_FLOW)
            {
                if (!protoResources.ContainsKey(resourceName))
                    return 0f;
                List<ProtoPartResourceSnapshot> resourceShapshots = protoResources[resourceName];
                count = resourceShapshots.Count;

                double currentDemand = demand;
                for (int index = 0; index < count; index++)
                {
                    //Skip locked resources
                    if (!resourceShapshots[index].flowState)
                        continue;

                    if (resourceShapshots[index].amount > currentDemand)
                    {
                        resourceShapshots[index].amount -= currentDemand;
                        supply += currentDemand;
                        currentDemand = 0;
                    }
                    else //Current demand > what the part has.
                    {
                        supply += resourceShapshots[index].amount;
                        currentDemand -= resourceShapshots[index].amount;
                        resourceShapshots[index].amount = 0;
                    }
                }
            }
            else //Check the part
            {
                count = protoPart.resources.Count;
                for (int index = 0; index < count; index++)
                {
                    //Skip locked resources
                    if (!protoPart.resources[index].flowState)
                        continue;

                    if (protoPart.resources[index].resourceName == resourceName)
                    {
                        supply = protoPart.resources[index].amount;
                        if (supply >= demand)
                        {
                            protoPart.resources[index].amount = supply - demand;
                            return demand;
                        }
                        else
                        {
                            //Supply < demand
                            protoPart.resources[index].amount = 0;
                            return supply;
                        }
                    }
                }
            }

            return supply;
        }

        protected double getAmount(string resourceName, ResourceFlowMode flowMode)
        {
            double amount = 0;
            int count;

            if (flowMode != ResourceFlowMode.NO_FLOW)
            {
                if (!protoResources.ContainsKey(resourceName))
                    return 0f;
                List<ProtoPartResourceSnapshot> resourceShapshots = protoResources[resourceName];
                count = resourceShapshots.Count;
                for (int index = 0; index < count; index++)
                {
                    if (resourceShapshots[index].flowState)
                        amount += resourceShapshots[index].amount;
                }
            }
            else //Check the part
            {
                count = protoPart.resources.Count;
                for (int index = 0; index < count; index++)
                {
                    if (protoPart.resources[index].resourceName == resourceName && protoPart.resources[index].flowState)
                        return protoPart.resources[index].amount;
                }
            }

            return amount;
        }
        #endregion
    }
}
