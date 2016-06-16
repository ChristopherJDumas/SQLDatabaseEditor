using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Linq;

namespace DatabaseEditor
{
    class Program 
    {
        static void Main(string[] args)
        {
            DataContext db = getDataContext();

            Table<User> Users = db.GetTable<User>(); //Only supports Users table defined in User.cs

            DisplayMainMenu(db, Users);
        }

        private static DataContext getDataContext() //NEEDS TO BE CHANGED FOR DIFFERENT SERVERS
        {
            return new DataContext(@"
            Server=CHRIS-WINDOWS;
            Database=NETData;
            Trusted_Connection=Yes;"
            );
        }

        private static void DisplayMainMenu(DataContext db, Table<User> Users)
        {
            while (true)
            {
                Console.WriteLine("MAIN MENU (1, 2, 3, or 4)\n 1. Search and Update/Delete\n 2. Add a record\n 3. Display the table\n 4. Exit");

                string str = Console.ReadLine();

                switch (str.First())
                {
                    case '1':
                        SearchTable(Users, db);
                        break;

                    case '2':
                        AddRecord(Users, db);
                        break;

                    case '3':
                        OutputTable(Users);
                        break;

                    case '4':
                        while (true)
                        {
                            Console.WriteLine("Are you sure you want to exit? (y/n)");
                            string str2 = Console.ReadLine();
                            if (str2.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                            {
                                Console.WriteLine("Goodbye!");
                                System.Environment.Exit(1);
                                return;
                            }
                            else if (str2.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                                break;
                            else
                                Console.WriteLine("Unrecognized response");
                        }
                        break;

                    default:
                        Console.WriteLine("Unrecognized option");
                        break;
                }
            }
        }

        private static void SearchTable(Table<User> Users, DataContext db)
        {
            Console.WriteLine("Which columns would you like to search? (comma delimited)");
            string[] columns = getColumnsFromUser();
            string[] operators = getOperatorsFromUser(columns);
            Console.WriteLine("What value(s) are you searching for? (one per column enterd, comma delimited)");
            string[] values = getValuesFromUser(columns);

            string where = columns[0] + " " + operators[0] + " '" + values[0] + "'";

            for (int i = 1; i < columns.Length; i++)
            {
                where += " and " + columns[i] + " " + operators[i] + " '" + values[i] + "'";
            }


            try
            {
                List<User> query = db.ExecuteQuery<User>(@"Select * From Users 
                                                Where " + where).ToList();
                int count = 0;
                foreach (User user in query)
                {
                    count++;
                    Console.WriteLine("ID= {0}, FirstName= {1}, LastName= {2}, UserName= {3}, Email= {4}, Created= {5}", user.ID.ToString(), user.FirstName, user.LastName, user.UserName, user.Email, user.Created.ToString());
                }
                Console.WriteLine("Your query returned " + count + " results.");
                DisplayQueryMenu(Users, query, db);
                return;
            }
            catch (System.Data.SqlClient.SqlException err)
            {
                Console.WriteLine("SQL Server returned the following error: " + err.Message);
            }
        }

        private static void DisplayQueryMenu<T>(Table<T> table, IEnumerable<T> query, DataContext db) where T : class
        {
            while (true)
            {
                Console.WriteLine("What would you like to do with these result(s)? (1,2, or 3)\n 1. Nothing\n 2. Update\n 3. Delete");

                string str = Console.ReadLine().Trim();

                switch (str.First())
                {
                    case '1':
                        Console.WriteLine("Returning to Main Menu...");
                        return;

                    case '2':
                        UpdateRows(table, query, db);
                        return;

                    case '3':
                        DeleteRows(table, query, db);
                        return;

                    default:
                        Console.WriteLine("Unrecognized option.");
                        break;
                }
            }
        }

        private static void UpdateRows<T>(Table<T> table, IEnumerable<T> query, DataContext db) where T : class
        {
            Console.WriteLine("Which columns would you like to update? (comma delimited)");
            string[] columns = getColumnsFromUser();
            retry:
            Console.WriteLine("What values should be entered into those columns? (one per column entered, comma delimited)");
            string[] values = getValuesFromUser(columns);

            foreach (T row in query)
            {
                int i = 0;
                foreach (string column in columns)
                {
                    try
                    {
                        row.GetType().GetProperty(column).SetValue(row, values[i]);
                    }
                    catch (ArgumentException)
                    {
                        int intval;
                        if (!Int32.TryParse(values[i], out intval))
                        {
                            DateTime dateval;
                            if (!DateTime.TryParse(values[i], out dateval)) {
                                Console.WriteLine("The value in column " + column + " must be of the correct type");
                                goto retry;
                            }
                            else
                                row.GetType().GetProperty(column).SetValue(row, dateval);
                        }
                        else
                            row.GetType().GetProperty(column).SetValue(row, intval);
                    }
                    i++;
                }
            }
            try {
                db.SubmitChanges();
                Console.WriteLine("Update submitted!");
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("Unable to submit your change.");
            }
        }

        private static void DeleteRows<T>(Table<T> table, IEnumerable<T> query, DataContext db) where T : class
        {
            while (true)
            {
                Console.WriteLine("Are you sure you wish to delete these rows from the table? (y/n)");
                string str = Console.ReadLine();
                if (str.Equals("y", StringComparison.InvariantCultureIgnoreCase))
                {
                    foreach (T row in query)
                    {
                        table.DeleteOnSubmit(row);
                    }
                    db.SubmitChanges();
                    Console.WriteLine("Deleted record(s)!");
                    return;
                }
                else if (str.Equals("n", StringComparison.InvariantCultureIgnoreCase))
                {
                    Console.WriteLine("Returning to main menu...");
                    return;
                }
                else
                {
                    Console.WriteLine("Unrecognized response.");
                }
            }
        }

        private static string[] getValuesFromUser(string[] columns)
        {
            string[] values;



            while (true)
            {
                string str = Console.ReadLine();
                values = str.Split(',').Select(value => value.Trim()).ToArray();
                if (values.Length != columns.Length)
                {
                    Console.WriteLine("You need one and only one value for every column");
                    goto loop3;
                }
                break;
                loop3:;
            }

            return values;
        }

        private static string[] getOperatorsFromUser(string[] columns)
        {
            string[] operators;

            while (true)
            {
                Console.WriteLine("What operator(s) will you use? i.e. =, >, <=, LIKE (one per column entered, comma delimited)");
                string str = Console.ReadLine();
                operators = str.Split(',').Select(op => op.Trim()).ToArray();

                if (operators.Length != columns.Length)
                {
                    Console.WriteLine("You need one and only one operator for every column");
                    goto loop2;
                }

                foreach (string op in operators)
                {
                    if (!(op == "=" || op == "<>" || op == ">" || op == "<" || op == ">=" ||
                         op == "<=" || op == "LIKE"))
                    {
                        Console.WriteLine("One or more of your operators are invalid");
                        goto loop2;
                    }
                }
                break;
                loop2:;
            }

            return operators;
        }

        private static string[] getColumnsFromUser()
        {
            string[] columns;
            while (true)
            {
                string str = Console.ReadLine();
                columns = str.Split(',').Select(column => column.Trim()).ToArray();
                string[] tableColumns = (from t in Type.GetType("DatabaseEditor.User").GetProperties()
                                         select t.Name).ToArray();
                foreach (string column in columns)
                {
                    if (!(tableColumns.Contains(column)))
                    {
                        Console.WriteLine("One or more of your column names do not exist in the table.");
                        goto loop;
                    }
                }
                break;
                loop:;
            }

            return columns;
        }

        private static void AddRecord(Table<User> Users, DataContext db)
        {
            Console.WriteLine("FirstName: ");
            string firstName = Console.ReadLine();
            Console.WriteLine("LastName: ");
            string lastName = Console.ReadLine();
            Console.WriteLine("UserName: ");
            string userName = Console.ReadLine();
            Console.WriteLine("Email: ");
            string email = Console.ReadLine();

            User person = new User
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = userName,
                Email = email
            };
            Users.InsertOnSubmit(person);

            db.SubmitChanges();
            Console.WriteLine("Submitted a record!");
        }

        private static void OutputTable(Table<User> Users)
        {
            IQueryable query = from user in Users
                               select user;

            foreach (User user in query)
            {
                Console.WriteLine("ID= {0}, FirstName= {1}, LastName= {2}, UserName= {3}, Email= {4}, Created= {5}", user.ID.ToString(), user.FirstName, user.LastName, user.UserName, user.Email, user.Created.ToString());
            }
        }
    }
}
