using NUnit.Framework;
using OTS_Supermarket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OTS_Supermarket.Test
{
    [TestFixture]
    public class CartTest
    {
        [Test]
        public void AddOneToCart_ShouldAddItemToCart_Success()
        {
            //ARRANGE

            Cart cart = new Cart();
            Monitor monitor = new Monitor();

            //ACT

            cart.AddOneToCart(monitor);

            //ASSERT

            Assert.That(cart.Size, Is.EqualTo(1));
            Assert.That(cart.Amount, Is.EqualTo(100));
        }

        [Test]
        public void AddMultipleToCart_AddAndUpdateCounter_Success()
        {
            Cart cart = new Cart();

            Monitor monitor = new Monitor();
            cart.AddMultipleToCart(monitor, 5);

            Assert.That(cart.Size, Is.EqualTo(5));
            Assert.That(cart.Amount, Is.EqualTo(500));
            Assert.That(cart.Monitor_counter, Is.EqualTo(5));
        }

        [Test]
        public void AddOneToCart_ThrowsExceptionWhenOverMaxSize_Success()
        {
            Cart cart = new Cart();
            cart.AddMultipleToCart(new Monitor(), 10);

            var ex = Assert.Throws<Exception>(() => cart.AddOneToCart(new Monitor()));
            Assert.That(ex.Message, Is.EqualTo("Number of items in cart must be 10 or less!"));
        }

        [Test]
        public void Print_ThrowsWhenEmpty_Success()
        {
            Cart cart = new Cart();

            var ex = Assert.Throws<Exception>(() => cart.Print());
            Assert.That(ex.Message, Is.EqualTo("Cannot print empty cart!"));
        }

        [Test]
        public void Calculate_AppliesDiscountForOverNineItemsAndLaptop_Success()
        {
            Cart cart = new Cart();
            cart.Budget = 2000;

            cart.AddOneToCart(new Laptop());
            cart.AddMultipleToCart(new Monitor(), 8);

            DateTime today = DateTime.Today;
            int offset = 1;
            while (offset <= 3 && (today.AddDays(offset).DayOfWeek == DayOfWeek.Saturday || today.AddDays(offset).DayOfWeek == DayOfWeek.Sunday))
            {
                offset++;
            }

            if (offset > 3)
            {
                Assert.Inconclusive("No weekday within the next 3 days to reliably run this test.");
            }

            string dateString = today.AddDays(offset).ToString("yyyy-MM-dd");

            cart.Calculate(dateString);

            double expectedAmount = 1600;
            double expectedPrice = expectedAmount - (expectedAmount * 0.08);

            Assert.That(cart.Budget, Is.EqualTo(2000 - expectedPrice));
        }

        [Test]
        public void DeleteAll_ClearsCart_WhenNotEmpty()
        {
            Cart cart = new Cart();
            cart.AddMultipleToCart(new Monitor(), 2);
            cart.AddOneToCart(new Keyboard());

            cart.DeleteAll();

            Assert.That(cart.Size, Is.EqualTo(0));
            Assert.That(cart.Items.Count, Is.EqualTo(0));
            Assert.That(cart.Monitor_counter, Is.EqualTo(0));
            Assert.That(cart.Keyboard_counter, Is.EqualTo(0));
        }

        [Test]
        public void DeleteAll_ThrowsWhenEmpty()
        {
            Cart cart = new Cart();

            var ex = Assert.Throws<Exception>(() => cart.DeleteAll());
            Assert.That(ex.Message, Is.EqualTo("Cannot restore empty cart!"));
        }

        [Test]
        public void Print_ReturnsExpectedString_ForNonEmptyCart()
        {
            Cart cart = new Cart();
            cart.AddOneToCart(new Computer());

            string output = cart.Print();

            Assert.That(output, Does.Contain("Item: Computer"));
            Assert.That(output, Does.Contain("Price: 1200"));
        }

        [Test]
        public void Calculate_ThrowsOnInvalidDateFormat()
        {
            Cart cart = new Cart();

            var ex = Assert.Throws<Exception>(() => cart.Calculate("01-01-2025"));
            Assert.That(ex.Message, Is.EqualTo("Wrong date format! Date must be in format yyyy-MM-dd"));
        }

        [Test]
        public void Calculate_ThrowsWhenDateIsToday()
        {
            Cart cart = new Cart();
            string today = DateTime.Today.ToString("yyyy-MM-dd");

            var ex = Assert.Throws<Exception>(() => cart.Calculate(today));
            Assert.That(ex.Message, Is.EqualTo("Date of delivery can't be today's date!"));
        }

        [Test]
        public void Calculate_ThrowsWhenDaysGreaterThanSeven()
        {
            Cart cart = new Cart();
            string date = DateTime.Today.AddDays(8).ToString("yyyy-MM-dd");

            var ex = Assert.Throws<Exception>(() => cart.Calculate(date));
            Assert.That(ex.Message, Is.EqualTo("Days for delivery must be less than 7!"));
        }

        [Test]
        public void Calculate_AppliesTenPercentDiscount_Branch()
        {
            Cart cart = new Cart();
            cart.Budget = 10000;

            
            cart.AddMultipleToCart(new Computer(), 3);
            cart.AddMultipleToCart(new Monitor(), 6);

            DateTime today = DateTime.Today;
            int offset = FindNextWeekdayOffset(1, 3);
            if (offset == -1) Assert.Inconclusive("No weekday within next 3 days");

            string dateString = today.AddDays(offset).ToString("yyyy-MM-dd");
            cart.Calculate(dateString);

            double expectedAmount = (3 * 1200) + (6 * 100);
            double expectedPrice = expectedAmount - (expectedAmount * 0.10);

            Assert.That(cart.Budget, Is.EqualTo(10000 - expectedPrice));
        }

        [Test]
        public void Calculate_AppliesTwentyPercentDiscount_ForLaterWindow()
        {
            Cart cart = new Cart();
            cart.Budget = 20000;

            
            cart.AddMultipleToCart(new Laptop(), 3);
            cart.AddMultipleToCart(new Monitor(), 6); 

            DateTime today = DateTime.Today;
            int offset = FindNextWeekdayOffset(4, 7);
            if (offset == -1) Assert.Inconclusive("No weekday within days 4..7");

            string dateString = today.AddDays(offset).ToString("yyyy-MM-dd");
            cart.Calculate(dateString);

            double expectedAmount = (3 * 800) + (6 * 100);
            double expectedPrice = expectedAmount - (expectedAmount * 0.20);

            Assert.That(cart.Budget, Is.EqualTo(20000 - expectedPrice));
        }

        [Test]
        public void Calculate_ThrowsWhenNotEnoughBudget()
        {
            Cart cart = new Cart();
            cart.Budget = 50;
            cart.AddOneToCart(new Monitor());

            DateTime today = DateTime.Today;
            int offset = FindNextWeekdayOffset(1, 3);
            if (offset == -1) Assert.Inconclusive("No weekday within next 3 days");

            string dateString = today.AddDays(offset).ToString("yyyy-MM-dd");

            var ex = Assert.Throws<Exception>(() => cart.Calculate(dateString));
            Assert.That(ex.Message, Is.EqualTo("Not enough budget!"));
        }

        private int FindNextWeekdayOffset(int minDaysInclusive, int maxDaysInclusive)
        {
            DateTime today = DateTime.Today;
            for (int d = minDaysInclusive; d <= maxDaysInclusive; d++)
            {
                var dow = today.AddDays(d).DayOfWeek;
                if (dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday) return d;
            }
            return -1;
        }


    }
}
