//	Copyright (C) 2005-2006 Dmitry S. Kataev
//
//	This program is free software; you can redistribute it and/or
//	modify it under the terms of the GNU General Public License
//	as published by the Free Software Foundation; either version 2
//	of the License, or (at your option) any later version.
//
//	This program is distributed in the hope that it will be useful,
//	but WITHOUT ANY WARRANTY; without even the implied warranty of
//	MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//	GNU General Public License for more details.
//
//	You should have received a copy of the GNU General Public License
//	along with this program; if not, write to the Free Software
//	Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.

using System;
using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Data.Common;

namespace MaxDB.Data
{
	/// <summary>
	/// Represents a collection of parameters relevant to a <see cref="MaxDBCommand"/> as 
	/// well as their respective mappings to columns in a <see cref="DataSet"/>. This class cannot be inherited.
	/// </summary>
	public sealed class MaxDBParameterCollection : DbParameterCollection, IDataParameterCollection, ICloneable
	{
		private MaxDBParameter[] mCollection = new MaxDBParameter[0];

		#region "IDataParameterCollection implementation"

		object IDataParameterCollection.this[string parameterName]
		{
			get
			{
				return this[IndexOf(parameterName)];
			}
			set
			{
				this[IndexOf(parameterName)] = (MaxDBParameter)value;
			}
		}

		/// <summary>
		/// Gets a value indicating whether a MaxDBParameter exists in the collection.
		/// </summary>
		/// <param name="parameterName">The value of the <see cref="MaxDBParameter"/> object to find. </param>
		/// <returns>true if the collection contains the <see cref="MaxDBParameter"/> object; otherwise, false.</returns>
		public override bool Contains(string parameterName)
		{
			return (-1 != IndexOf(parameterName));
		}

		/// <summary>
		/// Gets the location of a <see cref="MaxDBParameter"/> in the collection.
		/// </summary>
		/// <param name="parameterName">The <see cref="MaxDBParameter"/> object to locate. </param>
		/// <returns>The zero-based location of the <see cref="MaxDBParameter"/> in the collection.</returns>
		public override int IndexOf(string parameterName)
		{
			int index = 0;
			foreach (MaxDBParameter item in this)
			{
                if (_cultureAwareCompare(item.ParameterName, parameterName) == 0)
                {
                    return index;
                }

				index++;
			}
			return -1;
		}

		/// <summary>
		/// Removes the specified <see cref="MaxDBParameter"/> from the collection using a name.
		/// </summary>
		/// <param name="parameterName">The name of the parameter.</param>
		public override void RemoveAt(string parameterName)
		{
			RemoveAt(IndexOf(parameterName));
		}

		private static int _cultureAwareCompare(string strA, string strB)
		{
			return CultureInfo.InvariantCulture.CompareInfo.Compare(strA, strB, CompareOptions.IgnoreKanaType | CompareOptions.IgnoreWidth | CompareOptions.IgnoreCase);
		}

		#endregion

		#region "ICollection implementation"

		/// <summary>
		/// Copy values to the one-dimensional array starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">A target array.</param>
		/// <param name="index">The index in the array at which to begin copying.</param>
		public override void CopyTo(Array array, int index)
		{
			mCollection.CopyTo(array, index);
		}

		/// <summary>
		/// Copy values to the one-dimensional <see cref="MaxDBParameter"/> array starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">A target array.</param>
		/// <param name="index">The index in the array at which to begin copying.</param>
		public void CopyTo(MaxDBParameter[] array, int index)
		{
			mCollection.CopyTo(array, index);
		}

		/// <summary>
		/// Gets the number of <see cref="MaxDBParameter"/> objects in the collection.
		/// </summary>
		public override int Count
		{
			get
			{
				return mCollection.Length;
			}
		}

		/// <summary>
		/// Gets a value indicating whether collection is synchronized.
		/// </summary>
		public override bool IsSynchronized
		{
			get
			{
				return mCollection.IsSynchronized;
			}
		}

		/// <summary>
		/// Gets an object that can be used to synchronize access to the collection. 
		/// </summary>
		public override object SyncRoot
		{
			get
			{
				return mCollection.SyncRoot;
			}
		}

		#endregion

		#region "IEnumerable implementation"

		/// <summary>
		/// Returns an enumerator to support iterating through the collection. 
		/// </summary>
		/// <returns>An enumerator object.</returns>
		public override IEnumerator GetEnumerator()
		{
			return mCollection.GetEnumerator();
		}

		#endregion

		#region "IList implementation"

		/// <summary>
		/// Adds the specified <see cref="MaxDBParameter"/> object to the <see cref="MaxDBParameterCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="MaxDBParameter"/> to add to the collection.</param>
		/// <returns>The index of the new <see cref="MaxDBParameter"/> object.</returns>
		public override int Add(object value)
		{
			Add((MaxDBParameter)value);
			return mCollection.Length - 1;
		}

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		public override void Clear()
		{
			mCollection = new MaxDBParameter[0];
		}

		/// <summary>
		/// Gets a value indicating whether a <see cref="MaxDBParameter"/> exists in the collection.
		/// </summary>
		/// <param name="value">The value of the <see cref="MaxDBParameter"/> object to find.</param>
		/// <returns>true if the collection contains the <see cref="MaxDBParameter"/> object and false otherwise.</returns>
		public override bool Contains(object value)
		{
			foreach (MaxDBParameter param in mCollection)
				if (param == value)
					return true;

			return false;
		}

		/// <summary>
		/// Removes the specified <see cref="MaxDBParameter"/> from the collection using a specific index.
		/// </summary>
		/// <param name="index">The zero-based index of the parameter.</param>
		public override void RemoveAt(int index)
		{
			List<MaxDBParameter> tmp_array = new List<MaxDBParameter>(mCollection);
			tmp_array.RemoveAt(index);
			mCollection = new MaxDBParameter[tmp_array.Count];
			tmp_array.CopyTo(mCollection);
		}

		/// <summary>
		/// Gets the location of a <see cref="MaxDBParameter"/> in the collection.
		/// </summary>
		/// <param name="value">The <see cref="MaxDBParameter"/> object to locate. </param>
		/// <returns>The zero-based location of the <see cref="MaxDBParameter"/> in the collection.</returns>
		public override int IndexOf(object value)
		{
            for (int index = 0; index < mCollection.Length; index++)
            {
                if (mCollection[index] == value)
                {
                    return index;
                }
            }

			return -1;
		}

		/// <summary>
		/// Inserts a <see cref="MaxDBParameter"/> into the collection at the specified index.
		/// </summary>
		/// <param name="index">Collection index.</param>
		/// <param name="value"><see cref="MaxDBParameter"/> object to insert.</param>
		public void Insert(int index, MaxDBParameter value)
		{
			List<MaxDBParameter> tmp_array = new List<MaxDBParameter>(mCollection);
			tmp_array.Insert(index, (MaxDBParameter)value);
			mCollection = new MaxDBParameter[tmp_array.Count];
			tmp_array.CopyTo(mCollection);
		}

		/// <summary>
		/// Inserts a <see cref="MaxDBParameter"/> into the collection at the specified index.
		/// </summary>
		/// <param name="index">Collection index.</param>
		/// <param name="value">Object to insert.</param>
		public override void Insert(int index, object value)
		{
			Insert(index, (MaxDBParameter)value);
		}

		/// <summary>
		/// Gets a value indicating whether the collection has fixed size.
		/// </summary>
		public override bool IsFixedSize
		{
			get
			{
				return mCollection.IsFixedSize;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// </summary>
		public override bool IsReadOnly
		{
			get
			{
				return mCollection.IsReadOnly;
			}
		}

		/// <summary>
		/// Removes the specified <see cref="MaxDBParameter"/> from the collection.
		/// </summary>
		/// <param name="value">Object to remove</param>
		public override void Remove(object value)
		{
			Remove((MaxDBParameter)value);
		}

		/// <summary>
		/// Removes the specified <see cref="MaxDBParameter"/> from the collection.
		/// </summary>
		/// <param name="value">The <see cref="MaxDBParameter"/> to remove</param>
		public void Remove(MaxDBParameter value)
		{
			int index = ((IList)this).IndexOf(value);
			if (index >= 0)
				RemoveAt(index);
		}

		object IList.this[int index]
		{
			get
			{
				return mCollection[index];
			}
			set
			{
				mCollection[index] = (MaxDBParameter)value;
			}
		}

		#endregion

		/// <summary>
		/// Gets or sets <see cref="MaxDBParameter"/> by index.
		/// </summary>
		/// <param name="index">The index of the <see cref="MaxDBParameter"/></param>
		/// <returns>The <see cref="MaxDBParameter"/> object.</returns>
		public new MaxDBParameter this[int index]
		{
			get
			{
				return (MaxDBParameter)mCollection[index];
			}
			set
			{
				mCollection[index] = value;
			}
		}

		/// <summary>
		/// Adds the specified <see cref="MaxDBParameter"/> object to the <see cref="MaxDBParameterCollection"/>.
		/// </summary>
		/// <param name="value">The <see cref="MaxDBParameter"/> to add to the collection.</param>
		/// <returns>The newly added <see cref="MaxDBParameter"/> object.</returns>
		public MaxDBParameter Add(MaxDBParameter value)
		{
            if (value == null)
            {
                throw new MaxDBException(MaxDBMessages.Extract(MaxDBError.PARAMETER_NULL, "value"));
            }

            if (value.ParameterName != null)
            {
                MaxDBParameter[] new_collection = new MaxDBParameter[mCollection.Length + 1];
                mCollection.CopyTo(new_collection, 0);
                new_collection[mCollection.Length] = value;
                mCollection = new_collection;
                return value;
            }
            else
            {
                throw new ArgumentException(MaxDBMessages.Extract(MaxDBError.UNNAMED_PARAMETER));
            }
		}

		/// <summary>
		/// Adds a <see cref="MaxDBParameter"/> to the <see cref="MaxDBParameterCollection"/> given the specified parameter name and type.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="type">The <see cref="MaxDBType"/> of the <see cref="MaxDBParameter"/>.</param>
		/// <returns>The newly added <see cref="MaxDBParameter"/> object.</returns>
		public MaxDBParameter Add(string parameterName, MaxDBType type)
		{
			return Add(new MaxDBParameter(parameterName, type));
		}

		/// <summary>
		/// Adds a <see cref="MaxDBParameter"/> to the <see cref="MaxDBParameterCollection"/> given the specified parameter name and value.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="value">The <see cref="MaxDBParameter.Value"/> of the <see cref="MaxDBParameter"/> to add to the collection.</param>
		/// <returns>The newly added <see cref="MaxDBParameter"/> object.</returns>
		public MaxDBParameter Add(string parameterName, object value)
		{
			return Add(new MaxDBParameter(parameterName, value));
		}

		/// <summary>
		/// Adds a <see cref="MaxDBParameter"/> to the <see cref="MaxDBParameterCollection"/> given the specified parameter name, type and size.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="type">The <see cref="MaxDBType"/> of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="size">The size of the <see cref="MaxDBParameter"/>.</param>
		/// <returns>The newly added <see cref="MaxDBParameter"/> object.</returns>
		public MaxDBParameter Add(string parameterName, MaxDBType type, int size)
		{
			return Add(new MaxDBParameter(parameterName, type, size));
		}

		/// <summary>
		/// Adds a <see cref="MaxDBParameter"/> to the <see cref="MaxDBParameterCollection"/> given the specified parameter name, type, size
		/// and source column.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="type">The <see cref="MaxDBType"/> of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="size">The size of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="sourceColumn">The source column of the <see cref="MaxDBParameter"/>.</param>
		/// <returns>The newly added <see cref="MaxDBParameter"/> object.</returns>
		public MaxDBParameter Add(string parameterName, MaxDBType type, int size, string sourceColumn)
		{
			return Add(new MaxDBParameter(parameterName, type, size, sourceColumn));
		}

		/// <summary>
		/// Adds a <see cref="MaxDBParameter"/> to the <see cref="MaxDBParameterCollection"/>. 
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="type">The <see cref="MaxDBType"/> of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="size">The size of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="direction">The <see cref="ParameterDirection"/> of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="isNullable">Whether <see cref="MaxDBParameter"/> is nullable.</param>
		/// <param name="precision">The precision of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="scale">The scale of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="sourceColumn">The source column of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="sourceVersion">The row version of the <see cref="MaxDBParameter"/>.</param>
		/// <param name="value">The <see cref="MaxDBParameter.Value"/> of the <see cref="MaxDBParameter"/> to add to the collection.</param>
		/// <returns>The newly added <see cref="MaxDBParameter"/> object.</returns>
		public MaxDBParameter Add(string parameterName, MaxDBType type, int size, ParameterDirection direction,
			bool isNullable, byte precision, byte scale, string sourceColumn, DataRowVersion sourceVersion, object value)
		{
			return Add(new MaxDBParameter(parameterName, type, size, direction, isNullable, precision, scale, sourceColumn, sourceVersion, value));
		}

		/// <summary>
		/// Adds elements to the end of the <see cref="MaxDBParameterCollection"/>. 
		/// </summary>
		/// <param name="values">Adds an array of values to the end of the <see cref="MaxDBParameterCollection"/>.</param>
		public override void AddRange(Array values)
		{
            if (values == null)
            {
                throw new MaxDBException(MaxDBMessages.Extract(MaxDBError.PARAMETER_NULL, "values"));
            }

            foreach (MaxDBParameter param in values)
            {
                Add(param);
            }
		}

		/// <summary>
		/// Adds elements to the end of the <see cref="MaxDBParameterCollection"/>. 
		/// </summary>
		/// <param name="values">Adds an array of <see cref="MaxDBParameter"/> 
		/// values to the end of the <see cref="MaxDBParameterCollection"/>.</param>
		public void AddRange(MaxDBParameter[] values)
		{
            if (values == null)
            {
                throw new MaxDBException(MaxDBMessages.Extract(MaxDBError.PARAMETER_NULL, "values"));
            }

            foreach (MaxDBParameter param in values)
            {
                Add(param);
            }
		}

		/// <summary>
		/// This method is intended for internal use and can not to be called directly from your code.
		/// </summary>
		/// <param name="index">The index of the <see cref="DbParameter"/>.</param>
		/// <returns>The <see cref="DbParameter"/> object.</returns>
		protected override DbParameter GetParameter(int index)
		{
			return this[index];
		}

		/// <summary>
		/// This method is intended for internal use and can not to be called directly from your code.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="DbParameter"/>.</param>
		/// <returns>The <see cref="DbParameter"/> object.</returns>
		protected override DbParameter GetParameter(string parameterName)
		{
			return this[parameterName];
		}

		/// <summary>
		/// This method is intended for internal use and can not to be called directly from your code.
		/// </summary>
		/// <param name="index">The index of the <see cref="DbParameter"/>.</param>
		/// <param name="value">The <see cref="MaxDBParameter.Value"/> of the <see cref="MaxDBParameter"/> to set.</param>
		protected override void SetParameter(int index, DbParameter value)
		{
			this[index] = (MaxDBParameter)value;
		}

		/// <summary>
		/// This method is intended for internal use and can not to be called directly from your code.
		/// </summary>
		/// <param name="parameterName">The name of the <see cref="DbParameter"/>.</param>
		/// <param name="value">The <see cref="MaxDBParameter.Value"/> of the <see cref="MaxDBParameter"/> to set.</param>
		protected override void SetParameter(string parameterName, DbParameter value)
		{
			this[parameterName] = (MaxDBParameter)value;
		}

		#region ICloneable Members

		object ICloneable.Clone()
		{
			return this.Clone();
		}

		/// <summary>
		/// Clone <see cref="MaxDBParameterCollection"/> object.
		/// </summary>
		/// <returns>The cloned <see cref="MaxDBParameterCollection"/> object.</returns>
		public MaxDBParameterCollection Clone()
		{
			MaxDBParameterCollection clone = new MaxDBParameterCollection();
			foreach (MaxDBParameter param in this)
				clone.Add(((ICloneable)param).Clone());
			return clone;
		}

		#endregion
	}
}
