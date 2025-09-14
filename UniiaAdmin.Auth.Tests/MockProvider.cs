using Moq;
using System.Collections;

namespace UniiaAdmin.Auth.Tests;

public class MockProvider : IEnumerable<KeyValuePair<Type, Mock>>
{
	private readonly Dictionary<Type, Mock> _mocks = new();

	public Mock<T> Mock<T>() where T : class
	{
		if (!this._mocks.ContainsKey(typeof(T)))
		{
			_mocks[typeof(T)] = new Mock<T>();
		}

		return (Mock<T>)_mocks[typeof(T)];
	}

	public IEnumerator<KeyValuePair<Type, Mock>> GetEnumerator()
	{
		return _mocks.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
