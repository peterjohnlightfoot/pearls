using System;
using System.Threading;

// ReSharper disable EmptyGeneralCatchClause
namespace Ng8
{
   /// <summary>
   ///    Provides a base implementation of <see cref="IDisposable" />.
   /// </summary>
   /// <remarks> This class is thread safe. </remarks>
   /// <example>
   ///    // Derived classes having unmanaged resources should define the following finalizer 
   ///    // <b> ONLY IF </b> no other class up the inheritance tree has done so:
   ///    ~DisposableBase() {
   ///      Dispose(false);
   ///    }
   /// </example>
   /// <example>
   ///    // Member implementation where inconsistent state might be a concern:
   ///    public void Foo() {
   ///      ThrowIfDisposed();
   ///      // ... your code here
   ///    }
   /// </example>
   public abstract class DisposableBase : IDisposable
   {
      // PL : 2012.01.10

      /// <summary>
      ///    "Not Disposed" state.
      /// </summary>
      private const int NOT_DISPOSED = 0;

      /// <summary>
      ///    "Disposing" state.
      /// </summary>
      private const int DISPOSING = 1;

      /// <summary>
      ///    "Disposed" state.
      /// </summary>
      private const int DISPOSED = 2;

      /// <summary>
      ///    The <see cref="Disposing" /> event manager.
      /// </summary>
      private readonly EventBase<EventHandler> _Disposing;

      /// <summary>
      ///    The <see cref="Disposed" /> event manager.
      /// </summary>
      private readonly EventBase<EventHandler> _Disposed;

      /// <summary>
      ///    Flag to indicate current state.
      /// </summary>
      private int _State;

      /// <summary>
      ///    Constructor.
      /// </summary>
      protected DisposableBase() {
         _Disposing = new EventBase<EventHandler>();
         _Disposed = new EventBase<EventHandler>();
         _State = NOT_DISPOSED;
      }

      /// <summary>
      ///    Gets a flag to indicate whether the current instance has been previously disposed.
      /// </summary>
      public bool IsDisposed => Thread.VolatileRead(ref _State) == DISPOSED;

      /// <summary>
      ///    Occurs when the instance is being disposed.
      /// </summary>
      public event EventHandler Disposing {
         add { _Disposing.Add(value); }
         remove { _Disposing.Remove(value); }
      }

      /// <summary>
      ///    Occurs when the instance has been disposed.
      /// </summary>
      public event EventHandler Disposed {
         add { _Disposed.Add(value); }
         remove { _Disposing.Remove(value); }
      }

      /// <summary>
      ///    Notifies listeners that the instance is being disposed.
      /// </summary>
      protected virtual void OnDisposing() {
         _Disposing.Invoke(this, EventArgs.Empty);
      }

      /// <summary>
      ///    Notifies listeners that the instance has been disposed.
      /// </summary>
      protected virtual void OnDisposed() {
         _Disposed.Invoke(this, EventArgs.Empty);
      }

      /// <summary>
      ///    Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      /// <filterpriority> 2 </filterpriority>
      public void Dispose() {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      /// <summary>
      ///    Final implementation of IDisposable.
      /// </summary>
      /// <param name="disposing"> true to indicate explicit cleanup (i.e. from <see cref="Dispose" />()), otherwise false. </param>
      private void Dispose( bool disposing ) {
         try {
            // 'promote' dispose functionality: CLR will try defer throwing asynchronous exceptions in a "finally"

         } finally {

            if( !IsDisposed ) {

               // if called from Dispose()
               if( disposing ) {

                  // if _State == NOT_DISPOSED, _State = DISPOSING, enter
                  if( Interlocked.CompareExchange(ref _State, DISPOSING, NOT_DISPOSED) == NOT_DISPOSED ) {

                     // notify listeners
                     try {
                        OnDisposing();
                     } catch {
                        // explicit suppression of exceptions during calls to Dispose() or ~T().
                     }

                     // clean up managed resources
                     try {
                        DisposeManagedResources();
                     } catch {
                        // explicit suppression of exceptions during calls to Dispose() or ~T().
                     }

                     // clean up delegates
                     try {
                        RemoveDelegates();
                     } catch {
                        // explicit suppression of exceptions during calls to Dispose() or ~T().
                     }

                     // clean up unmanaged resources
                     try {
                        ReleaseUnmanagedResources();
                     } catch {
                        // explicit suppression of exceptions during calls to Dispose() or ~T().
                     }

                     // mark as disposed
                     Interlocked.Increment(ref _State); // _State = DISPOSED

                     // suppress finalisation
                     try {
                        GC.SuppressFinalize(this);
                     } catch {
                        // explicit suppression of exceptions during calls to Dispose() or ~T().
                     }

                     // notify listeners
                     try {
                        OnDisposed();
                     } catch {
                        // explicit suppression of exceptions during calls to Dispose() or ~T().
                     }

                  } else {
                     // called from finalizer, clean up unmanaged resources
                     try {
                        ReleaseUnmanagedResources();
                     } catch {
                        // explicit suppression of exceptions during calls to Dispose() or ~T().
                     }
                  }
               }
            }
         }
      }

      /// <summary>
      ///    Throws an exception if the current instance has been disposed.
      /// </summary>
      protected void ThrowIfDisposed() {
         if( IsDisposed ) {
            throw new ObjectDisposedException(GetType().FullName, "Access to a disposed object is not allowed.");
         }
      }

      /// <summary>
      ///    When overridden in a derived class, this method should call <see cref="Dispose()" />() on all <see cref="IDisposable" /> resources owned by the instance.
      /// </summary>
      /// <remarks> Called only from <see cref="Dispose()" /> (i.e. explicit cleanup). </remarks>
      protected virtual void DisposeManagedResources() { }

      /// <summary>
      ///    When overridden in a derived class, this method should remove all <b> managed </b> delegates referenced by the instance.
      /// </summary>
      /// <remarks> Called only from <see cref="Dispose()" /> (i.e. explicit cleanup). </remarks>
      protected virtual void RemoveDelegates() { }

      /// <summary>
      ///    When overridden in a derived class, this method should clean up any unmanaged resources.
      /// </summary>
      protected virtual void ReleaseUnmanagedResources() { }
   }
}
