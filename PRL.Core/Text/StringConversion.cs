using System;
using System.ComponentModel;

namespace Ng8
{
   // PL : 2011.02.25

   /// <summary>
   ///    Tries to convert the given <paramref name="valueString" /> string to the specified <paramref name="type" />.
   /// </summary>
   /// <param name="valueString"> The source value string. </param>
   /// <param name="type"> The target type for <paramref name="value" />. </param>
   /// <param name="value"> The converted value if the conversion was successful; otherwise null. </param>
   /// <returns> true if the conversion was successful; otherwise false. </returns>
   public delegate bool ConvertFromStringDelegate( string valueString, Type type, out object value );

   /// <summary>
   ///    Tries to convert the given <paramref name="value" /> to its <see cref="string" /> representation.
   /// </summary>
   /// <param name="value"> The source value. </param>
   /// <param name="valueString"> The converted value string if the conversion was successful; otherwise null. </param>
   /// <returns> true if the conversion was successful; otherwise false. </returns>
   public delegate bool ConvertToStringDelegate( object value, out string valueString );

   /// <summary>
   ///    Provides helper functions for string conversion.
   /// </summary>
   public static class StringConversion
   {
      // PL : 2011.02.25

      /// <summary>
      ///    "Conversion From String" error message.
      /// </summary>
      internal const string CONVERSION_FROM_STRING_ERROR = @"No built-in conversion exists for '{0}' from string '{1}'. Try supplying a custom {2} handler.";

      /// <summary>
      ///    "Conversion To String" error message.
      /// </summary>
      internal const string CONVERSION_TO_STRING_ERROR = @"No built-in conversion exists for '{0}' from value '{1}'. Try supplying a custom {2} handler.";

      /// <summary>
      ///    A string converter.
      /// </summary>
      private static readonly TypeConverter __StringConverter;

      /// <summary>
      ///    Static constructor.
      /// </summary>
      static StringConversion() {
         __StringConverter = TypeDescriptor.GetConverter(typeof(string));
      }

      /// <summary>
      ///    Tries to convert the given <paramref name="valueString" /> string to the specified <paramref name="type" />.
      /// </summary>
      /// <param name="valueString"> The source value string. </param>
      /// <param name="type"> The target type for <paramref name="value" />. </param>
      /// <param name="value"> The converted value if the conversion was successful; otherwise null. </param>
      /// <returns> true if the conversion was successful; otherwise false. </returns>
      public static bool TryConvertValueFromString( string valueString, Type type, out object value ) {
         if( __StringConverter.CanConvertTo(type) ) {
            value = __StringConverter.ConvertTo(valueString, type);
            return true;
         }
         TypeConverter converter = TypeDescriptor.GetConverter(type);
         if( converter.CanConvertFrom(typeof(string)) ) {
            value = converter.ConvertFrom(valueString);
            return true;
         }
         value = null;
         return false;
      }

      /// <summary>
      ///    Tries to convert the given <paramref name="value" /> to its <see cref="string" /> representation.
      /// </summary>
      /// <param name="value"> The source value. </param>
      /// <param name="valueString"> The converted value string if the conversion was successful; otherwise <paramref name="value" />.ToString(). </param>
      /// <returns> true if the conversion was successful; otherwise false. </returns>
      public static bool TryConvertValueToString( object value, out string valueString ) {
         Type type = value.GetType();
         if( __StringConverter.CanConvertFrom(type) ) {
            valueString = __StringConverter.ConvertFrom(value) as string;
            return true;
         }
         TypeConverter converter = TypeDescriptor.GetConverter(type);
         if( converter.CanConvertTo(typeof(string)) ) {
            valueString = converter.ConvertTo(value, typeof(string)) as string;
            return true;
         }
         valueString = value.ToString();
         return false;
      }
   }
}
