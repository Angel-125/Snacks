            
This precondition checks to see if a vessel or roster resource meets the supplied parameters. Example definition: PRECONDITION { name = CheckResource resourceName = Stress checkType = checkEquals valueToCheck = 3.0 } 
        
## Fields

### resourceName
Name of the resource to check
### checkType
The conditional type to use during the validation.
### valueToCheck
The value to check for
### checkMaxAmount
Flag to indicate whether or not to check the resource's max amount instead of the curren amount;
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


