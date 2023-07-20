            
This is the base class for a resource processor. Similar to ModuleResourceConverter, the consumer will consume resources and produce resources, but it happens at the vessel level, not the part level. It's also designed to work with both loaded and unloaded vessels. Another important difference is that consumed/produced resources can occur on a per crewmember basis; a vessel with 5 crew will consume and/or produce 5 times the resources as a vessel with 1 crewmember. The configuration of a BaseResourceProcessor is done through config files.
        
## Fields

### name
Name of the resource processor
### secondsPerCycle
Number of seconds that must pass before running the consumer.
## Methods


### onKerbalEVA(ProtoCrewMember,Part)
Handles the situation where the kerbal went on EVA.
> #### Parameters
> **astronaut:** The kerbal that went on EVA.

> **part:** The part that the kerbal left.


### onKerbalBoardedVessel(ProtoCrewMember,Part)
Handles the situation where a kerbal boards a vessel.
> #### Parameters
> **astronaut:** The kerbal boarding a vessel.

> **part:** The part boarded.


### onKerbalAdded(ProtoCrewMember)
Handles adding of a new kerbal, giving the consumer a chance to add custom roster data.
> #### Parameters
> **astronaut:** The kerbal being added.


### onKerbalRemoved(ProtoCrewMember)
Handles removal of a kerbal, giving the consumer a chance to update custom data if needed.
> #### Parameters
> **astronaut:** The kerbal being removed.


### onKerbalNameChanged(ProtoCrewMember,System.String,System.String)
Handles a kerbal's name change.
> #### Parameters
> **astronaut:** The kerbal whose name has changed. Note that roster data is already being carried over, this event is used to give consumers a chance to update custom data kept outside of the roster.

> **previousName:** The kerbal's previous name.

> **newName:** The kerbal's new name.


### onVesselLoaded(Vessel)
Handles vessel loaded event, for instance, adding resources that should be on the vessel.
> #### Parameters
> **vessel:** The vessel that was loaded.


### onVesselRecovered(ProtoVessel)
Handles the vessel recovery event
> #### Parameters
> **protoVessel:** The ProtoVessel being recovered


### onVesselGoOffRails(Vessel)
Handles the situation where the vessel goes off rails.
> #### Parameters
> **vessel:** The Vessel going off rails


### OnGameSettingsApplied
Handles changes to game settings.

### LoadProcessors
Loads the SNACKS_RESOURCE_PROCESSOR config nodes and returns a list of processors.
> #### Return value
> A list of resource processors.

### Initialize
Initializes the consumer

### Destroy
Cleanup as processor is about to be destroyed

### OnLoad(ConfigNode)
De-serializes persistence data
> #### Parameters
> **node:** The ConfigNode with the persistence data


### OnSave
Saves persistence data to a ConfigNode and returns it.
> #### Return value
> A ConfigNode containing persistence data, if any.

### AddConsumedAndProducedResources(Vessel,System.Double,System.Collections.Generic.List{ResourceRatio},System.Collections.Generic.List{ResourceRatio})
Used primarily for simulations, returns the consumed and produced resources for the given unit of time.
> #### Parameters
> **vessel:** The vessel to query for data.

> **secondsPerCycle:** The number of seconds to calculate total inputs and outputs.

> **consumedResources:** The list of consumed resources to add the inputs to.

> **producedResources:** The list of produced resources to add the outputs to.


### AddConsumedAndProducedResources(System.Int32,System.Double,System.Collections.Generic.List{ResourceRatio},System.Collections.Generic.List{ResourceRatio})
Used primarily for simulations, returns the consumed and produced resources for the given unit of time.
> #### Parameters
> **crewCount:** The number of crew to simulate.

> **secondsPerCycle:** The number of seconds to calculate total inputs and outputs.

> **consumedResources:** The list of consumed resources to add the inputs to.

> **producedResources:** The list of produced resources to add the outputs to.


### GetUnloadedResourceTotals(ProtoVessel,System.String,System.Double@,System.Double@)
Returns the amount and max amount of the desired resource in the unloaded vessel.
> #### Parameters
> **protoVessel:** The vessel to query for the resource totals.

> **resourceName:** The name of the resource to query.

> **amount:** The amount of the resource that the entire vessel has.

> **maxAmount:** The max amount of the resource that the entire vessel has.


### ProcessResources(Vessel,System.Double,System.Int32,System.Int32)
Runs the processor, consuming input resources, producing output resources, and collating results.
> #### Parameters
> **vessel:** The vessel to run the consumer on.

> **elapsedTime:** Number of seconds that have passed.

> **crewCount:** Number of crew aboard the vessel.

> **crewCapacity:** The vessel's total crew capacity.


