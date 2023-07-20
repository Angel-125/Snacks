            
This precondition checks to see if a kerbal or vessel is in an environemnt with breathable air, and matches it with the expected parameter. The vessel's celestial body must have an atmosphere with oxygen, and the vessel altitude must be between sea level and half the atmosphere height. Example definition: PRECONDITION { name = CheckBreathableAir mustExist = false }
        
## Fields

### mustExist
Flag to indicate pressence (true) or absence (false) of the value to check.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


