            
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


