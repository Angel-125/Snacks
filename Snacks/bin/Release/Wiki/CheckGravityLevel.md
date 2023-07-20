            
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


