using MySqlConnector;
using System.Windows;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 This Class will hold the queries required for the Database
May need to Re-write in a new class in case this doesnt work due to being made originally in a Form
*/


namespace DatabaseConnectionClass
{
    class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string uid;
        private string password;
        private string database;
        private string table;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initializes Values
        private void Initialize()
        {
            server = "localhost"; //Since the Bot will be run on the same system, this way will work 
            uid = "testing";
            password = "testing123";
            database = "animals";
            table = "pets";
            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";"
                                + "UID=" + uid + ";" + "PASSWORD=" + password + ";";
            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try



            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.Write("Cannot Connect to Database Server. Contact Admin");
                        break;
                    case 1045:
                        Console.Write("Invalid username/password for Database, Please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.Write(ex.Message);
                return false;
            }
        }

        //Insert statement
        public void Insert()
        {
            string query = "INSERT INTO " + table + "(`name`, species, age) VALUES('Roxxane', 'c', 4)";
            //Open Connection
            if (this.OpenConnection() == true)
            {
                //create command and assign the query and connectiojn from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //execute command 
                cmd.ExecuteNonQuery();
                //close connection
                this.CloseConnection();
            }
        }

        //Update statement
        public void Update()
        {
            string query = "UPDATE " + table + " SET species = 'D' WHERE id = 4";
            //Opens Connection
            if (this.OpenConnection() == true)
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //assign the query usign command text
                cmd.CommandText = query;
                //assign the connection using connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();
                //Close connection
                this.CloseConnection();
            }
        }

        //Delete statement
        public void Delete()
        {
            string query = "DELETE FROM " + table + " WHERE `name` = 'Delete Me'";

            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
        }

        //Select statement
        public List<string>[] Select()
        {
            string query = "SELECT * FROM " + table;
            //Create a List to store the result
            List<string>[] list = new List<string>[5];
            list[0] = new List<string>(); //Pet Id
            list[1] = new List<string>(); //Pet Name
            list[2] = new List<string>(); //Pet Species
            list[3] = new List<string>(); //Pet Age
            list[4] = new List<string>(); //Entry Date 

            //open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data readeer and execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    list[0].Add(dataReader["id"] + "");
                    list[1].Add(dataReader["Name"] + "");
                    list[2].Add(dataReader["Species"] + "");
                    list[3].Add(dataReader["Age"] + "");
                    list[4].Add(dataReader["Created_Date"] + "");
                }

                //Clsoe Data reader
                dataReader.Close();
                //Close Connection
                this.CloseConnection();

                //return list to be displayed
                return list;
            }
            else
            {
                return list;
            }
        }

        //Count statement
        public int Count()
        {
            string query = "SELECT Count(*) FROM " + table;
            int Count = -1;

            //Open Connection
            if (this.OpenConnection() == true)
            {
                //Create MySql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //ExecuteScaler will return one Value
                Count = int.Parse(cmd.ExecuteScalar() + "");

                //Close Connection
                this.CloseConnection();

                return Count;
            }
            else
            {
                return Count;
            }
        }

        //Backup
        public void Backup()
        {
        }

        //Restore
        public void Restore()
        {
        }

    }
}
