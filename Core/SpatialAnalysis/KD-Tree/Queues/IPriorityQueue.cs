using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDTree
{
    public interface IPriorityQueue<TItem, TPriority>
	{
		void Enqueue(TItem item, TPriority priority);

		TItem Dequeue();

		int Count { get; }
	}
}
