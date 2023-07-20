            
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


