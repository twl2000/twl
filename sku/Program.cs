using System;
using System.Collections.Generic;
using System.Linq;

namespace sku
{
    //** use console to test CheckOut class
    //** type "scan <item name>" or "remove <item name>"
    //** console will display checkout details based on calcluation done by CheckOut class
    public class Program
    {
        static void Main(string[] args)
        {   
            var co = new CheckOut();

            while (true)
            {
                Console.WriteLine("Enter Scan <item> or Remove <item> where <item> = A or B or C or D" + "\n");
                string input = Console.ReadLine();
                string[] inputs = input.Split(' ');
                if (inputs[0].ToLower() == "exit")
                {
                    break;
                }
                if (inputs.Count() == 2)
                {
                    switch (inputs[0].ToLower())
                    {
                        case "scan":
                            Console.WriteLine("You scanned item " + inputs[1]);
                            co.scan(inputs[1]);
                            co.display_checkout();

                            break;
                        case "remove":
                            Console.WriteLine("You removed item " + inputs[1]);
                            co.remove(inputs[1]);
                            co.display_checkout();
                            break;
                    }
                }
            }
        }
    }

    //** implemenation for SKU
    public class CheckOut
    {
        public decimal total { get; set; }
        public static List<scanned_item> scanned_items = new List<scanned_item>();
        public store store = new store();

        //** method to be called when an item is scanned
        public void scan(string name)
        {
            //** check if scanned item is an item available in store
            var Found_item_in_store = store.store_items.FirstOrDefault(a => a._name.ToLower() == name.ToLower());
            if (Found_item_in_store != null)
            {
                //** check if scanned_item already exist in system
                var Found_scanned_item = scanned_items.FirstOrDefault(a => a._name.ToLower() == name.ToLower());
                if (Found_scanned_item == null)
                {
                    //** scanned item does not exisit, add a new one
                    scanned_items.Add(new scanned_item(1, name.ToUpper(), Found_item_in_store._price));
                    total += Found_item_in_store._price;
                }
                else
                {
                    //** scanned item already exist, so update quantity and price
                    Found_scanned_item._quantity += 1;
                    Found_scanned_item._price += Found_item_in_store._price;
                    total += Found_item_in_store._price;

                    //** check if pricing rule applied to scanned item
                    var Found_item_with_pricing_rule = store.store_pricing_rules.FirstOrDefault(a => a._name.ToLower() == name.ToLower());
                    if (Found_item_with_pricing_rule != null)
                    {
                        int remainder = Found_scanned_item._quantity % Found_item_with_pricing_rule._quantity;
                        int quotient = Found_scanned_item._quantity / Found_item_with_pricing_rule._quantity;

                        if (quotient >= 1)
                        {
                            total -= Found_scanned_item._price;
                            Found_scanned_item._price = (quotient * Found_item_with_pricing_rule._special_price) + (remainder * Found_item_in_store._price);
                            total += Found_scanned_item._price;
                        }
                    }
                }
                //    }
            }
        }

        //** method to be called when an item is removed from checkout
        public void remove(string name)
        {
            //** check if removed item is an item available in store
            var Found_item_in_store = store.store_items.FirstOrDefault(a => a._name.ToLower() == name.ToLower());
            if (Found_item_in_store != null)
            {
                //** check if removed item already exist in system. It should...
                var Found_scanned_item = scanned_items.FirstOrDefault(a => a._name.ToLower() == name.ToLower());
                if (Found_scanned_item == null)
                {
                    return;
                }
                else
                {
                    //** removed item already exist, so update quantity and price, or delete it if its the last one
                    if (Found_scanned_item._quantity == 1)
                    {
                        scanned_items.Remove(Found_scanned_item);
                        total -= Found_scanned_item._price;
                        return;
                    }
                    Found_scanned_item._quantity -= 1;
                    Found_scanned_item._price -= Found_item_in_store._price;
                    total -= Found_item_in_store._price;

                    //** check if pricing rule applied to removed item
                    var Found_item_with_pricing_rule = store.store_pricing_rules.FirstOrDefault(a => a._name.ToLower() == name.ToLower());
                    if (Found_item_with_pricing_rule != null)
                    {
                        int remainder = Found_scanned_item._quantity % Found_item_with_pricing_rule._quantity;
                        int quotient = Found_scanned_item._quantity / Found_item_with_pricing_rule._quantity;

                        if (quotient >= 1)
                        {
                            total -= Found_scanned_item._price;
                            Found_scanned_item._price = (quotient * Found_item_with_pricing_rule._special_price) + (remainder * Found_item_in_store._price);
                            total += Found_scanned_item._price;
                        }
                        else
                        {
                            total -= Found_scanned_item._price;
                            Found_scanned_item._price = Found_scanned_item._quantity * Found_item_in_store._price;
                            total += Found_scanned_item._price;
                        }
                    }
                }
            }
        }

        //** display checkout details (for testing purpose)
        public void display_checkout()
        {
            Console.WriteLine("Checkout Details \n");
            foreach (var x in scanned_items)
            {
                Console.WriteLine(x._quantity + " x item " + x._name + " cost £" + x._price);
            }
            Console.WriteLine("Total cost: £" + total + "\n");
        }
    }

    //** to save all items in store available for scanning
    public class item
    {
        public  string _name { get; set; }
        public  decimal _price { get; set; }

        public item (string name, decimal price)
        {
            _name = name;
            _price = price;
        }
    }

    //** to save all pricing rules
    public class pricing_rules
    {
        public string _name;
        public int _quantity { get; set; }
        public decimal _special_price {get; set;}

        public pricing_rules(string name, int quanity, decimal special_price)
        {
            _name = name;
            _quantity = quanity;
            _special_price = special_price;
        }
    }

    //** to save and keep track of all checkout items (all scanned and removed items)
    public class scanned_item
    {
        public int _quantity { get; set; }
        public string _name { get; set; }
        public decimal _price { get; set; }

        public scanned_item(int quantity, string name, decimal price)
        {
            _quantity = quantity;
            _name = name;
            _price = price;
        }
    }


    //** to fetch data (all items details and pricing rules) to be used by this app
    //** some hardcoded data is used for this application.
    //** In production, this data should be fetched from dB or config file
    //** interface allows another class to be implemented to fetch data from other sources
    public interface Istoredata
    {
        void get_data();
    }

    public class store: Istoredata
    {
        public List<item> store_items = new List<item>();
        public List<pricing_rules> store_pricing_rules = new List<pricing_rules>();
        public store()
        {
            get_data();
        }

        public void get_data()
        {
            store_items.Add(new item("A", 50));
            store_items.Add(new item("A", 50));
            store_items.Add(new item("B", 30));
            store_items.Add(new item("C", 20));
            store_items.Add(new item("D", 15));

            store_pricing_rules.Add(new pricing_rules("A", 3, 130));
            store_pricing_rules.Add(new pricing_rules("B", 2, 45));
        }
    }
}
