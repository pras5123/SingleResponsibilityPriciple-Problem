# SingleResponsibilityPriciple-Problem


To understand the need for Single responsibity classes, lets take a simple real time requirement.

# Requirement : 
Given the list of items on a website, we must be able to add items to our cart and then check out to proceed to a payment.


# Design : 
Without thinking much about the design principles, if we ever need to design the classes and functionality, it can be written as follows :
1. OrderItem class - Contains list of the items to be purchased
2. ShoppingCart class- Contains OrderItem information 
3. PaymentDetails class- Contains the information related to payment of a customer ( Mode of payment, credit card, debit card etc)
4. Order class - Responsible for  reserving the Inventory , charging the card and notify the customer over the email about the order.


# Problem 1

Imagine that the Order object can be used by different platforms: an e-commerce website with credit card payments or a physical shop where you pick 
your own goods from the shelf and pay by cash. Which leads to several other issues as well:Cheque payments don’t need card processing: cards are only charged in the Checkout method if the customer is paying by card – in any other case we  should not involve the idea of card processing at all

# Problem 2
Inventory reservations should be carried out by a separate service in case we’re buying in a physical shop.The customer will probably only need an email notification if they use the web platform of the business – otherwise the customer won’t even provide an email address

# Problem 3
After all, why would you want to be notified by email if you buy the goods in person in a shop?The problem here is that no matter what platform consumes the Order object it will need to know about the concepts of inventory management, credit card processing and emails. So any change in these concepts will affect not only the Order object but all others that depend on it.

Take a look at the code written under this project to understand this concept even better. 

In the solution concept, I will cover how to overcome the above problems.
