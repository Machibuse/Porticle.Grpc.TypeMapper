namespace Porticle.Grpc.TypeMapper;

public static class ListWrappers
{
    public static string IListWithRangeAdd = """
                                             public interface IListWithRangeAdd<T> : System.Collections.Generic.IList<T>, System.Collections.IList
                                             {
                                                 void Add(IEnumerable<T> items);
                                             }        
                                             """;

    public static string RepeatedFieldGuidWrapper = """
                                                    class RepeatedFieldGuidWrapper : IListWithRangeAdd<System.Guid>
                                                    {
                                                        private readonly Google.Protobuf.Collections.RepeatedField<string> _internList;

                                                        public RepeatedFieldGuidWrapper(Google.Protobuf.Collections.RepeatedField<string> internList)
                                                        {
                                                            this._internList = internList;
                                                        }

                                                        public System.Collections.Generic.IEnumerator<System.Guid> GetEnumerator()
                                                        {
                                                            using var enumerator = _internList.GetEnumerator();
                                                            while (enumerator.MoveNext())
                                                            {
                                                                yield return System.Guid.Parse(enumerator.Current);
                                                            }
                                                        }

                                                        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                                                        {
                                                            return GetEnumerator();
                                                        }

                                                        public void Add(System.Guid item)
                                                        {
                                                            _internList.Add(item.ToString("D"));
                                                        }

                                                        public void Add(IEnumerable<System.Guid> items)
                                                        {
                                                            _internList.AddRange(items.Select(i => i.ToString("D")));
                                                        }

                                                        public void Clear()
                                                        {
                                                            _internList.Clear();
                                                        }

                                                        public bool Contains(System.Guid item)
                                                        {
                                                            return _internList.Contains(item.ToString("D"));
                                                        }

                                                        public void CopyTo(System.Guid[] array, int arrayIndex)
                                                        {
                                                            System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(_internList, System.Guid.Parse)).CopyTo(array, arrayIndex);
                                                        }

                                                        public bool Remove(System.Guid item)
                                                        {
                                                            return _internList.Remove(item.ToString("D"));
                                                        }

                                                        public int Count => _internList.Count;

                                                        int System.Collections.ICollection.Count => _internList.Count;

                                                        public bool IsReadOnly => _internList.IsReadOnly;

                                                        bool System.Collections.IList.IsReadOnly => _internList.IsReadOnly;

                                                        public int IndexOf(System.Guid item)
                                                        {
                                                            return _internList.IndexOf(item.ToString("D"));
                                                        }

                                                        public void Insert(int index, System.Guid item)
                                                        {
                                                            _internList.Insert(index, item.ToString("D"));
                                                        }

                                                        public void RemoveAt(int index)
                                                        {
                                                            _internList.RemoveAt(index);
                                                        }

                                                        public System.Guid this[int index]
                                                        {
                                                            get =>  System.Guid.Parse(_internList[index]);
                                                            set => _internList[index] = value.ToString("D");
                                                        }

                                                        System.Guid System.Collections.Generic.IList<System.Guid>.this[int index]
                                                        {
                                                            get => System.Guid.Parse(_internList[index]);
                                                            set => _internList[index] = value.ToString("D");
                                                        }

                                                        // IList (non-generic) explicit implementation
                                                        int System.Collections.IList.Add(object value)
                                                        {
                                                            if (value is System.Guid guid)
                                                            {
                                                                Add(guid);
                                                                return Count - 1;
                                                            }
                                                            throw new System.ArgumentException("Value must be of type System.Guid", nameof(value));
                                                        }

                                                        bool System.Collections.IList.Contains(object value)
                                                        {
                                                            if (value is System.Guid guid)
                                                            {
                                                                return Contains(guid);
                                                            }
                                                            return false;
                                                        }

                                                        int System.Collections.IList.IndexOf(object value)
                                                        {
                                                            if (value is System.Guid guid)
                                                            {
                                                                return IndexOf(guid);
                                                            }
                                                            return -1;
                                                        }

                                                        void System.Collections.IList.Insert(int index, object value)
                                                        {
                                                            if (value is System.Guid guid)
                                                            {
                                                                Insert(index, guid);
                                                                return;
                                                            }
                                                            throw new System.ArgumentException("Value must be of type System.Guid", nameof(value));
                                                        }

                                                        void System.Collections.IList.Remove(object value)
                                                        {
                                                            if (value is System.Guid guid)
                                                            {
                                                                Remove(guid);
                                                            }
                                                        }

                                                        bool System.Collections.IList.IsFixedSize => false;

                                                        object System.Collections.IList.this[int index]
                                                        {
                                                            get => System.Guid.Parse(_internList[index]);
                                                            set
                                                            {
                                                                if (value is System.Guid guid)
                                                                {
                                                                    _internList[index] = guid.ToString("D");
                                                                    return;
                                                                }
                                                                throw new System.ArgumentException("Value must be of type System.Guid", nameof(value));
                                                            }
                                                        }

                                                        // ICollection (non-generic) explicit implementation
                                                        void System.Collections.ICollection.CopyTo(System.Array array, int index)
                                                        {
                                                            if (array is System.Guid[] guidArray)
                                                            {
                                                                CopyTo(guidArray, index);
                                                                return;
                                                            }
                                                            var guids = System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(_internList, System.Guid.Parse));
                                                            System.Array.Copy(guids, 0, array, index, guids.Length);
                                                        }

                                                        bool System.Collections.ICollection.IsSynchronized => false;

                                                        object System.Collections.ICollection.SyncRoot => ((System.Collections.ICollection)_internList).SyncRoot;
                                                    }
                                                    """;
}