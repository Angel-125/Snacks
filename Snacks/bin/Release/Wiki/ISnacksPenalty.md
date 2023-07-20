            
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

