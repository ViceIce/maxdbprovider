using System;
using System.Data;
using MaxDBDataProvider.MaxDBProtocol;

namespace MaxDBDataProvider
{
	/// <summary>
	/// Summary description for MaxDBException.
	/// </summary>
	public class MaxDBException : SystemException
	{
		private int m_detailErrorCode = -708;

		public MaxDBException() : base()
		{
		}

		public MaxDBException(string message) : base(message)
		{
		}

		public MaxDBException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public MaxDBException(string message, int rc, Exception innerException) : base(message, innerException)
		{
			m_detailErrorCode = rc;
		}

		public int DetailErrorCode
		{
			get
			{
				return m_detailErrorCode;
			}
		}
	}

	public class PartNotFound : Exception 
	{
		public PartNotFound() : base() 
		{
		}
	}

	public class MaxDBSQLException : DataException
	{
		private int m_errPos = -10899;
		private string m_sqlState;
		private int m_vendorCode;

		public MaxDBSQLException() : base()
		{
		}

		public MaxDBSQLException(string message) : base(message)
		{
		}

		public MaxDBSQLException(string message, string sqlState) : base(message)
		{
			m_sqlState = sqlState;
		}

		public MaxDBSQLException(string message, string sqlState, int vendorCode) : base(message)
		{
			m_sqlState = sqlState;
			m_vendorCode = vendorCode;
		}

		public MaxDBSQLException(string message, string sqlState, int vendorCode, int errpos) : base(message)
		{
			m_sqlState = sqlState;
			m_vendorCode = vendorCode;
			m_errPos = errpos;
		}

		public int VendorCode
		{
			get
			{
				return m_vendorCode;
			}
		}

		public int ErrorPos
		{
			get
			{
				return m_errPos;
			}
		}

		public virtual bool isConnectionReleasing
		{
			get
			{
				switch(m_vendorCode) 
				{
					case -904:  // Space for result tables exhausted
					case -708:  // SERVERDB system not available
					case +700:  // Session inactivity timeout (work rolled back)
					case -70:   // Session inactivity timeout (work rolled back)
					case +710:  // Session terminated by shutdown (work rolled back)
					case -71:   // Session terminated by shutdown (work rolled back)
					case +750:  // Too many SQL statements (work rolled back)
					case -75:   // Too many SQL statements (work rolled back)
						return true;
				}
				return false;
			}
		}
	}

	public class DatabaseException : MaxDBSQLException 
	{
		public DatabaseException(string message, string sqlState, int vendorCode, int errpos) : base((errpos > 1) 
			? MessageTranslator.Translate(MessageKey.ERROR_DATABASEEXCEPTION, vendorCode.ToString(), errpos.ToString(), message)
			: MessageTranslator.Translate(MessageKey.ERROR_DATABASEEXCEPTION_WOERRPOS, vendorCode.ToString(), message),
			sqlState, vendorCode, errpos)
		{     
		}
	}

	public class ConnectionException : MaxDBSQLException 
	{
		public ConnectionException(MaxDBException ex) : base("[" + ex.DetailErrorCode + "] " + ex.Message, "08000", ex.DetailErrorCode, 0)
		{
		}
    
		public ConnectionException(MaxDBSQLException ex) : base(ex.Message, "08000", ex.VendorCode) 
		{
		}

		public override bool isConnectionReleasing
		{
			get
			{
				return true;
			}
		}
	}

	public class TimeoutException : DatabaseException
	{
		public TimeoutException() : base(MessageTranslator.Translate(MessageKey.ERROR_TIMEOUT), "08000", 700, 0)
		{
		}

		public override bool isConnectionReleasing
		{
			get
			{
				return true;
			}
		}
	}

}
