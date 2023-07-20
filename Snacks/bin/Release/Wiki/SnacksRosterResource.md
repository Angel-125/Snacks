            
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


