using System;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ng8.Tests
{
   /// <summary>
   ///    Tests for <see cref="DisposableBase" />.
   /// </summary>
   [TestClass]
   public class DisposableBaseTests
   {
      // PL : 2012.01.10

      /// <summary>
      ///    The test instance.
      /// </summary>
      private readonly TestDisposable _TestInstance = new TestDisposable();

      /// <summary>
      ///    Event counter.
      /// </summary>
      private int _Count;

      /// <summary>
      ///    Flag indicating if <see cref="_TestInstance" /> is disposed.
      /// </summary>
      private bool? _IsDisposed;

      /// <summary>
      ///    Test for Dispose().
      /// </summary>
      [TestMethod]
      [Description("Test for Dispose().")]
      public void CanDispose() {
         Assert.IsFalse(_TestInstance.IsDisposed);
         _TestInstance.Dispose();
         Assert.IsTrue(_TestInstance.IsDisposed);
      }

      /// <summary>
      ///    Test raising of the Disposing event.
      /// </summary>
      [TestMethod]
      [Description("Test raising of the Disposing event.")]
      public void RaisesDisposing() {
         Assert.AreEqual(0, _Count);
         Assert.IsFalse(_IsDisposed.HasValue);
         _TestInstance.Disposing += TestInstanceDisposing;
         _TestInstance.Dispose();
         _TestInstance.Disposing -= TestInstanceDisposing;
         Assert.AreEqual(1, _Count);
         Assert.IsTrue(_IsDisposed.HasValue);
         Assert.IsFalse(_IsDisposed.Value);
      }

      /// <summary>
      ///    Handles the Disposing event.
      /// </summary>
      /// <param name="sender"> The originator of the event. </param>
      /// <param name="e"> The event arguments. </param>
      private void TestInstanceDisposing( object sender, EventArgs e ) {
         if( sender is TestDisposable instance ) {
            _IsDisposed = instance.IsDisposed;
         }
         _Count++;
      }

      /// <summary>
      ///    Test raising of the Disposed event.
      /// </summary>
      [TestMethod]
      [Description("Test raising of the Disposed event.")]
      public void RaisesDisposed() {
         Assert.AreEqual(0, _Count);
         Assert.IsFalse(_IsDisposed.HasValue);
         _TestInstance.Disposed += TestInstanceDisposed;
         _TestInstance.Dispose();
         _TestInstance.Disposed -= TestInstanceDisposed;
         Assert.AreEqual(1, _Count);
         Assert.IsTrue(_IsDisposed.HasValue);
         Assert.IsTrue(_IsDisposed.Value);
      }

      /// <summary>
      ///    Handles the Disposed event.
      /// </summary>
      /// <param name="sender"> The originator of the event. </param>
      /// <param name="e"> The event arguments. </param>
      private void TestInstanceDisposed( object sender, EventArgs e ) {
         if( sender is TestDisposable instance ) {
            _IsDisposed = instance.IsDisposed;
         }
         _Count++;
      }

      /// <summary>
      ///    Test throwing on access of disposed instance.
      /// </summary>
      [TestMethod]
      [ExpectedException(typeof(ObjectDisposedException))]
      [Description("Test throwing on access of disposed instance.")]
      public void ThrowsIfDisposed() {
         _TestInstance.Dispose();
         _TestInstance.InvalidMemberAccess();
      }

      /// <summary>
      ///    A test implementation of <see cref="DisposableBase" />.
      /// </summary>
      private class TestDisposable : DisposableBase
      {
         /// <summary>
         ///    Access to potentially invalid state.
         /// </summary>
         public void InvalidMemberAccess() {
            ThrowIfDisposed();
         }
      }
   }
}
