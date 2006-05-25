using System;
using NUnit.Framework;
using MaxDBDataProvider;
using System.Data;
using System.Data.Common;

namespace MaxDBConsole.UnitTesting
{
	/// <summary>
	/// Summary description for DataReaderTests.
	/// </summary>
	[TestFixture()]
	public class DataReaderTests : BaseTest
	{
		public DataReaderTests()
		{
			//
			// TODO: Add constructor logic here
			//
		}

		[TestFixtureSetUp]
		public void Init() 
		{
			Init("CREATE TABLE Test (id INT NOT NULL, name VARCHAR(100), d DATE, t TIME, dt TIMESTAMP, b1 LONG BYTE, c1 LONG ASCII, PRIMARY KEY(id))");
		}

		[TestFixtureTearDown]
		public void TestFixtureTearDown()
		{
			Close();
		}

		[Test]
		public void TestResultSet()
		{
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();

				MaxDBCommand cmd = new MaxDBCommand(string.Empty, m_conn);
				
				// insert 100 records
				cmd.CommandText = "INSERT INTO Test (id, name) VALUES (:id, 'test')";
				cmd.Parameters.Add(new MaxDBParameter(":id", 1));
				for (int i = 1; i <= 100; i++)
				{
					cmd.Parameters[0].Value = i;
					cmd.ExecuteNonQuery();
				}
				
				cmd = new MaxDBCommand("SELECT * FROM Test WHERE id >= 50", m_conn);

				// execute it one time
				reader = cmd.ExecuteReader();
				Assert.IsNotNull(reader);
				while(reader.Read());
				Assert.IsTrue(reader.HasRows);
				Assert.AreEqual(7, reader.FieldCount);
				reader.Close();

				// execute it again
				reader = cmd.ExecuteReader();
				Assert.IsNotNull(reader);
				while(reader.Read());
				Assert.IsTrue(reader.HasRows);
				Assert.AreEqual(7, reader.FieldCount);
				reader.Close();
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test()]
		public void TestNotReadingResultSet()
		{
			try
			{
				ClearTestTable();

				for (int x=0; x < 10; x++)
				{
					MaxDBCommand cmd = new MaxDBCommand("INSERT INTO Test (id, name, b1) VALUES(:val, 'Test', NULL)", m_conn);
					cmd.Parameters.Add(new MaxDBParameter(":val", x));
					int affected = cmd.ExecuteNonQuery();
					Assert.AreEqual(1, affected );

					cmd = new MaxDBCommand("SELECT * FROM Test", m_conn);
					cmd.ExecuteReader().Close();
				}
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		[Test]
		public void TestSingleRowBehavior()
		{
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();

				MaxDBCommand cmd = new MaxDBCommand(string.Empty, m_conn);
				
				cmd.CommandText = "INSERT INTO Test(id,name) VALUES(1,'test1')";
				cmd.ExecuteNonQuery();
				cmd.CommandText = "INSERT INTO Test(id,name) VALUES(2,'test2')";
				cmd.ExecuteNonQuery();
				cmd.CommandText = "INSERT INTO Test(id,name) VALUES(3,'test3')";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT * FROM Test";
				reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
				Assert.IsTrue(reader.Read(), "First read");
				Assert.IsFalse(reader.Read(), "Second read");
				reader.Close();

				cmd.CommandText = "SELECT * FROM test WHERE id = 1";
				reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
				Assert.IsTrue(reader.Read());
				Assert.AreEqual("test1", reader.GetString(1).Trim());
				Assert.IsFalse(reader.Read());
				reader.Close();

				cmd.CommandText = "SELECT * FROM test WHERE rowno <= 2";
				reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
				Assert.IsTrue(reader.Read());
				Assert.AreEqual("test1", reader.GetString(1).Trim());
				Assert.IsFalse(reader.Read());
				reader.Close();
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test()]
		public void TestCloseConnectionBehavior() 
		{
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();

				MaxDBCommand cmd = new MaxDBCommand(String.Empty, m_conn);

				cmd.CommandText = "INSERT INTO Test(id, name) VALUES(1, 'test')";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT * FROM Test";

				reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
				Assert.IsTrue(reader.Read());
				reader.Close();
				reader = null;
				Assert.IsTrue(m_conn.State == ConnectionState.Closed);

				m_conn.Open();
			}
			catch (Exception ex) 
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test]
		public void TestSchemaOnlyBehavior() 
		{
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();

				MaxDBCommand cmd = new MaxDBCommand(string.Empty, m_conn);
				
				cmd.CommandText = "INSERT INTO Test(id, name) VALUES(1, 'test1')";
				cmd.ExecuteNonQuery();
				cmd.CommandText = "INSERT INTO Test(id, name) VALUES(2, 'test2')";
				cmd.ExecuteNonQuery();
				cmd.CommandText = "INSERT INTO Test(id, name) VALUES(3, 'test3')";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT * FROM Test";
				reader = cmd.ExecuteReader(CommandBehavior.SchemaOnly);
				DataTable table = reader.GetSchemaTable();
				Assert.AreEqual(7, table.Rows.Count);
				Assert.AreEqual(13, table.Columns.Count);
				Assert.IsFalse(reader.Read());
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test()]
		public void TestDBNulls() 
		{
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();

				MaxDBCommand cmd = new MaxDBCommand(string.Empty, m_conn);
				
				cmd.CommandText = "INSERT INTO Test(id, name) VALUES(1, 'Test')";
				cmd.ExecuteNonQuery();
				cmd.CommandText = "INSERT INTO Test(id, name) VALUES(2, :null)";
				cmd.Parameters.Add(":null", MaxDBType.VarCharA).Value = DBNull.Value;
				cmd.ExecuteNonQuery();
				cmd.CommandText = "INSERT INTO Test(id, name) VALUES(3, 'Test2')";
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT * FROM Test";

				reader = cmd.ExecuteReader();
				reader.Read();
				Assert.AreEqual(1, reader.GetValue(0));
				Assert.AreEqual(1, reader.GetInt32(0));
				Assert.AreEqual("Test", reader.GetValue(1).ToString().Trim());
				Assert.AreEqual("Test", reader.GetString(1).Trim());
				reader.Read();
				Assert.AreEqual(2, reader.GetValue(0));
				Assert.AreEqual(2, reader.GetInt32(0));
				Assert.AreEqual(DBNull.Value, reader.GetValue(1));
				Assert.AreEqual(null, reader.GetString(1));
				reader.Read();
				Assert.AreEqual(3, reader.GetValue(0));
				Assert.AreEqual(3, reader.GetInt32(0));
				Assert.AreEqual("Test2", reader.GetValue(1).ToString().Trim());
				Assert.AreEqual("Test2", reader.GetString(1).Trim());
				Assert.IsFalse(reader.Read());
			}
			catch (Exception ex) 
			{
				Assert.Fail( ex.Message );
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test]
		public void TestGetByte() 
		{
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();
				MaxDBCommand cmd = new MaxDBCommand("INSERT INTO Test (id, name) VALUES (123, 'a')", m_conn);
				cmd.ExecuteNonQuery();

				cmd = new MaxDBCommand("SELECT * FROM Test", m_conn);
				reader = cmd.ExecuteReader();
				reader.Read();
				Assert.AreEqual(123, reader.GetByte(0));
				Assert.AreEqual(97, reader.GetByte(1));
			}
			catch (Exception ex) 
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test()]
		public void TestGetBytes()
		{
			int len = 50000;
			byte[] bytes = new byte[len];
			for (int i = 0; i < len; i++)
				bytes[i] = (byte)(i % 0xFF);
			
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();

				MaxDBCommand cmd = new MaxDBCommand("INSERT INTO Test (id, name, b1) VALUES(1, :t, :b1)", m_conn);
				cmd.Parameters.Add(":t", "Test");
				cmd.Parameters.Add(":b1", MaxDBType.LongB).Value = bytes;
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT * FROM Test";

				//  now check with sequential access
				reader = cmd.ExecuteReader();
				Assert.IsTrue(reader.Read());
				int mylen = len;
				int startIndex = 0;
				byte[] buff = new byte[8192];
				while (mylen > 0)
				{
					int readLen = Math.Min(mylen, buff.Length);
					int retVal = (int)reader.GetBytes(5, startIndex, buff, 0, readLen);
					Assert.AreEqual(readLen, retVal);
					for (int i = 0; i < readLen; i++)
						Assert.AreEqual(bytes[startIndex+i], buff[i]);
					startIndex += readLen;
					mylen -= readLen;
				}
			}
			catch (Exception ex) 
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test]
		public void TestGetChar() 
		{
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();
				MaxDBCommand cmd = new MaxDBCommand("INSERT INTO Test (id, name) VALUES (1, 'a')", m_conn);
				cmd.ExecuteNonQuery();

				cmd = new MaxDBCommand("SELECT * FROM Test", m_conn);
				reader = cmd.ExecuteReader();
				reader.Read();
				Assert.AreEqual('a', reader.GetChar(1));
			}
			catch (Exception ex) 
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test()]
		public void TestGetChars()
		{
			int len = 20000;
			char[] chars = new char[len];
			for (int i = 0; i < len; i++)
				chars[i] = 'a';//(char)(i % 128);
			
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();

				MaxDBCommand cmd = new MaxDBCommand("INSERT INTO Test (id, name, c1) VALUES(1, :t, :c1)", m_conn);
				cmd.Parameters.Add(":t", "Test");
				cmd.Parameters.Add(":c1", MaxDBType.LongA).Value = chars;
				cmd.ExecuteNonQuery();

				cmd.CommandText = "SELECT * FROM Test";

				// now check with sequential access
				reader = cmd.ExecuteReader();
				Assert.IsTrue(reader.Read());
				int mylen = len;
				int startIndex = 0;
				char[] buff = new char[8192];

				while (mylen > 0)
				{
					int readLen = Math.Min(mylen, buff.Length);
					int retVal = (int)reader.GetChars(6, startIndex, buff, 0, readLen);
					Assert.AreEqual(readLen, retVal);
					for (int i = 0; i < readLen; i++)
						Assert.AreEqual(chars[startIndex + i], buff[i]);
					startIndex += readLen;
					mylen -= readLen;
				}
			}
			catch (Exception ex) 
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test]
		public void TestTextFields() 
		{
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();
				MaxDBCommand cmd = new MaxDBCommand("INSERT INTO Test (id, name) VALUES (1, 'Text value')", m_conn);
				cmd.ExecuteNonQuery();

				cmd = new MaxDBCommand("SELECT * FROM Test", m_conn);
				reader = cmd.ExecuteReader();
				reader.Read();
				Assert.AreEqual( "Text value", reader["name"].ToString().Trim());
			}
			catch (Exception ex) 
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test]
		public void TestDateAndTimeFields() 
		{
			MaxDBDataReader reader = null;
			try 
			{
				ClearTestTable();

				DateTime dt = DateTime.Now;

				MaxDBCommand cmd = new MaxDBCommand("INSERT INTO Test (id, d, t, dt) VALUES (1, :a, :b, :c)", m_conn);
				cmd.Parameters.Add(new MaxDBParameter(":a", MaxDBType.Date)).Value = dt.Date;
				cmd.Parameters.Add(new MaxDBParameter(":b", MaxDBType.Time)).Value = dt.TimeOfDay;
				cmd.Parameters.Add(new MaxDBParameter(":c", MaxDBType.TimeStamp)).Value = dt;
				cmd.ExecuteNonQuery();

				cmd = new MaxDBCommand("SELECT * FROM Test", m_conn);
				reader = cmd.ExecuteReader();
				reader.Read();

				long period = TimeSpan.TicksPerMillisecond / 1000;

				Assert.AreEqual(dt.Date, reader["d"]);
				Assert.AreEqual(dt.TimeOfDay.Ticks / TimeSpan.TicksPerSecond, ((DateTime)reader["t"]).Ticks / TimeSpan.TicksPerSecond);
				Assert.AreEqual(dt.Ticks / period, ((DateTime)reader["dt"]).Ticks / period);
			}
			catch (Exception ex) 
			{
				Assert.Fail(ex.Message);
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		[Test]
		[ExpectedException(typeof(MaxDBException))]
		public void TestReadingBeforeRead() 
		{
			MaxDBDataReader reader = null;
			try 
			{
				MaxDBCommand cmd = new MaxDBCommand("SELECT * FROM Test", m_conn);
				reader = cmd.ExecuteReader();
				reader.GetInt32(0);
			}
			catch (Exception) 
			{
				throw;
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}
	}
}