using System;
using System.Collections.Generic;
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
            return Math.Abs(key.GetHashCode()) % _capacity;
        }

        public void Insert(T key)
        {
            int index = GetHash(key);
            Console.WriteLine($"Inserting key: {key} at index {index}");

            // Handle collisions with linear probing
            while (_buckets[index] != null)
            {
                if (_buckets[index].Equals(key))
                {
                    // Update the value if it already exists
                    _buckets[index] = key;
                    Console.WriteLine($"Updated {key} at index {index}");
                    return;
                }
                index = (index + 1) % _capacity;
            }

            _buckets[index] = key;
            _size++;
            Console.WriteLine($"Inserted {key} at index {index}");

            if (ShouldResize()) Resize();
        }

        public void Remove(T key)
        {
            int index = GetHash(key);
            while (_buckets[index] != null)
            {
                if (_buckets[index].Equals(key))
                {
                    _buckets[index] = default(T); // Set the bucket to default
                    _size--;
                    Console.WriteLine($"Removed {key} from index {index}");
                    return;
                }
                index = (index + 1) % _capacity;
            }
            Console.WriteLine($"Key {key} not found.");
        }

        public T GetByKey(T key)
        {
            int index = GetHash(key);
            while (_buckets[index] != null)
            {
                if (_buckets[index].Equals(key))
                {
                    return _buckets[index];
                }
                index = (index + 1) % _capacity;
            }
            return default(T);
        }

        public bool Contains(T key)
        {
            return !Equals(GetByKey(key), default(T));
        }

        public void DisplayContents()
        {
            for (int i = 0; i < _capacity; i++)
            {
                if (_buckets[i] != null)
                {
                    Console.WriteLine($"Index {i}: {_buckets[i]}");
                }
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

        private bool ShouldResize()
        {
            return (float)_size / _capacity > _loadFactor;
        }

        private void Resize()
        {
            Console.WriteLine("Resizing HashTable...");
            int newCapacity = _capacity * 2;
            T[] oldBuckets = _buckets;

            _capacity = newCapacity;
            _buckets = new T[_capacity];
            _size = 0;

            foreach (var item in oldBuckets)
            {
                if (item != null)
                {
                    Insert(item);
                }
            }
        }

        public int Count()
        {
            return _size;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _capacity)
                    throw new IndexOutOfRangeException("Index out of range");
                return _buckets[index];
            }
            set
            {
                if (index < 0 || index >= _capacity)
                    throw new IndexOutOfRangeException("Index out of range");
                _buckets[index] = value;
            }
        }


        // Add the Update method
        public bool Update(T updatedItem)
        {
            int index = GetHash(updatedItem);

            // Check if the item exists by probing
            while (_buckets[index] != null)
            {
                if (_buckets[index].Equals(updatedItem))
                {
                    _buckets[index] = updatedItem; // Update the item
                    Console.WriteLine($"Updated {updatedItem} at index {index}");
                    return true;
                }
                index = (index + 1) % _capacity;
            }

            // Return false if the item wasn't found to update
            Console.WriteLine($"Item to update not found.");
            return false;
        }

        public bool DeleteByKey(int key)
        {
            for (int i = 0; i < _capacity; i++)
            {
                if (_buckets[i] != null)
                {
                    var item = _buckets[i];
                    var idProperty = item.GetType().GetProperty("OwnerId") ?? item.GetType().GetProperty("PetId");

                    if (idProperty != null && (int)idProperty.GetValue(item) == key)
                    {
                        _buckets[i] = default(T);
                        _size--;
                        Console.WriteLine($"Deleted item with key {key} from index {i}");
                        return true;
                    }
                }
            }

            Console.WriteLine($"No item with key {key} found.");
            return false;
        }

        public bool DeletePetByPetId(int petId)
        {
            for (int i = 0; i < _capacity; i++)
            {
                if (_buckets[i] != null)
                {
                    var pet = _buckets[i];

                    // Check if the pet has a PetId property
                    var petIdProperty = pet.GetType().GetProperty("PetId");

                    if (petIdProperty != null && (int)petIdProperty.GetValue(pet) == petId)
                    {
                        // Delete the pet by its PetId
                        _buckets[i] = default(T); // Set the bucket to default (null)
                        _size--;
                        Console.WriteLine($"Deleted pet with PetId {petId} at index {i}");
                        return true;
                    }
                }
            }

            Console.WriteLine($"No pet found with PetId {petId}.");
            return false;
        }

        public T SearchByKey(int key)
        {
            for (int i = 0; i < _capacity; i++)
            {
                if (_buckets[i] != null)
                {
                    var item = _buckets[i];
                    var idProperty = item.GetType().GetProperty("AppointmentId")
                                     ?? item.GetType().GetProperty("PetId")
                                     ?? item.GetType().GetProperty("OwnerId");

                    if (idProperty != null && (int)idProperty.GetValue(item) == key)
                    {
                        return item;
                    }
                }
            }

            return default;
        }

    }
}
