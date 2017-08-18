using System;

namespace Ng8
{
   /// <summary>
   ///    Extension methods for <see cref="EventBase{T}" />.
   /// </summary>
   public static class EventBaseExtensions
   {
      // PL : 2009.10.19

      /// <summary>
      ///    Invokes the handlers in the <see cref="EventBase{T}" /> list.
      /// </summary>
      /// <typeparam name="TArgs"> The <see cref="System.Type" /> of the event arguments. </typeparam>
      /// <typeparam name="T"> The <see cref="System.Type" /> of the event handler. </typeparam>
      /// <param name="list"> The <see cref="EventBase{T}" /> list. </param>
      /// <param name="sender"> The originator of the event. </param>
      /// <param name="args"> The event arguments. </param>
      public static void Invoke<TArgs, T>( this EventBase<T> list, object sender, TArgs args )
         where TArgs : EventArgs
         where T : class {

         list?.Invoke_Internal(sender, args);
      }
   }
}
