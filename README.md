# Commander.NET

C# command-line argument parsing and serialization via attributes. Inspired by [JCommander](http://jcommander.org/).

If you would like to expand this library, please include examples in your PR or issue, of widely-used CLI tools that
support your suggested practice or format.

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

### Positional parameters

You can define positional parameters using the `PositionalParameter` attribute.

```csharp
[PositionalParameter(0, "operation", Description = "The operation to perform.")]
public string Operation;

[PositionalParameter(1, "target", Description = "The host to connect to.")]
public string Host = "127.0.0.1";
```

When printing out the usage, positional parameters will be shown like the example below.
However, they can be passed in any order in relation to the options.

```
Usage: Tests.exe [options] <operation> [target]
    operation: The operation to perform.
    target: The host to connect to.
Options:
    ...
```

In general, any argument passed that is neither the name of the parameter, 
nor the value of a non-boolean named parameter will be considered a positional parameter
and assigned to the appropriate index.

You may also get all the positional parameters that were passed, using the `PositionalParameterList' attribute.

```csharp
[PositionalParameterList]
public string[] Parameters;
```

### Key-Value separators

By default, the parser will only consider key-value pairs that are separated by a space.
You can change that by setting the Separators flags of the parser.
The example below will parse both the `--key value` format and the `--key=value` format.

```csharp
CommanderParser<Options> parser = new CommanderParser<Options>();
Options options = parser.Add(args)
                        .Separators(Separators.Space | Separators.Equals)
                        .Parse();
```

Currently available separators:

- Separators.Space
- Separators.Equals
- Separators.Colon
- Separators.All

### Value validation

```csharp
[Parameter("-a", "--age")]
public int Age;

[ParameterValidator("-a")]
public bool PositiveInteger(string value)
{
	int intVal;
	return int.TryParse(value, out intVal) && intVal > 0;
}
```

### Regular Expression Validation

You can also validate the values of a parameter through a regular expression, by setting the `Regex` property.

```csharp
[Parameter("-n", "--name", Regex = "^john|mary$")]
public string Name;
```

```csharp
string[] args = { "-n", "james" };

try
{
	Options opts = CommanderParser.Parse<Options>(args);
	Console.WriteLine(opts.ToString());
}
catch (ParameterMatchException ex)
{
	// Parameter -n: value "james" did not match the regular expression "^john|mary$"
	Console.WriteLine(ex.Message);
}
```

### //TODO

- Reverse positional indexing
- Passing multiple comma-separated values
- Specifying possible values. (f.e bacon|onions|tomatoes) Will be doable by default with regex, but enum support will be nice.
- Specifying methods for input validation (these are not allowed in attributes, will likely have to be other methods within the class)
- [Commands](http://jcommander.org/#_more_complex_syntaxes_commands)

