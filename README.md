# NX

C# Necessary eXtensions library.

# Usage

Add NX.cs to your project.

# Classes & Structs

## class TComparer\<T\>, class TEquialityComparer\<T\>

Grab ```IComparer<T>``` from ```Func<T, T, bool>```.

## static class New

Initialise ```T[]``` and ```IEnumerable<T>``` with ```params T[]```.

## static class Seq

Extensions for ```IEnumerable<T>```.

Contains ```Map```, ```Filter```, ```FoldR```, ```JoinToString```, ```Match```, ```Diff```, and so on.

## class Diff, class DiffItem, enum DiffState

Compare two ```IEnumerable<T>``` sequences and show *which* item is added or removed.

example:
```csharp
var s1 = New.Seq(1, 2, 3, 4, 5);
var s2 = New.Seq(1, 3, 0, 4, 6);
var diff = s1.Diff(s2);
diff.Print();
//  1
//- 2
//  3
//+ 0
//  4
//- 5
//+ 6
```

## static class EnumNX

Generic ```Enum.Parse```.

## class Using\<T\>, static class Using

Wrap ```IDisposable<T>``` with psuedo-monadic style.

example:

```csharp
string s =
    from x in File.OpenRead("a.txt")
    from y in new StreamReader(x)
    return y.ReadToEnd();
```

Contains ```Use```, ```Map```, ```SelectMany```, and so on.

## struct Unit

*True* unit type.

## class Option\<T\>, static class Option

More convenient optional value.

example:
```csharp
peoples
    .Try(xs => xs.First(x => x.Name == "John"))
    .Match(
        x => GiveMoney(x, 100),
        () => Console.WriteLine("John not found!")
    );
```

Contains ```Map```, ```Try```, ```Match```, ```Default```, and so on.

## static class StreamNX

Write string to stream directly.

## static class Shell

Execute shell command with less typing.

example:

```csharp
var foo = Shell.Eval("echo", "foo");
Shell.Execute("vim", "a.txt");
```

## static class ConsoleNX

```ColoredWrite``` and ```ColoredWriteLine```.
