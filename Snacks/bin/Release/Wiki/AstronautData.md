            
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


