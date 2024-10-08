using NUnit.Framework;
using System.Collections.Generic;

namespace SwinAdventure
{
    [TestFixture]
    public class MetamorphicTests
    {
        [Test]
        public void CommandCommutativityRelation()
        {
            Player player1 = new Player("Player1", "Test player 1");
            Player player2 = new Player("Player2", "Test player 2");
            Item item1 = new Item(new string[] { "item1" }, "Item 1", "Test item 1");
            Item item2 = new Item(new string[] { "item2" }, "Item 2", "Test item 2");
            Item item3 = new Item(new string[] { "item3" }, "Item 3", "Test item 3");
            Bag bag = new Bag(new string[] { "bag" }, "Test Bag", "A test bag");
            CommandProcessor processor = new CommandProcessor();

            // Setup: Add items to players' inventories
            player1.Inventory.Put(item1);
            player1.Inventory.Put(item2);
            player1.Inventory.Put(item3);
            player1.Inventory.Put(bag);

            player2.Inventory.Put(item2);
            player2.Inventory.Put(item3);
            player2.Inventory.Put(item1);
            player2.Inventory.Put(bag);

            // Sequence 1
            processor.Execute(player1, new string[] { "look", "at", "item1" });
            processor.Execute(player1, new string[] { "put", "item2", "in", "bag" });
            processor.Execute(player1, new string[] { "look", "at", "bag" });
            processor.Execute(player1, new string[] { "take", "item2", "from", "bag" });
            processor.Execute(player1, new string[] { "drop", "item3" });

            // Sequence 2 (different order)
            processor.Execute(player2, new string[] { "put", "item2", "in", "bag" });
            processor.Execute(player2, new string[] { "look", "at", "item1" });
            processor.Execute(player2, new string[] { "drop", "item3" });
            processor.Execute(player2, new string[] { "take", "item2", "from", "bag" });
            processor.Execute(player2, new string[] { "look", "at", "bag" });

            // Verify final game states are equivalent
            Assert.AreEqual(player1.Inventory.ItemList, player2.Inventory.ItemList, "Inventory sizes should be the same");

            foreach (string itemId in new[] { "item1", "item2", "bag" })
            {
                Assert.IsTrue(player1.Inventory.HasItem(itemId), $"Player1 should have {itemId}");
                Assert.IsTrue(player2.Inventory.HasItem(itemId), $"Player2 should have {itemId}");
            }

            Assert.IsFalse(player1.Inventory.HasItem("item3"), "Player1 should not have item3");
            Assert.IsFalse(player2.Inventory.HasItem("item3"), "Player2 should not have item3");

            // Verify bag contents
            Bag bag1 = (Bag)player1.Inventory.Fetch("bag");
            Bag bag2 = (Bag)player2.Inventory.Fetch("bag");
            Assert.AreEqual(bag1.Inventory.ItemList, bag2.Inventory.ItemList, "Bag contents should be the same");

            // Final look command should produce the same result
            string look1 = processor.Execute(player1, new string[] { "look" });
            string look2 = processor.Execute(player2, new string[] { "look" });
            Assert.AreEqual(look1, look2, "Final look command should produce the same result");
        }
    }
}