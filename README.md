# NX

C# Necessary eXtensions library.

* ML-style Option type w/ OCaml's Option module
* OCaml's Seq module (for IEnumerable)
* monadic IDisposable/using wrapper
* functional try/catch wrapper

# Classes & Structs

## struct Option\<T\>, static class Option

Represents optional value, which can be used to eliminate the null.

Any null occurrence inside the ```Option``` world will be treated as an ```Option.None```, so there is no worry for the NullReferenceException.

Respects OCaml's Option module.

```csharp
var s1 = Option.Some("some"); // Option<string>
var s2 = "some".Some();       // Option<string>

Console.WriteLine(s1 == s2);  // true

var n1 = Option.None;         // Option<Any Type>

void Show(Option<string> message)
{
    message.Match(
        x  => Console.WriteLine(x),
        () => Console.WriteLine("(no message)")
    );
}

Show(s1);       // "some"
Show(n1);       // "(no message)"
Show("hello");  // "hello" - supports implicit cast from T to Option<T>
```

It contains several extension methods to help you write a clean code.

```csharp
var enabled = settings
    .Find(x => x.Name == currentUser.Name)
    .Map(x => x.Flags["enabled"])
    .Default(false); // if not found, 'enabled' will be false
```

In addition, this can be used to set the default value of optional arguments in methods.

```csharp
void Hello(Option<string> name = default(Option<string>))
{
    name.Match(
        x  => Console.WriteLine("Hello, {0}."),
        () => Console.WriteLine("Hello.")
    );
}

Hello();
Hello("World"); // supports implicit cast from T to Option<T>
```

## struct Either\<T, U\>, static class Either

Represents a value that can be one of the two different types/values.

```csharp
var x1 = new Either<string, int>("left");  // Either<string, int>
var x2 = Either.Inl("left");               // Either<string, Any Type>

Console.WriteLine(x1 == x2); // true

var y = 2.Inr();                           // Either<Any Type, int>

void Show(Either<string, int> x)
{
    x.Match(
        l => Console.WriteLine(l),         // l : string
        r => Console.WriteLine(r + 1)      // r : int
    );
}

Show(x2); // "left"
Show(y);  // 3
```

Several helper methods are available.

```csharp
var x = "left".Inl();

var y = x.MapLeft(s => s.Length);

y.May(left: i => Console.WriteLine(i + 1)); // 5

Option<string> xo = x.LeftOrNone();
int yd = y.LeftOrDefault(0);
```

## struct TryResult, static class TryNX

Wraps try-catch statement.

```csharp
int index = "filename.txt"
    .Try(x => File.ReadAllLines(x))
    .Map(x => x[10])
    .Map(x => int.Parse(x))
    .Catch<FileNotFoundException>(e => -1)
    .Catch<FormatException>(e => 0)
    .Evaluate();

// NoValueException will be thrown if an uncatched exception has happened
// the original exception will be available in NoValueException.InnerException
```

Instead of calling ```Evaluate()```, you can convert the result to Option type by using ```ToOption()``` method.

```csharp
var lines = "filename.txt"
    .Try(x => File.ReadAllLines(x))
    .ToOption()
    .ForceUnwrap(() => {
        Console.WriteLine("error: failed to read the file.");
        Environment.Exit(1);
    });
```

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

It also contains several extension methods such as ```FoldR```, ```Find```, ```Iter```, and ```JoinToString```.

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

## static class EnumNX

Generic ```Enum.Parse```.

## class TComparer\<T\>, class TEquialityComparer\<T\>

Grabs ```IComparer<T>``` from ```Func<T, T, int>```.

