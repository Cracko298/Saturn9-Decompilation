using System;

namespace Saturn9;

internal class OperationCompletedEventArgs : EventArgs
{
	private IAsyncResult asyncResult;

	public IAsyncResult AsyncResult
	{
		get
		{
			return asyncResult;
		}
		set
		{
			asyncResult = value;
		}
	}

	public OperationCompletedEventArgs(IAsyncResult asyncResult)
	{
		this.asyncResult = asyncResult;
	}
}
