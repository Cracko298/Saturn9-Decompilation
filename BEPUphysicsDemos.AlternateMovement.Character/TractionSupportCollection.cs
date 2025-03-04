using System;
using System.Collections;
using System.Collections.Generic;
using BEPUphysics.CollisionTests;
using BEPUphysics.DataStructures;

namespace BEPUphysicsDemos.AlternateMovement.Character;

public struct TractionSupportCollection : IEnumerable<ContactData>, IEnumerable
{
	public struct Enumerator : IEnumerator<ContactData>, IDisposable, IEnumerator
	{
		private int currentIndex;

		private RawList<SupportContact> supports;

		public ContactData Current => supports.Elements[currentIndex].Contact;

		object IEnumerator.Current => Current;

		public Enumerator(RawList<SupportContact> supports)
		{
			currentIndex = -1;
			this.supports = supports;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			while (++currentIndex < supports.Count)
			{
				if (supports.Elements[currentIndex].HasTraction)
				{
					return true;
				}
			}
			return false;
		}

		public void Reset()
		{
			currentIndex = -1;
		}
	}

	private RawList<SupportContact> supports;

	public TractionSupportCollection(RawList<SupportContact> supports)
	{
		this.supports = supports;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(supports);
	}

	IEnumerator<ContactData> IEnumerable<ContactData>.GetEnumerator()
	{
		return new Enumerator(supports);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return new Enumerator(supports);
	}
}
