using Cousework.DataStructures; // Adjust based on your actual namespace
using Cousework.Models; // Adjust if needed
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
            // Arrange
            var owner = new Owner { OwnerId = 1, Name = "John Doe", Email = "john@example.com", Phone = "12345" };

            // Act
            _hashTable.Insert(owner);

            // Assert
            var result = _hashTable.GetByKey(owner);  // Assuming GetByKey retrieves an owner by their key (OwnerId)
            Assert.AreEqual(owner, result, "The inserted owner was not found.");
        }
    }
}
