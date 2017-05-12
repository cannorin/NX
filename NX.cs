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

namespace NX.Variant
{
    public struct Option<T> : IEquatable<Option<T>>
    {
        public bool HasValue { get; private set; }

        public T Value { get; private set; }

        public Option(T a)
        {
            if (a == null)
            {
                this.HasValue = false;
                this.Value = default(T);
            }
            else
            {
                this.HasValue = true;
                this.Value = a;
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
            return a.Map2(bf(a.Value), f);
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
                throw new NullReferenceException(string.Format("This is None<{0}>.", typeof(T).Name));
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
    }

    public struct Either<T, U>
    {
        T _left;
        public T Left { get { if(IsLeft) return _left; throw new NoValueException("This is Right<{0}>.", typeof(U).Name); } }
        
        U _right;
        public U Right { get { if(IsRight) return _right; throw new NoValueException("This is Left<{0}>.", typeof(T).Name); } }
        
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

        public static V Match<T, U, V>(this Either<T, U> a, Func<T, V> Left, Func<U, V> Right)
        {
            if(a.IsLeft)
                return Left(a.Left);
            else
                return Right(a.Right);
        }

        public static Option<T> LeftOrNone<T, U>(this Either<T, U> a)
        {
            return a.Match(l => Option.Some(l), _ => Option.None);
        }

        public static Option<U> RightOrNone<T, U>(this Either<T, U> a)
        {
            return a.Match(_ => Option.None, r => Option.Some(r));
        }
        
        public static Either<T2, U> MapLeft<T1, T2, U>(this Either<T1, U> a, Func<T1, T2> f)
        {
            return a.Bimap(f, x => x);
        }

        public static Either<T, U2> MapRight<T, U1, U2>(this Either<T, U1> a, Func<U1, U2> f)
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

        public static T LeftOrDefault<T, _>(this Either<T, _> a, T def)
        {
            return a.IsLeft ? a.Left : def;
        }

        public static U RightOrDefault<_, U>(this Either<_, U> a, U def)
        {
            return a.IsRight ? a.Right : def;
        }
    }

    public class NoValueException : Exception
    {
        public NoValueException(string message, params object[] args)
            : base(string.Format(message, args))
        {
            
        }
    }
}

namespace NX
{
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

        public static Unit Value
        {
            get
            {
                return new Unit();
            }
        }
    }
}

