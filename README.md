# Commander.NET

C# command-line argument parsing and serialization via attributes. Inspired by [JCommander](http://jcommander.org/).

If you would like to expand this library, please include examples in your PR or issue, of widely-used CLI tools that
support your suggested practice or format.

# Usage

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

## Required parameters

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

## Generate a usage summary

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

## Exceptions

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

## Positional parameters

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

## Key-Value separators

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

## Value validation

This section lists the ways with which validate, format or even convert the values that 
are passed to your parameters. Each individual value, goes through the following steps in that order:

1. Regex validation
2. Method validation
3. Formatting

### Regular Expression validation

You can validate the values of a parameter through a regular expression, by setting the `Regex` property.

```csharp
[Parameter("-n", "--name", Regex = "^john|mary$")]
public string Name;
```

If the regex match failes, a `ParameterMatchException` is thrown, as it shown below.

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

### Method validation

You can alsouse your own validation methods for values that are passed to a specific parameter by 
implementing the `IParameterValidator` interface.

If the `Validate()` method returns `false`, then a `ParameterValidationException` is thrown
by the parser. Alternatively, you can use this method to throw your own exceptions, and they will rise
to where you called you called the parser from.

The following basic example makes sure that the argument passed to the `Age` parameter is a positive integer.

```csharp
using Commander.NET.Interfaces;

class PositiveInteger : IParameterValidator
{
	bool IParameterValidator.Validate(string name, string value)
	{
		int intVal;
		return int.TryParse(value, out intVal) && intVal > 0;
	}
}
```

```csharp
[Parameter("-a", "--age", ValidateWith = typeof(PositiveInteger))]
public int Age;
```

### Value formatting

Similar to [method validation](#method_validation), you may want to declare your own methods
for formatting - or event converting - certain parameter values. You may do this by implementing 
the `IParameterFormatter` interface.

The object returned by the `Format()` method will be directly set to the 
parameter with no other formatting or type conversion.

The following basic example, converts whatever value is passed to the `--ascii` parameter into a byte array.

```csharp
using Commander.NET.Interfaces;

class StringToBytes : IParameterFormatter
{
	object IParameterFormatter.Format(string name, string value)
	{
		return Encoding.ASCII.GetBytes(value);
	}
}
```

```csharp
[Parameter("--ascii", FormatWith = typeof(StringToBytes))]
public byte[] ascii;
```

## Commands

Commands are usually a very important method for separating the different functionalities of a CLI program.
Consider the following example, based on the popular `git` tool.

```csharp
class Git
{
	[Command("commit")]
	public Commit Commit;

	[Command("push")]
	public Push Push;
}

class Commit
{
	[Parameter("-m")]
	public string Message;
}

class Push
{
	[PositionalParameter(0, "remote")]
	public string Remote;

	[PositionalParameter(1, "branch")]
	public string Branch;
}
```

```csharp
Git git = new CommanderParser<Git>()
                         .Parse(args);

if (git.Commit != null)
{
	Console.WriteLine("Commiting: " + git.Commit.Message);
}
else if (git.Push != null)
{
	Console.WriteLine("Pushing to: " + git.Push.Remote + " " + git.Push.Branch);
}
```

Any arguments passed **after** the name of the command, are parsed and serialized into that command.


## //TODO

- Reverse positional indexing
- Passing multiple comma-separated values
- Specifying possible values. (f.e bacon|onions|tomatoes) Will be doable by default with regex, but enum support will be nice.
- [Commands](http://jcommander.org/#_more_complex_syntaxes_commands)

