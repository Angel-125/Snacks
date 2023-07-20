            
This outcome sets a condition on the affected kerbals. If that condition is defined in a SKILL_LOSS_CONDITION config node, then the kerbals' skills will be removed until the condition is cleared. Example definition: OUTCOME { name = ClearCondition conditionSummary = Stressed Out }   
        
## Fields

### conditionName
The name of the condition to set. If defined in a SKILL_LOSS_CONDITION node then the affected kerbals will lose their skills until the condition is cleared.
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **conditionName:** The name of the condition to set. It must be added to a SKILL_LOSS_CONDITION config node in order for the kerbal to lose its skills.

> **canBeRandom:** If set to true it can be randomly selected from the outcomes list.

> **playerMessage:** A string containing the bad news.


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. Parameters in the class also apply.


