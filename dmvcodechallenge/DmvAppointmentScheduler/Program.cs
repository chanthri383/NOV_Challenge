using System;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace DmvAppointmentScheduler
{
    class Program
    {
        public static Random random = new Random();
        public static List<Appointment> appointmentList = new List<Appointment>();
        static void Main(string[] args)
        {
            CustomerList customers = ReadCustomerData();
            TellerList tellers = ReadTellerData();
            Dictionary<string, int> teller_occurrences = new Dictionary<string, int>();
            Dictionary<string, int> teller_type = new Dictionary<string, int>();
            Dictionary<String, ArrayList> teller_list = new Dictionary<String, ArrayList>();
            Dictionary<string, int> customer_position = new Dictionary<string, int>();
            Dictionary<string, int> amountTellerPerType = new Dictionary<string, int>();
            Dictionary<string, Teller> tellerConvertedToDictionary = listToDictionary(tellers);
            customer_position = createCustomerMatchingDictionaryPosition(customers);
            teller_list = separateTellerIntoDictionary(tellers);
            amountTellerPerType = sizeOfEachTellers(teller_list);
            Calculation(customers, amountTellerPerType, customer_position, teller_list, tellerConvertedToDictionary);
            OutputTotalLengthToConsole();


        }
        public static Dictionary<string, int> createCustomerMatchingDictionaryPosition(CustomerList customers)
        {
            Dictionary<string, int> customer_position = new Dictionary<string, int>();
            foreach (Customer customer in customers.Customer)
            {
                string id = customer.type;
                if (!customer_position.ContainsKey(id))
                {
                    customer_position.Add(id, 0);
                }
            }
            return customer_position;
        }

        public static Dictionary<string, int> sizeOfEachTellers(Dictionary<String, ArrayList> client_list)
        {
            Dictionary<string, int> tellerSize = new Dictionary<string, int>();
            foreach (var item in client_list)
            {
                if (!tellerSize.ContainsKey(item.Key))
                {
                    tellerSize.Add(item.Key, client_list[item.Key].Count);
                }
            }
            return tellerSize;
        }

        static Dictionary<string, Teller> listToDictionary(TellerList tellers)
        {
            Dictionary<string, Teller> listConverted = new Dictionary<string, Teller>();
            foreach (Teller teller in tellers.Teller)
            {
                if (!listConverted.ContainsKey(teller.id))
                {
                    listConverted.Add(teller.id, teller);
                }
            }
            return listConverted;
        }

        public static string getKeyLowestValue(Dictionary<string, int> input)
        {
            var key = input.Keys.Min();
            return key;
        }

        public static int returnCustomerWithLowestPosition(Dictionary<string, int> customersDictInput)
        {
            int lowest_column = 2147483647;
            foreach (var item in customersDictInput)
            {
                int number_occurrences = item.Value;
                if (number_occurrences <= lowest_column)
                {
                    lowest_column = number_occurrences;
                }
            }
            return lowest_column;
        }

        public static Dictionary<string, ArrayList> separateTellerIntoDictionary(TellerList list)
        {
            Dictionary<String, ArrayList> teller_list = new Dictionary<String, ArrayList>();
            foreach (Teller teller in list.Teller)
            {


                string specialtyType = teller.specialtyType;
                if (!teller_list.ContainsKey(specialtyType))
                {
                    teller_list.Add(specialtyType, new ArrayList());
                }
                teller_list[specialtyType].Add(teller.id);
            }
            return teller_list;
        }

        private static CustomerList ReadCustomerData()
        {
            string fileName = "CustomerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            CustomerList customerData = JsonConvert.DeserializeObject<CustomerList>(jsonString);
            return customerData;

        }
        private static TellerList ReadTellerData()
        {
            string fileName = "TellerData.json";
            string path = Path.Combine(Environment.CurrentDirectory, @"InputData\", fileName);
            string jsonString = File.ReadAllText(path);
            TellerList tellerData = JsonConvert.DeserializeObject<TellerList>(jsonString);
            return tellerData;

        }


        static void Calculation(CustomerList customers, Dictionary<string, int> amountTellerPerType, Dictionary<string, int> customer_position, Dictionary<String, ArrayList> teller_list, Dictionary<string, Teller> tellerConvertedToDictionary)
        {
            foreach (Customer customer in customers.Customer)
            {

                string typeCustomer = customer.type;
                if (amountTellerPerType.ContainsKey(typeCustomer))
                {
                    string tellerGet = teller_list[typeCustomer][customer_position[typeCustomer]].ToString();
                    var appointment = new Appointment(customer, tellerConvertedToDictionary[tellerGet]);
                    appointmentList.Add(appointment);
                    customer_position[typeCustomer] += 1;
                    if (customer_position[typeCustomer] >= amountTellerPerType[typeCustomer])
                    {
                        customer_position[typeCustomer] = 0;
                    }
                }
                else
                {
                    string tellerMostAvailable = getKeyLowestValue(amountTellerPerType);
                    string tellerGet = teller_list[tellerMostAvailable][customer_position[typeCustomer]].ToString();
                    var appointment = new Appointment(customer, tellerConvertedToDictionary[tellerGet]);
                    appointmentList.Add(appointment);
                    customer_position[typeCustomer] += 1;
                    if (customer_position[typeCustomer] >= amountTellerPerType[tellerMostAvailable])
                    {
                        customer_position[typeCustomer] = 0;
                    }
                }
            }
        }
        static void OutputTotalLengthToConsole()
        {
            var tellerAppointments =
                from appointment in appointmentList
                group appointment by appointment.teller into tellerGroup
                select new
                {
                    teller = tellerGroup.Key,
                    totalDuration = tellerGroup.Sum(x => x.duration),
                };
            var max = tellerAppointments.OrderBy(i => i.totalDuration).LastOrDefault();
            Console.WriteLine("Teller " + max.teller.id + " will work for " + max.totalDuration + " minutes!");
        }

    }
}
