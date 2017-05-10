/*
The X11 License
NX - extension objects and methods of C#
Copyright(c) 2015 cannorin

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace NX
{
    internal class TComparer<T> : IComparer<T>
    {
        public Func<T, T, int> Comparer { get; private set; }

        public TComparer(Func<T, T, int> f)
        {
            Comparer = f;
        }

        public int Compare(T x, T y)
        {
            return Comparer(x, y);
        }
    }

    internal static class TComparer
    {
        public static TComparer<T> Create<T>(Func<T, T, int> f)
        {
            return new TComparer<T>(f);
        }
    }

    internal class TEquialityComparer<T> : IEqualityComparer<T>
    {
        public Func<T, T, bool> Comparer { get; private set; }

        public TEquialityComparer(Func<T, T, bool> f)
        {
            Comparer = f;
        }

        public bool Equals(T a, T b)
        {
            return Comparer(a, b);
        }

        public int GetHashCode(T a)
        {
            return a.GetHashCode();
        }
    }

    internal static class TEquialityComparer
    {
        public static TEquialityComparer<T> Create<T> (Func<T, T, bool> f)
        {
            return new TEquialityComparer<T>(f);
        }
    }

    internal class StringBuilderNX
    {
        string buf;

        public override string ToString()
        {
            return buf;
        }

        public StringBuilderNX(string s = "")
        {
            buf = s;
        }

        public void Write(string s, params object[] args)
        {
            buf += string.Format(s, args);
        }

        public void WriteLine(string s = "")
        {
            buf += s;
            buf += Environment.NewLine;
        }

        public void WriteLine(string s, params object[] args)
        {
            buf += string.Format(s, args);
            buf += Environment.NewLine;
        }
    }

    internal static class StringNX
    {
        public static string[] Split(this string s, bool removeEmptyEntries, params string[] seps)
        {
            return s.Split(seps, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
        }

        public static string Build(Action<StringBuilderNX> f)
        {
            var sb = new StringBuilderNX();
            f(sb);
            return sb.ToString();
        }

        public static string Repeat(this string s, int count)
        {
            if (count > 10)
            {
                var sb = new StringBuilder();
                Seq.Repeat(count).Iter(_ => sb.Append(s));
                return sb.ToString();
            }
            else if (count <= 0)
                return "";
            else
            {
                var _s = s;
                for (var i = 1; i < count; i++)
                    _s += s;
                return _s;
            }
        }
    }

    internal static class New
    {
        public static T[] Array<T>(params T[] ts)
        {
            return ts;
        }

        public static IEnumerable<T> Seq<T>(params T[] ts)
        {
            return ts;
        }

        public static VTuple<T, U> VTuple<T, U>(T l, U r)
        {
            return new VTuple<T, U>(l, r);
        }

        public static VTuple<T, U, V> VTuple<T, U, V>(T l, U c, V r)
        {
            return new VTuple<T, U, V>(l, c, r);
        }

        public static VTuple<T, U, V, W> VTuple<T, U, V, W>(T l, U cl, V cr, W r)
        {
            return new VTuple<T, U, V, W>(l, cl, cr, r);
        }
    }

    internal static class Seq
    {
        public static IEnumerable<string> EnumerateLines(this StreamReader sr)
        {
            while (!sr.EndOfStream)
                yield return sr.ReadLine();
        }

        public static string JoinToString<T>(this IEnumerable<T> source)
        {
            return string.Concat(source);
        }

        public static string JoinToString<T>(this IEnumerable<T> source, string separator)
        {
            return string.Join(separator, source);
        }

        public static IEnumerable<T> Singleton<T>(this T t)
        {
            return new[] { t };
        }

        public static IEnumerable<int> ZeroTo(int n)
        {
            return Init(_ => _, n);
        }

        public static IEnumerable<T> Init<T>(Func<int, T> f, int n = int.MaxValue)
        {
            return Enumerable.Range(0, n).Map(f);
        }

        public static IEnumerable<T> Repeat<T>(T a, int n = int.MaxValue)
        {
            return n == int.MaxValue ? repeatInf(a) : Enumerable.Repeat(a, n);
        }

        public static IEnumerable<bool> Inf(bool b = false)
        {
            return repeatInf(b);
        }

        static IEnumerable<T> repeatInf<T>(T a)
        {
            while (true)
                yield return a;
        }

        public static int LengthNX<T>(this IEnumerable<T> seq)
        {
            return seq.Count();
        }

        public static long LongLengthNX<T>(this IEnumerable<T> seq)
        {
            return seq.LongCount();
        }

        public static Option<T> Head<T>(this IEnumerable<T> seq)
        {
            return seq.Try<IEnumerable<T>, T>(Enumerable.First);
        }

        public static Option<T> Tail<T>(this IEnumerable<T> seq)
        {
            return seq.Try<IEnumerable<T>, T>(Enumerable.Last);
        }

        public static Option<T> Nth<T>(this IEnumerable<T> seq, int n)
        {
            if (seq.GetEnumerator().Try(x => x.Reset()).HasException)
                return seq.Try(xs => xs.Skip(n).First());
            else
            {
                var enumerable = seq.ToArray();
                if (n > enumerable.Length - 1)
                    return Option.None;
                else
                    return enumerable[n].Some();
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> seq, T a, IEqualityComparer<T> comp)
        {
            var i = 0;
            foreach (T item in seq)
            {
                if (comp.Equals(item, a))
                    return i;
                i++;
            }
            return -1;
        }

        public static int IndexOf<T>(this IEnumerable<T> seq, T a)
        {
            return seq.IndexOf(a, EqualityComparer<T>.Default);
        }

        public static IEnumerable<T> Rev<T>(this IEnumerable<T> seq)
        {
            return seq.Reverse();
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> s1, IEnumerable<T> s2)
        {
            return s1.Concat(s2);
        }

        public static IEnumerable<T> RevAppend<T>(this IEnumerable<T> s1, IEnumerable<T> s2)
        {
            return s1.Reverse().Concat(s2);
        }

        public static IEnumerable<T> ConcatNX<T>(this IEnumerable<IEnumerable<T>> seq)
        {
            return seq.SelectMany(x => x);
        }

        public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> seq)
        {
            return seq.SelectMany(x => x);
        }

        public static IEnumerable<T2> Choose<T1, T2>(this IEnumerable<T1> seq, Func<T1, Option<T2>> f)
        {
            return seq.Map(f).Filter(x => x.HasValue).Map(x => x.Value); 
        }

        public static Option<T> FindSome<T>(this IEnumerable<Option<T>> seq)
        {
            return seq.Find(x => x.HasValue).Flatten();
        }

        public static IEnumerable<T2> Map<T1, T2>(this IEnumerable<T1> seq, Func<T1, T2> f)
        {
            return seq.Select(f);
        }

        public static IEnumerable<T2> MapI<T1, T2>(this IEnumerable<T1> seq, Func<T1, int, T2> f)
        {
            return seq.Select(f);
        }

        public static void Iter<T>(this IEnumerable<T> seq, Action<T> f)
        {
            foreach (var x in seq)
                f(x);
        }

        public static void IterI<T>(this IEnumerable<T> seq, Action<T, int> f)
        {
            var i = 0;
            foreach(var x in seq)
            {
                f(x, i);
                i++;
            }
        }

        public static T FoldL<T>(this IEnumerable<T> seq, Func<T, T, T> f)
        {
            return seq.Aggregate(f);
        }

        public static T FoldR<T>(this IEnumerable<T> seq, Func<T, T, T> f)
        {
            return seq.Reverse().Aggregate(f);
        }

        public static TR FoldL<T, TR>(this IEnumerable<T> seq, Func<TR, T, TR> f, TR a)
        {
            return seq.Aggregate(a, f);
        }

        public static TR FoldR<T, TR>(this IEnumerable<T> seq, Func<TR, T, TR> f, TR a)
        {
            return seq.Reverse().Aggregate(a, f);
        }

        public static IEnumerable<TR> Unfold<T, TR>(this T s, Func<T, Tuple<TR, T>> n)
        {
            T x = s;
            while (true)
            {
                var y = n(x);
                yield return y.Item1;
                x = y.Item2;
            }
        }

        public static void Iter2<T1, T2>(this IEnumerable<T1> s1, IEnumerable<T2> s2, Action<T1, T2> f)
        {
            var e1 = s1.GetEnumerator();
            var e2 = s2.GetEnumerator();
            bool b1, b2;
            while ((b1 = e1.MoveNext()) & (b2 = e2.MoveNext()))
                f(e1.Current, e2.Current);

            if (b1 != b2)
                throw new ArgumentOutOfRangeException("s1", "Length not match");
        }

        public static void Iter2I<T1, T2>(this IEnumerable<T1> s1, IEnumerable<T2> s2, Action<T1, T2, int> f)
        {
            var e1 = s1.GetEnumerator();
            var e2 = s2.GetEnumerator();
            var i = 0;
            bool b1, b2;
            while ((b1 = e1.MoveNext()) & (b2 = e2.MoveNext()))
            {
                f(e1.Current, e2.Current, i);
                i++;
            }

            if (b1 != b2)
                throw new ArgumentOutOfRangeException("s1", "Length not match");
        }

        public static IEnumerable<T3> Map2<T1, T2, T3>(this IEnumerable<T1> s1, IEnumerable<T2> s2, Func<T1, T2, T3> f)
        {
            var e1 = s1.GetEnumerator();
            var e2 = s2.GetEnumerator();
            bool b1, b2;
            while ((b1 = e1.MoveNext()) & (b2 = e2.MoveNext()))
                yield return f(e1.Current, e2.Current);

            if (b1 != b2)
                throw new ArgumentOutOfRangeException("s1", "Length not match");
        }

        public static IEnumerable<T3> Map2I<T1, T2, T3>(this IEnumerable<T1> s1, IEnumerable<T2> s2, Func<T1, T2, int, T3> f)
        {
            var e1 = s1.GetEnumerator();
            var e2 = s2.GetEnumerator();
            var i = 0;
            bool b1, b2;
            while ((b1 = e1.MoveNext()) & (b2 = e2.MoveNext()))
            {
                yield return f(e1.Current, e2.Current, i);
                i++;
            }
            if (b1 != b2)
                throw new ArgumentOutOfRangeException("s1", "Length not match");
        }

        public static T3 FoldL2<T1, T2, T3>(this IEnumerable<T1> s1, IEnumerable<T2> s2, Func<T3, T1, T2, T3> f, T3 a)
        {
            var e1 = s1.GetEnumerator();
            var e2 = s2.GetEnumerator();
            var e3 = a;
            bool b1, b2;
            while ((b1 = e1.MoveNext()) & (b2 = e2.MoveNext()))
                e3 = f(e3, e1.Current, e2.Current);

            if (b1 != b2)
                throw new ArgumentOutOfRangeException();
            return e3;
        }

        public static T3 FoldR2<T1, T2, T3>(this IEnumerable<T1> s1, IEnumerable<T2> s2, Func<T3, T1, T2, T3> f, T3 a)
        {
            return s1.Reverse().FoldL2(s2.Reverse(), f, a);
        }


        public static Option<T> Find<T>(this IEnumerable<T> seq, Func<T, bool> f)
        {
            return seq.Map(Option.Some).FirstOrDefault(x => x.Match(f, () => false));
        }

        public static IEnumerable<T> Filter<T>(this IEnumerable<T> seq, Func<T, bool> f)
        {
            return seq.Where(f);
        }

        public static IEnumerable<T> FilterOut<T>(this IEnumerable<T> seq, Func<T, bool> f)
        {
            return seq.Where(x => !f(x));
        }

        public static Tuple<IEnumerable<T>, IEnumerable<T>> Partition<T>(this IEnumerable<T> seq, Func<T, bool> f)
        {
            return Tuple.Create(seq.Where(f), seq.Where(x => !f(x)));
        }

        public static T2 Assoc<T1, T2>(this IEnumerable<Tuple<T1, T2>> seq, T1 a)
        {
            return seq.First(x => x.Item1.Equals(a)).Item2;
        }

        public static T2 Assoc<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> seq, T1 a)
        {
            return seq.First(x => x.Key.Equals(a)).Value;
        }

        public static bool MemAssoc<T1, T2>(this IEnumerable<Tuple<T1, T2>> seq, T1 a)
        {
            return seq.Any(x => x.Item1.Equals(a));
        }

        public static bool MemAssoc<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> seq, T1 a)
        {
            return seq.Any(x => x.Key.Equals(a));
        }

        public static IEnumerable<Tuple<T1, T2>> RemoveAssoc<T1, T2>(this IEnumerable<Tuple<T1, T2>> seq, T1 a)
        {
            var found = false;
            return seq.SkipWhile(x => !found && (found = x.Item1.Equals(a)));
        }

        public static IEnumerable<KeyValuePair<T1, T2>> RemoveAssoc<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> seq, T1 a)
        {
            var found = false;
            return seq.SkipWhile(x => !found && (found = x.Key.Equals(a)));
        }

        public static Tuple<IEnumerable<T1>, IEnumerable<T2>> SplitNX<T1, T2>(this IEnumerable<Tuple<T1, T2>> seq)
        {
            return Tuple.Create(seq.Map(x => x.Item1), seq.Map(x => x.Item2));
        }

        public static KeyValuePair<IEnumerable<T1>, IEnumerable<T2>> SplitNX<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> seq)
        {
            return new KeyValuePair<IEnumerable<T1>, IEnumerable<T2>>(seq.Map(x => x.Key), seq.Map(x => x.Value));
        }

        public static IEnumerable<Tuple<T1, T2>> CombineNX<T1, T2>(this Tuple<IEnumerable<T1>, IEnumerable<T2>> seq)
        {
            return CombineTupleNX(seq.Item1, seq.Item2);
        }

        public static IEnumerable<KeyValuePair<T1, T2>> CombineNX<T1, T2>(this KeyValuePair<IEnumerable<T1>, IEnumerable<T2>> seq)
        {
            return CombineKvpNX(seq.Key, seq.Value);
        }

        public static IEnumerable<KeyValuePair<T1, T2>> CombineNX<T1, T2>(this IEnumerable<T1> s1, IEnumerable<T2> s2)
        {
            return s1.Map2(s2, (x, y) => new KeyValuePair<T1, T2>(x, y));
        }

        public static IEnumerable<Tuple<T1, T2>> CombineTupleNX<T1, T2>(this IEnumerable<T1> s1, IEnumerable<T2> s2)
        {
            return s1.Map2(s2, (x, y) => Tuple.Create(x, y));
        }

        public static IEnumerable<KeyValuePair<T1, T2>> CombineKvpNX<T1, T2>(this IEnumerable<T1> s1, IEnumerable<T2> s2)
        {
            return s1.Map2(s2, (x, y) => new KeyValuePair<T1, T2>(x, y));
        }

        public static IEnumerable<T> Sort<T>(this IEnumerable<T> seq, Func<T, T, int> f)
        {
            return seq.OrderBy(x => x, new TComparer<T>(f));
        }

        public static IEnumerable<T> Sort<T>(this IEnumerable<T> seq)
        {
            return seq.OrderBy(x => x);
        }

        public static T2 Match<T, T2>(this IEnumerable<T> seq, Func<T, IEnumerable<T>, T2> head_tail, Func<T2> empty)
        {
            if (seq.Any())
                return head_tail(seq.First(), seq.Skip(1));
            else
                return empty();
        }

        public static Diff<T> Diff<T>(this IEnumerable<T> seq, IEnumerable<T> target)
        {
            return seq.Diff(target, EqualityComparer<T>.Default);
        }

        public static Diff<T> Diff<T>(this IEnumerable<T> seq, IEnumerable<T> target, Func<T, T, bool> comparer)
        {
            return seq.Diff(target, new TEquialityComparer<T>(comparer));
        }

        public static Diff<T> Diff<T>(this IEnumerable<T> seq, IEnumerable<T> target, IEqualityComparer<T> comp)
        {
            return new Diff<T>(seq, target, comp);
        }
    }

    public enum DiffState
    {
        Added,
        Removed,
        NoChange
    }

    internal class DiffItem<T>
    {
        public T Item { get; private set; }

        public DiffState State { get; private set; }

        public DiffItem(T i, DiffState s)
        {
            Item = i;
            State = s;
        }
    }

    internal class Diff<T>
    {
        public IEnumerable<DiffItem<T>> Items { get; private set; }

        public IEnumerable<T> AddedItems
        {
            get
            {
                return Items.Filter(x => x.State == DiffState.Added).Map(x => x.Item);
            }
        }

        public IEnumerable<T> RemovedItems
        {
            get
            {
                return Items.Filter(x => x.State == DiffState.Removed).Map(x => x.Item);
            }
        }

        public IEnumerable<T> NoChangeItems
        {
            get
            {
                return Items.Filter(x => x.State == DiffState.NoChange).Map(x => x.Item);
            }
        }

        public IEnumerable<T> Before
        {
            get
            {
                return Items.Filter(x => x.State == DiffState.Removed || x.State == DiffState.NoChange).Map(x => x.Item);
            } 
        }

        public IEnumerable<T> After
        {
            get
            {
                return Items.Filter(x => x.State == DiffState.Added || x.State == DiffState.NoChange).Map(x => x.Item);
            } 
        }

        public Diff(IEnumerable<T> x, IEnumerable<T> y)
            : this(x, y, EqualityComparer<T>.Default)
        {
        }

        public Diff(IEnumerable<T> x, IEnumerable<T> y, IEqualityComparer<T> comp)
        {
            Items = CreateItems(x, y, comp).ToArray();
        }

        public void Print()
        {
            foreach (var x in Items)
            {
                switch (x.State)
                {
                    case DiffState.NoChange:
                        Console.WriteLine("  {0}", x.Item);
                        break;
                    case DiffState.Added:
                        ConsoleNX.ColoredWriteLine("+ {0}", ConsoleColor.DarkGreen, x.Item);
                        break;
                    case DiffState.Removed:
                        ConsoleNX.ColoredWriteLine("- {0}", ConsoleColor.DarkRed, x.Item);
                        break;
                }
            }
        }

        static IEnumerable<DiffItem<TI>> CreateItems<TI>(IEnumerable<TI> a, IEnumerable<TI> b, IEqualityComparer<TI> comp)
        {
            var c = b.Filter(x => a.Any(y => comp.Equals(x, y))).ToArray();
            if (c.LengthNX() == 0)
            {
                foreach (var x in a)
                    yield return new DiffItem<TI>(x, DiffState.Removed);
                foreach (var x in b)
                    yield return new DiffItem<TI>(x, DiffState.Added);
            }
            else
            {
                var ae = a.GetEnumerator();
                var be = b.GetEnumerator();
                foreach (var z in c)
                {
                    while (ae.MoveNext())
                    {
                        if (comp.Equals(ae.Current, z))
                            break;
                        yield return new DiffItem<TI>(ae.Current, DiffState.Removed);
                    }
                    while (be.MoveNext())
                    {
                        if (comp.Equals(be.Current, z))
                            break;
                        yield return new DiffItem<TI>(be.Current, DiffState.Added);
                    }
                    yield return new DiffItem<TI>(z, DiffState.NoChange);
                }
                while (ae.MoveNext())
                {
                    yield return new DiffItem<TI>(ae.Current, DiffState.Removed);
                }
                while (be.MoveNext())
                {
                    yield return new DiffItem<TI>(be.Current, DiffState.Added);
                }
            }
        }
    }

    internal static class EnumNX
    {
        public static T Parse<T>(object s)
        {
            return (T)Enum.Parse(typeof(T), s.ToString());
        }
    }

    internal class Using<T>
        where T : IDisposable
    {
        public T Source { get; set; }

        public Using(T source)
        {
            Source = source;
        }
    }

    internal static class Using
    {
        public static Using<T> Use<T>(this T source)
            where T : IDisposable
        {
            return new Using<T>(source);
        }

        public static TR Map<T, TR>(this Using<T> source, Func<T, TR> selector)
            where T : IDisposable
        {
            using (source.Source)
                return selector(source.Source);
        }

        public static TR Map2<T1, T2, TR>(this Using<T1> a, Using<T2> b, Func<T1, T2, TR> f)
            where T1 : IDisposable
            where T2 : IDisposable
        {
            using (a.Source)
            using (b.Source)
                return f(a.Source, b.Source);
        }

        public static TR SelectMany<T, T2, TR>
        (this Using<T> source, Func<T, Using<T2>> second, Func<T, T2, TR> selector)
            where T : IDisposable
            where T2 : IDisposable
        {
            return Map2(source, second(source.Source), selector);
        }

        public static void SelectMany<T, T2>
        (this Using<T> source, Func<T, Using<T2>> second, Action<T, T2> selector)
            where T : IDisposable
            where T2 : IDisposable
        {
            Map2(source, second(source.Source), (x, y) =>
            {
                selector(x, y);
                return Unit.Value;
            });
        }

        public static TR Select<T, TR>(this Using<T> source, Func<T, TR> selector)
            where T : IDisposable
        {
            return source.Map(selector);
        }
    }

    internal struct Option<T> : IEquatable<Option<T>>
    {
        public bool HasValue { get; private set; }

        public T Value { get; private set; }

        public bool HasException { get; private set; }

        public Exception InnerException { get; private set; }

        public Option(T a)
        {
            if (a is Object && a == null)
            {
                this.HasValue = this.HasException = false;
                this.Value = default(T);
                this.InnerException = null;
            }
            else
            {
                this.HasValue = true;
                this.Value = a;
                this.InnerException = null;
                this.HasException = false;
            }
        }

        public Option(Exception e, bool dummy)
        {
            this.HasValue = false;
            this.HasException = true;
            this.Value = default(T);
            this.InnerException = e;
        }

        public static implicit operator Option<T>(Option<DummyNX> d)
        {
            return new Option<T>();
        }

        public static implicit operator Option<T>(T a)
        {
            return new Option<T>(a);
        }

        public bool Equals(Option<T> other)
        {
            if (this.HasValue && other.HasValue)
                return EqualityComparer<T>.Default.Equals(this.Value, other.Value);
            else if (!this.HasValue && !other.HasValue)
                return true;
            else
                return false;
        }

        public bool WeakEquals(Option<T> other)
        {
            return this.Equals(other);
        }

        public bool WeakEquals<T2>(Option<T2> other)
        {
            if (!this.HasValue && !other.HasValue)
                return true;
            else
                return false;
        }

        public override string ToString()
        {
            if (HasValue)
                return string.Format("Some({0})", Value);
            else if (HasException)
                return string.Format("None(\"{0}\")", InnerException.Message);
            else
                return "None";
        }

        public override bool Equals(object obj)
        {
            if (obj is Option<T>)
                return ((Option<T>)obj).Equals(this);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return 421 ^ this.HasValue.GetHashCode() ^ (this.HasValue ? 0 : Value.GetHashCode());
        }

        public static bool operator ==(Option<T> a, Option<T> b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Option<T> a, Option<T> b)
        {
            return !a.Equals(b);
        }

        public static Option<T> operator |(Option<T> a, Option<T> b)
        {
            return a.HasValue ? a : b;
        }

        public static Option<Tuple<T, T>> operator &(Option<T> a, Option<T> b)
        {
            return (a.HasValue && b.HasValue) ? Tuple.Create(a.Value, b.Value).Some() : Option.None;
        }

        public static bool operator true(Option<T> a)
        {
            return a.HasValue;
        }

        public static bool operator false(Option<T> a)
        {
            return !a.HasValue;
        }

        /// <summary>
        /// You can also use <code>Option.None</code>.
        /// </summary>
        public static Option<T> None
        {
            get
            {
                return new Option<T>();
            }
        }
    }

    internal struct DummyNX
    {

    }

    public sealed class Unit : IEquatable<Unit>
    {
        public override bool Equals(object obj)
        {
            return obj is Unit;
        }

        public bool Equals(Unit o)
        {
            return true;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "";
        }

        public static Unit Value
        {
            get
            {
                return new Unit();
            }
        }
    }

    internal static class Option
    {
        /// <summary>
        /// <code>Some(null)</code> will be <code>None</code>.
        /// </summary>
        public static Option<T> Some<T>(this T a)
        {
            return new Option<T>(a);
        }

        public static Option<Unit> Some()
        {
            return Some(Unit.Value);
        }

        /// <summary>
        /// You can also use <code>Option&lt;T&gt;.None</code>.
        /// </summary>
        public static Option<DummyNX> None
        {
            get
            {
                return new Option<DummyNX>();
            }
        }

        public static void May<T>(this Option<T> o, Action<T> f)
        {
            if (o.HasValue)
                f(o.Value);
        }

        /// <summary>
        /// If <paramref name="f"/> can throw an exception, use <code>TryMap</code> instead.
        /// </summary>
        public static Option<T2> Map<T, T2>(this Option<T> a, Func<T, T2> f)
        {
            return a.HasValue ? Some(f(a.Value)) : None;
        }

        public static Option<T2> Select<T, T2>(this Option<T> a, Func<T, T2> f)
        {
            return a.TryMap(f);
        }

        public static Option<T> Flatten<T>(this Option<Option<T>> a)
        {
            return a.Match(x => x, () => None);
        }

        /// <summary>
        /// If <paramref name="f"/> can throw an exception, use <code>TryMap2</code> instead.
        /// </summary>
        public static Option<TR> Map2<T1, T2, TR>(this Option<T1> a, Option<T2> b, Func<T1, T2, TR> f)
        {
            return (a.HasValue && b.HasValue) ? Some(f(a.Value, b.Value)) : None;
        }

        public static Option<TR> SelectMany<T1, T2, TR>(this Option<T1> a, Func<T1, Option<T2>> bf, Func<T1, T2, TR> f)
        {
            return a.TryMap2(bf(a.Value), f);
        }

        public static Option<T2> Bind<T, T2>(this Option<T> a, Func<T, Option<T2>> f)
        {
            return a.HasValue ? f(a.Value) : None;
        }

        public static T Default<T>(this Option<T> a, T b)
        {
            return a.HasValue ? a.Value : b;
        }

        public static T DefaultLazy<T>(this Option<T> a, Func<T> b)
        {
            return a.HasValue ? a.Value : b();
        }

        /// <summary>
        /// throws NullReferenceException when None.
        /// </summary>
        public static T ForceUnwrap<T>(this Option<T> a)
        {
            if (a.HasValue)
                return a.Value;
            else
                throw new NullReferenceException("This is None.");
        }

        /// <param name="aborter">You must abort execution (throw, Environment.Exit, etc) inside here. </param>
        public static T AbortNone<T>(this Option<T> a, Action aborter)
        {
            if (a.HasValue)
                return a.Value;
            else
            {
                aborter();
                throw new NotImplementedException("The aborter did not abort!");
            }
        }

        /// <summary>
        /// If <paramref name="f"/> can throw an exception, use <code>TryMapDefault</code> instead.
        /// </summary>
        public static T2 MapDefault<T, T2>(this Option<T> a, Func<T, T2> f, T2 b)
        {
            return a.HasValue ? f(a.Value) : b;
        }

        public static TR Match<T, TR>(this Option<T> a, Func<T, TR> Some, Func<TR> None)
        {
            return a.HasValue ? Some(a.Value) : None();
        }

        public static void Match<T>(this Option<T> a, Action<T> Some, Action None)
        {
            if (a.HasValue)
                Some(a.Value);
            else
                None();
        }

        public static TR MatchEx<T, TR>(this Option<T> a, Func<T, TR> some, Func<Exception, TR> err, Func<TR> none)
        {
            return a.HasValue 
                ? some(a.Value) 
                : a.HasException 
                    ? err(a.InnerException) 
                    : none();
        }

        public static void MatchEx<T>(this Option<T> a, Action<T> some, Action<Exception> err, Action none = null)
        {
            if (a.HasValue)
                some(a.Value);
            else if (a.HasException)
                err(a.InnerException);
            else if (none != null)
                none();
        }

        public static Option<T> Filter<T>(this Option<T> a, Func<T, bool> pred)
        {
            return a.Match(x => pred(x), () => false) ? a : None;
        }

        public static Option<T> FilterOut<T>(this Option<T> a, Func<T, bool> pred)
        {
            return a.Match(x => !pred(x), () => false) ? a : None;
        }

        public static Option<T> Where<T>(this Option<T> a, Func<T, bool> pred)
        {
            return a.Filter(pred);
        }

        public static bool Check<T>(this Option<T> a, Func<T, bool> pred)
        {
            if (a.HasValue)
                return pred(a.Value);
            else
                return false;
        }

        public static Option<T> Overwrite<T>(this Option<T> a, T value)
        {
            return value.Some();
        }

        public static IEnumerable<T> ToSeq<T>(this Option<T> a)
        {
            return a.Map(x => New.Seq(x)).DefaultLazy(() => New.Seq<T>());
        }

        public static IEnumerable<T> FlattenToSeq<T>(this Option<IEnumerable<T>> a)
        {
            return a.DefaultLazy(() => New.Seq<T>());
        }

        /// <summary>
        /// Alias of <code>TryMap</code>.
        /// </summary>
        public static Option<TR> Try<T, TR>(this Option<T> a, Func<T, TR> f)
        {
            return a.TryMap(f);
        }

        public static Option<TR> Try<T, TR>(this T t, Func<T, TR> f)
        {
            return Try(() => f(t));
        }

        public static Option<T> Try<T>(this Func<T> f)
        {
            try
            {
                return Some(f());
            }
            catch (Exception e)
            {
                return new Option<T>(e, false);
            }
        }

        /// <summary>
        /// Alias of <code>TryMap</code>.
        /// </summary>
        public static Option<Unit> Try<T, TR>(this Option<T> a, Action<T> f)
        {
            return a.TryMap(f);
        }

        public static Option<Unit> Try<T>(this T t, Action<T> f)
        {
            return Try(() => f(t));
        }

        public static Option<Unit> Try(this Action f)
        {
            try
            {
                f();
                return Some(Unit.Value);
            }
            catch (Exception e)
            {
                return new Option<Unit>(e, false);
            }
        }

        public static Option<TR> TryMap<T, TR>(this Option<T> t, Func<T, TR> f)
        {
            return !t.HasValue ?
                t.HasException ?
                new Option<TR>(t.InnerException, false) :
                new Option<TR>(new NullReferenceException("This is None<" + typeof(T).Name + ">"), false)
                    : Try(() => f(t.Value));
        }

        public static Option<Unit> TryMap<T>(this Option<T> t, Action<T> f)
        {
            return !t.HasValue ? 
                t.HasException ? 
                new Option<Unit>(t.InnerException, false) :
                new Option<Unit>(new NullReferenceException("This is None<" + typeof(T).Name + ">"), false)
                    : Try(() => f(t.Value));
        }

        public static Option<TR> TryMap2<T1, T2, TR>(this Option<T1> a, Option<T2> b, Func<T1, T2, TR> f)
        {
            return a.HasValue && b.HasValue 
                ? Try(() => f(a.Value, b.Value)) 
                    : a.HasValue 
                ? b.HasException 
                ? new Option<TR>(b.InnerException, false) 
                    : new Option<TR>(new NullReferenceException("Second value is None<" + typeof(T1).Name + ">"), false)
                    : a.HasException
                ? new Option<TR>(a.InnerException, false) 
                    : new Option<TR>(new NullReferenceException("First value is None<" + typeof(T2).Name + ">"), false);
        }

        public static TR TryMapDefault<T, TR>(this Option<T> t, Func<T, TR> f, TR d)
        {
            return t.TryMap(f).Default(d);
        }
    }

    internal struct VTuple<T, U>
    {
        public readonly T Item1;
        public readonly U Item2;

        public VTuple(T l, U r)
        {
            Item1 = l; Item2 = r;
        }
    }

    internal struct VTuple<T, U, V>
    {
        public readonly T Item1;
        public readonly U Item2;
        public readonly V Item3;

        public VTuple(T l, U c, V r)
        {
            Item1 = l;
            Item2 = c;
            Item3 = r;
        }
    }

    internal struct VTuple<T, U, V, W>
    {
        public readonly T Item1;
        public readonly U Item2;
        public readonly V Item3;
        public readonly W Item4;

        public VTuple(T l, U cl, V cr, W r)
        {
            Item1 = l;
            Item2 = cl;
            Item3 = cr;
            Item4 = r;
        }
    }

    internal static class VTupleExtensions
    {
        public static VTuple<T, U> Create<T, U>(T l, U r)
        {
            return new VTuple<T, U>(l, r);
        }

        public static VTuple<T, U, V> Create<T, U, V>(T l, U c, V r)
        {
            return new VTuple<T, U, V>(l, c, r);
        }

        public static VTuple<T, U, V, W> Create<T, U, V, W>(T l, U cl, V cr, W r)
        {
            return new VTuple<T, U, V, W>(l, cl, cr, r);
        }

        public static TR Merge <T1, T2, TR>(this VTuple<T1, T2> t, Func<T1, T2, TR> f)
        {
            return f(t.Item1, t.Item2);
        }

        public static TR Merge <T1, T2, T3, TR>(this VTuple<T1, T2, T3> t, Func<T1, T2, T3, TR> f)
        {
            return f(t.Item1, t.Item2, t.Item3);
        }

        public static TR Merge <T1, T2, T3, T4, TR>(this VTuple<T1, T2, T3, T4> t, Func<T1, T2, T3, T4, TR> f)
        {
            return f(t.Item1, t.Item2, t.Item3, t.Item4);
        }

        public static VTuple<T1, T2> Map<T1, T2>(this VTuple<T1, T2> t, Func<T1, T1> f1, Func<T2, T2> f2)
        {
            return New.VTuple(f1(t.Item1), f2(t.Item2));
        }

        public static VTuple<T1, T2, T3> Map<T1, T2, T3>(this VTuple<T1, T2, T3> t, Func<T1, T1> f1, Func<T2, T2> f2, Func<T3, T3> f3)
        {
            return New.VTuple(f1(t.Item1), f2(t.Item2), f3(t.Item3));
        }

        public static VTuple<T1, T2, T3, T4> Map<T1, T2, T3, T4>(this VTuple<T1, T2, T3, T4> t, Func<T1, T1> f1, Func<T2, T2> f2, Func<T3, T3> f3, Func<T4, T4> f4)
        {
            return New.VTuple(f1(t.Item1), f2(t.Item2), f3(t.Item3), f4(t.Item4));
        }

        public static T Nth<T>(this VTuple<T, T> t, int i)
        {
            if (i == 0)
                return t.Item1;
            else if (i == 1)
                return t.Item2;
            else
                throw new IndexOutOfRangeException(i + " is out of range " + string.Format("(VTuple<{0}, {0}>)", typeof(T).Name));
        }

        public static T Nth<T>(this VTuple<T, T, T> t, int i)
        {
            if (i == 0)
                return t.Item1;
            else if (i == 1)
                return t.Item2;
            else if (i == 2)
                return t.Item3;
            else
                throw new IndexOutOfRangeException(i + " is out of range " + string.Format("(VTuple<{0}, {0}, {0}>)", typeof(T).Name));
        }

        public static T Nth<T>(this VTuple<T, T, T, T> t, int i)
        {
            if (i == 0)
                return t.Item1;
            else if (i == 1)
                return t.Item2;
            else if (i == 2)
                return t.Item3;
            else if (i == 3)
                return t.Item4;
            else
                throw new IndexOutOfRangeException(i + " is out of range " + string.Format("(VTuple<{0}, {0}, {0}>)", typeof(T).Name));
        }
    }

    internal static class TupleNX
    {
        public static TR Merge <T1, T2, TR>(this Tuple<T1, T2> t, Func<T1, T2, TR> f)
        {
            return f(t.Item1, t.Item2);
        }

        public static TR Merge <T1, T2, T3, TR>(this Tuple<T1, T2, T3> t, Func<T1, T2, T3, TR> f)
        {
            return f(t.Item1, t.Item2, t.Item3);
        }

        public static TR Merge <T1, T2, T3, T4, TR>(this Tuple<T1, T2, T3, T4> t, Func<T1, T2, T3, T4, TR> f)
        {
            return f(t.Item1, t.Item2, t.Item3, t.Item4);
        }

        public static Tuple<T1, T2> Map<T1, T2>(this Tuple<T1, T2> t, Func<T1, T1> f1, Func<T2, T2> f2)
        {
            return Tuple.Create(f1(t.Item1), f2(t.Item2));
        }

        public static Tuple<T1, T2, T3> Map<T1, T2, T3>(this Tuple<T1, T2, T3> t, Func<T1, T1> f1, Func<T2, T2> f2, Func<T3, T3> f3)
        {
            return Tuple.Create(f1(t.Item1), f2(t.Item2), f3(t.Item3));
        }

        public static Tuple<T1, T2, T3, T4> Map<T1, T2, T3, T4>(this Tuple<T1, T2, T3, T4> t, Func<T1, T1> f1, Func<T2, T2> f2, Func<T3, T3> f3, Func<T4, T4> f4)
        {
            return Tuple.Create(f1(t.Item1), f2(t.Item2), f3(t.Item3), f4(t.Item4));
        }

        public static T Nth<T>(this Tuple<T, T> t, int i)
        {
            if (i == 0)
                return t.Item1;
            else if (i == 1)
                return t.Item2;
            else
                throw new IndexOutOfRangeException(i + " is out of range " + string.Format("(Tuple<{0}, {0}>)", typeof(T).Name));
        }

        public static T Nth<T>(this Tuple<T, T, T> t, int i)
        {
            if (i == 0)
                return t.Item1;
            else if (i == 1)
                return t.Item2;
            else if (i == 2)
                return t.Item3;
            else
                throw new IndexOutOfRangeException(i + " is out of range " + string.Format("(Tuple<{0}, {0}, {0}>)", typeof(T).Name));
        }

        public static T Nth<T>(this Tuple<T, T, T, T> t, int i)
        {
            if (i == 0)
                return t.Item1;
            else if (i == 1)
                return t.Item2;
            else if (i == 2)
                return t.Item3;
            else if (i == 3)
                return t.Item4;
            else
                throw new IndexOutOfRangeException(i + " is out of range " + string.Format("(Tuple<{0}, {0}, {0}>)", typeof(T).Name));
        }
    }

    internal static class StreamNX
    {
        public static void WriteString(this Stream stream, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        public static void WriteString(this Stream stream, string value, Encoding enc)
        {
            var bytes = enc.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }
    }

    internal static class Shell
    {
        public static string GetUnixEnvironmentVariable(string variable)
        {
            var env = Environment.GetEnvironmentVariable(variable);
            if (!string.IsNullOrEmpty(env))
                return env;
            else
                return Shell.Eval("sh", "-c", string.Format("'echo ${0}'", variable)).Replace(Environment.NewLine, "");
        }

        public static string Eval(string command, params string[] args)
        {
            var p = new Process();
            p.EnableRaisingEvents = false;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = args.JoinToString(" ");
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();
            return p.StandardOutput.ReadToEnd();
        }

        public static void Execute(string command, params string[] args)
        {
            var p = new Process();
            p.EnableRaisingEvents = false;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.FileName = command;
            p.StartInfo.Arguments = args.JoinToString(" ");
            p.Start();
            p.WaitForExit(); 
        }
    }

    internal static class ConsoleNX
    {
        public static void ColoredWrite(string s, ConsoleColor foreground, params object[] args)
        {
            Console.ForegroundColor = foreground;
            Console.Write(s, args);
            Console.ResetColor();
        }

        public static void ColoredWriteLine(string s, ConsoleColor foreground, params object[] args)
        {
            Console.ForegroundColor = foreground;
            Console.WriteLine(s, args);
            Console.ResetColor();
        }

        public static void ColoredWrite(string s, ConsoleColor foreground, ConsoleColor background, params object[] args)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Write(s, args);
            Console.ResetColor();
        }

        public static void ColoredWriteLine(string s, ConsoleColor foreground, ConsoleColor background, params object[] args)
        {
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.WriteLine(s, args);
            Console.ResetColor();
        }
    }
}
