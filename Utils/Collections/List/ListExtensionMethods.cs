using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoBlock
{
    namespace Utils.Collections.List
    {
        public static class ListExtensionMethods
        {
            // exclusive
            public static void AddRange<T>(
                this List<T> destinationList,
                IList<T> sourceList,
                int beginIndex,
                int endIndex)
            {
                ListUtils.AssertRangeIndexWithinRange<T>(sourceList, beginIndex, "beginIndex");
                ListUtils.AssertRangeIndexWithinRange<T>(sourceList, beginIndex, "endIndex");

                int numberOfItemsAddedToList = (endIndex - beginIndex);
                int numberOfItemsInListAfterAdd = destinationList.Count + numberOfItemsAddedToList;

                // number of items in list after add is greater than current list capacity,
                // and doubling list capacity fails to allocate enough space for added items
                if (destinationList.Capacity * 2 < numberOfItemsInListAfterAdd)
                {
                    // manually allocate additional memory to list 
                    destinationList.Capacity = numberOfItemsInListAfterAdd;
                }

                // add items from sourceList in range [beginIndex - endIndex) to destinationList
                for(int i = beginIndex; i < endIndex; i++)
                {
                    T sourceListItem = sourceList[i];
                    destinationList.Add(sourceListItem);
                }
            }
        }
    }

}
