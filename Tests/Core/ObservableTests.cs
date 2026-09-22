using DisplayNodes.Core;

namespace DisplayNodes.Tests.Core;

[TestFixture]
public class ObservableTests
{
	[Test]
	public void Constructor_SetsInitialValue()
	{
		var observable = new Observable<int>(42);
		Assert.That(observable.Value, Is.EqualTo(42));
	}

	[Test]
	public void Subscribe_CalledImmediatelyOnValueChange()
	{
		var observable = new Observable<string>("initial");
		string? lastValue = null;

		_ = observable.Subscribe(v => lastValue = v);
		observable.Value = "new";

		Assert.That(lastValue, Is.EqualTo("new"));
	}

	[Test]
	public void Subscribe_NotCalledWhenValueUnchanged()
	{
		var observable = new Observable<int>(10);
		int callCount = 0;

		_ = observable.Subscribe(_ => callCount++);
		observable.Value = 10; // Same value

		Assert.That(callCount, Is.EqualTo(0));
	}

	[Test]
	public void Unsubscribe_StopsNotifications()
	{
		var observable = new Observable<int>(0);
		int lastValue = -1;

		var subscription = observable.Subscribe(v => lastValue = v);
		observable.Value = 1;
		Assert.That(lastValue, Is.EqualTo(1));

		subscription.Dispose();
		observable.Value = 2;
		Assert.That(lastValue, Is.EqualTo(1)); // Not updated
	}

	[Test]
	public void MultipleSubscribers_AllNotified()
	{
		var observable = new Observable<int>(0);
		int subscriber1 = 0, subscriber2 = 0;

		_ = observable.Subscribe(v => subscriber1 = v);
		_ = observable.Subscribe(v => subscriber2 = v);

		observable.Value = 42;

		using (Assert.EnterMultipleScope())
		{
			Assert.That(subscriber1, Is.EqualTo(42));
			Assert.That(subscriber2, Is.EqualTo(42));
		}
	}

	[Test]
	public void ExceptionInSubscriber_DoesNotBreakChain()
	{
		var observable = new Observable<int>(0);
		int lastValue = -1;

		_ = observable.Subscribe(_ => throw new InvalidOperationException("Test"));
		_ = observable.Subscribe(v => lastValue = v);

		Assert.DoesNotThrow(() => observable.Value = 10);
		Assert.That(lastValue, Is.EqualTo(10));
	}
}