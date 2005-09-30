using System;
using System.Data;

namespace MaxDBDataProvider
{
	public class MaxDBTransaction : IDbTransaction
	{
		private MaxDBConnection conn;

		internal MaxDBTransaction(MaxDBConnection conn)
		{
			this.conn = conn;
		}

		public IsolationLevel IsolationLevel 
		{
			/*
			 * Should return the current transaction isolation
			 * level. For the template, assume the default
			 * which is ReadCommitted.
			 */
			get 
			{ 
				switch (SQLDBC.SQLDBC_Connection_getTransactionIsolation(conn.connHandler))
				{
					case 0:
						return IsolationLevel.ReadUncommitted;
					case 1: case 10:
						return IsolationLevel.ReadCommitted;
					case 2: case 20:
						return IsolationLevel.RepeatableRead;
					case 3: case 30:
						return IsolationLevel.Serializable;
					default:
						return IsolationLevel.Unspecified;
				}
			}
		}

		public void Commit()
		{
			/*
			 * Implement Commit here. Although the template does
			 * not provide an implementation, it should never be 
			 * a no-op because data corruption could result.
			 */
			int rc = SQLDBC.SQLDBC_Connection_commit(conn.connHandler);
			if(rc != 0) 
			{
				IntPtr herror = SQLDBC.SQLDBC_Connection_getError(conn.connHandler);
				throw new MaxDBException("COMMIT failed " + SQLDBC.SQLDBC_ErrorHndl_getErrorText(herror));
			}
		}

		public void Rollback()
		{
			/*
			 * Implement Rollback here. Although the template does
			 * not provide an implementation, it should never be
			 * a no-op because data corruption could result.
			 */
			int rc = SQLDBC.SQLDBC_Connection_rollback(conn.connHandler);
			if(rc != 0) 
			{
				IntPtr herror = SQLDBC.SQLDBC_Connection_getError(conn.connHandler);
				throw new MaxDBException("ROLLBACK failed " + SQLDBC.SQLDBC_ErrorHndl_getErrorText(herror));
			}
		}

		public IDbConnection Connection
		{
			/*
			 * Return the connection for the current transaction.
			 */

			get  { return this.conn; }
		}    

		public void Dispose() 
		{
			this.Dispose(true);
			System.GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing) 
		{
			if (disposing) 
			{
				if (null != this.Connection) 
				{
					// implicitly rollback if transaction still valid
					this.Rollback();
				}                
			}
		}

	}
}
