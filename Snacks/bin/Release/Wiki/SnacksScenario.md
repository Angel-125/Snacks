            
The SnacksScenario class is the heart of Snacks. It runs all the processes.
        
## Fields

### onSnapshotsUpdated
Tells listeners that snapshots were created.
### onSimulatorCreated
Tells listeners that a simulator was created. Gives mods a chance to add custom converters not covered by Snacks.
### onBackgroundConvertersCreated
Tells listeners that background converters were created. Gives mods a chance to add custom converters not covered by Snacks.
### onSnackTime
Signifies that snacking has occurred.
### onRosterResourceUpdated
Signifies that the roster resource has been updated
### Instance
Instance of the scenario.
### LoggingEnabled
Flag indicating whether or not logging is enabled.
### sciencePenalties
Map of sciecnce penalties sorted by vessel.
### crewData
Map of astronaut data, keyed by astronaut name.
### exemptKerbals
List of kerbals that are exempt from outcome effects.
### cycleStartTime
Last time the processing cycle started.
### backgroundConverters
Map of the background conveters list, keyed by vessel.
### resourceProcessors
List of resource processors that handle life support consumption and waste production.
### snacksPartResources
List of resources that will be added to parts as they are created or loaded.
### snacksEVAResources
List of resources that are added to kerbals when they go on EVA.
### snapshotMap
Map of snapshots, keyed by vessel, that give a status of each vessel's visible life support resources and crew status.
### bodyVesselCountMap
Helper that gives a count, by celestial body id, of how many vessels are on or around the celestial body.
### rosterResources
Map of all roster resources to add to kerbals as they are created.
### lossOfSkillConditions
List of conditions that will cause a skill loss. These conditions are defined via SKILL_LOSS_CONDITION nodes.
### converterWatchlist
List of converters to watch for when creating snapshot simulations.
### simulatorSecondsPerCycle
How many simulated seconds pass per simulator cycle.
### maxSimulatorCycles
Maximum number of simulator cycles to run.
### maxThreads
Max number of simulator threads to create.
## Methods


### UpdateSnapshots
Updates the resource snapshots for each vessel in the game that isn't Debris, a Flag, a SpaceObject, or Unknown.

### GetCrewCapacity(Vessel)
Returns the crew capacity of the vessel
> #### Parameters
> **vessel:** The Vessel to query.

> #### Return value
> The crew capacity.

### FixedUpdate
FixedUpdate handles all the processing tasks related to life support resources and event processing.

### RunSnackCyleImmediately(System.Double)
Runs the snack cyle immediately.
> #### Parameters
> **secondsElapsed:** Seconds elapsed.


### FindVessel(ProtoCrewMember)
Finds the vessel that the kerbal is residing in.
> #### Parameters
> **astronaut:** The astronaut to check.

> #### Return value
> The Vessel where the kerbal resides.

### ShouldRemoveSkills(ProtoCrewMember)
Determines whether or not the kerbal's skills should be removed.
> #### Parameters
> **astronaut:** the ProtoCrewMember to investigate.

> #### Return value
> true, if remove skills should be removed, false otherwise.

### RemoveSkillsIfNeeded(ProtoCrewMember)
Removes the skills if needed. The supplied kerbal must have at least one condition registered in a SKILL_LOSS_CONDITION config node in order to remove the skills.
> #### Parameters
> **astronaut:** The kerbal to check.


### RestoreSkillsIfNeeded(ProtoCrewMember)
Restores the skills if needed. The kerbal in question must not have any conditions that would result in a loss of skill.
> #### Parameters
> **astronaut:** The kerbal to query.


### RemoveSkills(ProtoCrewMember)
Removes skills from the desired kerbal. Does not check to see if they should be removed based on condition summary.
> #### Parameters
> **astronaut:** The ProtoCrewMember to remove skills from.


### RestoreSkills(ProtoCrewMember)
Restores skills to the desired kerbal. Does not check to see if they can be restored based on condition summary.
> #### Parameters
> **astronaut:** 


### SetExemptCrew(System.String)
Adds the name of the kerbal to the exemptions list.
> #### Parameters
> **exemptedCrew:** The name of the kerbal to add to the list.


### RegisterCrew(Vessel)
Registers crew into the astronaut database.
> #### Parameters
> **vessel:** The vessel to search for crew.


### UnregisterCrew(ProtoVessel)
Unregisters the crew from the astronaut database.
> #### Parameters
> **protoVessel:** The vessel to search for crew to unregister.


### UnregisterCrew(Vessel)
Unregisters the crew from the astronaut database.
> #### Parameters
> **vessel:** The vessel to search for crew to unregister.


### RegisterCrew(ProtoCrewMember)
Registers the astronaut into the astronaut database.
> #### Parameters
> **astronaut:** The astronaut to register.


### UnregisterCrew(ProtoCrewMember)
Unregisters the astronaut from the astronaut database.
> #### Parameters
> **astronaut:** The astronaut to unregister.


### UnregisterCrew(Snacks.AstronautData)
Unregisters the astronaut data from the astronaut database.
> #### Parameters
> **data:** The astronaut data to unregister.


### GetNonExemptCrewCount(Vessel)
Returns the number of crew that aren't exempt.
> #### Parameters
> **vessel:** The vessel to query for crew.

> #### Return value
> The number of victims. Er, number of non-exempt crew.

### GetNonExemptCrew(Vessel)
Returns the non-exempt crew in the vessel.
> #### Parameters
> **vessel:** The Vessel to query.

> #### Return value
> An array of ProtoCrewMember objects if there are non-exempt crew, or null if not.

### GetAstronautData(ProtoCrewMember)
Returns the astronaut data associated with the astronaut.
> #### Parameters
> **astronaut:** The ProtoCrewMember to check for astronaut data.

> #### Return value
> The AstronautData associated with the kerbal.

### SetAstronautData(Snacks.AstronautData)
Saves the astronaut data into the database.
> #### Parameters
> **data:** The AstronautData to save.


### AddStressToCrew(Vessel,System.Single)
Adds the stress to crew if Stress is enabled. This is primarily used by 3rd party mods like BARIS.
> #### Parameters
> **vessel:** The Vessel to query for crew.

> **stressAmount:** The amount of Stress to add.


### FormatTime(System.Double,System.Boolean)
Formats the supplied seconds into a string.
> #### Parameters
> **secondsToFormat:** The number of seconds to format.

> **showCompact:** A flag to indicate whether or not to show the compact form.

> #### Return value
> 

### GetSecondsPerDay
Gets the number of seconds per day on the homeworld.
> #### Return value
> The lenght of the solar day in seconds of the homeworld.

### GetSolarFlux(Vessel)
Gets the solar flux based on vessel location.
> #### Parameters
> **vessel:** The vessel to query.

> #### Return value
> The level of solar flux at the vessel's location.

### CreatePrecondition(ConfigNode)
Creates a new precondition based on the config node data passed in.
> #### Parameters
> **node:** The ConfigNode containing data to parse.

> #### Return value
> A BasePrecondition containing the precondition object, or null if the config node couldn't be parsed.

### CreateOutcome(ConfigNode)
Creates a new outcome based on the config node data passed in.
> #### Parameters
> **node:** The ConfigNode containing data to parse.

> #### Return value
> The outcome corresponding to the desired config.

