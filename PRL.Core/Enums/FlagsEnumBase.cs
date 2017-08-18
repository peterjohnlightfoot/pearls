using System;
using System.Linq.Expressions;

namespace Ng8
{
   /// <summary>
   ///    Represents enum flags.
   /// </summary>
   public class FlagsEnumBase<T> : EnumBase<T>
      where T : struct
   {
      // PL : 2013.07.06

      /// <summary>
      ///    Boolean OR.
      /// </summary>
      private static readonly Func<T, T, T> __Or;

      /// <summary>
      ///    Boolean AND.
      /// </summary>
      private static readonly Func<T, T, T> __And;

      /// <summary>
      ///    Boolean NOT.
      /// </summary>
      private static readonly Func<T, T> __Not;

      /// <summary>
      ///    Static constructor.
      /// </summary>
      static FlagsEnumBase() {
         if( !typeof(T).IsDefined(typeof(FlagsAttribute), false) ) {
            throw new NotSupportedException("The type '" + typeof(T).FullName + "' is not a flags enum.");
         }
         // Parameters for various expression trees
         ParameterExpression param1 = Expression.Parameter(typeof(T), "x");
         ParameterExpression param2 = Expression.Parameter(typeof(T), "y");
         Expression convertedParam1 = Expression.Convert(param1, UnderlyingType);
         Expression convertedParam2 = Expression.Convert(param2, UnderlyingType);
         __Or = Expression.Lambda<Func<T, T, T>>(Expression.Convert(Expression.Or(convertedParam1, convertedParam2), typeof(T)), param1, param2).Compile();
         __And = Expression.Lambda<Func<T, T, T>>(Expression.Convert(Expression.And(convertedParam1, convertedParam2), typeof(T)), param1, param2).Compile();
         __Not = Expression.Lambda<Func<T, T>>(Expression.Convert(Expression.Not(convertedParam1), typeof(T)), param1).Compile();

         UsedBits = default(T);
         foreach( T value in Values ) {
            UsedBits = __Or(UsedBits, value);
         }
         AllBits = __Not(default(T));
         UnusedBits = __And(AllBits, (__Not(UsedBits)));
      }

      /// <summary>
      ///    Constructor.
      /// </summary>
      public FlagsEnumBase() : this(default(T)) { }

      /// <summary>
      ///    Constructor.
      /// </summary>
      /// <param name="value"> The initial value. </param>
      public FlagsEnumBase( T value ) : base(value) { }

      /// <summary>
      ///    Gets or sets the instance value.
      /// </summary>
      public override T Value {
         get { return base.Value; }
         set {
            T original = base.Value;
            if( !Equals(original, value) ) {
               value = ValidateFlags(original, value);
            }
            base.Value = value;
         }
      }

      /// <summary>
      ///    The used bits.
      /// </summary>
      public static T UsedBits { get; }

      /// <summary>
      ///    A value representing all bits.
      /// </summary>
      public static T AllBits { get; }

      /// <summary>
      ///    A value representing the unused bits.
      /// </summary>
      public static T UnusedBits { get; }

      /// <summary>
      ///    Occurs when the flags value is changing.
      /// </summary>
      public event EventHandler<FlagsChangeEventArgs<T>> FlagsChanging;

      /// <summary>
      ///    Called when the flag value is changing.
      /// </summary>
      /// <param name="original"> The original value. </param>
      /// <param name="proposed"> The proposed new value. </param>
      /// <returns> The (potentially) amended new value. </returns>
      private T ValidateFlags( T original, T proposed ) {
         T switchedOn = __And(proposed, __Not(original));
         T switchedOff = __And(original, __Not(proposed));
         OnValidateFlags(ref proposed, switchedOn, switchedOff);
         return proposed;
      }

      /// <summary>
      ///    Called when the flag value is changing.
      /// </summary>
      /// <param name="proposed"> The proposed new value. </param>
      /// <param name="switchedOn"> The switched-on flags. </param>
      /// <param name="switchedOff"> The switched-off flags. </param>
      protected virtual void OnValidateFlags( ref T proposed, T switchedOn, T switchedOff ) {
         if( FlagsChanging != null ) {
            var args = new FlagsChangeEventArgs<T>(proposed, switchedOn, switchedOff);
            FlagsChanging.Invoke(this, args);
            proposed = args.Proposed.Value;
         }
      }

      /// <summary>
      ///    Checks whether the given flag is present in the current value.
      /// </summary>
      /// <param name="flag"> The flags to check for. </param>
      /// <returns> true if <see cref="EnumBase{T}.Value" /> contains <paramref name="flag" /> , otherwise false. </returns>
      public bool Has( T flag ) {
         return Equals(flag, __And(Value, flag));
      }

      /// <summary>
      ///    Performs a bitwise "or" operation.
      /// </summary>
      /// <param name="flag"> The flags to add. </param>
      /// <returns> The new value. </returns>
      public void Add( T flag ) {
         Value = __Or(Value, flag);
      }

      /// <summary>
      ///    Performs a bitwise "and" operation.
      /// </summary>
      /// <param name="flag"> The flags to remove. </param>
      /// <returns> The new value. </returns>
      public void Remove( T flag ) {
         Value = __And(Value, __Not(flag));
      }

      /// <summary>
      ///    Switches the given flags in the specified direction.
      /// </summary>
      /// <param name="flag"> The flags to switch. </param>
      /// <param name="direction"> true to switch the given flags ON, otherwise false. </param>
      /// <returns> The new value. </returns>
      public void Switch( T flag, bool direction ) {
         if( direction ) {
            Add(flag);
         } else {
            Remove(flag);
         }
      }

      /// <summary>
      ///    Performs a bitwise "or" operation.
      /// </summary>
      /// <param name="flag"> The flags to add. </param>
      /// <returns> The new value. </returns>
      public FlagsEnumBase<T> WithAdded( T flag ) {
         Add(flag);
         return this;
      }

      /// <summary>
      ///    Performs a bitwise "and" operation.
      /// </summary>
      /// <param name="flag"> The flags to remove. </param>
      /// <returns> The new value. </returns>
      public FlagsEnumBase<T> WithRemoved( T flag ) {
         Remove(flag);
         return this;
      }

      /// <summary>
      ///    Switches the given flags in the specified direction.
      /// </summary>
      /// <param name="flag"> The flags to switch. </param>
      /// <param name="direction"> true to switch the given flags ON, otherwise false. </param>
      /// <returns> The new value. </returns>
      public FlagsEnumBase<T> WithSwitched( T flag, bool direction ) {
         Switch(flag, direction);
         return this;
      }

      /// <summary>
      ///    Performs a bitwise "or" operation.
      /// </summary>
      /// <param name="left"> The left value. </param>
      /// <param name="right"> The right value. </param>
      /// <returns> The result of the operation. </returns>
      public static T Or( T left, T right ) {
         return __Or(left, right);
      }

      /// <summary>
      ///    Performs a bitwise "and" operation.
      /// </summary>
      /// <param name="left"> The left value. </param>
      /// <param name="right"> The right value. </param>
      /// <returns> The result of the operation. </returns>
      public static T And( T left, T right ) {
         return __And(left, right);
      }

      /// <summary>
      ///    Performs a bitwise "not" operation.
      /// </summary>
      /// <param name="value"> The value. </param>
      /// <returns> The result of the operation. </returns>
      public static T Not( T value ) {
         return __Not(value);
      }

      /// <summary>
      ///    Checks whether the given <paramref name="flag" /> is present in the current <paramref name="value" />.
      /// </summary>
      /// <param name="value"> The current value. </param>
      /// <param name="flag"> The flags to check for. </param>
      /// <returns> true if <paramref name="value" /> contains <paramref name="flag" /> , otherwise false. </returns>
      public static bool Has( T value, T flag ) {
         return Equals(flag, __And(value, flag));
      }

      /// <summary>
      ///    Performs a bitwise "or" operation.
      /// </summary>
      /// <param name="value"> The current value. </param>
      /// <param name="flag"> The flags to add. </param>
      /// <returns> The new value. </returns>
      public static T Add( T value, T flag ) {
         return __Or(value, flag);
      }

      /// <summary>
      ///    Performs a bitwise "and" operation.
      /// </summary>
      /// <param name="value"> The current value. </param>
      /// <param name="flag"> The flags to remove. </param>
      /// <returns> The new value. </returns>
      public static T Remove( T value, T flag ) {
         return __And(value, __Not(flag));
      }

      /// <summary>
      ///    Switches the given flags in the specified direction.
      /// </summary>
      /// <param name="value"> The current value. </param>
      /// <param name="flag"> The flags to switch. </param>
      /// <param name="direction"> true to switch the given flags ON, otherwise false. </param>
      /// <returns> The new value. </returns>
      public static T Switch( T value, T flag, bool direction ) {
         return direction
            ? Add(value, flag)
            : Remove(value, flag);
      }

      /// <summary>
      ///    Implicit cast from enum flags value.
      /// </summary>
      /// <param name="value"> The value. </param>
      public static implicit operator FlagsEnumBase<T>( T value ) {
         return new FlagsEnumBase<T>(value);
      }

      /// <summary>
      ///    Explicit cast to enum flags value.
      /// </summary>
      /// <param name="flags"> The flags container instance. </param>
      public static explicit operator T( FlagsEnumBase<T> flags ) {
         return flags.Value;
      }
   }
}
