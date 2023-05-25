using MySqlConnector;
using System.Windows;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Xml.Linq;
using System.Collections;
using System.Net.NetworkInformation;
using System.Diagnostics.Metrics;
using System.Security.Principal;

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
        private string table; //No longer needed since there are multiple tables in the DB now

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initializes Values
        private void Initialize()
        {
            server = "localhost"; //Since the Bot will be run on the same system, this way will work 
            uid = "Bot"; //Will eventually make these values tokens for security purposes
            password = "cosmicBot123";
            database = "tcn";
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

        //Custom Statements here for calling
        public string pinger()
        {
            string query = "CALL pinging();";
            string dbPing;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader;

                dataReader = cmd.ExecuteReader(0);
                while (dataReader.Read())
                {
                    dbPing = dataReader.GetString(0);
                    this.CloseConnection();
                    return dbPing;
                }
                dbPing = "No Connection Please try again Later or contact your admin";
                return dbPing;
            }
            else
            {
                dbPing = "No Connection Please try again Later or contact your admin";
                return dbPing;
            }
        }

        /*UserInsert should only be used when a new member goes to create their first nation, and if they are not
         * in the the DB already, this function goes off and adds them
         */
        public void UserInsert(string nt, ulong dID)
        {
            string query = $"INSERT INTO users(nameTag, discordID) VALUES('{nt}', '{dID}');";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
            else
            {
                Console.WriteLine("Error, could not open a connection to the database");
            }
        }

        public int IsUser(ulong uid) //0 is true, 1 is false, anything else is a DB connection error 
        {
            string query = $"CALL isUser({uid});";
            long isUser;
            string errorMessage;
            int returnVal = 2;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader;

                dataReader = cmd.ExecuteReader(0);
                while (dataReader.Read())
                {
                    isUser = dataReader.GetInt64(0);
                    this.CloseConnection();
                    if (isUser == 0)
                    {
                        returnVal = 0;
                        return returnVal;
                    }
                    else if (isUser == 1)
                    {
                        returnVal = 1;
                        return returnVal;
                    }
                    else
                    {
                        return returnVal; //Returns default value of 2, which is a error 
                    }
                }
                errorMessage = "No Connection Please try again Later or contact your admin, return value 2";
                Console.WriteLine(errorMessage);
                return 2;
            }
            else
            {
                errorMessage = "No Connection Please try again Later or contact your admin, return value 3";
                return 3;
            }
        }

        public int GetUserDBID(ulong uid)
        {
            int userDBID; //The Users DB ID 
            string query = $"SELECT userID FROM users WHERE discordID = {uid};";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader;
                dataReader = cmd.ExecuteReader(0);
                while (dataReader.Read())
                {
                    userDBID = dataReader.GetInt32(0);
                    this.CloseConnection();
                    return userDBID;
                }
                return 2;
            }
            else
            {
                return 2;
            }
        }

        public void NationInsert(string natName, ulong uid, int userDBID)
        {
            string query = $"INSERT INTO nations (userID, nationName, dateFounded, techID) " +
                           $"VALUES ({userDBID}, '{natName}', CURDATE(), 1);";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
            else
            {
                Console.WriteLine("Error, could not open a connection to the database for nation insert");
            }
        }

        //This function will add a nation to a user who creates it,
        //and if the user does not exist it will create a user aswell
        public void CreateNationInsert(string natName, ulong uid, string nt)
        {
            //First verify if they are a user or not
            //0 is true, 1 is false, anything else is a DB connection error 
            int isUser = IsUser(uid);
            if (isUser == 0) //If they are a user
            {
                int userDBID = GetUserDBID(uid); //The Users DB ID
                if (userDBID == 0) { Console.WriteLine("Error with getting the user DBID"); }
                else
                {
                    NationInsert(natName, uid, userDBID);
                }
            }
            else if (isUser == 1) //If they are not a user
            {
                UserInsert(nt, uid);
                int userDBID = GetUserDBID(uid); //The Users DB ID
                if (userDBID == 0)
                {
                    Console.WriteLine("Error with getting the user DBID");
                }
                else
                {
                    NationInsert(natName, uid, userDBID);
                }
            }
            else
            {
                Console.WriteLine("Error, Issue with createNationInsert Function");
            }
        }

        //Renaming a Nation starts here
        // 0 it is a nation, 1 it is not a nation, anything else is a error
        public int IsNation(string natName) //Checks if the nation exists
        {
            string query = $"CALL isNation('{natName}');";
            if (this.OpenConnection() == true)
            {
                long isNation;
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader;
                dataReader = cmd.ExecuteReader(0);
                while (dataReader.Read())
                {
                    isNation = dataReader.GetInt64(0);
                    this.CloseConnection();
                    //Console.WriteLine(isNation); //testing purposes only
                    return ((int)isNation);
                }
                return 1;
            }
            else
            {
                Console.WriteLine("Error, could not open a connection to the database to check if its a nation");
                return 1;
            }
        }
        public bool IsUsersNation(string natName, ulong uid)
        {
            int isNation = IsNation(natName);
            int userDB = GetUserDBID(uid);
            // 0 true, 1 False
            string query = $"SELECT IF(ISNULL((SELECT nationName FROM nations WHERE nationname = '{natName}' AND userID = {userDB})), 1, 0);";
            if (isNation == 0)
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader dataReader;
                    dataReader = cmd.ExecuteReader(0);
                    while (dataReader.Read())
                    {
                        int controlled = dataReader.GetInt32(0);
                        this.CloseConnection();
                        if (controlled == 0) { return true; }
                        else { return false; }
                    }
                    return false;
                }
                return false;
            }
            return false;
        }

        public int GetNationDBID(string natNM)
        {
            int natDBID; //The Nations DB ID 
            string query = $"SELECT nationID FROM nations WHERE nationName = '{natNM}' LIMIT 1;";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader;
                dataReader = cmd.ExecuteReader(0);
                while (dataReader.Read())
                {
                    natDBID = dataReader.GetInt32(0);
                    this.CloseConnection();
                    return natDBID;
                }
                return 0;
            }
            else
            {
                return 0;
            }
        }

        public bool CheckNationUpdate(string newNM, int userDBID)
        {
            string query = $"SELECT nationName FROM nations WHERE nationName = '{newNM}' AND  userID = {userDBID} LIMIT 1;";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader;
                dataReader = cmd.ExecuteReader(0);
                while (dataReader.Read())
                {
                    var curName = dataReader.GetString(0);
                    this.CloseConnection();
                    if (curName == newNM)
                    {
                        return true; //Returns true only when the current name in the database matches the new nations name
                    }
                    else
                    {
                        return false;
                    }
                }
                this.CloseConnection();
                return false;
            }
            this.CloseConnection();
            return false;
        }
        public void RenameNationUpdate(string oldNM, string newNM, int userDBID) //actually renames the nation
        {
            int nationDBID = GetNationDBID(oldNM);
            string query = $"UPDATE nations SET nationName = '{newNM}' WHERE (nationID = {nationDBID} AND userID = {userDBID});";
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                this.CloseConnection();
            }
            else
            {
                Console.WriteLine("Error, could not open a connection to the database for rename nation");
            }
        }
        public bool RenameNation(string oldNM, string newNM, ulong uid, string nt)
        {
            //First Check if they are a user
            int isUser = IsUser(uid);
            if (isUser == 0) //If they are a user
            {
                int userDBID = GetUserDBID(uid); //The Users DB ID
                if (userDBID == 0)
                {
                    Console.WriteLine("Error with getting the user DBID");
                    return false;
                }
                else
                { //The only path that matters here, the rest will return false as a
                  //nation would not have been made as a user would not have existed to create it
                    int isNation = IsNation(oldNM);
                    if (isNation == 0)
                    {
                        RenameNationUpdate(oldNM, newNM, userDBID);
                        bool check = CheckNationUpdate(newNM, userDBID);
                        if(check == true){ return true; }
                        else { return false; }
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else if (isUser == 1) //If they are not a user
            {
                UserInsert(nt, uid);
                int userDBID = GetUserDBID(uid); //The Users DB ID
                if (userDBID == 0)
                {
                    Console.WriteLine("Error with getting the user DBID");
                    return false;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Console.WriteLine("Error, Issue with RenameNation Function");
                return false;
            }
        }

        //Listing a users Nations here
        public int GetNationCount(ulong uid)
        {
            int userDBID = GetUserDBID(uid);
            string query = $"SELECT COUNT(nationName) FROM nations WHERE userID = {userDBID};";
            int natCount;
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader;
                dataReader = cmd.ExecuteReader(0);
                while (dataReader.Read())
                {
                    natCount = dataReader.GetInt32(0);
                    this.CloseConnection();
                    return natCount;
                }
                return 0;
            }
            return 0;
        }
        public string[] GetUsersNations(ulong uid)
        {
            int userDBID = GetUserDBID(uid);
            string query = $"SELECT nationName FROM nations WHERE userID = {userDBID};";
            int nationCount = GetNationCount(uid);
            string[] nations = new string[nationCount];
            if (this.OpenConnection() == true)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader;
                dataReader = cmd.ExecuteReader(0);
                int num = 0;
                while (dataReader.Read())
                {
                    nations[num] = dataReader.GetString(0);
                    num++;
                }
                this.CloseConnection();
                return nations;
            }
            else
            {
                nations[1] = "No Connection";
                return nations;
            }
        }

        public int GetNationsUserDBID(string nation)
        {
            string query = $"SELECT userID FROM nations WHERE nationName = '{nation}';";
            int userDBID;
            try
            {
                if (this.OpenConnection() == true)
                {
                    MySqlCommand cmd = new MySqlCommand(query, connection);
                    MySqlDataReader dataReader;
                    dataReader = cmd.ExecuteReader(0);
                    while (dataReader.Read())
                    {
                        userDBID = dataReader.GetInt32(0);
                        this.CloseConnection();
                        return userDBID;
                    }
                    this.CloseConnection();
                    return 0;
                }
                return 0;
            }
            catch { return 0; }
        }

        public ulong GetNationsUser(string nation)
        {
            ulong zero = 0; //Purely to send a ulong 0 for incase a error happens
            int userDBID = GetNationsUserDBID(nation);
            ulong discordID;
            string query = $"SELECT discordID FROM users WHERE userID = {userDBID};";
            if (this.OpenConnection() == true && userDBID != 0)
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                MySqlDataReader dataReader;
                dataReader = cmd.ExecuteReader(0);
                while (dataReader.Read())
                {
                    discordID = dataReader.GetUInt64(0);
                    this.CloseConnection();
                    return discordID;
                }
                this.CloseConnection();
                return zero;
            }
            this.CloseConnection();
            return zero;
        }




        // These statements will be removed once I am finished with this class
        //Insert statement
        public void Insert()
        {
            string query = "INSERT INTO (`name`, species, age) VALUES('Roxxane', 'c', 4)";
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
