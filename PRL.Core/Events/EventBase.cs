using System;
using System.Collections.Generic;
using System.Threading;

namespace Ng8
{
   /// <summary>
   ///    Represents an event manager.
   /// </summary>
   /// <typeparam name="T"> The event handler type. </typeparam>
   /// <remarks>
   ///    Type <typeparamref name="T" /> must be a <see cref="System.Delegate" />.
   /// </remarks>
   public sealed class EventBase<T>
      where T : class
   {
      // PL : 2009.10.19

      /// <summary>
      ///   Type not supported error message.
      /// </summary>
      private const string TYPE_NOT_SUPPORTED = "The type ('{0}') is not supported. Type must be a delegate.";

      /// <summary>
      ///   Item not supported error message.
      /// </summary>
      private const string ITEM_NOT_SUPPORTED = "The item supplied is of an unsupported type ('{0}'). Item must be a delegate type.";

      /// <summary>
      ///   The <see cref="EventHandlerInfo" /> items.
      /// </summary>
      private readonly List<EventHandlerInfo> _Items;

      /// <summary>
      ///   The thread lock.
      /// </summary>
      private readonly object _InstanceLock;

      /// <summary>
      ///   The event hook count.
      /// </summary>
      private long _Count;

      /// <summary>
      ///    Constructor.
      /// </summary>
      public EventBase() {
         if( !typeof(Delegate).IsAssignableFrom(typeof(T)) ) {
            throw new NotSupportedException(string.Format(TYPE_NOT_SUPPORTED, typeof(T).FullName));
         }
         _Items = new List<EventHandlerInfo>();
         _InstanceLock = new object();
      }

      /// <summary>
      ///    Gets the array of <see cref="EventHandlerInfo" /> items.
      /// </summary>
      public EventHandlerInfo[] Items {
         get {
            lock( _InstanceLock ) {
               return _Items.ToArray();
            }
         }
      }

      /// <summary>
      ///    Gets the event hook count.
      /// </summary>
      public long Count => Interlocked.Read(ref _Count);

      /// <summary>
      ///    Adds the given <paramref name="item" /> to the list.
      /// </summary>
      /// <param name="item"> The item. </param>
      /// <exception cref="System.NotSupportedException"> If <paramref name="item" /> is not a <see cref="System.Delegate" />. </exception>
      public void Add( T item ) {
         if( item == null ) {
            return;
         }
         lock( _InstanceLock ) {
            // ensure we have a delegate
            var handler = item as Delegate;
            if( handler == null ) {
               throw new NotSupportedException(string.Format(ITEM_NOT_SUPPORTED, typeof(T)));
            }
            // find the handler info
            var info = new EventHandlerInfo(handler);
            int index = _Items.IndexOf(info);
            if( index < 0 ) {
               // add the new one
               _Items.Add(info);
            } else {
               // get the listed item
               info = _Items[index];
               // increment the count
               info.Add();
            }
            // increment the count
            Interlocked.Increment(ref _Count);
         }
      }

      /// <summary>
      ///    Removes the specified <see paramref="item" /> from the list.
      /// </summary>
      /// <param name="item"> The item. </param>
      /// <exception cref="System.NotSupportedException"> If <paramref name="item" /> is not a <see cref="System.Delegate" />. </exception>
      public void Remove( T item ) {
         if( item == null ) {
            return;
         }
         lock( _InstanceLock ) {
            // ensure we have a delegate
            var handler = item as Delegate;
            if( handler == null ) {
               throw new NotSupportedException(string.Format(ITEM_NOT_SUPPORTED, typeof(T)));
            }
            // find the handler info
            var info = new EventHandlerInfo(handler);
            int index = _Items.IndexOf(info);
            if( index <= -1 ) {
               return;
            }
            // get the listed item
            info = _Items[index];
            // decrement the count
            info.Subtract();
            // check if there are any hooks left
            if( info.IsEmpty ) {
               // remove the item
               _Items.RemoveAt(index);
            }
            // decrement the count
            Interlocked.Decrement(ref _Count);
         }
      }

      /// <summary>
      ///    Invokes the handlers in the list.
      /// </summary>
      /// <typeparam name="TArgs"> The <see cref="System.Type" /> of the event arguments. </typeparam>
      /// <param name="sender"> The originator of the event. </param>
      /// <param name="args"> The event arguments. </param>
      internal void Invoke_Internal<TArgs>( object sender, TArgs args ) where TArgs : EventArgs {
         foreach( EventHandlerInfo info in Items ) {
            info.Invoke(sender, args);
         }
      }

      /// <summary>
      ///    Adds the given <paramref name="item" /> to the specified <paramref name="list" />.
      /// </summary>
      /// <param name="list"> The list. </param>
      /// <param name="item"> The item. </param>
      /// <returns> The list. </returns>
      /// <remarks>
      ///    Will create a new <see cref="EventBase{T}" /> list if the given <paramref name="list" /> is null.
      /// </remarks>
      /// <exception cref="System.NotSupportedException"> If <paramref name="item" /> is not a delegate. </exception>
      public static EventBase<T> Add( EventBase<T> list, T item ) {
         if( list == null ) {
            list = new EventBase<T>();
         }
         list.Add(item);
         return list;
      }

      /// <summary>
      ///    Removes the specified <paramref name="item" /> from the given <paramref name="list" />.
      /// </summary>
      /// <param name="list"> The list. </param>
      /// <param name="item"> The item. </param>
      /// <returns> The list. </returns>
      /// <remarks>
      ///    Will create a new <see cref="EventBase{T}" /> list if the given <paramref name="list" /> is null.
      /// </remarks>
      /// <exception cref="System.NotSupportedException"> If <paramref name="item" /> is not a delegate. </exception>
      public static EventBase<T> Remove( EventBase<T> list, T item ) {
         if( list == null ) {
            list = new EventBase<T>();
         }
         list.Remove(item);
         return (list.Count == 0)
            ? null
            : list;
      }

      /// <summary>
      ///    Adds the given <paramref name="item" /> to the specified <paramref name="list" />.
      /// </summary>
      /// <param name="list"> The list. </param>
      /// <param name="item"> The item. </param>
      /// <returns> The list. </returns>
      /// <remarks>
      ///    Will create a new <see cref="EventBase{T}" /> list if the given <paramref name="list" /> is null.
      /// </remarks>
      /// <exception cref="System.NotSupportedException"> If <paramref name="item" /> is not a delegate. </exception>
      public static EventBase<T> operator +( EventBase<T> list, T item ) {
         return Add(list, item);
      }

      /// <summary>
      ///    Removes the specified <paramref name="item" /> from the given <paramref name="list" />.
      /// </summary>
      /// <param name="list"> The list. </param>
      /// <param name="item"> The item. </param>
      /// <returns> The list. </returns>
      /// <remarks>
      ///    Will create a new <see cref="EventBase{T}" /> if the given <paramref name="list" /> is null.
      /// </remarks>
      /// <exception cref="System.NotSupportedException"> If <paramref name="item" /> is not a delegate. </exception>
      public static EventBase<T> operator -( EventBase<T> list, T item ) {
         return Remove(list, item);
      }

      #region Nested Type: EventHandlerInfo

      /// <summary>
      ///    Represents event handler info.
      /// </summary>
      public class EventHandlerInfo
      {
         // PL : 2009.10.19

         /// <summary>
         ///    The synchronization context.
         /// </summary>
         private readonly SynchronizationContext _Context;

         /// <summary>
         ///    The handler <see cref="System.Delegate" />.
         /// </summary>
         private readonly Delegate _Handler;

         /// <summary>
         ///    The event hook count.
         /// </summary>
         private int _Count;

         /// <summary>
         ///    Constructor.
         /// </summary>
         /// <param name="handler"> The handler <see cref="System.Delegate" />. </param>
         public EventHandlerInfo( Delegate handler ) {
            _Context = SynchronizationContext.Current;
            _Handler = handler;
            _Count = 1;
         }

         /// <summary>
         ///    true if there are no hooks.
         /// </summary>
         public bool IsEmpty => _Count == 0;

         /// <summary>
         ///    Increments the hook count.
         /// </summary>
         public void Add() {
            _Count++;
         }

         /// <summary>
         ///    Decrements the hook count.
         /// </summary>
         public void Subtract() {
            _Count--;
         }

         /// <summary>
         ///    Invokes the handler <see cref="System.Delegate" />.
         /// </summary>
         /// <typeparam name="TArgs"> The <see cref="System.Type" /> of the event arguments. </typeparam>
         /// <param name="sender"> The originator of the event. </param>
         /// <param name="args"> The event arguments. </param>
         public void Invoke<TArgs>( object sender, TArgs args ) where TArgs : EventArgs {
            if( _Context != null ) {
               var state = new EventHandlerInfoState(_Handler, sender, args);
               for( int n = 0; n < _Count; n++ ) {
                  _Context.Send(InvokeCallback, state);
               }
            } else {
               for( int n = 0; n < _Count; n++ ) {
                  _Handler.DynamicInvoke(sender, args);
               }
            }
         }

         /// <summary>
         ///    Callback used to marshal the event to the original <see cref="System.Threading.SynchronizationContext" /> that created it.
         /// </summary>
         /// <param name="state"> The event handler info state. </param>
         private static void InvokeCallback( object state ) {
            var infoState = state as EventHandlerInfoState;
            infoState?.Handler.DynamicInvoke(infoState.Sender, infoState.Arguments);
         }

         /// <summary>
         ///    Serves as a hash function for a particular type.
         /// </summary>
         /// <returns> A hash code for the current <see cref="T:System.Object" />. </returns>
         /// <filterpriority> 2 </filterpriority>
         public override int GetHashCode() {
            unchecked {
               return (_Context?.GetHashCode() ?? 0) ^ (_Handler?.GetHashCode() ?? 0);
            }
         }

         /// <summary>
         ///    Determines whether the specified <see cref="EventHandlerInfo" /> is equal to the current <see cref="EventHandlerInfo" />.
         /// </summary>
         /// <param name="obj"> The <see cref="EventHandlerInfo" /> to compare with the current <see cref="EventHandlerInfo" />. </param>
         /// <returns> true if the specified <see cref="EventHandlerInfo" /> is equal to the current <see cref="EventHandlerInfo" /> ; otherwise, false. </returns>
         public bool Equals( EventHandlerInfo obj ) {
            if( ReferenceEquals(null, obj) ) {
               return false;
            }
            if( ReferenceEquals(this, obj) ) {
               return true;
            }
            return Equals(obj._Context, _Context) && Equals(obj._Handler, _Handler);
         }

         /// <summary>
         ///    Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" />.
         /// </summary>
         /// <param name="obj"> The <see cref="T:System.Object" /> to compare with the current <see cref="T:System.Object" />. </param>
         /// <returns> true if the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Object" /> ; otherwise, false. </returns>
         public override bool Equals( object obj ) {
            if( ReferenceEquals(null, obj) ) {
               return false;
            }
            if( ReferenceEquals(this, obj) ) {
               return true;
            }
            return obj.GetType() == typeof(EventHandlerInfo) && Equals((EventHandlerInfo)obj);
         }

         /// <summary>
         ///    Equality operator.
         /// </summary>
         /// <param name="a"> The one instance. </param>
         /// <param name="b"> The other instance. </param>
         /// <returns> true if the instances are considered equal; otherwise, false. </returns>
         public static bool operator ==( EventHandlerInfo a, EventHandlerInfo b ) {
            return ReferenceEquals(a, null)
               ? ReferenceEquals(b, null)
               : a.Equals(b);
         }

         /// <summary>
         ///    Inequality operator.
         /// </summary>
         /// <param name="a"> The one instance. </param>
         /// <param name="b"> The other instance. </param>
         /// <returns> true if the instances are considered unequal; otherwise, false. </returns>
         public static bool operator !=( EventHandlerInfo a, EventHandlerInfo b ) {
            return !(a == b);
         }

         #region Nested Type: EventHandlerInfoState

         /// <summary>
         ///    Represents event handler info state.
         /// </summary>
         private class EventHandlerInfoState
         {
            // PL : 2009.10.19

            /// <summary>
            ///    Constructor.
            /// </summary>
            /// <param name="handler"> The event handler. </param>
            /// <param name="sender"> The originator of the event. </param>
            /// <param name="args"> The event arguments. </param>
            public EventHandlerInfoState( Delegate handler, object sender, object args ) {
               Handler = handler;
               Sender = sender;
               Arguments = args;
            }

            /// <summary>
            ///    The event handler.
            /// </summary>
            public Delegate Handler { get; }

            /// <summary>
            ///    The originator of the event.
            /// </summary>
            public object Sender { get; }

            /// <summary>
            ///    Gets the event arguments.
            /// </summary>
            public object Arguments { get; }
         }

         #endregion
      }

      #endregion
   }
}
