using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Simple.Store
{
    public class Product
    {
        // Product properties
        public string Name { get; set; }
        public decimal PriceSEK { get; set; } // Price in SEK
        public decimal PriceEUR { get; set; } // Price in EUR
        public decimal PriceCHF { get; set; } // Price in CHF
        public int Quantity { get; set; }     // Quantity of product

        // Static list of products
        public static List<Product> ProductsAvailable { get; } = new List<Product>
        {
            new Product { Name = "Pepsi", PriceSEK = 9.95m, PriceEUR = 0.93m, PriceCHF = 0.99m },
            new Product { Name = "Bagel", PriceSEK = 0.99m, PriceEUR = 0.09m, PriceCHF = 0.10m },
            new Product { Name = "Kvarg", PriceSEK = 20.95m, PriceEUR = 1.95m, PriceCHF = 2.05m },
            new Product { Name = "Coffee", PriceSEK = 64.95m, PriceEUR = 6.05m, PriceCHF = 6.45m },
            new Product { Name = "Olive Oil", PriceSEK = 149.00m, PriceEUR = 13.85m, PriceCHF = 14.70m },
            new Product { Name = "Asparagus", PriceSEK = 49.95m, PriceEUR = 4.65m, PriceCHF = 5.00m },
        };

        // To avoid problem of items not matching while saved in the cart for later
        public override bool Equals(object obj)
        {
            if (obj is Product other)
            {
                // Check for equality based on unique identifying properties
                return Name == other.Name &&
                       PriceSEK == other.PriceSEK &&
                       PriceEUR == other.PriceEUR &&
                       PriceCHF == other.PriceCHF;
            }
            return false;
        }
        public override int GetHashCode()
        {
            // Generate hash code based on the same properties used in Equals
            return HashCode.Combine(Name, PriceSEK, PriceEUR, PriceCHF);
        }

        // Static dictionary to hold products in the cart
        private static Dictionary<Product, int> cart = new Dictionary<Product, int>();

        public static void AddProductToCart(Product product, Dictionary<Product, int> cart)
        {
            if (product.Quantity <= 0)
            {
                Console.WriteLine($"Invalid quantity for {product.Name}. Quantity must be greater than 0.");
                return;
            }

            // Check if the product is already in the cart
            if (cart.ContainsKey(product))
            {
                // Update the quantity
                cart[product] += product.Quantity;
                Console.WriteLine($"Updated quantity of {product.Name} to {cart[product]}.");
            }
            else
            {
                // Add the product to the cart
                cart[product] = product.Quantity;
                Console.WriteLine($"Added {product.Name} to the cart with quantity {product.Quantity}.");
            }

            // Display the cart
            Product.ViewProductsInCart(cart);
        }

        // Method to clear the cart
        public static void ClearCart()
        {
            cart.Clear();
        }

        // Method to get the total amount of products in different currencies
        public static (decimal totalSEK, decimal totalEUR, decimal totalCHF) GetCartAmount()
        {
            decimal totalSEK = 0m;
            decimal totalEUR = 0m;
            decimal totalCHF = 0m;

            foreach (var item in cart)
            {
                totalSEK += item.Key.PriceSEK * item.Value;
                totalEUR += item.Key.PriceEUR * item.Value;
                totalCHF += item.Key.PriceCHF * item.Value;
            }

            return (totalSEK, totalEUR, totalCHF);
        }

        // Method to save the cart to a file
        public static void SaveCart(string cartfilePath, Dictionary<Product, int> cart)
        {
                using (StreamWriter writer = new StreamWriter(cartfilePath, false)) // Overwrite the file
                {
                    if (cart.Count == 0)
                    {
                        Console.WriteLine("The cart is empty. Nothing to save.");
                        return;
                    }

                    foreach (var item in cart)
                    {
                        var product = item.Key;
                        var quantity = item.Value;
                        writer.WriteLine($"{product.Name},{product.PriceSEK.ToString("F2", CultureInfo.InvariantCulture)},{product.PriceEUR.ToString("F2", CultureInfo.InvariantCulture)},{product.PriceCHF.ToString("F2", CultureInfo.InvariantCulture)},{quantity}");
                        Console.WriteLine($"Saving: {product.Name}, {product.PriceSEK}, {quantity}");
                    }
                }

                Console.WriteLine("\nCart saved successfully.");
        }


        public static void LoadCart(string cartfilePath, out Dictionary<Product, int> cart)
        {
            cart = new Dictionary<Product, int>();

            // Check if the cart file exists
            if (!File.Exists(cartfilePath))
            {
                return; // No cart to load, return empty cart
            }

            try
            {
                using (StreamReader reader = new StreamReader(cartfilePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = line.Trim();
                        var parts = line.Split(',');

                        // Ensure the line is correctly formatted and that the prices and quantity can be parsed
                        if (parts.Length == 5 &&
                            decimal.TryParse(parts[1].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceSEK) &&
                            decimal.TryParse(parts[2].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceEUR) &&
                            decimal.TryParse(parts[3].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal priceCHF) &&
                            int.TryParse(parts[4].Trim(), out int quantity))
                        {
                            Product product = new Product
                            {
                                Name = parts[0].Trim(),
                                PriceSEK = priceSEK,
                                PriceEUR = priceEUR,
                                PriceCHF = priceCHF
                            };

                            cart[product] = quantity;
                            Console.WriteLine($"\nAdded: {product.Name}, {product.PriceSEK} kr, {product.PriceEUR} EUR, {product.PriceCHF} CHF, {quantity} pcs");
                        }
                        else
                        {
                            Console.WriteLine($"Skipping line: {line}"); // Invalid line format
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while loading the cart: {ex.Message}");
            }

            Console.WriteLine($"Number of items in cart after loading: {cart.Count}");
        }


        // Method to print the items added to the cart
        public static void ViewProductsInCart(Dictionary<Product, int> cart)
        {
            if (cart == null || cart.Count == 0)
            {
                Console.WriteLine("Your cart is empty.");
                return;
            }

            Console.WriteLine("\nYour cart contains the following items:");
            decimal totalSEK = 0;
            decimal totalEUR = 0;
            decimal totalCHF = 0;

            foreach (var item in cart)
            {
                Console.WriteLine($"- {item.Key.Name}: {item.Value} pcs at {item.Key.PriceSEK:F2} kr each");
                totalSEK += item.Key.PriceSEK * item.Value; // Calculate total
                totalEUR += item.Key.PriceEUR * item.Value;
                totalCHF += item.Key.PriceCHF * item.Value;
            }

            Console.WriteLine($"Total Amount: {totalSEK:F2} kr, {totalEUR:F2} EUR, {totalCHF:F2} CHF\n\n\n\n");
        }

        public static void PayForItems(Dictionary<Product, int> cart, string cartfilePath)
        {
            decimal total = 0;

            // Calculate the total cost of the cart
            foreach (var item in cart)
            {
                Product product = item.Key;
                int quantity = item.Value;
                total += product.PriceSEK * quantity;
            }

            // Display the total amount
            Console.WriteLine($"Total amount to pay: {total:F2} kr");

            Console.WriteLine("Payment processed. Thank you for shopping!");

            // Optionally clear the cart after payment
            cart.Clear();

            // Save the updated (empty) cart back to the file
            SaveCart(cartfilePath, cart);
        }

    }
}
