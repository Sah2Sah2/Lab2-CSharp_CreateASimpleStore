using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Simple.Store
{
    class Program
    {
        static bool isLoggedIn = false; // Tracks whether the user is logged in
        static Customer loggedInCustomer = null;
        static string customerFilePath = "customers.txt"; // Path to the customer data file
        static List<Customer> customers = new List<Customer>(); // List to hold customers
        static Dictionary<Product, int> cart = new Dictionary<Product, int>(); // Cart

        static void Main(string[] args)
        {
            SeedCustomers();

            bool showMenu = true;
            bool isFirstVisit = true; // Track if it's the first visit
            string cartfilePath = "cart.txt"; // Path to the cart file

            while (showMenu)
            {
                showMenu = MainMenu(ref isFirstVisit, cartfilePath);
            }
        }

        private static void SeedCustomers()
        {
            customers.Add(new Customer("Sara", "123"));
            customers.Add(new Customer("Jimmy", "456"));
            customers.Add(new Customer("Alessia", "789"));
        }

        private static bool MainMenu(ref bool isFirstVisit, string cartfilePath)
        {
            string username, password;

            if (isFirstVisit)
            {
                Console.WriteLine(@"
         _____________________ 
        |                     |  
        |    WELCOME TO THE   |  
        |     SIMPLE STORE!   |  
        |_____________________|");
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.Write("\n Here you can find the best products at the best prices in town!");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n ---------------------------------------------------------------");
                isFirstVisit = false;
                Console.ReadKey();
            }

            // Show different options depending on whether the user is logged in
            Console.ResetColor();
            if (!isLoggedIn)
            {
                Console.WriteLine("\nChoose an option:");
                Console.WriteLine("1) Register new customer");
                Console.WriteLine("2) Log in");
                Console.WriteLine("3) Exit");
                Console.Write("\nSelect an option: ");

                switch (Console.ReadLine())
                {
                    case "1":
                        RegisterNewCustomer(); // Register new customer
                        isLoggedIn = true; // Automatically logged in after registration
                        return true; // Return to menu
                    case "2":
                        // Prompt for username and password for logging in
                        Console.Write("Enter username: ");
                        username = Console.ReadLine();
                        Console.Write("Enter password: ");
                        password = Console.ReadLine();

                        // Log in and check if successful
                        if (LogIn(username, password)) // Enter the username and password
                        {
                            isLoggedIn = true;
                        }
                        else
                        {
                            bool userExists = false;
                            foreach (var c in customers)
                            {
                                if (c.Name.Equals(username, StringComparison.OrdinalIgnoreCase))
                                {
                                    userExists = true;
                                }
                            }
                            if (userExists)
                            {
                                //Reput pw
                                while (!LogIn(username, password))
                                {
                                    Console.Write("Try again!\nEnter password: ");
                                    password = Console.ReadLine();
                                }
                                isLoggedIn = true;
                            }
                            else
                            {
                                Console.WriteLine("Invalid credentials. Would you like to register as a new customer? (Yes/No)");
                                string response = Console.ReadLine(); // Read the response directly

                                // Checking for multiple forms of "Yes" and "No"
                                if (response.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                                {
                                    RegisterNewCustomer();
                                    if (loggedInCustomer != null)
                                    {
                                        isLoggedIn = true;
                                        Console.WriteLine($"Wlecome, {loggedInCustomer.Name}! You are now logged in.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Registration failed. Returning to the main menu...");
                                    }
                                }
                                else if (response.Equals("No", StringComparison.OrdinalIgnoreCase))
                                {
                                    Console.WriteLine("Returning to the main menu...");
                                    System.Threading.Thread.Sleep(1000); //Wait before exiting 
                                }
                                else
                                {
                                    Console.WriteLine("Invalid choice. Returning to the main menu...");
                                    System.Threading.Thread.Sleep(1000);
                                }
                            }
                        }
                        return true; // Return to menu

                    case "3":
                        Console.WriteLine("Exiting the application...");
                        System.Threading.Thread.Sleep(2000);
                        return false; // Close the application
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        return true;
                }
            }
            else
            {
                Console.Clear(); // Clear the console from the previously logged information to increase readability

                // Show options after logging in or registering
                Console.WriteLine("\nChoose an option:");
                Console.WriteLine("1) Go shopping");
                Console.WriteLine("2) Display your account information");
                Console.WriteLine("3) View cart");
                Console.WriteLine("4) Log out");
                Console.Write("\r\nSelect an option: ");

                Dictionary<Product, int> cart; // Declare the cart variable
                switch (Console.ReadLine())
                {
                    case "1":
                        StartShopping();
                        return true; // Stay in menu after shopping
                    case "2":
                        if (loggedInCustomer != null)
                        {
                            Product.LoadCart(cartfilePath, out cart); // Load the cart
                            loggedInCustomer._shoppingCart = cart; // Assign the cart to the logged-in customer
                            DisplayAccountInformation();
                        }
                        else
                        {
                            Console.WriteLine("No customer is currently logged in.");
                        }
                        Console.WriteLine("\n\n\nPress any key to continue");
                        Console.ReadKey();
                        return true;
                    case "3":
                        Product.LoadCart(cartfilePath, out cart); // Load the cart
                        Product.ViewProductsInCart(cart);   // Call the method to view the cart with the loaded cart

                        //Give the user the option of what to do next
                        Console.WriteLine("\nChoose an option:");
                        Console.WriteLine("1) Pay for items");
                        Console.WriteLine("2) Exit");
                        Console.Write("\r\nSelect an option: ");

                        switch (Console.ReadLine())
                        {
                            case "1":
                                // Process the payment for items in the cart
                                Product.PayForItems(cart, cartfilePath);
                                Console.WriteLine("\n\n\nPress any key to continue");
                                Console.ReadKey();
                                return true; // Stay in the menu after payment

                            case "2":
                                // Exit back to the main menu
                                return true;

                            default:
                                // Invalid choice
                                Console.WriteLine("Invalid choice. Please try again.");
                                return true;
                        }
                    case "4":
                        Console.WriteLine("You have been logged out.");
                        isLoggedIn = false;
                        return false;
                    default:
                        Console.WriteLine("Invalid choice. Please try again.");
                        return true;
                }
            }
        }
        private static void RegisterNewCustomer()
        {
            if (customers.Count > 0)
            {
                foreach (var customer in customers)
                {
                    Console.WriteLine($"- {customer.Name}"); // Debugging output
                }
            }
            else
            {
                Console.WriteLine("No customers found."); // Handle empty case
            }

            // Get customer details and handle registration
            if (!Customer.HandleCustomers(out string name, out string password))
            {
                Console.WriteLine("Registration failed. Please try again.");
                return; // Exit if registration failed
            }

            // Check for duplicate username
            if (customers.Any(c => c.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Username already exists. Please choose a different username.");
                Console.ResetColor();
                return; // Exit if the username already exists
            }

            // Create the new customer and add it to the list
            var newCustomer = new Customer(name, password);
            customers.Add(newCustomer);

            // New customer is now set as logged in
            loggedInCustomer = newCustomer;
            Console.WriteLine($"Welcome, {loggedInCustomer.Name}! You are now logged in.");
        }

        private static void StartShopping()
        {
            Dictionary<Product, int> cart = new Dictionary<Product, int>();
            string cartfilePath = "cart.txt";

            // Clear the previous cart
            Product.ClearCart();

            // Display the cart content initially 
            Product.ViewProductsInCart(cart);

            // List of available products 
            List<Product> productsAvailable = Product.ProductsAvailable;

            bool shopping = true;

            while (shopping)
            {
                // Display available products
                Console.WriteLine("\nAvailable products:");
                for (int i = 0; i < productsAvailable.Count; i++)
                {
                    var product = productsAvailable[i];
                    Console.WriteLine($"{i + 1}. {product.Name} - {product.PriceSEK:F2} kr / {product.PriceEUR:F2} EUR / {product.PriceCHF:F2} CHF");
                }
                Console.Write("\nEnter: \n-the number of the product (1\\2\\3\\4\\5) you want to add to the cart\n-or type 'pay' to finish shopping,\n-or 'save' to save cart for later: ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int productIndex) && productIndex > 0 && productIndex <= productsAvailable.Count)
                {
                    // Get the selected product
                    Product selectedProduct = productsAvailable[productIndex - 1];

                    // Add the selected product to the cart, incrementing the quantity
                    if (cart.ContainsKey(selectedProduct))
                    {
                        cart[selectedProduct]++;
                        Console.WriteLine($"{selectedProduct.Name} quantity increased in the cart.");
                    }
                    else
                    {
                        cart[selectedProduct] = 1;
                        Console.WriteLine($"{selectedProduct.Name} added to the cart.");
                    }

                    //Discount levels 
                    decimal discountFactor;
                    switch (loggedInCustomer.GetMembershipStatus())
                    {
                        case "Gold Member!":
                            discountFactor = 1m - 0.15m;
                            break;
                        case "Silver Member!":
                            discountFactor = 1m - 0.10m;
                            break;
                        case "Bronze Member!":
                            discountFactor = 1m - 0.05m;
                            break;
                        default:
                            discountFactor = 1m;
                            break;

                    }
                    loggedInCustomer.TotalSpent += discountFactor * selectedProduct.PriceSEK;

                    // Print current cart contents for debugging
                    Console.WriteLine("Current cart contents:");
                    foreach (var item in cart)
                    {
                        Console.WriteLine($"- {item.Key.Name}: {item.Value} pcs");
                    }
                }
                else if (input.ToLower() == "save")
                {
                    if (cart.Count > 0)
                    {
                        try
                        {
                            // Save the current cart to file
                            Product.SaveCart(cartfilePath, cart);
                            Console.WriteLine("Your cart has been saved for later.");
                            shopping = false;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error saving cart: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Your cart is empty. Please add products before saving.");
                    }
                }
                else if (input.ToLower() == "pay")
                {
                    if (cart.Count > 0)
                    {
                        Console.WriteLine("Thank you for your purchase!");

                        // End the shopping loop and show the final cart contents and total price
                        shopping = false;

                        decimal totalSEK = 0m;
                        decimal totalEUR = 0m;
                        decimal totalCHF = 0m;

                        Console.WriteLine("\nYour cart contains the following items:");
                        foreach (var item in cart)
                        {
                            // Calculate the total price for this item based on quantity
                            decimal itemTotalPriceSEK = item.Key.PriceSEK * item.Value;
                            decimal itemTotalPriceEUR = item.Key.PriceEUR * item.Value;
                            decimal itemTotalPriceCHF = item.Key.PriceCHF * item.Value;

                            Console.WriteLine($"- {item.Key.Name}: {item.Value} pcs - {item.Key.PriceSEK:F2} kr each, Total: {itemTotalPriceSEK:F2} kr");

                            totalSEK += itemTotalPriceSEK;
                            totalEUR += itemTotalPriceEUR;
                            totalCHF += itemTotalPriceCHF;
                        }

                        //Membership status 
                        switch (loggedInCustomer.GetMembershipStatus())
                        {
                            case "Gold Member!":
                                //
                                totalSEK = (1m - 0.15m) * totalSEK;
                                break;
                            case "Silver Member!":
                                totalSEK = (1m - 0.10m) * totalSEK;
                                break;
                            case "Bronze Member!":
                                totalSEK = (1m - 0.05m) * totalSEK;
                                break;
                            default:
                                Console.WriteLine("No discount applied");
                                break;

                        }

                        // Display the final total amounts in different currencies 
                        Console.WriteLine($"\nTotal Amount: {totalSEK:F2} kr / {totalEUR:F2} EUR / {totalCHF:F2} CHF");
                        Console.WriteLine("Thank you for shopping with us!");

                    }
                    else
                    {
                        Console.WriteLine("Your cart is empty. Please add products before paying.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid option. Please try again.");
                }
            }
        }
        private static void DisplayAccountInformation()
        {
            // Display customer information
            if (loggedInCustomer != null)
            {
                Console.WriteLine(loggedInCustomer.ToString());
            }
            else
            {
                Console.WriteLine("No customer is currently logged in.");
            }
        }

        private static bool LogIn(string username, string password)
        {
            // Check if credentials match any existing customer
            Customer customer = null;

            foreach (var c in customers)
            {
                if (c.Name.Equals(username, StringComparison.OrdinalIgnoreCase) && c.VerifyPassword(password))
                {
                    customer = c;
                    break;
                }
            }
            if (customer != null)
            {
                loggedInCustomer = customer; // Set logged-in customer
                return true; // Login successful
            }
            return false; // Login failed
        }
    }
}