using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RTree;

namespace RStarTree
{
    /// <summary>
    /// Source: https://en.wikipedia.org/wiki/R*_tree
    /// The R*-tree uses the same algorithm as the regular R-tree for query and delete operations.
    /// When inserting, the R*-tree uses a combined strategy.For leaf nodes, overlap is minimized, while for inner nodes, enlargement and area are minimized.
    /// When splitting, the R*-tree uses a topological split that chooses a split axis based on perimeter, then minimizes overlap.
    /// In addition to an improved split strategy, the R*-tree also tries to avoid splits by reinserting objects and subtrees into the tree, inspired by the concept of balancing a B-tree.
    /// </summary>
    public class RStarTree<T>
    {
        private int maxNodeEntries;
        private int entryCount;
        private int[] ids;
        private RTreeNode<T>[] entries;

        internal void ReorganizeTree(RStarTree<T> rtree)
        {
            int countdownIndex = rtree.maxNodeEntries - 1;
            for (int index = 0; index < entryCount; index++)
            {
                if (entries[index] == null)
                {
                    while (entries[countdownIndex] == null && countdownIndex > index)
                        countdownIndex--;

                    entries[index] = entries[countdownIndex];
                    ids[index] = ids[countdownIndex];
                    entries[countdownIndex] = null;
                }
            }
        }
    }
}
