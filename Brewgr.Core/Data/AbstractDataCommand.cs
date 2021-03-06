using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace ctorx.Core.Data
{
	public abstract class AbstractDataCommand : IDataCommand, IDisposable
	{
		string ConnectionString;
		SqlCommand Command;

		/// <summary>
		/// ctor the Mighty
		/// </summary>
		protected AbstractDataCommand(CommandType commandType, string commandText)
		{
			this.SetCommand(commandType, commandText);
		}

		/// <summary>
		/// Sets the command
		/// </summary>
		void SetCommand(CommandType commandType, string commandText)
		{
			this.Command = new SqlCommand(commandText) {CommandType = commandType};
		}

		/// <summary>
		/// Sets the connection string to be used
		/// </summary>
		public IDataCommand UsingConnection(string connectionString)
		{
			this.ConnectionString = connectionString;
			return this;
		}

		/// <summary>
		/// Adds an input parameter to the command
		/// </summary>
		public IDataCommand WithParam(string parameterName, object value, int? size = 0)
		{
			this.AddParameter(parameterName, value, ParameterDirection.Input, size);
			return this;
		}

		/// <summary>
		/// Adds an Output parameter to the command
		/// </summary>
		public IDataCommand WithOutputParam(string parameterName, object value = null, int? size = null)
		{
			this.AddParameter(parameterName, value, ParameterDirection.Output, size);
			return this;
		}

		/// <summary>
		/// Sets the command timeout
		/// </summary>
		public IDataCommand WithTimeout(int timeout)
		{
			this.Command.CommandTimeout = timeout;
			return this;
		}

		/// <summary>
		/// Adds a Parameter to the Procedure Command
		/// </summary>
		void AddParameter(string parameterName, object value, ParameterDirection parameterDirection = ParameterDirection.Input, int? size = null)
		{
			var parameter = new SqlParameter(parameterName, value);

			if (parameterDirection != ParameterDirection.Input)
			{
				parameter.Direction = parameterDirection;
			}

			if(size.HasValue)
			{
				parameter.Size = size.Value;
			}

			this.Command.Parameters.Add(parameter);
		}

		/// <summary>
		/// Exexutes a non query command
		/// </summary>
		public int ExecuteNonQuery()
		{
			using (var connection = new SqlConnection(this.ConnectionString))
			{
				this.Command.Connection = connection;
                this.Command.Connection.Open();
                return this.Command.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Executes a scalar command
		/// </summary>
		public object ExecuteScalar()
		{
			using (var connection = new SqlConnection(this.ConnectionString))
			{
				this.Command.Connection = connection;
                this.Command.Connection.Open(); 
                return this.Command.ExecuteScalar();
			}
		}

		/// <summary>
		/// Executes, returning a DataSet
		/// </summary>
		public DataSet GetDataSet()
		{
			var dataSet = new DataSet();
			using (var connection = new SqlConnection(this.ConnectionString))
			{
				this.Command.Connection = connection;

				using (var adapter = new SqlDataAdapter(this.Command))
				{
					adapter.Fill(dataSet);
					return dataSet;
				}
			}
		}

		/// <summary>
		/// Gets the value of a Parameter
		/// </summary>
		public object GetParameterValue(string parameterName)
		{
			return this.Command.Parameters.Cast<SqlParameter>()
				.Where(x => x.ParameterName == parameterName)
				.Select(x => x.Value)
				.FirstOrDefault();
		}

		/// <summary>
		/// Disposes the object
		/// </summary>
		public void Dispose()
		{
			this.Command.Dispose();
		}
	}
}