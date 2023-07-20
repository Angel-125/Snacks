# SnacksUtils


# CheckBreathableAir
            
This precondition checks to see if a kerbal or vessel is in an environemnt with breathable air, and matches it with the expected parameter. The vessel's celestial body must have an atmosphere with oxygen, and the vessel altitude must be between sea level and half the atmosphere height. Example definition: PRECONDITION { name = CheckBreathableAir mustExist = false }
        
## Fields

### mustExist
Flag to indicate pressence (true) or absence (false) of the value to check.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckSkillLevel
            
This precondition checks to see if a kerbal's experience level matches the desired value and type of check to make. For instance you could check to see if a kerbal is above 3 stars. Example definition: PRECONDITION { name = CheckSkillLevel valueToCheck = 3 checkType = checkGreaterOrEqual //Default value } 
        
## Fields

### valueToCheck
The value to check for
### checkType
Type of check to make Default: checkGreaterOrEqual
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckSkill
            
This precondition checks to see if a kerbal's skill matches the desired parameter. For instance, you could check to see if a kerbal has the ScienceSkill. Example definition: PRECONDITION { name = CheckSkill skillToCheck = ScienceSkill mustExist = true } 
        
## Fields

### 
The value to check for
### 
Type of check to make Default: checkGreaterOrEqual
### skillToCheck
Name of the skill to check
### mustExist
Flag to indicate pressence (true) or absence (false) of the value to check.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckTrait
            
This precondition checks to see if a kerbal's trait matches the desired parameter. For instance, you could check to see if a kerbal is an Engineer. Example definition: PRECONDITION { name = CheckTrait traitToCheck = Engineer mustExist = true } 
        
## Fields

### traitToCheck
Name of the trait to check
### mustExist
Flag to indicate pressence (true) or absence (false) of the value to check.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckCourage
            
This precondition checks to see if a kerbal's courage matches the desired value and type of check to make. Example definition: PRECONDITION { name = CheckCourage valueToCheck = 0.5 checkType = checkEquals //Default value } 
        
## Fields

### valueToCheck
The value to check for
### checkType
Type of check to make Default: checkEquals
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckStupidity
            
This precondition checks to see if a kerbal's stupidity matches the desired value and type of check to make. Example definition: PRECONDITION { name = CheckStupidity valueToCheck = 0.5 checkType = checkEquals //Default value } 
        
## Fields

### valueToCheck
The value to check for
### checkType
Type of check to make Default: checkEquals
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckCrewCount
            
This precondition checks to see if a vessel's crew count matches the desired parameter. Example definition: PRECONDITION { name = CheckCrewCount valueToCheck = 1 checkType = checkEquals //Default value } 
        
## Fields

### valueToCheck
The value to check for
### checkType
Type of check to make
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckBadass
            
This precondition checks to see if a kerbal's badass status against the badassExists flag. Both must match in order for the precondition to be validated. Example definition: PRECONDITION { name = CheckBadass mustExist = true } 
        
## Fields

### mustExist
Flag to indicate pressence (true) or absence (false) of the value to check.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckVesselStatus
            
This precondition checks the vessel status against the supplied parameters. Example definition: PRECONDITION { name = CheckVesselStatus situation = LANDED situation = SPLASHED } 
        
## Fields

### situationsToCheck
List of situations to check the vessel against. In the config file, separate each situation to check on a separate line. Ex: situation = LANDED situation = SPLASHED Valid situations: LANDED, SPLASHED, ESCAPING, FLYING, ORBITING, SUB_ORBITAL, PRELAUNCH
### bodyName
Optional name of the planetary body where the vessel must be located.
### metersAltitude
Optional altitude in meters that the vessel must be at.
### checkType
The type of check to make against metersAltitude.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckGravityLevel
            
This precondition checks to see if a vessel or roster resource meets the supplied parameters. Gravity checks can be negated by setting CheckGravityLevel.checkType, where checkType is one of the conditional qualifiers. For instance, CheckGravityLevel.checkLesserOrEqual will disqualify any microgravity event checks and is useful for centrifuges. Example definition: PRECONDITION { name = CheckGravityLevel valueToCheck = 0.1 checkType = checkLesserOrEqual //Default value } 
        
## Fields

### valueToCheck
The value to check for
### checkType
The conditional type to use during the validation.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckValueConditionals
            
This enum represents the key-value conditionals to check.
        
## Fields

### checkEquals
Key-value must be equal to the supplied value.
### checkNotEqual
Key-value must not be equal to the supplied value.
### checkGreaterThan
Key-value must be greater than the supplied value.
### checkLessThan
Key-value must be less than the supplied value.
### checkGreaterOrEqual
Key-value must be greater than or equal to the supplied value.
### checkLesserOrEqual
Key-value must be less than or equal to the supplied value.

# CheckKeyValue
            
This precondition Checks a kerbal's key-value and validates it against the supplied parameters. Example definition: PRECONDITION { name = CheckKeyValue keyValueName = State checkType = checkEquals stringValue = Bored } 
        
## Fields

### keyValueName
Name of the key-value
### stringValue
String value of the key. Takes precedence over the int values.
### intValue
Integer value of the key
### checkType
Type of check to make
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckCondition
            
This precondition Checks a kerbal's condition summary to see if it exists or not. The precondition is valid if the kerbal's condition summary matches the parameters of the precondition. Example definition: PRECONDITION { name = CheckCondition conditionSummary = Sleepy mustExist = true } 
        
## Fields

### conditionSummary
Name of the condition to check
### mustExist
Flag to indicate pressence (true) or absence (false) of the value to check.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckRandomChance
            
This precondition rolls a random die between a minimum and maximum value and compares it to a target number. If the roll meets or exceeds the target number then the precondition passes. Example definition: PRECONDITION { name = CheckRandomChance dieRollMin = 1 dieRollMax = 1000 targetNumber = 999 } 
        
## Fields

### dieRollMin
Minimum value on the die roll
### dieRollMax
Maximum value on the die roll
### targetNumber
Target number required to declare the precondition valid.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# CheckResultTypes
            
Enumerator with the type of results to check.
        
## Fields

### resultConsumptionSuccess
Check for a successful consumption
### resultConsumptionFailure
Check for a consumption failure
### resultProductionSuccess
Check for a production success
### resultProductionFailure
Check for a production failure

# CheckProcessorResult
            
This precondition checks the specified processor for desired results. Example definition: PRECONDITION { name = CheckProcessorResult type = resultConsumptionFailure processorName = Snacks! resourceName = Snacks cyclesRequired = 1 } 
        
## Fields

### resultType
The type of result to check
### processorName
The name of the processor to check
### resourceName
The name of the resource to check
### cyclesRequired
The number of process cycles to check
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# ClearKeyValue
            
This outcome removes the desired key-value from the affected kerbals Example definition: OUTCOME { name = ClearKeyValues conditionSummary = Sick }  
        
## Fields

### keyValueName
Name of the key-value
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


# ClearCondition
            
This outcome removes the desired condition on the affected kerbals Example definition: OUTCOME { name = ClearCondition conditionSummary = Sick }   
        
## Fields

### conditionName
Name of the condition to set
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


# ConsumeResource
            
This outcome consumes the specified resource in the desired amount. It can be a vessel resource or a roster resource. Example definition: OUTCOME { name = ConsumeResource resourceName = Stress amount = 1 }
        
## Fields

### resourceName
Name of the resource to produce
### randomMin
Optional minimum value of random amount to produce.
### randomMax
Optional maximum value of random amount to produce.
### amount
Amount of resource to consume. Takes presedence over randomMin and randomMax
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


# BasePrecondition
            
A precondition is a check of some type that prevents outcomes from being applied unless the precondition's check suceeds.
        
## Fields

### name
Name of the precondition.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode specifying the initialization parameters.


### IsValid(ProtoCrewMember,Vessel)
Determines if the precondition is valid.
> #### Parameters
> **astronaut:** The ProtoCrewModule to check.

> **vessel:** The Vessel to check

> #### Return value
> 

### IsValid(ProtoCrewMember)
Determines if the precondition is valid.
> #### Parameters
> **astronaut:** The ProtoCrewModule to check.

> #### Return value
> 

# OnStrikePenalty
            
This outcome sets a condition on the affected kerbals. If that condition is defined in a SKILL_LOSS_CONDITION config node, then the kerbals' skills will be removed until the condition is cleared. Example definition: OUTCOME { name = ClearCondition conditionSummary = Stressed Out }   
        
## Fields

### conditionName
The name of the condition to set. If defined in a SKILL_LOSS_CONDITION node then the affected kerbals will lose their skills until the condition is cleared.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **conditionName:** The name of the condition to set. It must be added to a SKILL_LOSS_CONDITION config node in order for the kerbal to lose its skills.

> **canBeRandom:** If set to true it can be randomly selected from the outcomes list.

> **playerMessage:** A string containing the bad news.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


# ProduceResource
            
This outcome produces the specified resource in the desired amount. It can be a vessel resource or a roster resource. Example definition: OUTCOME { name = ProduceResource resourceName = Stress amount = 1 }
        
## Fields

### resourceName
Name of the resource to produce
### randomMin
Optional minimum value of random amount to produce.
### randomMax
Optional maximum value of random amount to produce.
### amount
Amount of resource to produce. Takes presedence over randomMin and randomMax
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **resourceName:** The name of the resource to produce. It can be a roster resource.

> **amount:** The amount of resource to produce

> **canBeRandom:** If set to true it can be randomly selected from the outcomes list.

> **playerMessage:** A message for the player.


# CheckResource
            
This precondition checks to see if a vessel or roster resource meets the supplied parameters. Example definition: PRECONDITION { name = CheckResource resourceName = Stress checkType = checkEquals valueToCheck = 3.0 } 
        
## Fields

### resourceName
Name of the resource to check
### checkType
The conditional type to use during the validation.
### valueToCheck
The value to check for
### checkMaxAmount
Flag to indicate whether or not to check the resource's max amount instead of the curren amount;
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


# ProcessedResource
            
This class represents resources consumed or produced by a SnacksResourceProcessor. Consumption and production is applied vessel-wide, or to individual kerbal roster entries depending on the configuration. If applied vessel-wide, the resource can be produced or consumed per kerbal. Finally, the resource can be displayed in the Snapshots view.
        
## Fields

### resourceName
Name of the consumed/produced resource
### dependencyResourceName
Name of the dependency resource if the resource to process depends upon the results of another resource's process result. E.G. 1 unit of Soil is produced for 1 unt of Snacks consumed.
### isRosterResource
Flag to indicate whether or not the resource is applied to roster entries instead of parts and vessels. if set to true, then appliedPerCrew is ignored. Default: false
### showInSnapshot
Flag to indicate whether or not to show the resource in the Snapshots window. Ignored if isRosterResource is set to true. Default: true
### failureResultAppliesOutcomes
Flag to indicate whether or not a failure result applies the processor's outcomes. Default: true
### amount
The amount of resource to consume or produce. If appliedPerCrew is true, then the amount consumed/produced is multiplied by the number of crew aboard the vessel. If isRosterResource is true, then each individual crew member's roster entry will be affected instead. Default: 0
### clearDataDuringRecovery
Flag to indicate that astronaut data should be cleared when a vessel is recovered. Default: true
## Methods


### Load(ConfigNode)
Loads the fields from the config node.
> #### Parameters
> **node:** A ConfigNode containing fields to load.


### Save
Saves current values to a ConfigNode.
> #### Return value
> A ConfigNode containing the field data.

### ConsumeResource(Vessel,System.Double,System.Int32,System.Int32)
Consumes the resource.
> #### Parameters
> **vessel:** The vessel to work on

> **elapsedTime:** Elapsed seconds

> **crewCount:** Current crew count

> **crewCapacity:** Current crew capacity

> #### Return value
> A SnacksConsumerResult containing the resuls of the consumption.

### ProduceResource(Vessel,System.Double,System.Int32,System.Int32,System.Collections.Generic.Dictionary{System.String,Snacks.SnacksProcessorResult})
Produces the resource
> #### Parameters
> **vessel:** The vessel to work on

> **elapsedTime:** Elapsed seconds

> **crewCount:** Current crew count

> **crewCapacity:** Current crew capacity

> **consumptionResults:** Results of resource consumption.

> #### Return value
> A SnacksConsumerResult containing the resuls of the production.

# SnacksProcessorResult
            
This is a result that has data regarding what happened during resource consumption or production.
        
## Fields

### resourceName
Name of the resource that was processed.
### resultType
Type of result
### completedSuccessfully
Flag to indicate whether or not the process completed successfully.
### appliedPerCrew
Flag indicating if the process was applied per crew member.
### affectedKerbalCount
Number of kerbals affected by the process.
### currentAmount
Current amount of the resource in the vessel/kerbal.
### maxAmount
Max amount of the resource in the vessel/kerbal.
### crewCount
Current number of crew aboard the vessel
### crewCapacity
Total crew capacity.
### afftectedAstronauts
List of individual astronauts affected by the result.

# BaseResourceProcessor
            
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


# AstronautData
            
This class contains data related to a kerbal. Information includes roster resources (characteristics of the kerbal akin to Courage and Stupidity), a condition summary specifying what states the kerbal is in, a list of disqualified conditions that will auto-fail precondition checks, a list of processor successes and failures, a key-value map suitable for tracking states in the event system, an exempt flag that exempts the kerbal from all outcomes.
        
## Fields

### name
Name of the kerbal.
### experienceTrait
The kerba's current experience trait.
### lastUpdated
Timestamp of when the astronaut data was last update.
### isExempt
Flag to indicate that the kerbal is exempt from outcomes.
### conditionSummary
Summary of all the conditions that the kerbal currently has. If a condition in the summary is defined in a SKILL_LOSS_CONDITION config node, then the kerbal will lose its skills until the condition is cleared.
### keyValuePairs
A map of key-value pairs.
### processedResourceSuccesses
Map of successful process cycles. The key is the name of the processor, the value is the number of successes.
### processedResourceFailures
Map of unsuccessfull process cycles. The key is the name of the processor, the value is the number of failures.
### rosterResources
A map of roster resources (characteristics of the kerbal), similar to vessel resources.
### disqualifiedPreconditions
Conditions that will automatically disqualify a precondition check.
## Methods


### Constructor
Initializes a new instance of the class.

### Load(ConfigNode)
Loads the astronaut data from the config node supplied.
> #### Parameters
> **node:** The ConfigNode to read data from.

> #### Return value
> A map keyed kerbal name that contains astronaut data.

### Save(DictionaryValueList{System.String,Snacks.AstronautData},ConfigNode)
Saves persistent astronaut data to the supplied config node.
> #### Parameters
> **crewData:** A map of astronaut data, keyed by kerbal name.

> **node:** The ConfigNode to save the data to.


### SetDisqualifier(System.String)
Sets a disqualifier that will automatically fail a precondition check.
> #### Parameters
> **disqualifier:** The name of the disqualifier to set.


### ClearDisqualifier(System.String)
Clears a disqualifier that will no longer fail a precondition check.
> #### Parameters
> **disqualifier:** The name of the disqualifier to clear.


### SetCondition(System.String)
Sets a condition that could result in loss of skills if defined in a SKILL_LOSS_CONDITION config node. The condition will appear in the kerbal's condition summary in the status window.
> #### Parameters
> **condition:** The name of the condition to set.


### ClearCondition(System.String)
Clears a condition, removing it from the condition summary display. If the condition is defined in a SKILL_LOSS_CONDITION config node, and the kerbal has no other conditions that result from skill loss, then the kerbal will regain its skills.
> #### Parameters
> **condition:** The name of the condition to clear.


# DeathPenalty
            
This outcome causes affected kerbals to die. Example definition: OUTCOME { name = DeathPenalty resourceName = Snacks cyclesBeforeDeath = 10 }   
        
## Fields

### resourceName
The name of the resource to check for failed processor cycles.
### cyclesBeforeDeath
The number of cycles that must fail before the kerbal dies.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **resourceName:** The name of the resource to check. If no processor has a failed cycle with the resource then the outcome is invalidated.

> **cyclesBeforeDeath:** The number of failed processor cycles required before applying the outcome.

> **playerMessage:** A string containing the bad news for the player.


# FaintPenalty
            
This outcome causes affected kerbals to faint. Example definition: OUTCOME { name = FaintPenalty resourceName = Snacks cyclesBeforeFainting = 3 faintDurationSeconds = 180 }   
        
## Fields

### resourceName
The name of the resource to check for failed processor cycles.
### cyclesBeforeFainting
The number of cycles that must fail before the kerbal faints.
### faintDurationSeconds
The number of seconds that the kerbal will faint for.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **resourceName:** The name of the resource to check. If none of the resource processors have failed cycles containing the resource then the outcome is invalidated.

> **cyclesBeforeFainting:** The number of failed cycles required before applying the outcome..

> **faintDurationSeconds:** Faint duration seconds.

> **playerMessage:** A string containing the bad news for the player.


# FundingPenalty
            
This outcome fines the space agency by a certain amount per affected kerbal. Example definition: OUTCOME { name = FundingPenalty finePerKerbal = 1000 }   
        
## Fields

### finePerKerbal
The amount of Funds to lose per kerbal.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **canBeRandom:** If set to true the outcome can be randomly selected from the outcome list.

> **playerMessage:** A string containing the bad news.

> **finePerKerbal:** The amount of Funds lost per affected kerval.


# SnacksRosterResource
            
This class represents a resource that's tied to individual kerbals instead of a part. One example is Stress, an abstracted habitation mechanic.
        
## Fields

### resourceName
The name of the roster resource.
### displayName
Public display name of the resource.
### showInSnapshot
Flag to indicate whether or not to show the resource in the Snapshots window. Default: true
### amount
The amount of resource available.
### maxAmount
The maximum amount of resource allowed.
### experienceBonusAmount
The amount of resource to add when the kerbal levels up.
### experienceBonusMaxAmount
The maximum amount of resource to add when the kerbal levels up.
## Methods


### onKerbalLevelUp(ProtoCrewMember)
Handles the kerbal level up event
> #### Parameters
> **astronaut:** The ProtoCrewMember that has leveled up.


### onCrewBoardedVessel(Part,Part)
Handles the crew boarded event. The resource is removed from the kerbal and added to the vessel.
> #### Parameters
> **evaKerbal:** The kerbal that is returning from EVA

> **boardedPart:** The part that the kerbal boarded


### onCrewEVA(Part,Part)
Handles the crew eva event. The kerbal gains the EVA resource and the vessel loses a corresponding amount.
> #### Parameters
> **evaKerbal:** The kerbal that went on EVA

> **partExited:** The part that the kerbal exited


### addResourceIfNeeded(ProtoCrewMember)
Adds the roster resource to the kerbal if needed
> #### Parameters
> **astronaut:** The ProtoCrewMember to check.


### addResourceIfNeeded(Vessel)
Adds the roster resource to the vessel's kerbal if needed
> #### Parameters
> **vessel:** The Vessel whose crew to check


# StressRosterResource
            
This is a helper class to handle the unique conditions of a kerbal leveling up with the Stress resource.
        

# StressProcessor
            
The Stress processor is designed to work with the Stress roster resource. Essentially, Stress is an abstracted habitation mechanic that takes into account a variety of different events. The main thing that causes Stress is being aboard a vessel; you don't want to send kerbals to Jool in a Mk1 command pod! NASA allocates 25 m^3 of space per astronaut per year aboard the ISS, and Stress is based off that number. The larger the habitable volume, the greater a kerbal's maximum Stress becomes, and it's dynamically updated whenever a kerbal changes craft. Assuming no other events, a kerbal will accumulate 1 point of Stress per day, and when the kerbal reaches it's maximum Stress, bad things happen.
        
## Fields

### MaxSeatsForMultiplier
The first N seats use the multiplier instead of the N^3 formula.
### SpacePerSeatMultiplier
How much Space a single seat provides, assuming that the vessel's number of seats is less than or equal to MaxSeatsForMultiplier.
## Methods


### CalculateSpace(Vessel)
Calculates how much Space a vessel has. It is a function of crew capacity and is influenced by the number of crew currently aboard.
> #### Parameters
> **vessel:** The Vessel to query.

> #### Return value
> The amount of Space aboard the vessel.

### CalculateSpace(System.Int32,System.Int32)
Calculates how much Space a vessel has. It is a function of crew capacity and is influenced by the number of crew currently aboard.
> #### Parameters
> **crewCount:** Current crew count aboard the vessel

> **crewCapacity:** Current crew capacity of the vessel

> #### Return value
> The amount of Space aboard the vessel.

# SnacksEventCategories
            
Enumerator specifying the different types of events
        
## Fields

### categoryPostProcessCycle
Event is processed after the resource process cycle completes.
### categoryEventCard
The event is chosen at random once per process cycle.
### categoryKerbalLevelUp
The event is processed when a kerbal levels up.

# KerbalsAffectedTypes
            
Enumerator specifying which kerbals are affected by the preconditions.
        
## Fields

### affectsRandomAvailable
A single available kerbal is chosen at random.
### affectsRandomAssigned
A single assigned kerbal is chosen at random.
### affectsAllAvailable
All available kerbals are affected.
### affectsAllAssigned
All assigned kerbals are affected.
### affectsRandomCrewPerVessel
A single random kerbal is chosesn amongst each crewed vessel.

# SnacksEvent
            
This class represents an "event" in Snacks. Events consist of one or more preconditions and one or more outcomes. Preconditions are things like random numbers, the pressence of specific conditions, and the like. All preconditions must be met before the event outcomes can be applied. The outcomes include all the Snacks penalties as well as other things such as setting conditions.
        
## Fields

### 
Event is processed after the resource process cycle completes.
### 
The event is chosen at random once per process cycle.
### 
The event is processed when a kerbal levels up.
### eventCategory
The event's category
### affectedKerbals
The type of kerbals affected by the event.
### secondsBetweenChecks
Number of seconds that must pass before the event can be checked.
### daysBetweenChecks
The number of day that must pass before the event can be checked. Overrides secondsBetweenChecks.
### playerMessage
Player-friendly message to display when outcomes are going to be applied.
### name
Name of the event
## Methods


### Constructor
Initializes a new instance of the class.

### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode specifying the initialization parameters.


### ApplyOutcomes(ProtoCrewMember,Vessel)
Applies outcomes to the supplied astronaut
> #### Parameters
> **astronaut:** The ProtoCrewMember to apply outcomes to.

> **vessel:** The Vessel to check


### ApplyOutcomes(ProtoCrewMember)
Applies outcomes to the supplied astronaut
> #### Parameters
> **astronaut:** The ProtoCrewMember to apply outcomes to.


### PreconditionsValid(ProtoCrewMember,Vessel)
Checks all preconditions against the supplied astronaut
> #### Parameters
> **astronaut:** The ProtoCrewMember to check

> **vessel:** The Vessel to check

> #### Return value
> 

### PreconditionsValid(ProtoCrewMember)
Checks all preconditions against the supplied astronaut
> #### Parameters
> **astronaut:** The ProtoCrewMember to check

> #### Return value
> 

### IsTimeToCheck(System.Double)
Determines if the event can be evaluated based on the supplied elapsed time.
> #### Parameters
> **elapsedTime:** The number of seconds that have passed since the last inquiry.

> #### Return value
> true if it's time to evaluate the event, false if not.

### ProcessEvent(System.Double)
Processes the event based on elapsed time, event type, and kerbals affected.
> #### Parameters
> **elapsedTime:** The elapsed time since the last process cycle, ignored for event cards.


### Load(ConfigNode)
Loads the persistent data.
> #### Parameters
> **node:** A ConfigNode with persistent data.


### Save
Saves the persistent data.
> #### Return value
> A ConfigNode with persistent data.

# SetCondition
            
This outcome sets the desired condition on the affected kerbals Example definition: OUTCOME { name = SetCondition conditionSummary = Sick }   
        
## Fields

### conditionName
Name of the condition to set
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


# SetKeyValue
            
This outcome sets the desired key-vale on the affected kerbals Example definition: OUTCOME { name = SetKeyValue keyValueName = DaysBored intValue = 1 }  
        
## Fields

### keyValueName
Name of the key-value
### stringValue
String value of the key. Takes precedence over the int values.
### intValue
Integer value of the key
### addIntValue
Integer value to add to the existing key value. If key doesn't exist then it will be set to this value instead. Taks precedence over intValue.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


# SnacksDisqualifier
            
This part module is designed to negate one or more preconditions so long as the kerbal resides in the part. An example would be a centrifuge
        
## Fields

### disqualifiedPreconditions
Contains the disqualified preconditions such as CheckGravityLevel.checkLesserOrEqual for low gravity checks. Separate disqualified preconditions by semicolon. Most of the preconditions can be disqualified simply by stating their name. If a precondition requires something different, be sure to check its documentation.

# RepPenalty
            
This outcome reduces the space agency's reputation based on the supplied parameters. Example definition: OUTCOME { name = RepPenalty repLossPerKerbal = 5 }   
        
## Fields

### repLossPerKerbal
The rep loss per kerbal.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **canBeRandom:** If set to true it can be randomly selected from the outcomes list.

> **repLossPerKerbal:** Rep loss per kerbal.

> **playerMessage:** A string containing the bad news.


# SciencePenalty
            
This outcome disrupts science experiments aboard a vessel. Example definition: OUTCOME { name = SciencePenalty }   
        
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **canBeRandom:** If set to true it can be randomly selected from the outcomes list.


# ISnacksPenalty
            
Interface for creating and running penalties when a processor resource runs out or has too much aboard the vessel or kerbal.
        
## Methods


### IsEnabled
Indicates whether or not the penalty is enabled.
> #### Return value
> true if inabled, false if not.

### AlwaysApply
Indicates whether or not the penalty is always applied instead of randomly chosen.
> #### Return value
> true if the penalty should always be applied, false if not.

### ApplyPenalty(System.Int32,Vessel)
Applies the penalty to the affected kerbals
> #### Parameters
> **affectedKerbals:** An int containing the number of kerbals affected by the penalty.

> **vessel:** The vessel to apply the penalty to.


### RemovePenalty(Vessel)
Removes penalty effects.
> #### Parameters
> **vessel:** The vessel to remove the penalt effects from.


### GameSettingsApplied
Handles changes in game settings, if any.

# SnacksScenario
            
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

# SnackProcessor
            
The SnacksProcessor grinds out Snacks from Ore. It is derived from the SnacksConverter. The output of the processor is affected by the game settings.
        
## Fields

### dailyOutput
A status field showing the daily output of Snacks.
### originalSnacksRatio
Helper field describing the original output ratio of Snacks.
### sourceInputRatio
Helper field to describe the original input ratio of Ore
## Methods


### GetDailySnacksOutput
Gets the daily snacks output.
> #### Return value
> The amount of Snacks produced daily, subjected to game settings.

# SnacksBackroundEmailTypes
            
This enum specifies the diffent types of emails to send during background processing.
        
## Fields

### missingResources
The processor is missing an input resource.
### missingRequiredResource
The processor is missing a required resource.
### containerFull
The vessel is out of room.
### yieldCriticalFail
The yield experienced a critical failure.
### yieldCriticalSuccess
The yield has had a critical success.
### yieldLower
The yield amount was lower than normal.
### yieldNominal
The yield amount was normal.

# SnacksBackgroundConverter
            
This class runs active converters in the background, consuming inputs, producing outputs, and yielding resources.
        
## Fields

### ConverterName
Name of the converter
### moduleName
Name of the converter part module.
### IsActivated
Flag indicating that the converter is active.
### isMissingResources
Flag indicating that the converter is missing resources.
### isContainerFull
Flag indicating that the container is full.
### inputEfficiency
The input efficiency.
### outputEfficiency
The output efficiency.
## Methods


### GetBackgroundConverters
Parses a vessel to find active converters to run in the background.
> #### Return value
> A map keyed by Vessel that has a list of running converters to run in the background.

### Constructor
Initializes a new instance of the class.
> #### Parameters
> **protoPart:** The ProtPartSnapshot that hosts the converter.

> **protoModule:** The ProtoPartModuleSnapshot representing the converter.

> **moduleIndex:** The module index.


### Constructor
Initializes a new instance of the class.

### CheckRequiredResources(ProtoVessel,System.Double)
Checks to be sure the vessel has the required resources.
> #### Parameters
> **vessel:** The Vessel to check.

> **elapsedTime:** The seconds that have elapsed.


### ConsumeInputResources(ProtoVessel,System.Double)
Consumes the input resources.
> #### Parameters
> **vessel:** The Vessel to consume resources from.

> **elapsedTime:** Converter elapsed time.


### ProduceOutputResources(ProtoVessel,System.Double)
Produces the output resources.
> #### Parameters
> **vessel:** The Vessel to add resources to.

> **elapsedTime:** Converter elapsed time.


### ProduceyieldsList(ProtoVessel)
Produces the yield resources
> #### Parameters
> **vessel:** The Vessel to add resources to.


### PrepareToProcess(ProtoVessel)
Prepares the converter to process.
> #### Parameters
> **vessel:** The Vessel to check for preparations.


### PostProcess(ProtoVessel)
Handles post process tasks for the converter.
> #### Parameters
> **vessel:** The Vessel to update.


# SnacksRosterRatio
            
The SnacksRosterRatio is a helper struct that is similar to a ResourceRatio, but it's designed for use with roster resources (characteristics of a kerbal).
        
## Fields

### ResourceName
The name of the resource.
### AmountPerDay
The amount per day. This value overwrites AmountPerSecond and is based on the homeworld's second per day.
### AmountPerSecond
The amount per second.

# SnacksConverter
            
An enhanced version of ModuleResourceConverter, the SnacksConverter offers a number of enhancements including producing resources after a set number of hours have elapsed (defined by YIELD_RESOURCES nodes), the ability to produce the yield resources based on the result of a random number generation, an optional flag that results in the part exploding as a result of a critical failure roll, an optional flag that can prevent the converter from being shut off, the ability to play effects, and the ability to be run in the background (when the vessel isn't loaded into the scene).
        
## Fields

### startEffect
Name of the effect to play when the converter starts.
### stopEffect
Name of the effect to play when the converter stops.
### runningEffect
Name of the effect to play while the converter is running.
### minimumVesselPercentEC
This is a threshold value to ensure that the converter will shut off if the vessel's ElectricCharge falls below the specified percentage. It is ignored if the converter doesn't use ElectricCharge.
### requiresHomeConnection
This flag tells the converter to check for a connection to the homeworld if set to true. If no connection is present, then the converter operations are suspended. It requires CommNet to be enabled.
### minimumCrew
This field specifies the minimum number of crew required to operate the converter. If the part lacks the minimum required crew, then operations are suspended.
### conditionSummary
This field specifies the condition summary to set when a kerbal enters the part and the converter is running. For example, the kerbal could be Relaxing. The condition summary appears in the kerbal's condition summary display. Certain conditions will result a loss of skills for the duration that the converter is running. For that to happen, be sure to define a SKILL_LOSS_CONDITION config node with the name of the condition.
### canBeShutdown
This field indicates whether or not the converter can be shut down. If set to false, then the converter will remove the shutdown and toggle actions and disable the shutdown button.
### ID
Unique ID of the converter. Used to identify it during background processing.
### dieRollMin
Minimum die roll
### dieRollMax
Maximum die roll
### minimumSuccess
On a roll of dieRollMin - dieRollMax, the minimum roll required to declare a successful resource yield. Set to 0 if you don't want to roll for success.
### criticalSuccess
On a roll of dieRollMin - dieRollMax, minimum roll for a resource yield to be declared a critical success.
### criticalFail
On a roll of dieRollMin - dieRollMax, the maximum roll for a resource yield to be declared a critical failure.
### hoursPerCycle
How many hours to wait before producing resources defined by YIELD_RESOURCE nodes.
### cycleStartTime
The time at which we started a new resource production cycle.
### progress
Current progress of the production cycle
### timeRemainingDisplay
Display field to show time remaining on the production cycle.
### lastAttempt
Results of the last production cycle attempt.
### criticalSuccessMultiplier
If the yield check is a critical success, multiply the units produced by this number. Default is 1.0.
### failureMultiplier
If the yield check is a failure, multiply the units produced by this number. Default is 1.0.
### explodeUponCriticalFail
Flag to indicate whether or not the part explodes if the yield roll critically fails.
### elapsedTime
The amount of time that has passed since the converter was last checked if it should produce yield resources.
### secondsPerCycle
The number of seconds per yield cycle.
### yieldsList
The list of resources to produce after the elapsedTime matches the secondsPerCycle.
### rosterInputList
Similar to an input list, this list contains the roster resources to consume during the converter's processing.
### rosterOutputList
Similar to an output list, this list contains the roster resources to produce during the converter's processing.
### missingResources
The converter is missing resources. If set to true then the converter's operations are suspended.
### crewEfficiencyBonus
The efficieny bonus of the crew.
## Methods


### PerformAnalysis
Performs the analysis roll to determine how many yield resources to produce. The roll must meet or exceed the minimumSuccess required in order to produce a nominal yield (the amount specified in a YIELD_RESOURCE's Ratio entry). If the roll fails, then a lower than normal yield is produced. If the roll exceeds the criticalSuccess number, then a higher than normal yield is produced. If the roll falls below the criticalFailure number, then no yield is produced, and the part will explode if the explodeUponCriticalFailure flag is set.

### CalculateProgress
Calculates and updates the progress of the yield production cycle.

### RemoveConditionIfNeeded
Removes the summaryCondition from all kerbals in the part if they have it set.

# SnacksResourceProcessor
            
The SnacksResourceProcessor is a specialized version of the BaseResourceProcessor. It has the distict advantage of making use of the game settings for Snacks, whereas BaseResourceProcessor is entirely configured via config files.
        

# SnacksPartResource
            
When a part with crew capacity is loaded in the editor and it lacks this resource, or when a vessel is loaded into the scene and its parts with crew capacity lack this resource, add it to the part. Doesn’t apply to kerbals going on EVA. Use SNACKS_EVA_RESOURCE for that. Use the SNACKS_PART_RESOURCE to define resources to add.
        
## Fields

### resourceName
Name of the resource
### amount
Amount to add
### maxAmount
Max amount possible
### capacityAffectingModules
Parts with at least one of the modules on this list affect the part's capacity to store the resource (their equipment takes up additional space, for instance).
### capacityMultiplier
If a part has at least one part module on the capacityAffectingModules list then multiply resource amount and max amount by this multiplier. Default is 1.0
### isPerKerbal
If true (which is the default), then amount and maxAmount added are multiplied by the part's crew capacity.
## Methods


### LoadPartResources
Loads the SNACKS_PART_RESOURCE config nodes, if any, and returns SnacksPartResource objects.
> #### Return value
> A list of SnacksPartResource objects.

### addResourcesIfNeeded(Part)
If the part with crew capacity doesn't have the resource, then add it.
> #### Parameters
> **part:** 


### addResourcesIfNeeded(Vessel)
If the loaded vessel's parts with crew capacity don't have the resource, then load it.
> #### Parameters
> **vessel:** 


# SnacksEVAResource
            
When a kerbal goes on EVA, take this resource along and remove a corresponding amount from the vessel. Use the SNACKS_EVA_RESOURCE to define the resource to add.
        
## Fields

### resourceName
Name of the resource
### amount
Amount to add
### maxAmount
Max amount possible
### snacksEVAResource
The EVA resource that defines how many Snacks the kerbal gets. We track this so that we can update its amount and max amount based on game settings.
## Methods


### LoadEVAResources
Loads the SNACKS_EVA_RESOURCE config nodes, if any, and returns SnacksEVAResource objects.
> #### Return value
> A list of SnacksEVAResource objects.

### onCrewBoardedVessel(Part,Part)
Handles the crew boarded event. The resource is removed from the kerbal and added to the vessel.
> #### Parameters
> **evaKerbal:** The kerbal that is returning from EVA

> **boardedPart:** The part that the kerbal boarded


### onCrewEVA(Part,Part)
Handles the crew eva event. The kerbal gains the EVA resource and the vessel loses a corresponding amount.
> #### Parameters
> **evaKerbal:** The kerbal that went on EVA

> **partExited:** The part that the kerbal exited


### addResourcesIfNeeded(Vessel)
If the loaded vessel's parts with crew capacity don't have the resource
> #### Parameters
> **vessel:** 


# Snackshot
            
Represents a snapshot of the current and max units of a particular resource that is displayed in the Snapshots window.
        
## Fields

### resourceName
Name of the resource
### amount
Current amount in the vessel
### maxAmount
Max amount in the vessel
### showTimeRemaining
Flag to indicate whether to include the time remaining estimate in the display.
### isSimulatorRunning
Flag to indicate whether or not simulator is running.
### estimatedTimeRemaining
Estimated time remaining in seconds.

# SnackSimThread
            
This class represents a single simulator job. It will check its job list for jobs to process and synchronize with other simulator jobs.
        
## Fields

### 
Max number of threads allowed
### 
List of simulator jobs waiting to be run
## Methods


### Start
Starts the thread.

### Stop
Stops all current and pending jobs and kills the thread.

### AddJob(Snacks.SimSnacks)
Adds a simulator job to the job list.
> #### Parameters
> **simSnacks:** The simulator to add to the jobs list.


### ClearJobs
Clears all pending and running jobs.

### 
Handles the completion of a thread's simulation.
> #### Parameters
> **simulator:** The simulator that just finished.


### 
Handles completion of a thread's simulation cycle.
> #### Parameters
> **simulator:** The simulator that just completed its cycle.


### 
Handles the exception generated by a simulator.
> #### Parameters
> **simulator:** The simulator that generated the exception.

> **ex:** The Exception generated.


### 
Locks the vessel resource durations so that we can query them. Be sure to call UnlockResourceDurations when done.

### 
Unlocks resource durations so that threads can operate on them.

### 
Returns the vessel resource definitions for the requested vessel. Be sure to call LockResourceDurations before calling this method. Be sure to call UnlockResourceDurations after you're done.
> #### Parameters
> **vessel:** The vessel to query

> #### Return value
> The resource durations for the specified vessel, or null if they don't exist.

### 
Determines whether or not the simulator had to assume that converters were on.
> #### Parameters
> **vessel:** The vessel to query

> #### Return value
> true if the simulator couldn't determine activation state and had to assume converters were on; false if not.

### 
Removes the vessel resource durations for the specified vessel if they exist. Be sure to call LockResourceDurations before calling this method. Be sure to call UnlockResourceDurations after you're done.
> #### Parameters
> **vessel:** The vessel that will no longer have resource durations.


### 
Adds a simulator to the job list.
> #### Parameters
> **simulator:** The SimSnacks simulator to add to the list.


### 
Stops all current and pending jobs.

# SimResource
            
This struct tracks vessel resources in the simulator. For the sake of simplicity, simulated resources aren't locked and are considered flow mode ALL_VESSEL.
        

# SimConverter
            
This class is a simulated ModuleResourceConverter. It processes inputs, produces outputs, and when the time's up, generates yield resources. For the sake of simplicity, all vessel resources are available.
        
## Methods


### ProcessResources(System.Collections.Generic.Dictionary{System.String,Snacks.SimResource},System.Double)
Processes resources, consuming inputs, producing outputs, and when time expires, producing yield resources. For the purposes of simulation, we assume dumpExcess = true, yield resources always suceed, no heat generation, and no crew bonuses.
> #### Parameters
> **resources:** The map of vessel resources to process.

> **secondsPerSimulatorCycle:** The number of seconds per simulator cycle.


# OnConvertersRunCompleteDelegate
            
Signifies that the converters have completed their run.
            
> **simulator:** The simulator that invoked the delegate method.

        

# OnConsumersRunCompleteDelegate
            
Signifies that the consumers have completed their run.
            
> **simulator:** The simulator that invoked the delegate method.

        

# OnSimulatorCycleCompleteDelegate
            
Signifies that the simulation cycle has completed.
            
> **simulator:** The simulator that invoked the delegate method.

        

# OnSimulationCompleteDelegate
            
Signifies that the simulation has completed.
            
> **simulator:** The simulator that invoked the delegate method.

        

# OnSimulatorExceptionDelegate
            
Signifies that the simulation experienced an error.
            
> **simulator:** The simulator generating the error.

            
> **ex:** The Exception that was generated.

        

# SimulatedVesselTypes
            
Type of vessel being simulated
        

# SimulatorContext
            
Context for how the simulator is being created. Typically used when Snacks fires an event to give mods a chance to add additional custom converters not covered by Snacks.
        
## Fields

### simulatedVesselType
Type of vessel being simulated.
### vessel
Vessel object for loaded/unloaded vessels being simulated.
### shipConstruct
Ship constructor for editor vessel being simulated.

# SimSnacks
            
This class determines how long consumed resources like Snacks will last by simulating resource consumption and simulating running converters like soil recyclers and snacks processors. It is designed to allow for an arbitrary number of resource production chains and an arbitrary number of consumed resources. Conditions: The only inputs allowed into the system are those consumed by kerbals. Ex: kerbals eat Snacks and produce Soil. Resources aboard the vessel that aren't directly involved in resource consumption are fixed. Ex: Resource harvesters that produce Ore aren't accounted for. Running simulations is computationally expensive. This class should be run in a thread.
        
## Methods


### CreateSimulator(ShipConstruct)
Creates a simulator from the supplied ship construct
> #### Parameters
> **ship:** A ShipConstruct to simulate

> #### Return value
> A SimSnacks simulator

### CreateSimulator(ProtoVessel)
Creates a simulator from the proto vessel
> #### Parameters
> **protoVessel:** The unloaded vessel to query for resources and converters.

> #### Return value
> A SimSnacks simulator.

### CreateSimulator(Vessel)
Creates a simulator from a loaded vessel
> #### Parameters
> **vessel:** The Vessel object to query for resources and converters.

> #### Return value
> A SimSnacks simulator.

# SnackSimThreadPool
            
This class handles simulator thread creation, data synching, and the job list.s
        
## Fields

### maxThreads
Max number of threads allowed
### jobList
List of simulator jobs waiting to be run
## Methods


### OnThreadSimulationComplete(Snacks.SimSnacks)
Handles the completion of a thread's simulation.
> #### Parameters
> **simulator:** The simulator that just finished.


### OnThreadSimulatorCycleComplete(Snacks.SimSnacks)
Handles completion of a thread's simulation cycle.
> #### Parameters
> **simulator:** The simulator that just completed its cycle.


### OnSimulatorException(Snacks.SimSnacks,System.Exception)
Handles the exception generated by a simulator.
> #### Parameters
> **simulator:** The simulator that generated the exception.

> **ex:** The Exception generated.


### LockResourceDurations
Locks the vessel resource durations so that we can query them. Be sure to call UnlockResourceDurations when done.

### UnlockResourceDurations
Unlocks resource durations so that threads can operate on them.

### GetVesselResourceDurations(Vessel)
Returns the vessel resource definitions for the requested vessel. Be sure to call LockResourceDurations before calling this method. Be sure to call UnlockResourceDurations after you're done.
> #### Parameters
> **vessel:** The vessel to query

> #### Return value
> The resource durations for the specified vessel, or null if they don't exist.

### ConvertersAssumedActive(Vessel)
Determines whether or not the simulator had to assume that converters were on.
> #### Parameters
> **vessel:** The vessel to query

> #### Return value
> true if the simulator couldn't determine activation state and had to assume converters were on; false if not.

### RemoveVesselResourceDurations(Vessel)
Removes the vessel resource durations for the specified vessel if they exist. Be sure to call LockResourceDurations before calling this method. Be sure to call UnlockResourceDurations after you're done.
> #### Parameters
> **vessel:** The vessel that will no longer have resource durations.


### AddSimulatorJob(Snacks.SimSnacks)
Adds a simulator to the job list.
> #### Parameters
> **simulator:** The SimSnacks simulator to add to the list.


### StopAllJobs
Stops all current and pending jobs.

# SoilRecycler
            
The SoilRecycler is designed to recycle Soil into Snacks. It is derived from SnacksProcessor (), which is derived from SnacksConverter. SoilRecycler config nodes should be calibrated to turn 1 Soil into 1 Snacks; game settings will adjust the recycler based on desired difficulty.
        
## Fields

### RecyclerCapacity
The number of kerbals that the recycler supports.

# BaseOutcome
            
The BaseOutcome class is the basis for all outcome processing. An outcome is used with the resource processors as well as by the event system. It represents the consequences (or benefits) of a process result as well as the actions to take when an event's preconditions are met.
        
## Fields

### canBeRandom
Flag to indicate whether or not the outcome can be randomly selected. Requires random outcomes to be turned on. If it isn't then the outcome is always applied.
### selectRandomCrew
Flag to indicate whether or not to select a random crew member for the outcome instead of applying the outcome to the entire crew.
### playerMessage
Optional message to display to the player.
### childOutcomes
Optional list of child outcomes to apply when the parent outcome is applied. Child outcomes use same vessel/kerbal as the parent.
## Methods


### Constructor
Initializes a new instance of the class.

### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **canBeRandom:** If set to true, the outcome can be randomly selected.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **canBeRandom:** If set to true, the outcome can be randomly selected.

> **playerMessage:** A string containing a message to the player that is shown when the outcome is applied..


### Load(ConfigNode)
Loads the configuration
> #### Parameters
> **node:** A ConfigNode containing data to load.


### IsEnabled
Indicates whether or not the outcome is enabled.
> #### Return value
> true if inabled, false if not.

### ApplyOutcome(Vessel,Snacks.SnacksProcessorResult)
Applies the outcome to the vessel's crew
> #### Parameters
> **vessel:** The Vessel being processed.

> **result:** The Result of the processing attempt.


### RemoveOutcome(Vessel)
Removes the outcome from the vessel's crew.
> #### Parameters
> **vessel:** The Vessel to process.
