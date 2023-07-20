            
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
> #### 