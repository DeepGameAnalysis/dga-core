using MathNet.Spatial.Euclidean;

namespace RTree
{
    public class RTreeNode<T>
    {
        internal int nodeID = 0;
        internal Rectangle2D mbr = null;
        internal Rectangle2D[] entries = null;
        internal int[] ids = null;
        internal int level;
        internal int entryCount;

        public RTreeNode(int nodeId, int level, int maxNodeEntries)
        {
            this.nodeID = nodeId;
            this.level = level;
            entries = new Rectangle2D[maxNodeEntries];
            ids = new int[maxNodeEntries];
        }

        internal void AddEntry(Rectangle2D r, int id)
        {
            ids[entryCount] = id;
            entries[entryCount] = r.Copy();
            entryCount++;
            if (mbr == null)
                mbr = r.Copy();
            else
                mbr.Union(r);
        }

        internal void AddEntryNoCopy(Rectangle2D r, int id)
        {
            ids[entryCount] = id;
            entries[entryCount] = r;
            entryCount++;
            if (mbr == null)
                mbr = r.Copy();
            else
                mbr.Union(r);
        }

        // Return the index of the found entry, or -1 if not found
        internal int FindEntry(Rectangle2D r, int id)
        {
            for (int i = 0; i < entryCount; i++)
                if (id == ids[i] && r.Equals(entries[i]))
                    return i;

            return -1;
        }

        // delete entry. This is done by setting it to null and copying the last entry into its space.
        internal void DeleteEntry(int i, int minNodeEntries)
        {
            int lastIndex = entryCount - 1;
            Rectangle2D deletedRectangle = entries[i];
            entries[i] = null;
            if (i != lastIndex)
            {
                entries[i] = entries[lastIndex];
                ids[i] = ids[lastIndex];
                entries[lastIndex] = null;
            }
            entryCount--;

            // if there are at least minNodeEntries, adjust the MBR.
            // otherwise, don't bother, as the Node<T> will be 
            // eliminated anyway.
            if (entryCount >= minNodeEntries)
                RecalculateMBR(deletedRectangle);

        }

        // oldRectangle is a rectangle that has just been deleted or made smaller.
        // Thus, the MBR is only recalculated if the OldRectangle influenced the old MBR
        internal void RecalculateMBR(Rectangle2D deletedRectangle)
        {
            if (mbr.edgeOverlaps(deletedRectangle))
            {
                //mbr.set(entries[0].min, entries[0].max);
                for (int i = 1; i < entryCount; i++)
                    mbr.Union(entries[i]);

            }
        }

        public int GetEntryCount()
        {
            return entryCount;
        }

        public Rectangle2D GetEntry(int index)
        {
            if (index < entryCount)
                return entries[index];

            return null;
        }

        public int GetID(int index)
        {
            if (index < entryCount)
                return ids[index];

            return -1;
        }

        internal bool isLeaf()
        {
            return (level == 1);
        }

        public int getLevel()
        {
            return level;
        }

        public Rectangle2D getMBR()
        {
            return mbr;
        }
    }

}