# NX

C# Necessary eXtensions library.

* ML-style Option type w/ OCaml's Option module
* OCaml's Seq module (for IEnumerable)
* Using class; monadic IDisposable/using wrapper
* try/catch wrapping integrated w/ Option
* and more

# Classes & Structs

## struct Option\<T\>, static class Option

Represents optional value, which can be used to eliminate the null.

Any null occurrence inside the ```Option``` world will be treated as an ```Option.None```, so there is no worry for the NullReferenceException.

Respects OCaml's Option module.

example:
```csharp
bool enabled = filename
    .Try(File.ReadAllText)
    .Map(MySettingFormat.Parse)
    .Map(x => x.Flags["enabled"])
    .Default(false); // if None, return false instead
```
If the operation can throw an exception and you want to ignore it, use ```Try``` (```TryMap```) instead of ```Map```.

You can use ```MatchEx``` to catch the exception then.

```csharp
Option.Try(TakeSomething)
    .MatchEx(
        x => Console.WriteLine("got something: {0}", x),
        ex => Console.WriteLine("error: {0}", ex.Message),
        () => Console.WriteLine("got nothing.")
    );
```

In addition, this can be used to set the default value of optional arguments in methods, as ```Option<T>``` is struct.

```csharp
void Hello(Option<string> name = default(Option<string>))
{
    name.Match(
        x => Console.WriteLine("Hello, {0}."),
        () => Console.WriteLine("Hello.")
    );
}

Hello();
Hello("World"); // supports implicit cast from T to Option<T>
```

Contains ```Map```, ```Try```, ```Match```, ```Default```, and so on.

## static class Seq

Extensions for ```IEnumerable<T>```.

Respects OCaml's Seq module.

example:
```csharp
classes
    .Map(x => x.Students)
    .Flatten()
    .Find(x => x.Name == "John")
    .Match(
        x => GiveMoney(x, 100),
        () => Console.WriteLine("John not found!")
    );
```

```Find```, ```Head```, ```Tail``` and ```Nth``` will return ```None``` instead of throwing exceptions, when the desired element doesn't exist.

```csharp
var john1 = people.FirstOrDefault(x => x.Name == "John");
if (john1 == null)
    return false;
else
    return john1.Money > 100 && Order(john1, OrderType.BuyAnApple).Succeed;

---

return people
    .Find(x => x.Name == "John")
    .Filter(x => x.Money > 100) 
    .Map(x => Order(x, OrderType.BuyAnApple).Succeed)
    .Default(false);
```

Contains ```Map```, ```Filter```, ```FoldR```, ```Find```, ```Iter```, ```JoinToString``` and so on.

## class Using\<T\>, static class Using

Wraps ```IDisposable<T>``` with psuedo-monadic style.

example:

```csharp
string s =
    from x in File.OpenRead("a.txt").Use()
    from y in new StreamReader(x).Use()
    select y.ReadToEnd();
```

Contains ```Use```, ```Map```, ```SelectMany```, and so on.

## class Diff, class DiffItem, enum DiffState

Compares two ```IEnumerable<T>``` sequences and show *which* item is added or removed.

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

## static class StreamNX

Writes string to stream directly.

## class TComparer\<T\>, class TEquialityComparer\<T\>

Grabs ```IComparer<T>``` from ```Func<T, T, int>```.

## static class New

Initialises ```T[]``` and ```IEnumerable<T>``` with ```params T[]```.

## static class Shell

Executes shell command with less typing.

example:

```csharp
var foo = Shell.Eval("echo", "foo");
Shell.Execute("vim", "a.txt");
```

## static class ConsoleNX

```ColoredWrite``` and ```ColoredWriteLine```.
