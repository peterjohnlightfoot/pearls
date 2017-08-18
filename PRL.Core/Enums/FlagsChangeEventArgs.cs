using System;

namespace Ng8
{
   /// <summary>
   ///    Represents arguments for flags change events.
   /// </summary>
   public class FlagsChangeEventArgs<T> : EventArgs
      where T : struct
   {
      // PL : 2013.07.12

      /// <summary>
      ///    The original value.
      /// </summary>
      private readonly T _OriginalValue;

      /// <summary>
      ///    Constructor.
      /// </summary>
      /// <param name="proposed"> The proposed value. </param>
      /// <param name="switchedOn"> The switched-on flags. </param>
      /// <param name="switchedOff"> The switched-off flags. </param>
      public FlagsChangeEventArgs( T proposed, T switchedOn, T switchedOff ) {
         _OriginalValue = proposed;
         Proposed = new FlagsEnumBase<T>(proposed);
         Proposed.FlagsChanging += OnFlagsChanging;
         SwitchedOn = switchedOn;
         SwitchedOff = switchedOff;
      }

      /// <summary>
      ///    Gets the switched-on flags.
      /// </summary>
      public T SwitchedOn { get; private set; }

      /// <summary>
      ///    Gets the switched-off flags.
      /// </summary>
      public T SwitchedOff { get; private set; }

      /// <summary>
      ///    Gets or sets the propsed value.
      /// </summary>
      public FlagsEnumBase<T> Proposed { get; }

      /// <summary>
      ///    Called when the attribute flags value is changing.
      /// </summary>
      /// <param name="sender"> The originator of the event. </param>
      /// <param name="e"> The event arguments. </param>
      private void OnFlagsChanging( object sender, FlagsChangeEventArgs<T> e ) {
         SwitchedOn = FlagsEnumBase<T>.And(e.Proposed.Value, FlagsEnumBase<T>.Not(_OriginalValue));
         SwitchedOff = FlagsEnumBase<T>.And(_OriginalValue, FlagsEnumBase<T>.Not(e.Proposed.Value));
      }
   }
}
