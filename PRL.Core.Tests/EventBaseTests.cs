using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ng8.Tests
{
   /// <summary>
   ///    Tests for <see cref="EventBase{T}" />.
   /// </summary>
   [TestClass]
   public class EventBaseTests
   {
      // PL : 2012.02.25

      /// <summary>
      ///    Event counter.
      /// </summary>
      private int _Counter;

      /// <summary>
      ///    The test handler instance.
      /// </summary>
      private EventBase<EventHandler> _Handler;

      /// <summary>
      ///    The invocation list.
      /// </summary>
      private List<InvocationInfo> _InvocationList;

      /// <summary>
      ///    Test initialization.
      /// </summary>
      [TestInitialize]
      [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "PreTest")]
      public void PreTestInitialize() {
         _Handler = new EventBase<EventHandler>();
         _InvocationList = new List<InvocationInfo>();
         _Counter = 0;
      }

      /// <summary>
      ///    Test the instantiation of an instance.
      /// </summary>
      [TestMethod]
      [Description("Test the instantiation of an instance.")]
      public void CanInstantiate() {
         Assert.IsNotNull(_Handler);
      }

      /// <summary>
      ///    Test the invokation of a null instance.
      /// </summary>
      [TestMethod]
      [Description("Test the invokation of a null instance.")]
      public void CanNullInvoke() {
         _Handler = null;
         Assert.IsNull(_Handler);
         _Handler.Invoke(this, EventArgs.Empty);
      }

      /// <summary>
      ///    Test adding and removing handlers.
      /// </summary>
      [TestMethod]
      [Description("Test adding and removing handlers.")]
      public void CanAddRemove() {

         // ensure there are no handlers
         Assert.AreEqual(0, _Handler.Count);

         EventBase<EventHandler>.Add(_Handler, HandleEvent);
         Assert.AreEqual(1, _Handler.Count);

         EventBase<EventHandler>.Add(_Handler, HandleEvent);
         Assert.AreEqual(2, _Handler.Count);

         EventBase<EventHandler>.Remove(_Handler, HandleEvent);
         Assert.AreEqual(1, _Handler.Count);

         EventBase<EventHandler>.Remove(_Handler, HandleEvent);
         Assert.AreEqual(0, _Handler.Count);

         EventBase<EventHandler>.Add(_Handler, HandleEvent);
         Assert.AreEqual(1, _Handler.Count);
      }

      /// <summary>
      ///    Test adding and removing handlers via the operators.
      /// </summary>
      [TestMethod]
      [Description("Test adding and removing handlers via the operators.")]
      public void CanPlusMinus() {

         // ensure there are no handlers
         Assert.AreEqual(0, _Handler.Count);

         _Handler += HandleEvent;
         Assert.AreEqual(1, _Handler.Count);

         _Handler += HandleEvent;
         Assert.AreEqual(2, _Handler.Count);

         _Handler -= HandleEvent;
         Assert.AreEqual(1, _Handler.Count);

         _Handler -= HandleEvent;
         Assert.IsNull(_Handler);

         _Handler += HandleEvent;
         Assert.AreEqual(1, _Handler.Count);
      }

      /// <summary>
      ///    Test invoking the event.
      /// </summary>
      [TestMethod]
      [Description("Test invoking the event.")]
      public void TestInvoke() {

         // ensure there are no handlers
         Assert.AreEqual(0, _Handler.Count);
         Assert.AreEqual(0, _Counter);

         EventBase<EventHandler>.Add(_Handler, HandleEvent);

         _Handler.Invoke(this, EventArgs.Empty);
         Assert.AreEqual(1, _InvocationList.Count);
      }

      /// <summary>
      ///    Test invoking the event multiple times.
      /// </summary>
      [TestMethod]
      [Description("Test invoking the event multiple times.")]
      public void TestInvokeMultiple() {

         // ensure there are no handlers
         Assert.AreEqual(0, _Handler.Count);
         Assert.AreEqual(0, _Counter);

         EventBase<EventHandler>.Add(_Handler, HandleEvent);
         EventBase<EventHandler>.Add(_Handler, HandleEvent);
         EventBase<EventHandler>.Add(_Handler, HandleEvent);

         _Handler.Invoke(this, EventArgs.Empty);
         Assert.AreEqual(3, _InvocationList.Count);
      }

      /// <summary>
      ///    Test invoking the event on a thread with a context.
      /// </summary>
      [TestMethod]
      [Description("Test invoking the event on a thread with a context.")]
      public void TestInvokeOnContext() {

         // ensure there are no handlers
         Assert.AreEqual(0, _Handler.Count);
         Assert.AreEqual(0, _Counter);

         var waitHandle = new ManualResetEvent(false);

         var thread = new Thread(() => {
            SynchronizationContext.SetSynchronizationContext(new TestSynchronizationContext(waitHandle));
            EventBase<EventHandler>.Add(_Handler, HandleEvent);
            waitHandle.Set();
         }) {
            Name = TestSynchronizationContext.THREAD_NAME
         };

         thread.Start();

         if( !waitHandle.WaitOne(1000) ) {
            Assert.Fail("event not set in time allocated");
         } else {
            waitHandle.Reset();
            _Handler.Invoke(this, EventArgs.Empty);
            if( !waitHandle.WaitOne(1000) ) {
               Assert.Fail("event not fired in time allocated");
            } else {
               Assert.AreEqual(1, _InvocationList.Count);
               InvocationInfo info = _InvocationList[0];
               Assert.IsInstanceOfType(info.SynchronizationContext, typeof(TestSynchronizationContext));
               Assert.AreEqual(TestSynchronizationContext.THREAD_NAME, info.ThreadName);
            }
         }
      }

      /// <summary>
      ///    Test adding an invalid handler type.
      /// </summary>
      [TestMethod]
      [ExpectedException(typeof(NotSupportedException))]
      [Description("Test adding an invalid handler type.")]
      public void TestTypeCheck() {
         EventBase<object> invalid = null;
         invalid += new System.ComponentModel.PropertyChangedEventHandler(InvalidEventHandler);
         Assert.IsNull(invalid); // won't reach this point .. the previous line should throw the expected exception
      }

      /// <summary>
      ///    Invalid event hander.
      /// </summary>
      /// <param name="sender"> The originator of the event. </param>
      /// <param name="e"> The event arguments. </param>
      private static void InvalidEventHandler( object sender, System.ComponentModel.PropertyChangedEventArgs e ) {
         throw new NotImplementedException();
      }

      /// <summary>
      ///    Handle the event .
      /// </summary>
      /// <param name="sender"> The originator of the event. </param>
      /// <param name="e"> The event arguments. </param>
      private void HandleEvent( object sender, EventArgs e ) {
         _Counter++;
         _InvocationList.Add(new InvocationInfo(
                                Thread.CurrentThread.Name,
                                SynchronizationContext.Current));
      }

      /// <summary>
      ///    Data stored from invocation.
      /// </summary>
      private class InvocationInfo
      {
         /// <summary>
         ///    Constructor.
         /// </summary>
         /// <param name="threadName"> The thread name. </param>
         /// <param name="synchronizationContext"> The context. </param>
         public InvocationInfo( string threadName, SynchronizationContext synchronizationContext ) {
            SynchronizationContext = synchronizationContext;
            ThreadName = threadName;
         }

         /// <summary>
         ///    Gets the thread name.
         /// </summary>
         public string ThreadName { get; }

         /// <summary>
         ///    Gets a reference to the synchronization context.
         /// </summary>
         public SynchronizationContext SynchronizationContext { get; }
      }

      /// <summary>
      ///    A synchronization context for testing.
      /// </summary>
      private class TestSynchronizationContext : SynchronizationContext
      {
         /// <summary>
         ///    The name of the dispatch thread.
         /// </summary>
         public const string THREAD_NAME = "TestSynchronizationContext_Dispatch_Thread";

         /// <summary>
         ///    The queue.
         /// </summary>
         private readonly Queue _Queue;

         /// <summary>
         ///    The waithandle for write synchronization.
         /// </summary>
         private readonly ManualResetEvent _ReadGate;

         /// <summary>
         ///    The thread that does the reading from the internal queue.
         /// </summary>
         // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
         private readonly Thread _ReadThread;

         /// <summary>
         ///    The wait handle.
         /// </summary>
         private readonly ManualResetEvent _WaitHandle;

         /// <summary>
         ///    <value> true </value>
         ///    to stop the reader.
         /// </summary>
         private bool _Stop;

         /// <summary>
         ///    Constructor.
         /// </summary>
         /// <param name="waitHandle"> The wait handle. </param>
         public TestSynchronizationContext( ManualResetEvent waitHandle ) {
            _WaitHandle = waitHandle;
            _Queue = Queue.Synchronized(new Queue());
            _ReadGate = new ManualResetEvent(false);
            _ReadThread = new Thread(ReaderMethod) {
               Name = THREAD_NAME,
               IsBackground = true
            };
            _ReadThread.Start();
         }

         /// <summary>
         ///    Post the message.
         /// </summary>
         /// <param name="d"> The callback. </param>
         /// <param name="state"> Additional information. </param>
         public override void Post( SendOrPostCallback d, object state ) {
            PushToQueue(new DispatchInfo {
               CallBack = d,
               State = state
            });
         }

         /// <summary>
         ///    Send the message.
         /// </summary>
         /// <param name="d"> The callback. </param>
         /// <param name="state"> Additional information. </param>
         public override void Send( SendOrPostCallback d, object state ) {
            PushToQueue(new DispatchInfo {
               CallBack = d,
               State = state
            });
         }

         /// <summary>
         ///    The reader method for reading from the internal queue.
         /// </summary>
         private void ReaderMethod() {
            SetSynchronizationContext(this);
            while( !_Stop ) {
               _ReadGate.WaitOne();
               if( _Queue.Count > 0 ) {
                  var info = (DispatchInfo)_Queue.Dequeue();
                  info.CallBack(info.State);
                  _WaitHandle.Set();
               } else {
                  _ReadGate.Reset();
                  _Stop = true;
               }
            }
         }

         /// <summary>
         ///    Push an entry to the queue.
         /// </summary>
         /// <param name="dispatchInfo"> The entry. </param>
         private void PushToQueue( DispatchInfo dispatchInfo ) {
            _Queue.Enqueue(dispatchInfo);
            _ReadGate.Set();
         }

         /// <summary>
         ///    Information used in the dispatch.
         /// </summary>
         private class DispatchInfo
         {
            /// <summary>
            ///    The callback.
            /// </summary>
            public SendOrPostCallback CallBack { get; set; }

            /// <summary>
            ///    The state.
            /// </summary>
            public object State { get; set; }
         }
      }
   }
}
