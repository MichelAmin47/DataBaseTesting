using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Xml;

namespace SQLTest.Tests
{
    [TestClass]
    public class CheckSQLDB
    {

        public string GetCredentionals(string databaseType, string cred)
        {
            string reqCred = null;
            XmlReader xmlReader = XmlReader.Create("Helpers/properties.xml");

            while (xmlReader.Read())
            {
                if ((xmlReader.NodeType == XmlNodeType.Element) && (xmlReader.Name == databaseType + "creds"))
                {
                    if (xmlReader.HasAttributes)
                    {
                        //Console.WriteLine(xmlReader.GetAttribute(cred));
                        reqCred = xmlReader.GetAttribute(cred);
                    }
                }
            }
            return reqCred;
        }

        [TestMethod]
        public void RetrieveXMLData()
        {
            Console.WriteLine(GetCredentionals("MSSQL", "userid")); 
            Console.WriteLine(GetCredentionals("MSSQL", "password"));
            Console.WriteLine(GetCredentionals("MSSQL", "server"));
        }


        [TestMethod]
        public void PingMsSQLAmazonRDS()
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "user id=" + GetCredentionals("MSSQL", "userid") + ";" +
                                       "password=" + GetCredentionals("MSSQL", "password") + ";" +
                                       "server=" + GetCredentionals("MSSQL", "server") + ";" +
                                       //"Trusted_Connection=yes;" +
                                       "database=TestDB; " +
                                       "connection timeout=30";
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand("SELECT 1", sqlConnection);
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                //Console.WriteLine(sqlDataReader.FieldCount);
                int fieldCount = sqlDataReader.FieldCount;
                //Console.WriteLine(sqlDataReader.HasRows);
                bool dbHasContent = sqlDataReader.HasRows;
                sqlDataReader.Close();

                Assert.IsTrue((dbHasContent && !(fieldCount.Equals(0)) ), "SQL query retrieving all returned empty");
            }
        }

        [TestMethod]
        public void RetrieveMsSQLAmazonRDSData()
        {
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = "user id=" + GetCredentionals("MSSQL", "userid") + ";" +
                                       "password=" + GetCredentionals("MSSQL", "password") + ";" +
                                       "server=" + GetCredentionals("MSSQL", "server") + ";" +
                                       //"Trusted_Connection=yes;" +
                                       "database=TestDB; " +
                                       "connection timeout=30";
                sqlConnection.Open();

                try
                {
                    SqlCommand sqlCommand = new SqlCommand("select * from Products", sqlConnection);
                    SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                    /*while (myReader.Read())
                    {
                        Console.WriteLine(myReader["ProductID"].ToString());
                        Console.WriteLine(myReader["ProductName"].ToString());
                        Console.WriteLine(myReader["Price"].ToString());
                        Console.WriteLine(myReader["ProductDescription"].ToString());
                    }*/

                    while (sqlDataReader.HasRows)
                    {
                        Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", sqlDataReader.GetName(0),
                            sqlDataReader.GetName(1),
                            sqlDataReader.GetName(2),
                            sqlDataReader.GetName(3));

                        while (sqlDataReader.Read())
                        {
                            Console.WriteLine("\t{0}\t{1}\t{2}\t{3}", sqlDataReader.GetInt32(0),
                                sqlDataReader.GetString(1),
                                sqlDataReader.GetSqlMoney(2),
                                sqlDataReader.GetString(3));
                        }
                        sqlDataReader.NextResult();
                    }

                    /*DataTable schemaTable = myReader.GetSchemaTable();

                    foreach (DataRow row in schemaTable.Rows)
                    {
                        foreach (DataColumn column in schemaTable.Columns)
                        {
                            Console.WriteLine(String.Format("{0} = {1}",
                               column.ColumnName, row[column]));
                        }
                    }*/
                    sqlDataReader.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }

        [TestMethod]
        public void RetrieveOracleAmazonRDSData()
        {
            using (OracleConnection oracleConnection = new OracleConnection())
            {
                OracleConnectionStringBuilder oracleConnectionStringBuilder = new OracleConnectionStringBuilder();
                oracleConnectionStringBuilder.Password = GetCredentionals("Oracle", "password");
                oracleConnectionStringBuilder.UserID = GetCredentionals("Oracle", "userid");
                oracleConnectionStringBuilder.DataSource = GetCredentionals("Oracle", "server");

                oracleConnection.ConnectionString = oracleConnectionStringBuilder.ConnectionString;
                oracleConnection.Open();
                Console.WriteLine("Connection established (" + oracleConnection.ServerVersion + ")");

                try
                {
                    string cmdQuery = "select * from PRODUCT";

                    // Create the OracleCommand
                    OracleCommand oracleCommand = new OracleCommand(cmdQuery);

                    oracleCommand.Connection = oracleConnection;
                    oracleCommand.CommandType = CommandType.Text;

                    // Execute command, create OracleDataReader object
                    OracleDataReader oracleDataReaderReader = oracleCommand.ExecuteReader();

                    while (oracleDataReaderReader.Read())
                    {
                        if (!oracleDataReaderReader.IsDBNull(0))
                        {
                            Console.WriteLine(oracleDataReaderReader.GetString(0));
                        }
                    }
                    oracleCommand.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}
