# Commander.NET

C# command-line argument parsing and serialization via attributes. Inspired by [JCommander](http://jcommander.org/)

## Usage

First you need to create a non-abstract class, with a public parameterless constructor, 
which will describe the parameters.

```csharp
using Commander.NET.Attributes;

class Options
{
	[Parameter("-i", Description = "The ID")]
	public int ID = 42;

	[Parameter("-n", "--name")]
	public string Name;

	[Parameter("-h", "--help", Description = "Print this message and exit.")]
	public bool Help;
}
```

Then simply parse the command line arguments

```csharp
string[] args = { "-i", "123", "--name", "john"};

CommanderParser<Options> parser = new CommanderParser<Options>();
Options options = parser.Add(args)
                        .Parse();
```

Or statically

```csharp
Options options = CommanderParser.Parse<Options>(args);
```

### Required parameters

You may specify which of the parameters are required by setting the `Required` property of the attributes.

```csharp
[Parameter("-r", Required = Required.Yes)]
public int RequiredParameter;

[Parameter("--lorem", Required = Required.No)]
public bool NotRequiredParameter;

[Parameter("--ipsum")]	// Required.Default
public string ThisOneIsRequired;

[Parameter("--dolor")]	// Required.Default
public string ThisOneIsNotRequired = "Because it has a default value";
```

As seen in the above example, the `Required` enum can have one of three values:

- **Required.Yes**
  - This parameter is required and omitting it will throw a `ParameterMissingException` during parsing.
- **Required.No**
  - This parameter is optional. If it is not set, the relevant field will maintain its default value.
- **Required.Default**
  - The parameter will be considered to be **required** only if the default value of the relevant field is `null`. 

### Generate a usage summary

```csharp
CommanderParser<Options> parser = new CommanderParser<Options>();

Console.WriteLine(parser.Usage());
```

Will print out the following.

```
Usage: <exe> [options]
Options:
      -h, --help
        Print this message and exit.
        Default: False
      -i
        The ID
        Default: 42
    * -n, --name
```

An asterisk prefix implies that the option is required.

### Exceptions

```csharp
using Commander.NET.Exceptions;

try
{
	Options opts = CommanderParser.Parse<Options>(args);
}
catch (ParameterMissingException ex)
{
	// A required parameter was missing
	Console.WriteLine("Missing parameter: " + ex.ParameterName);
}
catch (ParameterFormatException ex)
{
	/*
	*	A string-parsing method raised a FormatException
	*	ex.ParameterName
	*	ex.Value
	*	ex.RequiredType
	*/
	Console.WriteLine(ex.Message);
}
```


