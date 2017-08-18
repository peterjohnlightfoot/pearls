using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

// ReSharper disable StaticFieldInGenericType
namespace Ng8
{
   /// <summary>
   ///    Provides a generic enum accessor class.
   /// </summary>
   /// <typeparam name="T"> The enum type. Must be a struct enum. </typeparam>
   public class EnumBase<T>
      where T : struct
   {
      // PL : 2013.07.16

      /// <summary>
      ///    The value to description map.
      /// </summary>
      private static readonly Dictionary<T, string> __ValueToDescriptionMap;

      /// <summary>
      ///    The description to value map.
      /// </summary>
      private static readonly Dictionary<string, T> __DescriptionToValueMap;

      /// <summary>
      ///    The equality comparer.
      /// </summary>
      private static readonly Func<T, T, bool> __Equality;

      /// <summary>
      ///    A test for empty flags.
      /// </summary>
      private static readonly Func<T, bool> __IsEmpty;

      /// <summary>
      ///    The value.
      /// </summary>
      private T _Value;

      /// <summary>
      ///    Static constructor.
      /// </summary>
      static EnumBase() {
         Values = new ReadOnlyCollection<T>((T[])Enum.GetValues(typeof(T)));
         Names = new ReadOnlyCollection<string>(Enum.GetNames(typeof(T)));
         __ValueToDescriptionMap = new Dictionary<T, string>();
         __DescriptionToValueMap = new Dictionary<string, T>();
         foreach( T value in Values ) {
            string description = FindValueDescription(value);
            __ValueToDescriptionMap[value] = description;
            if( description != null && !__DescriptionToValueMap.ContainsKey(description) ) {
               __DescriptionToValueMap[description] = value;
            }
         }
         UnderlyingType = Enum.GetUnderlyingType(typeof(T));
         // Parameters for various expression trees
         ParameterExpression param1 = Expression.Parameter(typeof(T), "x");
         ParameterExpression param2 = Expression.Parameter(typeof(T), "y");
         Expression convertedParam1 = Expression.Convert(param1, UnderlyingType);
         Expression convertedParam2 = Expression.Convert(param2, UnderlyingType);
         __Equality = Expression.Lambda<Func<T, T, bool>>(Expression.Equal(convertedParam1, convertedParam2), param1, param2).Compile();
         __IsEmpty = Expression.Lambda<Func<T, bool>>(Expression.Equal(convertedParam1, Expression.Constant(Activator.CreateInstance(UnderlyingType))), param1).Compile();
      }

      /// <summary>
      ///    Constructor.
      /// </summary>
      /// <param name="value"> The initial value. </param>
      public EnumBase( T value ) {
         _Value = value;
      }

      /// <summary>
      ///    Gets or sets the instance value.
      /// </summary>
      public virtual T Value {
         get { return _Value; }
         set { _Value = value; }
      }

      /// <summary>
      ///    The values of the current type.
      /// </summary>
      public static IList<T> Values { get; }

      /// <summary>
      ///    The names of the current type.
      /// </summary>
      public static IList<string> Names { get; }

      /// <summary>
      ///    The underlying type.
      /// </summary>
      public static Type UnderlyingType { get; }

      /// <summary>
      ///    Tests whether the two values are equal.
      /// </summary>
      /// <param name="left"> The left value. </param>
      /// <param name="right"> The right value. </param>
      /// <returns> The result of the operation. </returns>
      public static bool Equals( T left, T right ) {
         return __Equality(left, right);
      }

      /// <summary>
      ///    Tests whether the given value is considered "empty".
      /// </summary>
      /// <param name="value"> The value. </param>
      /// <returns> The result of the operation. </returns>
      public static bool IsEmpty( T value ) {
         return __IsEmpty(value);
      }

      /// <summary>
      ///    Gets the description from a <see cref="System.ComponentModel.DescriptionAttribute" /> on the given enum value.
      /// </summary>
      /// <param name="value"> The enum value. </param>
      /// <returns> The enum value description (or its name if no description is defined). </returns>
      protected static string FindValueDescription( T value ) {
         string name = Enum.GetName(typeof(T), value);
         FieldInfo field = typeof(T).GetField(name);
         return field.GetCustomAttributes(typeof(DescriptionAttribute), false).Cast<DescriptionAttribute>().Select(x => x.Description).FirstOrDefault() ?? name;
      }

      /// <summary>
      ///    Gets the description for the given enum value.
      /// </summary>
      /// <param name="value"> The enum value. </param>
      /// <returns> The requested description. </returns>
      public static string GetDescription( T value ) {
         return __ValueToDescriptionMap[value];
      }

      /// <summary>
      ///    Gets the enum value of the specified type that corresponds to the given <paramref name="description" />.
      /// </summary>
      /// <param name="description"> The description. </param>
      /// <returns> The requested enum value. </returns>
      public static T GetValueFromDescription( string description ) {
         if( __DescriptionToValueMap.TryGetValue(description, out T result) ) {
            return result;
         }
         throw new InvalidOperationException("No member found in enumeration '" + typeof(T) + "' with a description matching '" + description + "'.");
      }
   }
}
// ReSharper restore StaticFieldInGenericType
