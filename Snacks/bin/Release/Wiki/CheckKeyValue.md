            
This precondition Checks a kerbal's key-value and validates it against the supplied parameters. Example definition: PRECONDITION { name = CheckKeyValue keyValueName = State checkType = checkEquals stringValue = Bored } 
        
## Fields

### keyValueName
Name of the key-value
### stringValue
String value of the key. Takes precedence over the int values.
### intValue
Integer value of the key
### checkType
Type of check to make
## Methods


### Constructor
Initializes a new instance of the class.
> #### Parameters
> **node:** A ConfigNode containing initialization parameters. parameters from the class also apply.


