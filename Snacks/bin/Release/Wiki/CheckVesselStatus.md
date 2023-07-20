            
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


