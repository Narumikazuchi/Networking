using Narumikazuchi.Collections.Abstract;
using System;
using System.Net.Sockets;

namespace Narumikazuchi.Networking.Sockets
{
    /// <summary>
    /// Contains the <see cref="Socket"/> instances for all to the <see cref="Server{T}"/> connected clients.
    /// </summary>
    public sealed class ClientCollection : ReadOnlyCollectionBase<Socket>
    {
        #region Constructor

        internal ClientCollection() : base() { }

        #endregion

        #region Collection Management

        internal void Add(Socket item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (this.Contains(item))
            {
                return;
            }
            lock (this._syncRoot)
            {
                this.EnsureCapacity(this.Count + 1);
                this._items[this._size++] = item;
            }
        }

        internal Boolean Remove(Socket item)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Int32 index = Array.IndexOf(this._items, item);
            if (index > -1)
            {
                this.RemoveAt(index);
                return true;
            }
            return false;
        }

        internal void RemoveAt(in Int32 index)
        {
            lock (this._syncRoot)
            {
                if (index >= 0 &&
                    index < this.Count)
                {
                    Array.Copy(this._items, index + 1, this._items, index, this.Count - index);
                    this._items[this.Count] = default;
                    this._size--;
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }

        internal void Clear()
        {
            lock (this._syncRoot)
            {
                Array.Clear(this._items, 0, this.Count);
                this._size = 0;
            }
        }

        #endregion
    }
}
