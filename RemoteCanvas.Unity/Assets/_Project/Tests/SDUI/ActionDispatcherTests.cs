using NUnit.Framework;
using SDUI.Core;

namespace SDUI.Tests
{
	[TestFixture]
	public class ActionDispatcherTests
	{
		private ActionDispatcher _dispatcher;

		[SetUp]
		public void SetUp()
		{
			_dispatcher = new ActionDispatcher();
		}

		[Test]
		public void Dispatch_CallsRegisteredHandler()
		{
			var called = false;
			_dispatcher.Register("open_shop", _ => called = true);

			_dispatcher.Dispatch("open_shop");

			Assert.IsTrue(called);
		}

		[Test]
		public void Dispatch_PassesPayload_WhenActionHasColon()
		{
			string receivedPayload = null;
			_dispatcher.Register("claim_reward", p => receivedPayload = p);

			_dispatcher.Dispatch("claim_reward:halloween_daily");

			Assert.AreEqual("halloween_daily", receivedPayload);
		}

		[Test]
		public void Dispatch_DoesNotThrow_WhenNoHandlerRegistered()
		{
			Assert.DoesNotThrow(() => _dispatcher.Dispatch("unknown_action"));
		}

		[Test]
		public void Dispatch_DoesNotThrow_WhenActionIsEmpty()
		{
			Assert.DoesNotThrow(() => _dispatcher.Dispatch(string.Empty));
		}

		[Test]
		public void Register_OverwritesPreviousHandler()
		{
			var firstCalled  = false;
			var secondCalled = false;

			_dispatcher.Register("open_shop", _ => firstCalled  = true);
			_dispatcher.Register("open_shop", _ => secondCalled = true);

			_dispatcher.Dispatch("open_shop");

			Assert.IsFalse(firstCalled);
			Assert.IsTrue(secondCalled);
		}
	}
}