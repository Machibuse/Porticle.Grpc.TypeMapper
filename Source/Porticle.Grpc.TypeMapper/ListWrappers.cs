namespace Porticle.Grpc.TypeMapper;

public static class ListWrappers
{
    public static string IListWithRangeAdd = """
                                             public interface IListWithRangeAdd<T> : System.Collections.Generic.IList<T>
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
                                                        
                                                        public bool IsReadOnly => _internList.IsReadOnly;
                                                        
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
                                                    }
                                                    """;

    public static string RepeatedFieldDecimalWrapper = """
                                                       class RepeatedFieldDecimalWrapper : IListWithRangeAdd<decimal>
                                                       {
                                                           private readonly Google.Protobuf.Collections.RepeatedField<string> _internList;

                                                           public RepeatedFieldDecimalWrapper(Google.Protobuf.Collections.RepeatedField<string> internList)
                                                           {
                                                               this._internList = internList;
                                                           }

                                                           public System.Collections.Generic.IEnumerator<decimal> GetEnumerator()
                                                           {
                                                               using var enumerator = _internList.GetEnumerator();
                                                               while (enumerator.MoveNext())
                                                               {
                                                                   yield return decimal.Parse(enumerator.Current, System.Globalization.CultureInfo.InvariantCulture);
                                                               }
                                                           }

                                                           System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                                                           {
                                                               return GetEnumerator();
                                                           }

                                                           public void Add(decimal item)
                                                           {
                                                               _internList.Add(item.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                                           }

                                                           public void Add(IEnumerable<decimal> items)
                                                           {
                                                               _internList.AddRange(items.Select(i => i.ToString(System.Globalization.CultureInfo.InvariantCulture)));
                                                           }

                                                           public void Clear()
                                                           {
                                                               _internList.Clear();
                                                           }

                                                           public bool Contains(decimal item)
                                                           {
                                                               return _internList.Contains(item.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                                           }

                                                           public void CopyTo(decimal[] array, int arrayIndex)
                                                           {
                                                               System.Linq.Enumerable.ToArray(System.Linq.Enumerable.Select(_internList, s => decimal.Parse(s, System.Globalization.CultureInfo.InvariantCulture))).CopyTo(array, arrayIndex);
                                                           }

                                                           public bool Remove(decimal item)
                                                           {
                                                               return _internList.Remove(item.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                                           }

                                                           public int Count => _internList.Count;

                                                           public bool IsReadOnly => _internList.IsReadOnly;

                                                           public int IndexOf(decimal item)
                                                           {
                                                               return _internList.IndexOf(item.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                                           }

                                                           public void Insert(int index, decimal item)
                                                           {
                                                               _internList.Insert(index, item.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                                           }

                                                           public void RemoveAt(int index)
                                                           {
                                                               _internList.RemoveAt(index);
                                                           }

                                                           public decimal this[int index]
                                                           {
                                                               get => decimal.Parse(_internList[index], System.Globalization.CultureInfo.InvariantCulture);
                                                               set => _internList[index] = value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                                                           }
                                                       }
                                                       """;
}