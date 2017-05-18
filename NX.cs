/*
   The X11 License
   NX - extension objects and methods of C#
   Copyright(c) 2017 cannorin

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
    public struct Option<T> : IEquatable<Option<T>>
    {
        public bool HasValue { get; private set; }

        readonly T _value;
        public T Value 
        { 
            get 
            { 
                if(HasValue) 
                    return _value; 
                else 
                    throw new NoValueException("This is None.");
            } 
        }

        public Option(T a)
        {
            if (a == null)
            {
                this.HasValue = false;
                this._value = default(T);
            }
            else
            {
                this.HasValue = true;
                this._value = a;
            }
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

        public static Option<ValueTuple<T, T>> operator &(Option<T> a, Option<T> b)
        {
            return (a.HasValue && b.HasValue) ? ValueTuple.Create(a.Value, b.Value).Some() : Option.None;
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

    public static class Option
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

        public static Option<T2> Map<T, T2>(this Option<T> a, Func<T, T2> f)
        {
            return a.HasValue ? Some(f(a.Value)) : None;
        }

        public static Option<T2> Select<T, T2>(this Option<T> a, Func<T, T2> f)
        {
            return a.Map(f);
        }

        public static Option<T> Flatten<T>(this Option<Option<T>> a)
        {
            return a.Match(x => x, () => None);
        }

        public static Option<TR> Map2<T1, T2, TR>(this Option<T1> a, Option<T2> b, Func<T1, T2, TR> f)
        {
            return (a.HasValue && b.HasValue) ? Some(f(a.Value, b.Value)) : None;
        }

        public static Option<TR> SelectMany<T1, T2, TR>(this Option<T1> a, Func<T1, Option<T2>> bf, Func<T1, T2, TR> f)
        {
            return a.Map2(a.Map(bf).Flatten(), f);
        }

        public static void SelectMany<T1, T2>(this Option<T1> a, Func<T1, Option<T2>> bf, Action<T1, T2> f)
        {
            a.SelectMany(bf, (t, u) => { f(t, u); return Unit.Value; });
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

        public static T ForceUnwrap<T>(this Option<T> a, Action ifNone = null)
        {
            if (a.HasValue)
                return a.Value;
            else 
            {
                if(ifNone != null)
                    ifNone();
                throw new InvalidOperationException(string.Format("This is None<{0}>.", typeof(T).Name));
            }
        }

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

        public static Either<T, U> InlOrDefault<T, U>(this Option<T> a, U def)
        {
            if(a.HasValue)
                return new Either<T, U>(a.Value);
            else 
                return new Either<T, U>(def);
        }

        public static Either<T, U> InrOrDefault<T, U>(this Option<U> a, T def)
        {
            if(a.HasValue)
                return new Either<T, U>(a.Value);
            else
                return new Either<T, U>(def);
        }

        public static Option<ValueTuple<T, U>> And<T, U>(this Option<T> a, Option<U> b)
        {
            if(a.HasValue && b.HasValue)
                return Option.Some(ValueTuple.Create(a.Value, b.Value));
            else
                return Option.None;
        }

        public static Option<Either<T, U>> Or<T, U>(this Option<T> a, Option<U> b)
        {
            if(a.HasValue)
                return Option.Some(new Either<T, U>(a.Value));
            else if(b.HasValue)
                return Option.Some(new Either<T, U>(b.Value));
            else
                return Option.None;
        }
    }

    public struct Either<T, U>
    {
        readonly T _left;
        public T Left 
        { 
            get 
            {
                if(IsLeft) 
                    return _left; 
                throw new NoValueException("This is Right<{0}>.", typeof(U).Name); 
            } 
        }
        
        readonly U _right;
        public U Right 
        { 
            get 
            { 
                if(IsRight) 
                    return _right; 
                throw new NoValueException("This is Left<{0}>.", typeof(T).Name); 
            }
        }
        
        public bool IsLeft { get; private set; }
        public bool IsRight { get { return !IsLeft; } }

        public Either(T l)
        {
            _left = l;
            _right = default(U);
            IsLeft = true;
        }

        public Either(U r)
        {
            _left = default(T);
            _right = r;
            IsLeft = false;
        }

        public override string ToString()
        {
            if(IsLeft)
                return string.Format("Left({0})", _left);
            else
                return string.Format("Right({0})", _right);
        }

        public static implicit operator Either<T, U>(Either<T, DummyNX> a)
        {
            return new Either<T, U>(a.Left);
        }

        public static implicit operator Either<T, U>(Either<DummyNX, U> a)
        {
            return new Either<T, U>(a.Right);
        }
    }

    public static class Either
    {
        public static Either<T, DummyNX> Inl<T>(this T left)
        {
            return new Either<T, DummyNX>(left);
        }

        public static Either<DummyNX, U> Inr<U>(this U right)
        {
            return new Either<DummyNX, U>(right);
        }

        public static Either<U, T> Swap<T, U>(this Either<T, U> a)
        {
            if(a.IsLeft)
                return new Either<U, T>(a.Left);
            else
                return new Either<U, T>(a.Right);
        }

        public static V Match<T, U, V>(this Either<T, U> a, Func<T, V> Left, Func<U, V> Right)
        {
            if(a.IsLeft)
                return Left(a.Left);
            else
                return Right(a.Right);
        }

        public static void Match<T, U>(this Either<T, U> a, Action<T> f, Action<U> g)
        {
            if(a.IsLeft)
                f(a.Left);
            else 
                g(a.Right);
        }

        public static U MatchLeft<T, U>(this Either<T, U> a, Func<T, U> f)
        {
            if(a.IsLeft)
                return f(a.Left);
            else
                return a.Right;
        }

        public static T MatchRight<T, U>(this Either<T, U> a, Func<U, T> f)
        {
            if(a.IsRight)
                return f(a.Right);
            else
                return a.Left;
        }

        public static void May<T, U>(this Either<T, U> a, Action<T> left = null, Action<U> right = null)
        {
            if(left != null && a.IsLeft)
                left(a.Left);
            if(right != null && a.IsRight)
                right(a.Right);
        }
        
        public static T LeftOrDefault<T, _>(this Either<T, _> a, T def)
        {
            return a.IsLeft ? a.Left : def;
        }

        public static U RightOrDefault<_, U>(this Either<_, U> a, U def)
        {
            return a.IsRight ? a.Right : def;
        }
        
        public static Option<T> LeftOrNone<T, U>(this Either<T, U> a)
        {
            return a.Match(l => Option.Some(l), _ => Option.None);
        }

        public static Option<U> RightOrNone<T, U>(this Either<T, U> a)
        {
            return a.Match(_ => Option.None, r => Option.Some(r));
        }

        public static Either<T2, U> LeftMap<T1, T2, U>(this Either<T1, U> a, Func<T1, T2> f)
        {
            return a.Bimap(f, x => x);
        }

        public static Either<T, U2> RightMap<T, U1, U2>(this Either<T, U1> a, Func<U1, U2> f)
        {
            return a.Bimap(x => x, f);
        }

        public static Either<T2, U2> Bimap<T1, U1, T2, U2>(this Either<T1, U1> a, Func<T1, T2> lf, Func<U1, U2> rf)
        {
            if(a.IsLeft)
                return new Either<T2, U2>(lf(a.Left));
            else
                return new Either<T2, U2>(rf(a.Right));
        }

        public static IEnumerable<T> Lefts<T, U>(this IEnumerable<Either<T, U>> xs)
        {
            return xs.Choose(Either.LeftOrNone);
        }

        public static IEnumerable<U> Rights<T, U>(this IEnumerable<Either<T, U>> xs)
        {
            return xs.Choose(Either.RightOrNone);
        }

        public static ValueTuple<IEnumerable<T>, IEnumerable<U>> SplitNX<T, U>(this IEnumerable<Either<T, U>> xs)
        {
            return ValueTuple.Create(xs.Lefts(), xs.Rights());
        }

        public static T Unwrap<T>(this Either<T, T> a)
        {
            if(a.IsLeft)
                return a.Left;
            return a.Right;
        }

        public static Option<T> Unwrap<T>(this Either<T, Unit> a)
        {
            if(a.IsLeft)
                return Option.Some(a.Left);
            return Option.None;
        }

        public static Option<T> Unwrap<T>(this Either<Unit, T> a)
        {
            if(a.IsRight)
                return Option.Some(a.Right);
            return Option.None;
        }
    }

    public class NoValueException : Exception
    {
        public NoValueException(string message, params object[] args)
            : base(string.Format(message, args))
        {
            
        }

        public NoValueException(Exception e, string message, params object[] args)
            : base(string.Format(message, args), e)
        {

        }

    }

    public struct TryResult<T>
    {
        readonly T _value;
        public T Value
        {
            get
            {
                if(HasException)
                    throw new NoValueException(InnerException, "Unhandled exception: '{0}'", InnerException.Message);
                else
                    return _value;
            }
        }

        public readonly bool HasException;
       
        readonly Exception _e;
        public Exception InnerException
        {
            get
            {
                if(HasException)
                    return _e;
                throw new NoValueException("This is Success<{0}>.", typeof(T).Name);
            }
        }

        public TryResult(T value)
        {
            _value = value;
            HasException = false;
            _e = null;
        }

        public TryResult(Exception e, Unit dummy)
        {
            _value = default(T);
            HasException = true;
            _e = e;
        }

        public override string ToString()
        {
            if(HasException)
                return InnerException.ToString();
            else
                return string.Format("Success({0})", Value);
        }

        public TryResult<T> Catch<TE>(Func<TE, T> c)
            where TE : Exception
        {
            if(HasException && InnerException is TE)
                return new TryResult<T>(c((TE)InnerException));
            else return this;
        }

        public TryResult<T> CatchWhen<TE>(Func<TE, bool> cond, Func<TE, T> c)
            where TE : Exception
        {
            if(HasException && InnerException is TE)
            {
                var e = (TE)InnerException;
                if(cond(e))
                    return new TryResult<T>(c(e));
            }
            return this;
        }

        public TryResult<Unit> Catch<TE>(Action<TE> c)
            where TE : Exception
        {
            if(HasException && InnerException is TE)
            {
                c((TE)InnerException);
                return new TryResult<Unit>(Unit.Value);
            }
            else if(HasException)
                return new TryResult<Unit>(InnerException, Unit.Value);
            else 
                return new TryResult<Unit>(Unit.Value);
        }

        public TryResult<Unit> CatchWhen<TE>(Func<TE, bool> cond, Action<TE> c)
            where TE : Exception
        {
            if(HasException && InnerException is TE)
            {
                var e = (TE)InnerException;
                if(cond(e))
                    return new TryResult<Unit>(Unit.Value);
            }
            if(HasException)
                return new TryResult<Unit>(InnerException, Unit.Value);
            else
                return new TryResult<Unit>(Unit.Value);
        }
        
        public TryResult<T> Abort<TE>(Action<TE> c)
            where TE : Exception
        {
            if(HasException && InnerException is TE)
            {
                c((TE)InnerException);
                throw new InvalidOperationException("Aborted by TryResult.Abort.", InnerException);
            }
            return this;
        }

        public TryResult<T> AbortWhen<TE>(Func<TE, bool> cond, Action<TE> c)
            where TE : Exception
        {
            if(HasException && InnerException is TE)
            {
                var e = (TE)InnerException;
                if(cond(e))
                    throw new InvalidOperationException("Aborted by TryResult.Abort.", InnerException);
            }
            return this;
        }
    }

    public static class TryNX
    {
        public static TryResult<TR> Try<T, TR>(this T a, Func<T, TR> f)
        {
            try
            {
                return new TryResult<TR>(f(a));
            }
            catch(Exception e)
            {
                return new TryResult<TR>(e, Unit.Value);
            }
        }

        public static TryResult<Unit> Try<T>(this T a, Action<T> f)
        {
            try
            {
                f(a);
                return new TryResult<Unit>(Unit.Value);
            }
            catch(Exception e)
            {
                return new TryResult<Unit>(e, Unit.Value);
            }
        }

        public static TryResult<T> Try<T>(Func<T> f)
        {
            try
            {
                return new TryResult<T>(f());
            }
            catch(Exception e)
            {
                return new TryResult<T>(e, Unit.Value);
            }
        }

        public static TryResult<Unit> Try(Action f)
        {
            try
            {
                f();
                return new TryResult<Unit>(Unit.Value);
            }
            catch(Exception e)
            {
                return new TryResult<Unit>(e, Unit.Value);
            }
        }

        public static TryResult<U> Map<T, U>(this TryResult<T> a, Func<T, U> f)
        {
            if(a.HasException)
                return new TryResult<U>(a.InnerException, Unit.Value);
            else
                try
                {
                    return new TryResult<U>(f(a.Value));
                }
                catch(Exception e)
                {
                    return new TryResult<U>(e, Unit.Value);
                }
        }

        public static TryResult<U> Select<T, U>(this TryResult<T> a, Func<T, U> f)
        {
            return a.Map(f);
        }

        public static TryResult<V> Map2<T, U, V>(this TryResult<T> a, TryResult<U> b, Func<T, U, V> f)
        {
            if(!a.HasException)
            {
                if(b.HasException)
                    return new TryResult<V>(b.InnerException, Unit.Value);
                else
                    try
                    {
                        return new TryResult<V>(f(a.Value, b.Value));
                    }
                    catch(Exception e)
                    {
                        return new TryResult<V>(e, Unit.Value);
                    }
            }
            else
                return new TryResult<V>(a.InnerException, Unit.Value);
        }

        public static TryResult<V> SelectMany<T, U, V>(this TryResult<T> a, Func<T, TryResult<U>> bf, Func<T, U, V> f)
        {
            return a.Map2(a.Map(bf).Value, f);
        }

        public static void SelectMany<T, U>(this TryResult<T> a, Func<T, TryResult<U>> bf, Action<T, U> f)
        {
            a.SelectMany(bf, (t, u) => { f(t, u); return Unit.Value; });
        }

        public static T CatchAll<T>(this TryResult<T> a, Func<Exception, T> c)
        {
            if(a.HasException)
                return c(a.InnerException);
            else
                return a.Value;
        }

        public static void CatchAll<T>(this TryResult<T> a, Action<Exception> c)
        {
            if(a.HasException)
                c(a.InnerException);
        }

        public static T Evaluate<T>(this TryResult<T> a)
        {
            if(a.HasException)
                throw new NoValueException(a.InnerException, "Unhandled Exception: '{0}'", a.InnerException.Message);
            else
                return a.Value;
        }

        public static void May<T>(this TryResult<T> a, Action<T> f)
        {
            if(!a.HasException)
                f(a.Value);
        }

        public static Option<T> ToOption<T>(this TryResult<T> a)
        {
            if(a.HasException)
                return Option.None;
            else
                return Option.Some(a.Value);
        }
    }

    public static class Seq
    {
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

        public static IEnumerable<bool> Inf(bool b = false)
        {
            return repeatInf(b);
        }

        static IEnumerable<T> repeatInf<T>(T a)
        {
            while (true)
                yield return a;
        }

        public static Option<T> Head<T>(this IEnumerable<T> seq)
        {
            return seq.Try<IEnumerable<T>, T>(Enumerable.First).ToOption();
        }

        public static Option<T> Tail<T>(this IEnumerable<T> seq)
        {
            return seq.Try<IEnumerable<T>, T>(Enumerable.Last).ToOption();
        }

        public static Option<T> Nth<T>(this IEnumerable<T> seq, int n)
        {
            if (seq.GetEnumerator().Try(x => x.Reset()).HasException)
                return seq.Try(xs => xs.Skip(n).First()).ToOption();
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

        public static IEnumerable<TR> Unfold<T, TR>(this T s, Func<T, ValueTuple<TR, T>> n)
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

        public static IEnumerable<T> Cons<T>(this T x, IEnumerable<T> xs)
        {
            return x.Singleton().Concat(xs);
        }

        public static IEnumerable<T> Snoc<T>(this IEnumerable<T> xs, T x)
        {
            return xs.Concat(x.Singleton());
        }

        public static ValueTuple<IEnumerable<T>, IEnumerable<T>> Partition<T>(this IEnumerable<T> seq, Func<T, bool> f)
        {
            return ValueTuple.Create(seq.Where(f), seq.Where(x => !f(x)));
        }

        public static T2 Assoc<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> seq, T1 a)
        {
            return seq.First(x => x.Item1.Equals(a)).Item2;
        }

        public static T2 Assoc<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> seq, T1 a)
        {
            return seq.First(x => x.Key.Equals(a)).Value;
        }

        public static bool MemAssoc<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> seq, T1 a)
        {
            return seq.Any(x => x.Item1.Equals(a));
        }

        public static bool MemAssoc<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> seq, T1 a)
        {
            return seq.Any(x => x.Key.Equals(a));
        }

        public static IEnumerable<ValueTuple<T1, T2>> RemoveAssoc<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> seq, T1 a)
        {
            var found = false;
            return seq.SkipWhile(x => !found && (found = x.Item1.Equals(a)));
        }

        public static IEnumerable<KeyValuePair<T1, T2>> RemoveAssoc<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> seq, T1 a)
        {
            var found = false;
            return seq.SkipWhile(x => !found && (found = x.Key.Equals(a)));
        }

        public static ValueTuple<IEnumerable<T1>, IEnumerable<T2>> SplitNX<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> seq)
        {
            return ValueTuple.Create(seq.Map(x => x.Item1), seq.Map(x => x.Item2));
        }

        public static KeyValuePair<IEnumerable<T1>, IEnumerable<T2>> SplitNX<T1, T2>(this IEnumerable<KeyValuePair<T1, T2>> seq)
        {
            return new KeyValuePair<IEnumerable<T1>, IEnumerable<T2>>(seq.Map(x => x.Key), seq.Map(x => x.Value));
        }

        public static IEnumerable<ValueTuple<T1, T2>> CombineNX<T1, T2>(this ValueTuple<IEnumerable<T1>, IEnumerable<T2>> seq)
        {
            return CombineValueTupleNX(seq.Item1, seq.Item2);
        }

        public static IEnumerable<KeyValuePair<T1, T2>> CombineNX<T1, T2>(this KeyValuePair<IEnumerable<T1>, IEnumerable<T2>> seq)
        {
            return CombineKvpNX(seq.Key, seq.Value);
        }

        public static IEnumerable<KeyValuePair<T1, T2>> CombineNX<T1, T2>(this IEnumerable<T1> s1, IEnumerable<T2> s2)
        {
            return s1.Map2(s2, (x, y) => new KeyValuePair<T1, T2>(x, y));
        }

        public static IEnumerable<ValueTuple<T1, T2>> CombineValueTupleNX<T1, T2>(this IEnumerable<T1> s1, IEnumerable<T2> s2)
        {
            return s1.Map2(s2, (x, y) => ValueTuple.Create(x, y));
        }

        public static IEnumerable<KeyValuePair<T1, T2>> CombineKvpNX<T1, T2>(this IEnumerable<T1> s1, IEnumerable<T2> s2)
        {
            return s1.Map2(s2, (x, y) => new KeyValuePair<T1, T2>(x, y));
        }

        public static IEnumerable<T> SortNX<T>(this IEnumerable<T> seq, Func<T, T, int> f)
        {
            return seq.OrderBy(x => x, new TComparer<T>(f));
        }

        public static IEnumerable<T> SortNX<T>(this IEnumerable<T> seq)
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
    }
    
    public class Using<T>
    {
        public Func<T> Source { get; set; }

        public Using(Func<T> source)
        {
            Source = source;
        }
    }

    public static class Using
    {
        public static Using<T> Use<T>(this T source)
            where T : IDisposable
        {
            return new Using<T>(() => source);
        }

        public static Using<TR> Map<T, TR>(this Using<T> source, Func<T, TR> selector)
            where T : IDisposable
        {
            return new Using<TR>(() => {
                using(var x = source.Source())
                    return selector(x);
            });
        }

        public static Using<TR> Map2<T1, T2, TR>(this Using<T1> a, Using<T2> b, Func<T1, T2, TR> f)
            where T1 : IDisposable
            where T2 : IDisposable
        {
            return new Using<TR>(() => {
                using(var x = a.Source())
                using(var y = b.Source())
                    return f(x, y);
            });
        }

        public static Using<TR> SelectMany<T, T2, TR>
        (this Using<T> source, Func<T, Using<T2>> second, Func<T, T2, TR> selector)
            where T : IDisposable
            where T2 : IDisposable
        {
            return Map2(source, source.Map(x => second(x).Source()), selector);
        }

        public static void SelectMany<T, T2>
        (this Using<T> source, Func<T, Using<T2>> second, Action<T, T2> selector)
            where T : IDisposable
            where T2 : IDisposable
        {
            SelectMany(source, second, (x, y) =>
            {
                selector(x, y);
                return Unit.Value;
            });
        }

        public static Using<TR> Select<T, TR>(this Using<T> source, Func<T, TR> selector)
            where T : IDisposable
        {
            return source.Map(selector);
        }

        public static void Do<T>(this Using<T> source, Action<T> f)
            where T : IDisposable
        {
            using(var x = source.Source())
                f(x);
        }

        public static T Evaluate<T>(this Using<T> source)
        {
            return source.Source();
        }
    }

    public struct DummyNX
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

        public void ToVoid()
        {
        }

        public static Unit Value
        {
            get
            {
                return new Unit();
            }
        }
    }

    public static class Nx
    {
        public static void Ignore<T>(this T _)
        {
        }

        public static TryResult<T> Try<T>(Func<T> f)
        {
            return TryNX.Try(f);
        }

        public static TryResult<Unit> Try(Action f)
        {
            return TryNX.Try(f);
        }

        public static Using<T> Use<T>(Func<T> f)
            where T : IDisposable
        {
            return new Using<T>(f);
        }
    }

    public class TComparer<T> : IComparer<T>
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

    public static class TComparer
    {
        public static TComparer<T> Create<T>(Func<T, T, int> f)
        {
            return new TComparer<T>(f);
        }
    }

    public class TEquialityComparer<T> : IEqualityComparer<T>
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

    public static class TEquialityComparer
    {
        public static TEquialityComparer<T> Create<T> (Func<T, T, bool> f)
        {
            return new TEquialityComparer<T>(f);
        }
    }

    public static class EnumNX
    {
        public static T Parse<T>(object s)
        {
            return (T)Enum.Parse(typeof(T), s.ToString());
        }
    }

}

