using Cousework.DataStructures;
using Cousework.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject1
{
    [TestClass]
    public sealed class HashTableTests
    {
        private HashTable<Owner> _hashTable;

        [TestInitialize]
        public void Setup()
        {
            _hashTable = new HashTable<Owner>();
        }

        [TestMethod]
        public void Insert_ShouldInsertItemCorrectly()
        {
            var owner = new Owner { OwnerId = 1, Name = "John Doe", Email = "john@example.com", Phone = "12345" };
            _hashTable.Insert(owner);
            var result = _hashTable.GetByKey(owner);
            Assert.AreEqual(owner, result);
        }

        [TestMethod]
        public void GetByKey_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            var owner = new Owner { OwnerId = 99 };
            var result = _hashTable.GetByKey(owner);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Delete_ShouldRemoveItem()
        {
            var owner = new Owner { OwnerId = 2, Name = "Alice", Email = "alice@example.com", Phone = "67890" };
            _hashTable.Insert(owner);
            _hashTable.Remove(owner);
            var result = _hashTable.GetByKey(owner);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Insert_DuplicateOwner_ShouldOverwrite()
        {
            var owner1 = new Owner { OwnerId = 3, Name = "Bob", Email = "bob@example.com", Phone = "11111" };
            var owner2 = new Owner { OwnerId = 3, Name = "Bob Updated", Email = "bob@updated.com", Phone = "22222" };

            _hashTable.Insert(owner1);
            _hashTable.Insert(owner2);

            var result = _hashTable.GetByKey(owner1);
            Assert.AreEqual(owner2.Email, result.Email);
            Assert.AreEqual(owner2.Phone, result.Phone);
        }

        [TestMethod]
        public void Insert_MultipleItems_ShouldAllBeRetrievable()
        {
            var ownerA = new Owner { OwnerId = 10, Name = "A" };
            var ownerB = new Owner { OwnerId = 20, Name = "B" };
            var ownerC = new Owner { OwnerId = 30, Name = "C" };

            _hashTable.Insert(ownerA);
            _hashTable.Insert(ownerB);
            _hashTable.Insert(ownerC);

            Assert.AreEqual(ownerA, _hashTable.GetByKey(ownerA));
            Assert.AreEqual(ownerB, _hashTable.GetByKey(ownerB));
            Assert.AreEqual(ownerC, _hashTable.GetByKey(ownerC));
        }
    }
}
