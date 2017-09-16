using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDTree
{
	public interface IKDTree<TKey, TValue> : IEnumerable<KDTreeNode<TKey, TValue>>
	{
		bool Add(TKey[] point, TValue value);

		bool TryFindValueAt(TKey[] point, out TValue value);

		TValue FindValueAt(TKey[] point);

		bool TryFindValue(TValue value, out TKey[] point);

		TKey[] FindValue(TValue value);

        KDTreeNode<TKey, TValue>[] RangeQuery(TKey[] center, TKey radius, int count);
        
        void RemoveAt(TKey[] point);

		void Clear();

		KDTreeNode<TKey, TValue>[] NearestNeighboursQuery(TKey[] point, int count = int.MaxValue);
		
		int Count { get; }
	}
}
