using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KDTree
{
    [Serializable]
	public class KDTreeNode<TKey, TValue>
	{
		public KDTreeNode()
		{
		}

		public KDTreeNode(TKey[] point, TValue value)
		{
			Point = point;
			Value = value;
		}

		public TKey[] Point;
		public TValue Value = default(TValue);

		internal KDTreeNode<TKey, TValue> LeftChild = null;
		internal KDTreeNode<TKey, TValue> RightChild = null;

		internal KDTreeNode<TKey, TValue> this[int compare]
		{
			get
			{
				if (compare <= 0)
					return LeftChild;
				else
					return RightChild;
			}
			set
			{
				if (compare <= 0)
					LeftChild = value;
				else
					RightChild = value;
			}
		}

		public bool IsLeaf
		{
			get
			{
				return (LeftChild == null) && (RightChild == null);
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();

			for (var dimension = 0; dimension < Point.Length; dimension++)
			{
				sb.Append(Point[dimension].ToString() + "\t");
			}

			if (Value == null)
				sb.Append("null");
			else
				sb.Append(Value.ToString());

			return sb.ToString();
		}
	}
}