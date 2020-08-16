using SRPProblem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SRPProblem
{
    class Program
    {
        static void Main(string[] args)
        {
            OrderItem objItem1 = new OrderItem{Identifier="Toy",Quantity=2};
            OrderItem objItem2 = new OrderItem{Identifier="Utensils",Quantity=1};
            List<OrderItem> items= new List<OrderItem>();
            items.Add(objItem1);
            items.Add(objItem2);
            ShoppingCart objCart = new ShoppingCart();
            objCart.CustomerEmail = "p.bhat@xyzcompany.com";
            objCart.Items = items;
            PaymentDetails objDetails = new PaymentDetails();
            objDetails.PaymentMethod= PaymentMethod.CreditCard;
            Order objOrder = new Order();
            objOrder.Checkout(objCart,objDetails,true);
        }
    }

    public class OrderItem
    {
        public string Identifier { get; set; }
        public int Quantity { get; set; }
    }
    public class ShoppingCart
    {
        public decimal TotalAmount { get; set; }
        public IEnumerable<OrderItem> Items { get; set; }
        public string CustomerEmail { get; set; }
    }
    public enum PaymentMethod
    {
        CreditCard, 
        Cheque
    }
    public class PaymentDetails
    {
        public PaymentMethod PaymentMethod { get; set; }
        public string CreditCardNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string CardholderName { get; set; }
    }

    //Responsibility of this class:  Charging the card > Reserving Inventory > Sending mails > Logging Exceptions
    public class Order
    {
        public void Checkout(ShoppingCart shoppingCart, PaymentDetails paymentDetails, bool notifyCustomer)
        {
            if (paymentDetails.PaymentMethod == PaymentMethod.CreditCard)
            {
                //Placing a due amount on Credit Card so that user can pay the money at the end
                ChargeCard(paymentDetails, shoppingCart);
            }
            //Reserving the estimated cost for Inventory (Inventory is a comlete list of Items)
            ReserveInventory(shoppingCart);
            if (notifyCustomer)
            {
                NotifyCustomer(shoppingCart);
            }
        }

        public void ChargeCard(PaymentDetails paymentDetails, ShoppingCart cart)
        {
            PaymentService paymentService = new PaymentService();
            try
            {
                paymentService.Credentials = "Credentials";
                paymentService.CardNumber = paymentDetails.CreditCardNumber;
                paymentService.ExpiryDate = paymentDetails.ExpiryDate;
                paymentService.NameOnCard = paymentDetails.CardholderName;
                paymentService.AmountToCharge = cart.TotalAmount;
                paymentService.Charge();
            }
            catch (AccountBalanceMismatchException ex)
            {
                throw new OrderException("The card gateway rejected the card based on the address provided.", ex);
            }
            catch (Exception ex)
            {
                throw new OrderException("There was a problem with your card.", ex);
            }

        }

        public void ReserveInventory(ShoppingCart cart)
        {
            foreach (OrderItem item in cart.Items)
            {
                try
                {
                    InventoryService inventoryService = new InventoryService();
                    inventoryService.Reserve(item.Identifier, item.Quantity);
                }
                catch (InsufficientInventoryException ex)
                {
                    throw new OrderException("Insufficient inventory for item " + item.Identifier, ex);
                }
                catch (Exception ex)
                {
                    throw new OrderException("Problem reserving inventory", ex);
                }
            }
        }

        public void NotifyCustomer(ShoppingCart cart)
        {
            string customerEmail = cart.CustomerEmail;
            if (!String.IsNullOrEmpty(customerEmail))
            {
                try
                {
                    //construct the email message and send it, implementation ignored
                }
                catch (Exception ex)
                {
                    //log the emailing error, implementation ignored
                }
            }
        }
    }

    public class OrderException : Exception
    {
        public OrderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}



/*
 The Single Responsibility Principle states that every object should only have one reason to change and a single focus of responsibility. 
 In other words every object should perform one thing only. You can apply this idea at different levels of your software: 
 a method should only carry out one action; a domain object should only represent one domain within your business; 
 the presentation layer should only be responsible for presenting your data; etc. This principle aims to achieve the following goals:
Short and concise objects: avoid the problem of a monolithic class design that is the software equivalent of a Swiss army knife
Testability: if a method carries out multiple tasks then it’s difficult to write a test for it
Readability: reading short and concise code is certainly easier than finding your way through some spaghetti code
Easier maintenance
 
 A responsibility of a class usually represents a feature or a domain in your application. If you assign many responsibilities to a class or 
  bloat your domain object then there’s a greater chance that you’ll need to change that class later. These responsibilities will be coupled 
  together in the class making each individual responsibility more difficult to change without introducing errors in another. We can also call 
  a responsibility a “reason to change”.
SRP is strongly related to what is called Separation of Concerns (SoC). SoC means dissecting a piece of software into distinct features that 
  encapsulate unique behaviour and data that can be used by other classes. Here the term ‘concern’ represents a feature or behaviour of a class. 
  Separating a programme into small and discrete ‘ingredients’ significantly increases code reuse, maintenance and testability.
Other related terms include the following:
Cohesion: how strongly related and focused the various responsibilities of a module are
Coupling: the degree to which each programme module relies on each one of the other modules
 
 In a good software design we are striving for a high level of cohesion and a low level of coupling. A high level of coupling, also called tight coupling, 
  usually means a lot of concrete dependency among the various elements of your software. This leads to a situation where changing the design of 
  one class leads to the need of changing other classes that are dependent on the class you’ve just changed. Also, with tight coupling changing 
  the design of one class can introduce errors in the dependent classes.
One last related technique is Test Driven Design or Test Driven Development (TDD). If you apply the test first approach of TDD and write your tests 
 carefully then it will help you fulfil SRP, or at least it is a good way to ensure that you’re not too far from SRP.
 
 */


/*
 Problem :
 * 
 I think first and foremost the greatest flaw is a conceptual one actually. What has the Order domain object got to do with sending emails? 
 What does it have to do with checking the inventory, logging exceptions or charging the credit card? These are all concepts that simply do not belong 
 in an Order domain.
Imagine that the Order object can be used by different platforms: an e-commerce website with credit card payments or a physical shop where you pick 
 your own goods from the shelf and pay by cash. Which leads to several other issues as well:
Cheque payments don’t need card processing: cards are only charged in the Checkout method if the customer is paying by card – in any other case we 
should not involve the idea of card processing at all
Inventory reservations should be carried out by a separate service in case we’re buying in a physical shop
The customer will probably only need an email notification if they use the web platform of the business – otherwise the customer won’t even 
provide an email address. After all, why would you want to be notified by email if you buy the goods in person in a shop?
The problem here is that no matter what platform consumes the Order object it will need to know about the concepts of inventory management, 
credit card processing and emails. So any change in these concepts will affect not only the Order object but all others that depend on it.
 
 
 
 */