using System;

public class Heap<T> where T: IHeapItem<T>
{ 
        private T[] _items;
        private int _currentItemCount;

        public Heap(int maxHeapSize)
        {
                _items = new T[maxHeapSize];
        }

        public void Add(T item)
        {
                //Item 
                item.HeapIndex = _currentItemCount;
                _items[_currentItemCount] = item;
                SortUp(item);
                _currentItemCount++;
        }

        public T RemoveFirst()
        {
                T firstItem = _items[0];
                _currentItemCount--;
                _items[0] = _items[_currentItemCount];
                _items[0].HeapIndex = 0;
                
                SortDown(_items[0]);
                return firstItem;
        }

        public bool Contains(T item)
        {
                return Equals(_items[item.HeapIndex], item);
        }

        public void UpdateItem(T item)
        {
                SortUp(item);
        }

        public int Count
        {
                get
                {
                        return _currentItemCount;
                }
        }

        void SortDown(T item)
        {
                while (true)
                {
                        //child left 2n + 1 right 2n +2
                        int leftchildIndex = item.HeapIndex * 2 + 1;
                        int rightchildIndex = item.HeapIndex * 2 + 2;
                        int swapIndex = 0;

                        //如果当前索引小于 
                        if (leftchildIndex < _currentItemCount)
                        {
                                swapIndex = leftchildIndex;
                                if (rightchildIndex < _currentItemCount)
                                {
                                        //>0 优先度高  < 优先度低
                                        if (_items[leftchildIndex].CompareTo(_items[rightchildIndex]) < 0)
                                        {
                                                swapIndex = rightchildIndex;
                                        }
                                }

                                if (item.CompareTo(_items[swapIndex]) < 0)
                                {
                                        Swap(item,_items[swapIndex]);
                                }
                                else
                                {
                                        return;
                                }
                        }
                        else //没有Child
                        {
                                return;
                        }
                        
                }
        }

        void SortUp(T item)
        {
                // Parent (n-1)/2 
                int parentIndex = (item.HeapIndex - 1) / 2;
                while (true)
                {
                        T parentItem = _items[parentIndex];
                        //
                        if (item.CompareTo(parentItem) > 0)
                        {
                                Swap(item,_items[parentIndex]);
                        }
                        else
                        {
                                break;
                        }
                        
                        parentIndex = (item.HeapIndex - 1) / 2;
                }
        }

        void Swap(T itemA, T itemB)
        {
                _items[itemA.HeapIndex] = itemB;
                _items[itemB.HeapIndex] = itemA;
                (itemA.HeapIndex, itemB.HeapIndex) = (itemB.HeapIndex, itemA.HeapIndex);
        }
}

public interface IHeapItem<T> : IComparable<T>
{
        int HeapIndex { get; set; }
}
//https://www.cs.usfca.edu/~galles/JavascriptVisual/Heap.html