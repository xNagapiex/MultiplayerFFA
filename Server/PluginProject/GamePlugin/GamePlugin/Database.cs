using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.IO;

namespace GamePlugin
{
    class Database
    {
        public SQLiteConnection myConnection;

        public Database()
        {
            myConnection = new SQLiteConnection("Data Source=GameDB.db");
            myConnection.Open();
        }

        //public void OpenConnection()
        //{
        //    if (myConnection.State != System.Data.ConnectionState.Open)
        //    {
        //        myConnection.Open();
        //    }
        //}

        //public void CloseConnection()
        //{
        //    if (myConnection.State != System.Data.ConnectionState.Closed)
        //    {
        //        myConnection.Close();
        //    }
        //}

        ~Database()
        {
            myConnection.Close();
        }
    }
}