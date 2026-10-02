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
using KSP.UI;

namespace Snacks
{
    /// <summary>
    /// Signifies that the converters have completed their run.
    /// </summary>
    /// <param name="simulator">The simulator that invoked the delegate method.</param>
    public delegate void OnConvertersRunCompleteDelegate(SimSnacks simulator);

    /// <summary>
    /// Signifies that the consumers have completed their run.
    /// </summary>
    /// <param name="simulator">The simulator that invoked the delegate method.</param>
    public delegate void OnConsumersRunCompleteDelegate(SimSnacks simulator);

    /// <summary>
    /// Signifies that the simulation cycle has completed.
    /// </summary>
    /// <param name="simulator">The simulator that invoked the delegate method.</param>
    public delegate void OnSimulatorCycleCompleteDelegate(SimSnacks simulator);

    /// <summary>
    /// Signifies that the simulation has completed.
    /// </summary>
    /// <param name="simulator">The simulator that invoked the delegate method.</param>
    public delegate void OnSimulationCompleteDelegate(SimSnacks simulator);

    /// <summary>
    /// Signifies that the simulation experienced an error.
    /// </summary>
    /// <param name="simulator">The simulator generating the error.</param>
    /// <param name="ex">The Exception that was generated.</param>
    public delegate void OnSimulatorExceptionDelegate(SimSnacks simulator, Exception ex);

    /// <summary>
    /// Type of vessel being simulated
    /// </summary>
    public enum SimulatedVesselTypes
    {
        simVesselLoaded,
        simVesselUnloaded,
        simEditor
    }

    /// <summary>
    /// Context for how the simulator is being created. Typically used when Snacks fires an event to give mods a chance to add additional custom converters not covered by Snacks.
    /// </summary>
    public struct SimulatorContext
    {
        /// <summary>
        /// Type of vessel being simulated.
        /// </summary>
        public SimulatedVesselTypes simulatedVesselType;

        /// <summary>
        /// Vessel object for loaded/unloaded vessels being simulated.
        /// </summary>
        public Vessel vessel;

        /// <summary>
        /// Ship constructor for editor vessel being simulated.
        /// </summary>
        public ShipConstruct shipConstruct;
    }

    /// <summary>
    /// This class determines how long consumed resources like Snacks will last by simulating resource consumption and simulating running converters like soil recyclers and snacks processors.
    /// It is designed to allow for an arbitrary number of resource production chains and an arbitrary number of consumed resources.
    /// 
    /// Conditions:
    /// The only inputs allowed into the system are those consumed by kerbals. Ex: kerbals eat Snacks and produce Soil.
    /// Resources aboard the vessel that aren't directly involved in resource consumption are fixed. Ex: Resource harvesters that produce Ore aren't accounted for.
    /// Running simulations is computationally expensive. This class should be run in a thread.
    /// </summary>
    public class SimSnacks
    {
        #region Housekeeping
        public static string converterWatchlist = "SnacksConverter;SnacksProcessor;SoilRecycler;WBIResourceConverter;WBIModuleResourceConverterFX;ModuleResourceConverter";

        public OnConvertersRunCompleteDelegate OnConvertersRunComplete;
        public OnConsumersRunCompleteDelegate OnConsumersRunComplete;
        public OnSimulatorCycleCompleteDelegate OnSimulatorCycleComplete;
        public OnSimulationCompleteDelegate OnSimulationComplete;
        public OnSimulatorExceptionDelegate OnSimulatorException;

        public Vessel vessel;
        public Dictionary<string, SimResource> resources;
        public List<SimConverter> converters;
        public List<ResourceRatio> consumedResources;
        public List<ResourceRatio> producedResources;
        public Dictionary<string, double> consumedResourceDurations;
        public bool convertersAssumedActive = false;

        public double secondsPerCycle = 3600;
        public int maxSimulatorCycles = 10000;
        public int currentSimulatorCycle = 0;

        public bool exitSimulation = false;
        public Exception exception = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a simulator from the supplied ship construct
        /// </summary>
        /// <param name="ship">A ShipConstruct to simulate</param>
        /// <returns>A SimSnacks simulator</returns>
        public static SimSnacks CreateSimulator(ShipConstruct ship)
        {
            VesselCrewManifest manifest = CrewAssignmentDialog.Instance.GetManifest();
            Part[] parts = EditorLogic.fetch.ship.parts.ToArray();
            Part part;
            int resourceCount;
            PartResource resource;
            SimResource simResource;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            ResourceRatio resourceRatio;
            SimConverter simConverter;

            converterWatchlist = SnacksScenario.Instance.converterWatchlist;
            SimSnacks simSnacks = new SimSnacks();
            simSnacks.secondsPerCycle = SnacksScenario.Instance.simulatorSecondsPerCycle;
            simSnacks.maxSimulatorCycles = SnacksScenario.Instance.maxSimulatorCycles;

            //No parts? Then we're done
            if (parts.Length == 0)
                return null;
            if (manifest == null)
                return null;
            if (manifest.CrewCount == 0)
                return null;

            for (int partIndex = 0; partIndex < parts.Length; partIndex++)
            {
                part = parts[partIndex];

                //Get resources
                resourceCount = part.Resources.Count;
                for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                {
                    resource = part.Resources[resourceIndex];

                    //Create sim resource if needed
                    if (!simSnacks.resources.ContainsKey(resource.resourceName))
                    {
                        simResource = new SimResource();
                        simResource.resourceName = resource.resourceName;
                        simResource.amount = resource.amount;
                        simResource.maxAmount = resource.maxAmount;
                        simSnacks.resources.Add(simResource.resourceName, simResource);
                    }
                    else
                    {
                        simResource = simSnacks.resources[resource.resourceName];
                        simResource.amount += resource.amount;
                        simResource.maxAmount += resource.maxAmount;
                        simSnacks.resources[resource.resourceName] = simResource;
                    }
                }

                //Get ElectricCharge generators
                getGenerators(part.partInfo.partConfig, simSnacks);

                //Deployable solar panels
                List<ModuleDeployableSolarPanel> solarPanels = part.FindModulesImplementing<ModuleDeployableSolarPanel>();
                ModuleDeployableSolarPanel solarPanel;
                int arrayCount = solarPanels.Count;
                for (int index = 0; index < arrayCount; index++)
                {
                    solarPanel = solarPanels[index];
                    if (solarPanel.isTracking && solarPanel.deployState != ModuleDeployablePart.DeployState.EXTENDED)
                        continue;

                    //Create new sim converter
                    simConverter = new SimConverter();
                    simConverter.converterName = solarPanel.ClassName;

                    resourceRatio = new ResourceRatio();
                    resourceRatio.ResourceName = solarPanel.resourceName;
                    resourceRatio.Ratio = (solarPanel.chargeRate / 2 ) * simSnacks.secondsPerCycle;

                    simConverter.outputList.Add(resourceRatio);

                    simSnacks.converters.Insert(0, simConverter);
                }

                //Get converters
                List<ModuleResourceConverter> converters = part.FindModulesImplementing<ModuleResourceConverter>();
                ModuleResourceConverter converter;
                int count = converters.Count;
                for (int index = 0; index < count; index++)
                {
                    converter = converters[index];

                    //Create new sim converter
                    simConverter = new SimConverter();
                    simConverter.converterName = converter.ConverterName;

                    //Get inputs
                    resourceCount = converter.inputList.Count;
                    for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                    {
                        resourceRatio = new ResourceRatio();
                        resourceRatio.ResourceName = converter.inputList[resourceIndex].ResourceName;
                        resourceRatio.Ratio = converter.inputList[resourceIndex].Ratio * simSnacks.secondsPerCycle;
                        simConverter.inputList.Add(resourceRatio);
                    }

                    //Get outputs
                    resourceCount = converter.outputList.Count;
                    for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                    {
                        resourceRatio = new ResourceRatio();
                        resourceRatio.ResourceName = converter.outputList[resourceIndex].ResourceName;
                        resourceRatio.Ratio = converter.outputList[resourceIndex].Ratio * simSnacks.secondsPerCycle;
                        simConverter.outputList.Add(resourceRatio);
                    }

                    //Get yied resources if any
                    if (converter is SnacksConverter)
                    {
                        SnacksConverter snacksConverter = (SnacksConverter)converter;
                        resourceCount = snacksConverter.yieldsList.Count;
                        for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                        {
                            resourceRatio = new ResourceRatio();
                            resourceRatio.ResourceName = snacksConverter.yieldsList[resourceIndex].ResourceName;
                            resourceRatio.Ratio = snacksConverter.yieldsList[resourceIndex].Ratio * simSnacks.secondsPerCycle;
                            simConverter.yieldsList.Add(resourceRatio);
                        }

                        simConverter.yieldSecondsPerCycle = snacksConverter.hoursPerCycle * 3600;
                    }

                    //Check for snack processor and soil recycler
                    if (converter is SoilRecycler)
                    {
                        SoilRecycler recycler = (SoilRecycler)converter;
                        updateSoilRecyclerOutput(simConverter.inputList, simConverter.outputList, recycler.RecyclerCapacity);
                    }
                    else if (converter is SnackProcessor)
                    {
                        updateSnacksProcessorOutput(simConverter.outputList);
                    }

                    //Add converter to the simulator
                    if (simConverter.inputList.Count > 0 || simConverter.outputList.Count > 0 || simConverter.yieldsList.Count > 0)
                    {
                        //We're in the editor, we have to assume the converters are on.
                        simSnacks.convertersAssumedActive = true;

                        simSnacks.converters.Add(simConverter);
                    }
                }
            }

            return simSnacks;
        }

        /// <summary>
        /// Creates a simulator from the proto vessel
        /// </summary>
        /// <param name="protoVessel">The unloaded vessel to query for resources and converters.</param>
        /// <returns>A SimSnacks simulator.</returns>
        public static SimSnacks CreateSimulator(ProtoVessel protoVessel)
        {
            ProtoPartSnapshot protoPart;
            ProtoPartModuleSnapshot moduleSnapshot;
            int moduleCount;
            int protoPartCount;

            converterWatchlist = SnacksScenario.Instance.converterWatchlist;

            SimSnacks simSnacks = new SimSnacks();
            simSnacks.secondsPerCycle = SnacksScenario.Instance.simulatorSecondsPerCycle;
            simSnacks.maxSimulatorCycles = SnacksScenario.Instance.maxSimulatorCycles;

            //Go through each part and get its resources and converters
            protoPartCount = protoVessel.protoPartSnapshots.Count;
            for (int partIndex = 0; partIndex < protoPartCount; partIndex++)
            {
                //Get the part
                protoPart = protoVessel.protoPartSnapshots[partIndex];

                //Get resources
                getPartResources(protoPart, simSnacks);

                //Try getting converters from persistent data
                moduleCount = protoPart.modules.Count;
                for (int moduleIndex = 0; moduleIndex < moduleCount; moduleIndex++)
                {
                    moduleSnapshot = protoPart.modules[moduleIndex];

                    //Get ElectricCharge generators
                    getGenerators(protoPart.partInfo.partConfig, simSnacks);
                    getSolarPanels(protoVessel, protoPart.partInfo.partConfig, moduleSnapshot, simSnacks);

                    //Special case: Get WBIOmniConverter data (if any). Has to be buil-into the part.
                    if (moduleSnapshot.moduleName == "WBIOmniConverter")
                        getOmniConverterResources(moduleSnapshot, simSnacks);

                    //Special case: WBIMultipurpseHab & WBIMultipurposeLab
                    else if (moduleSnapshot.moduleName == "WBIMultipurposeHab" || moduleSnapshot.moduleName == "WBIMultipurposeLab")
                        addMultipurposeConverters(protoPart, moduleSnapshot, moduleIndex, simSnacks);

                    //Normal case: If the module is on the watch list then we can simulate it. Add the converter to the collective if it's active.
                    else if (converterWatchlist.Contains(moduleSnapshot.moduleName))
                        addConverter(protoPart, moduleSnapshot, moduleIndex, simSnacks);
                }
            }

            return simSnacks;
        }

        /// <summary>
        /// Creates a simulator from a loaded vessel
        /// </summary>
        /// <param name="vessel">The Vessel object to query for resources and converters.</param>
        /// <returns>A SimSnacks simulator.</returns>
        public static SimSnacks CreateSimulator(Vessel vessel)
        {
            if (vessel.isEVA)
                return createEvaSimulator(vessel);
            if (!vessel.loaded)
            {
                SimSnacks simulator = CreateSimulator(vessel.protoVessel);
                simulator.vessel = vessel;
                return simulator;
            }

            SimConverter simConverter;
            int partCount = vessel.parts.Count;
            Part part;
            int resourceCount;
            PartResource resource;
            double amount;
            double maxAmount;
            SimResource simResource;
            PartResourceDefinitionList definitions = PartResourceLibrary.Instance.resourceDefinitions;
            ResourceRatio resourceRatio;

            //Get simulator settings
            converterWatchlist = SnacksScenario.Instance.converterWatchlist;

            SimSnacks simSnacks = new SimSnacks();
            simSnacks.secondsPerCycle = SnacksScenario.Instance.simulatorSecondsPerCycle;
            simSnacks.maxSimulatorCycles = SnacksScenario.Instance.maxSimulatorCycles;
            simSnacks.vessel = vessel;

            //Get resources
            for (int partIndex = 0; partIndex < partCount; partIndex++)
            {
                part = vessel.parts[partIndex];

                resourceCount = part.Resources.Count;
                for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                {
                    resource = part.Resources[resourceIndex];

                    //Get amount/max
                    part.GetConnectedResourceTotals(definitions[resource.resourceName].id, out amount, out maxAmount, true);

                    //Create sim resource if needed
                    if (!simSnacks.resources.ContainsKey(resource.resourceName))
                    {
                        simResource = new SimResource();
                        simResource.resourceName = resource.resourceName;
                        simResource.amount = amount;
                        simResource.maxAmount = maxAmount;
                        simSnacks.resources.Add(simResource.resourceName, simResource);
                    }
                }

                //Get ElectricCharge generators
                getGenerators(part.partInfo.partConfig, simSnacks);
            }

            //Deployable solar panels
            List<ModuleDeployableSolarPanel> solarPanels = vessel.FindPartModulesImplementing<ModuleDeployableSolarPanel>();
            ModuleDeployableSolarPanel solarPanel;
            int arrayCount = solarPanels.Count;
            for (int index = 0; index < arrayCount; index++)
            {
                solarPanel = solarPanels[index];
                if (solarPanel.isTracking && solarPanel.deployState != ModuleDeployablePart.DeployState.EXTENDED)
                    continue;

                //Create new sim converter
                simConverter = new SimConverter();
                simConverter.converterName = solarPanel.ClassName;

                resourceRatio = new ResourceRatio();
                resourceRatio.ResourceName = solarPanel.resourceName;
                resourceRatio.Ratio = (solarPanel.chargeRate / 2) * simSnacks.secondsPerCycle;
                resourceRatio.Ratio *= SnacksScenario.GetSolarFlux(vessel) / PhysicsGlobals.SolarLuminosityAtHome;

                simConverter.outputList.Add(resourceRatio);

                simSnacks.converters.Insert(0, simConverter);
            }

            //Get converters
            List<ModuleResourceConverter> converters = vessel.FindPartModulesImplementing<ModuleResourceConverter>();
            ModuleResourceConverter converter;
            int count = converters.Count;
            for (int index = 0; index < count; index++)
            {
                converter = converters[index];
                if (!converter.IsActivated)
                    continue;

                //Create new sim converter
                simConverter = new SimConverter();
                simConverter.converterName = converter.ConverterName;

                //Get inputs
                resourceCount = converter.inputList.Count;
                for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                {
                    resourceRatio = new ResourceRatio();
                    resourceRatio.ResourceName = converter.inputList[resourceIndex].ResourceName;
                    resourceRatio.Ratio = converter.inputList[resourceIndex].Ratio * simSnacks.secondsPerCycle;
                    simConverter.inputList.Add(resourceRatio);
                }

                //Get outputs
                resourceCount = converter.outputList.Count;
                for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                {
                    resourceRatio = new ResourceRatio();
                    resourceRatio.ResourceName = converter.outputList[resourceIndex].ResourceName;
                    resourceRatio.Ratio = converter.outputList[resourceIndex].Ratio * simSnacks.secondsPerCycle;
                    simConverter.outputList.Add(resourceRatio);
                }

                //Get yied resources if any
                if (converter is SnacksConverter)
                {
                    SnacksConverter snacksConverter = (SnacksConverter)converter;
                    resourceCount = snacksConverter.yieldsList.Count;
                    for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
                    {
                        resourceRatio = new ResourceRatio();
                        resourceRatio.ResourceName = snacksConverter.yieldsList[resourceIndex].ResourceName;
                        resourceRatio.Ratio = snacksConverter.yieldsList[resourceIndex].Ratio * simSnacks.secondsPerCycle;
                        simConverter.yieldsList.Add(resourceRatio);
                    }

                    simConverter.yieldSecondsPerCycle = snacksConverter.hoursPerCycle * 3600;
                }

                //Check for snack processor and soil recycler
                if (converter is SoilRecycler)
                {
                    SoilRecycler recycler = (SoilRecycler)converter;
                    updateSoilRecyclerOutput(simConverter.inputList, simConverter.outputList, recycler.RecyclerCapacity);
                }
                else if (converter is SnackProcessor)
                {
                    updateSnacksProcessorOutput(simConverter.outputList);
                }

                //Add converter to the simulator
                if (simConverter.inputList.Count > 0 || simConverter.outputList.Count > 0 || simConverter.yieldsList.Count > 0)
                    simSnacks.converters.Add(simConverter);
            }

            return simSnacks;
        }

        public SimSnacks()
        {
            resources = new Dictionary<string, SimResource>();
            converters = new List<SimConverter>();
            consumedResources = new List<ResourceRatio>();
            producedResources = new List<ResourceRatio>();
            consumedResourceDurations = new Dictionary<string, double>();
        }

        private static SimSnacks createEvaSimulator(Vessel vessel)
        {
            var simulator = new SimSnacks
            {
                vessel = vessel, secondsPerCycle = SnacksScenario.Instance.simulatorSecondsPerCycle,
                maxSimulatorCycles = SnacksScenario.Instance.maxSimulatorCycles
            };
            ProtoCrewMember astronaut = vessel.loaded ? vessel.GetVesselCrew()[0] : vessel.protoVessel.GetVesselCrew()[0];
            AstronautData data = SnacksScenario.Instance.GetAstronautData(astronaut);
            foreach (string resourceName in data.resources.Keys)
            {
                double amount, maximum;
                data.GetResourceAmounts(resourceName, out amount, out maximum);
                simulator.resources.Add(resourceName, new SimResource { resourceName = resourceName, amount = amount, maxAmount = maximum });
            }
            return simulator;
        }
        #endregion

        #region API
        public void RunSimulatorCycle()
        {
            try
            {
                int count;
                ResourceRatio consumerRatio;
                SimResource vesselResourceRatio;

                //Run the converters
                count = converters.Count;
                SimConverter converter;
                for (int index = 0; index < count; index++)
                {
                    converter = converters[index];

                    //Skip the converter if it's inactive.
                    if (!converter.isActivated)
                        continue;

                    //Now process the resources
                    converter.ProcessResources(resources, secondsPerCycle);
                }
                OnConvertersRunComplete?.Invoke(this);

                //Process consumed resources
                count = consumedResources.Count;
                for (int index = 0; index < count; index++)
                {
                    //Get the consumed resource
                    consumerRatio = consumedResources[index];

                    //Get the vessel resource
                    if (!resources.ContainsKey(consumerRatio.ResourceName))
                        continue;
                    vesselResourceRatio = resources[consumerRatio.ResourceName];

                    if (vesselResourceRatio.amount > 0)
                    {
                        //Update simulator cycle duration
                        vesselResourceRatio.durationSeconds += secondsPerCycle;

                        //Adjust amount
                        vesselResourceRatio.amount -= consumerRatio.Ratio;
                        if (vesselResourceRatio.amount <= 0)
                            vesselResourceRatio.amount = 0;

                        //Save current values
                        resources[consumerRatio.ResourceName] = vesselResourceRatio;
                    }
                }

                //Process produced resources
                count = producedResources.Count;
                for (int index = 0; index < count; index++)
                {
                    //Get the consumed resource
                    consumerRatio = producedResources[index];

                    //Get the vessel resource
                    if (!resources.ContainsKey(consumerRatio.ResourceName))
                        continue;
                    vesselResourceRatio = resources[consumerRatio.ResourceName];

                    if (vesselResourceRatio.amount < vesselResourceRatio.maxAmount)
                    {
                        //Update simulator cycle duration
                        vesselResourceRatio.durationSeconds += secondsPerCycle;

                        //Adjust amount
                        vesselResourceRatio.amount += consumerRatio.Ratio;
                        if (vesselResourceRatio.amount > vesselResourceRatio.maxAmount)
                            vesselResourceRatio.amount = vesselResourceRatio.maxAmount;

                        //Save current values
                        resources[consumerRatio.ResourceName] = vesselResourceRatio;
                    }
                }
                OnConsumersRunComplete?.Invoke(this);

                //Check consumed resources to see if we've depleted all of them.
                count = consumedResources.Count;
                int depletedResourcesCount = 0;
                for (int index = 0; index < count; index++)
                {
                    consumerRatio = consumedResources[index];

                    if (!resources.ContainsKey(consumerRatio.ResourceName))
                    {
                        depletedResourcesCount += 1;
                        continue;
                    }
                    vesselResourceRatio = resources[consumerRatio.ResourceName];

                    if (vesselResourceRatio.amount <= 0)
                        depletedResourcesCount += 1;
                }

                //Increment simulator cycle count and inform delegate.
                currentSimulatorCycle += 1;
                OnSimulatorCycleComplete?.Invoke(this);

                //Check our exit conditions
                if (currentSimulatorCycle >= maxSimulatorCycles || exitSimulation || depletedResourcesCount >= consumedResources.Count)
                {
                    //Make sure we exit the simulation.
                    exitSimulation = true;

                    //At this point all we care about are the consumed resource durations. We can discard the rest.
                    count = consumedResources.Count;
                    for (int index = 0; index < count; index++)
                    {
                        consumerRatio = consumedResources[index];

                        if (!resources.ContainsKey(consumerRatio.ResourceName))
                            continue;
                        vesselResourceRatio = resources[consumerRatio.ResourceName];

                        consumedResourceDurations.Add(vesselResourceRatio.resourceName, vesselResourceRatio.durationSeconds);
                    }

                    //Cleanup
                    resources.Clear();
                    consumedResources.Clear();
                    producedResources.Clear();
                    converters.Clear();

                    //Finally, inform the delegate
                    OnSimulationComplete?.Invoke(this);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
                exitSimulation = true;
                Debug.Log("[SimSnacks] - error encountered while running a simulator cycle: " + ex);
                OnSimulatorException?.Invoke(this, ex);
            }
        }
        #endregion

        #region Helpers
        protected static void getSolarPanels(ProtoVessel protoVessel, ConfigNode node, ProtoPartModuleSnapshot moduleSnapshot, SimSnacks simSnacks)
        {
            SimConverter simConverter;
            ResourceRatio resourceRatio;
            string moduleName = null;
            string deployState = string.Empty;
            double chargeRate = 0;
            double solarFlux = 0;

            if (protoVessel.vesselModules.HasNode("SnacksVesselModule"))
            {
                ConfigNode vesselNode = protoVessel.vesselModules.GetNode("SnacksVesselModule");
                if (vesselNode.HasValue("solarFlux"))
                    double.TryParse(vesselNode.GetValue("solarFlux"), out solarFlux);
            }

            if (!node.HasNode("MODULE"))
                return;
            ConfigNode[] moduleNodes = node.GetNodes("MODULE");
            ConfigNode moduleNode;

            for (int index = 0; index < moduleNodes.Length; index++)
            {
                moduleNode = moduleNodes[index];
                if (!moduleNode.HasValue("name"))
                    continue;

                moduleName = moduleNode.GetValue("name");
                if (moduleName != "ModuleDeployableSolarPanel" && moduleName != "KopernicusSolarPanel")
                    continue;

                //Make sure the array is extended (non-deployable arrays are always extended)
                if (moduleSnapshot.moduleValues.HasValue("deployState"))
                    deployState = moduleSnapshot.moduleValues.GetValue("deployState");
                if (deployState != "EXTENDED")
                    continue;

                //Create new sim converter
                simConverter = new SimConverter();
                simConverter.converterName = moduleName;

                if (moduleNode.HasValue("resourceName") && moduleNode.HasValue("chargeRate"))
                {
                    resourceRatio = new ResourceRatio();
                    resourceRatio.ResourceName = moduleNode.GetValue("resourceName");
                    double.TryParse(moduleNode.GetValue("chargeRate"), out chargeRate);
                    resourceRatio.Ratio = (chargeRate / 2) * simSnacks.secondsPerCycle;
                    resourceRatio.Ratio *= solarFlux / PhysicsGlobals.SolarLuminosityAtHome;

                    simConverter.outputList.Add(resourceRatio);

                    simSnacks.converters.Insert(0, simConverter);
                }
            }
        }

        protected static void getGenerators(ConfigNode node, SimSnacks simSnacks)
        {
            SimConverter simConverter;

            if (node != null && node.HasNode("MODULE"))
            {
                ConfigNode[] moduleNodes = node.GetNodes("MODULE");
                ConfigNode moduleNode;
                ConfigNode[] resourceNodes;
                ConfigNode resourceNode;
                string moduleName = null;
                bool isAlwaysActive;
                ResourceRatio ratio;
                double rate;

                for (int index = 0; index < moduleNodes.Length; index++)
                {
                    moduleNode = moduleNodes[index];
                    if (moduleNode.HasValue("name"))
                        moduleName = moduleNode.GetValue("name");
                    if (moduleName == "ModuleGenerator")
                    {
                        //Make sure it's active
                        isAlwaysActive = false;
                        if (moduleNode.HasValue("isAlwaysActive"))
                            bool.TryParse(moduleNode.GetValue("isAlwaysActive"), out isAlwaysActive);
                        if (!isAlwaysActive)
                            continue;

                        //Create converter
                        simConverter = new SimConverter();
                        simConverter.converterName = moduleName;

                        //Get output resources
                        if (moduleNode.HasNode("OUTPUT_RESOURCE"))
                        {
                            resourceNodes = moduleNode.GetNodes("OUTPUT_RESOURCE");
                            for (int resourceIndex = 0; resourceIndex < resourceNodes.Length; resourceIndex++)
                            {
                                resourceNode = resourceNodes[resourceIndex];
                                if (resourceNode.HasValue("name") && resourceNode.HasValue("rate"))
                                {
                                    ratio = new ResourceRatio();
                                    ratio.FlowMode = ResourceFlowMode.ALL_VESSEL;
                                    ratio.ResourceName = resourceNode.GetValue("name");
                                    double.TryParse(resourceNode.GetValue("rate"), out rate);
                                    ratio.Ratio = rate * simSnacks.secondsPerCycle;

                                    simConverter.outputList.Add(ratio);
                                }
                            }
                        }

                        if (simConverter.outputList.Count > 0)
                            simSnacks.converters.Add(simConverter);
                    }
                }
            }
        }

        protected static void getPartResources(ProtoPartSnapshot protoPart, SimSnacks simSnacks)
        {
            ProtoPartResourceSnapshot protoResource;
            SimResource simResource;
            int resourceCount;

            resourceCount = protoPart.resources.Count;
            for (int resourceIndex = 0; resourceIndex < resourceCount; resourceIndex++)
            {
                protoResource = protoPart.resources[resourceIndex];

                //Create resource if needed
                if (!simSnacks.resources.ContainsKey(protoResource.resourceName))
                {
                    simResource = new SimResource();
                    simResource.resourceName = protoResource.resourceName;
                    simResource.amount = 0;
                    simResource.maxAmount = 0;
                    simSnacks.resources.Add(simResource.resourceName, simResource);
                }

                //Now update the resource
                simResource = simSnacks.resources[protoResource.resourceName];
                simResource.amount += protoResource.amount;
                simResource.maxAmount += protoResource.maxAmount;
                simSnacks.resources[protoResource.resourceName] = simResource;
            }
        }

        protected static void addConverter(ProtoPartSnapshot protoPart, ProtoPartModuleSnapshot moduleSnapshot, int moduleIndex, SimSnacks simSnacks)
        {
            ConfigNode[] moduleNodes;
            ConfigNode node = null;
            SimConverter simConverter;
            string moduleName;
            int recyclerCapacity;

            //Skip it if the converter isn't activated.
            if (moduleSnapshot.moduleValues.HasValue("IsActivated"))
            {
                bool isActivated = false;
                bool.TryParse(moduleSnapshot.moduleValues.GetValue("IsActivated"), out isActivated);
                if (!isActivated)
                    return;
            }

            //Get the config node. Module index must match.
            moduleNodes = protoPart.partInfo.partConfig.GetNodes("MODULE");
            if (moduleIndex <= moduleNodes.Length - 1)
                node = moduleNodes[moduleIndex];
            else
                return;

            //We've got a config node, but is it a converter? if so, get its resource lists.
            if (node.HasValue("ConverterName"))
            {
                simConverter = new SimConverter();
                simConverter.converterName = node.GetValue("ConverterName");

                //Get input resources
                if (node.HasNode("INPUT_RESOURCE"))
                    getConverterResources("INPUT_RESOURCE", simConverter.inputList, node, simSnacks.secondsPerCycle);

                //Get output resources
                if (node.HasNode("OUTPUT_RESOURCE"))
                    getConverterResources("OUTPUT_RESOURCE", simConverter.outputList, node, simSnacks.secondsPerCycle);

                //SnacksProcessor and SoilRecycler have customized outputs depending upon game settings. Account for them here.
                moduleName = node.GetValue("name");
                if (moduleName == "SnacksProcessor")
                {
                    updateSnacksProcessorOutput(simConverter.outputList);
                }
                else if (moduleName == "SoilRecycler")
                {
                    recyclerCapacity = 0;
                    if (node.HasValue("RecyclerCapacity"))
                        int.TryParse(node.GetValue("RecyclerCapacity"), out recyclerCapacity);
                    updateSoilRecyclerOutput(simConverter.inputList, simConverter.outputList, recyclerCapacity);
                }

                //Get yield resources
                if (node.HasNode("YIELD_RESOURCE"))
                    getConverterResources("YIELD_RESOURCE", simConverter.yieldsList, node, simSnacks.secondsPerCycle);

                //Hours per cycle
                if (node.HasValue("hoursPerCycle"))
                    double.TryParse(node.GetValue("hoursPerCycle"), out simConverter.yieldSecondsPerCycle);
                simConverter.yieldSecondsPerCycle *= 3600;

                //Add converter to the simulator
                if (simConverter.inputList.Count > 0 || simConverter.outputList.Count > 0 || simConverter.yieldsList.Count > 0)
                    simSnacks.converters.Add(simConverter);
            }
        }

        protected static void updateSnacksProcessorOutput(List<ResourceRatio> outputList)
        {
            double productionEfficiency = SnacksProperties.ProductionEfficiency / 100;
            int count = outputList.Count;
            ResourceRatio ratio;

            for (int index = 0; index < count; index++)
            {
                ratio = outputList[index];
                ratio.Ratio *= productionEfficiency;
                outputList[index] = ratio;
            }
        }

        protected static void updateSoilRecyclerOutput(List<ResourceRatio> inputList, List<ResourceRatio> outputList, int recyclerCapacity)
        {
            int count = inputList.Count;
            ResourceRatio ratio;

            //Recyclers are calibrated for 1 snack per meal, 1 meal per day, at 100% efficiency.
            //For soil recyclers, we want the total recycler output, which is based on snacks per meal, meals per day, recycler capacity and recycler efficiency.
            double recyclerEfficiency = SnacksProperties.RecyclerEfficiency / 100;
            double inputRate = SnacksProperties.SnacksPerMeal * SnacksProperties.MealsPerDay * recyclerCapacity;
            double outputRate = inputRate * recyclerEfficiency;

            for (int index = 0; index < count; index++)
            {
                ratio = inputList[index];
                ratio.Ratio *= inputRate;
                inputList[index] = ratio;
            }

            count = outputList.Count;
            for (int index = 0; index < count; index++)
            {
                ratio = outputList[index];
                ratio.Ratio *= outputRate;
                outputList[index] = ratio;
            }
        }

        protected static void addMultipurposeConverters(ProtoPartSnapshot protoPart, ProtoPartModuleSnapshot moduleSnapshot, int moduleIndex, SimSnacks simSnacks)
        {
            ConfigNode[] moduleNodes;
            ConfigNode node = null;
            SimConverter simConverter;
            string currentTemplateName = string.Empty;
            string templateNodeNames = null;
            bool isDeployed = false;
            bool isInflatable = false;
            ConfigNode[] omniconverterNodes = GameDatabase.Instance.GetConfigNodes("OMNICONVERTER");
            string[] templateTypes; //different types of templates supported by the part
            ConfigNode[] templates = null; //templates in the template type
            List<ConfigNode> templateList = new List<ConfigNode>(); //List of all the templates supported
            ConfigNode currentTemplate = null;
            string moduleName;
            int recyclerCapacity;

            //Get the config node. Module index must match.
            moduleNodes = protoPart.partInfo.partConfig.GetNodes("MODULE");
            if (moduleIndex <= moduleNodes.Length - 1)
                node = moduleNodes[moduleIndex];

            //If the part is inflatable then make sure it's deployed.
            if (node.HasValue("isInflatable"))
            {
                bool.TryParse(node.GetValue("isInflatable"), out isInflatable);
                if (isInflatable)
                {
                    if (moduleSnapshot.moduleValues.HasValue("isDeployed"))
                    {
                        bool.TryParse(moduleSnapshot.moduleValues.GetValue("isDeployed"), out isDeployed);
                        if (!isDeployed)
                            return;
                    }
                    else
                    {
                        return;
                    }
                }
            }

            //Get the template nodes
            //NOTE: I can't determine if a converter in a template node is running so we have to assume that it is.
            if (node.HasValue("templateNodes"))
                templateNodeNames = node.GetValue("templateNodes");
            if (moduleSnapshot.moduleValues.HasValue("templateName"))
                currentTemplateName = moduleSnapshot.moduleValues.GetValue("templateName");
            if (string.IsNullOrEmpty(templateNodeNames))
                return;
            templateTypes = templateNodeNames.Split(new char[] { ';' });
            for (int templateIndex = 0; templateIndex < templateTypes.Length; templateIndex++)
            {
                templates = GameDatabase.Instance.GetConfigNodes(templateTypes[templateIndex]);
                for (int index = 0; index < templates.Length; index++)
                    templateList.Add(templates[index]);
            }

            //Now determine which template we're using.
            int templateCount = templateList.Count;
            for (int index = 0; index < templateCount; index++)
            {
                currentTemplate = templateList[index];
                if (currentTemplate.HasValue("name"))
                {
                    if (currentTemplate.GetValue("name") == currentTemplateName)
                        break;
                    else
                        currentTemplate = null;
                }
                else
                {
                    currentTemplate = null;
                }
            }
            if (currentTemplate == null)
                return;

            //Now find all the converters. They'll either be ModuleResourceConverter or WBIOmniConverter.
            //WBIOmniConverter is saved separately so we can just focus on ModuleResourceConverter.
            ConfigNode[] templateModuleNodes = currentTemplate.GetNodes("MODULE");
            if (templateModuleNodes == null || templateModuleNodes.Length <= 0)
                return;
            for (int index = 0; index < templateModuleNodes.Length; index++)
            {
                node = templateModuleNodes[index];
                if (!node.HasValue("name"))
                    continue;
                moduleName = node.GetValue("name");
                if (converterWatchlist.Contains(moduleName))
                {
                    //Create sim converter
                    simConverter = new SimConverter();
                    if (node.HasValue("ConverterName"))
                        simConverter.converterName = node.GetValue("ConverterName");

                    //Get input resources
                    if (node.HasNode("INPUT_RESOURCE"))
                        getConverterResources("INPUT_RESOURCE", simConverter.inputList, node, simSnacks.secondsPerCycle);

                    //Get output resources
                    if (node.HasNode("OUTPUT_RESOURCE"))
                        getConverterResources("OUTPUT_RESOURCE", simConverter.outputList, node, simSnacks.secondsPerCycle);

                    //SnacksProcessor and SoilRecycler have customized outputs depending upon game settings. Account for them here.
                    if (moduleName == "SnacksProcessor")
                    {
                        updateSnacksProcessorOutput(simConverter.outputList);
                    }
                    else if (moduleName == "SoilRecycler")
                    {
                        recyclerCapacity = 0;
                        if (node.HasValue("RecyclerCapacity"))
                            int.TryParse(node.GetValue("RecyclerCapacity"), out recyclerCapacity);
                        updateSoilRecyclerOutput(simConverter.inputList, simConverter.outputList, recyclerCapacity);
                    }

                    //Add converter to the simulator
                    if (simConverter.inputList.Count > 0 || simConverter.outputList.Count > 0)
                    {
                        simSnacks.converters.Add(simConverter);
                        simSnacks.convertersAssumedActive = true;
                    }
                }
            }
        }

        protected static void getOmniConverterResources(ProtoPartModuleSnapshot moduleSnapshot, SimSnacks simSnacks)
        {
            SimConverter simConverter;
            ConfigNode omniConverterNode = null;
            string currentTemplateName;
            string converterName = string.Empty;
            ConfigNode[] omniconverterNodes = GameDatabase.Instance.GetConfigNodes("OMNICONVERTER");

            //Skip it if the converter isn't activated.
            if (moduleSnapshot.moduleValues.HasValue("IsActivated"))
            {
                bool isActivated = false;
                bool.TryParse(moduleSnapshot.moduleValues.GetValue("IsActivated"), out isActivated);
                if (!isActivated)
                    return;
            }

            if (moduleSnapshot.moduleValues.HasValue("currentTemplateName"))
            {
                //Get the omniconverter template
                omniConverterNode = null;
                currentTemplateName = moduleSnapshot.moduleValues.GetValue("currentTemplateName");
                if (string.IsNullOrEmpty(currentTemplateName))
                    return;
                for (int templateIndex = 0; templateIndex < omniconverterNodes.Length; templateIndex++)
                {
                    if (omniconverterNodes[templateIndex].HasValue("ConverterName"))
                    {
                        converterName = omniconverterNodes[templateIndex].GetValue("ConverterName");
                        if (converterName == currentTemplateName)
                        {
                            omniConverterNode = omniconverterNodes[templateIndex];
                            break;
                        }
                    }
                }

                if (omniConverterNode != null)
                {
                    simConverter = new SimConverter();
                    simConverter.converterName = converterName;

                    //Get input resources
                    if (omniConverterNode.HasNode("INPUT_RESOURCE"))
                        getConverterResources("INPUT_RESOURCE", simConverter.inputList, omniConverterNode, simSnacks.secondsPerCycle);

                    //Get output resources
                    if (omniConverterNode.HasNode("OUTPUT_RESOURCE"))
                        getConverterResources("OUTPUT_RESOURCE", simConverter.outputList, omniConverterNode, simSnacks.secondsPerCycle);

                    //Get yield resources
                    if (omniConverterNode.HasNode("YIELD_RESOURCE"))
                        getConverterResources("YIELD_RESOURCE", simConverter.yieldsList, omniConverterNode, simSnacks.secondsPerCycle);

                    //Hours per cycle
                    if (omniConverterNode.HasValue("hoursPerCycle"))
                        double.TryParse(omniConverterNode.GetValue("hoursPerCycle"), out simConverter.yieldSecondsPerCycle);
                    simConverter.yieldSecondsPerCycle *= 3600;

                    //Add converter to the simulator
                    if (simConverter.inputList.Count > 0 || simConverter.outputList.Count > 0 || simConverter.yieldsList.Count > 0)
                        simSnacks.converters.Add(simConverter);
                }
            }
        }

        protected static void getConverterResources(string nodeName, List<ResourceRatio> resourceList, ConfigNode node, double secondsPerCycle)
        {
            ConfigNode[] resourceNodes;
            ConfigNode resourceNode;
            string resourceName;
            ResourceRatio ratio;

            resourceNodes = node.GetNodes(nodeName);
            for (int resourceIndex = 0; resourceIndex < resourceNodes.Length; resourceIndex++)
            {
                resourceNode = resourceNodes[resourceIndex];
                if (resourceNode.HasValue("ResourceName"))
                    resourceName = resourceNode.GetValue("ResourceName");
                else
                    resourceName = "";

                ratio = new ResourceRatio();
                ratio.ResourceName = resourceName;
                if (resourceNode.HasValue("Ratio"))
                    double.TryParse(resourceNode.GetValue("Ratio"), out ratio.Ratio);
                ratio.Ratio *= secondsPerCycle;
                resourceList.Add(ratio);
            }
        }
        #endregion
    }
}
