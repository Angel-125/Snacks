            
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

