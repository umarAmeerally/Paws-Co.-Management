using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cousework.Models;

namespace Cousework.DataStructures
{
    public class HashTable<T>
    {
        private int _capacity;
        private int _size;
        private float _loadFactor;
        private T[] _buckets;

        public HashTable(int capacity = 11, float loadFactor = 0.75f)
        {
            _capacity = capacity;
            _loadFactor = loadFactor;
            _buckets = new T[_capacity];
            _size = 0;
        }

        private int GetHash(T key)
        {
            return Math.Abs(key.GetHashCode()) % _capacity; // This gives us an index based on the hash code of the key.
        }

        public void Insert(T key)
        {
            int index = GetHash(key); // Get the index using the hash function
            Console.WriteLine($"Inserting key: {key} at index {index}");  // Debug line

            // Handle collisions using linear probing
            while (_buckets[index] != null && !_buckets[index].Equals(key))
            {
                index = (index + 1) % _capacity; // Find the next index if the spot is taken
            }

            // Insert the element if there's an empty spot
            if (_buckets[index] == null)
            {
                _buckets[index] = key; // Insert the key into the table
                _size++; // Increase the size of the table
                Console.WriteLine($"Inserted {key} at index {index}"); // Debug line
            }

            // Check if resizing is needed
            if (ShouldResize())
            {
                Resize();
            }
        }

        public T Get(int index)
        {
            return _buckets[index];
        }

        public bool Contains(T key)
        {
            int index = GetHash(key);

            while (_buckets[index] != null)
            {
                if (_buckets[index].Equals(key))
                {
                    return true;
                }

                index = (index + 1) % _capacity;
            }

            return false;
        }

        public void DisplayContents()
        {
            for (int i = 0; i < _capacity; i++)
            {
                if (_buckets[i] != null)
                {
                    // Assuming T is of type Owner, you can customize this based on your generic type T
                    var owner = (Owner)(object)_buckets[i]; // Cast to Owner
                    Console.WriteLine($"Index {i}: OwnerId: {owner.OwnerId}, Name: {owner.Name}, Email: {owner.Email}, Phone: {owner.Phone}, Address: {owner.Address}");
                }
            }
        }

        private void Resize()
        {
            // Resize logic here (doubling the size, rehashing existing keys, etc.)
        }

        // Check if resizing is needed based on load factor
        private bool ShouldResize()
        {
            return (float)_size / _capacity > _loadFactor;
        }

        // Add the Count property to return the number of elements in the table
        public int Count()
        {
            return _size;
        }

        // Allow access to the table like an array (if you want to get the elements by index)
        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _capacity)
                {
                    throw new IndexOutOfRangeException("Index out of range");
                }

                return _buckets[index];
            }
        }

        
        public IEnumerable<T> GetAllElements()
        {
            for (int i = 0; i < _buckets.Length; i++)
            {
                if (_buckets[i] != null)
                {
                    yield return _buckets[i];
                }
            }
        }

    }

}
