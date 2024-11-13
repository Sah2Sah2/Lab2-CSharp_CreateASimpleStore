using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Simple.Store
{
    public class Customer
    {
        public string Name { get; private set; }
        private string Password { get; set; } // Keep password private
        //private Dictionary<string, int> _shoppingCart; // Shopping cart holding Product names and quantities
        public Dictionary<Product, int> _shoppingCart { get; set; }

        public decimal TotalSpent { get; set; }

        // List to manage all customers
        private static List<Customer> customers = new List<Customer>();

        // Define a constant for the file path
        private const string customerFilePath = "customers.txt";

        // Constructor to initialize customer details
        public Customer(string name, string password)
        {
            Name = name;
            Password = password;
            //_shoppingCart = new Dictionary<string, int>(); 
            _shoppingCart = new Dictionary<Product, int>();
        }

        // Verify password method
        public bool VerifyPassword(string password)
        {
            return Password == password;
        }

        public override string ToString()
        {
            string cartItems = _shoppingCart.Count > 0
                ? string.Join(", ", _shoppingCart.Select(kvp => $"{kvp.Value} pcs {kvp.Key.Name}"))
                : "No items in cart";
            //return $"Name: {Name}, Shopping Cart: {cartItems}"; // Exclude password for security
            return $"Name: {Name}, Shopping Cart: {cartItems}, Password: {GetPassword(true)}, Total Spent: {TotalSpent} kr, Membership Status: {GetMembershipStatus()}";
        }

        // Method to get the password conditionally
        public string GetPassword(bool showPassword)
        {
            return showPassword ? Password : new string('*', Password.Length); // Show or hide password
        }

        // LogIn method 
        public static bool LogIn(out string? username, out string? password, string cartfilePath)
        {
            username = string.Empty;
            password = string.Empty;

            // Debugging output to confirm customer loading
            Console.WriteLine($"Debug: Customers count before login: {customers.Count}");

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter your name: ");
            Console.ResetColor();
            string? name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid choice. Please, enter your name.");
                Console.ResetColor();
                Console.Write("Enter your name: ");
                name = Console.ReadLine();
            }

            // Check if customer exists
            Customer? customer = customers.Find(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (customer != null)
            {
                bool passwordCorrect = false;

                while (!passwordCorrect)
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("Enter password: ");
                    Console.ResetColor();
                    string enteredPassword = Console.ReadLine();

                    while (string.IsNullOrWhiteSpace(enteredPassword))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Invalid choice. Please, enter your password.");
                        Console.ResetColor();
                        Console.Write("Enter password: ");
                        enteredPassword = Console.ReadLine();
                    }

                    if (customer.VerifyPassword(enteredPassword))
                    {
                        username = customer.Name;
                        password = customer.GetPassword(true);
                        Console.WriteLine($"Welcome back, {customer.Name}!");
                        return true;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Password incorrect. Try again.");
                        Console.ResetColor();

                        Console.Write("Do you want to try again? (Yes/No): ");
                        string retry = Console.ReadLine()?.ToLower();

                        if (retry != "yes")
                        {
                            Console.WriteLine("Exiting login...");
                            return false;
                        }
                    }
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nLogin failed. Customer \"{name}\" does not exist.");
                Console.ResetColor();

                if (AskToRegister())
                {
                    HandleCustomers(out string? newUsername, out string? newPassword);
                    Customer newCustomer = new Customer(newUsername, newPassword);
                    customers.Add(newCustomer); // Add the new customer to the list

                    username = newUsername;
                    password = newPassword;

                    Console.WriteLine($"Customer data saved successfully for {username}.");
                    Console.WriteLine($"Welcome to the Simple Store, {username}!");
                }
            }

            return false;
        }
        public static bool AskToRegister()
        {
            while (true)
            {
                Console.WriteLine("\nWould you like to register a new customer?");
                Console.WriteLine("1) Yes, I would like to join the Simple store!");
                Console.WriteLine("2) No. I want to log out.");
                Console.Write("\nSelect an option: ");

                string? input = Console.ReadLine();

                if (input == "1")
                {
                    HandleCustomers(out string? username, out string? password); // Call the RegisterNewCustomer method
                    return true; // Continue after registering
                }
                else if (input == "2")
                {
                    Console.WriteLine("Logging out...");
                    return false; // Log out
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid option. Please enter 1 to register or 2 to log out.");
                    Console.ResetColor();
                }
            }
        }
        //Method for discount/membership
        public virtual string GetMembershipStatus()
        {
            if (TotalSpent >= 500)
                return "Gold Member!";
            else if (TotalSpent >= 200)
                return "Silver Member!";
            else if (TotalSpent >= 150)
                return "Bronze Member!";
            else
                return "Regular Member!";
        }
        public static bool HandleCustomers(out string? username, out string? password)
        {
            username = null;
            password = null;

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter your name: ");
            Console.ResetColor();
            username = Console.ReadLine();

            // Validate the username input
            while (string.IsNullOrWhiteSpace(username))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input. Please, enter your name.");
                Console.ResetColor();
                Console.Write("Enter your name: ");
                username = Console.ReadLine();
            }

            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("Enter your new password: ");
            Console.ResetColor();
            password = Console.ReadLine();

            // Validate the password input
            while (string.IsNullOrWhiteSpace(password))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid input. Please, enter your password.");
                Console.ResetColor();
                Console.Write("Enter your password: ");
                password = Console.ReadLine();
            }

            // Clear sensitive information before moving on
            Console.WriteLine("Customer details received successfully.");
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();

            return true;
        }
    }
}
