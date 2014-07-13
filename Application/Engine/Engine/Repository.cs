using Engine.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Engine
{
    public class Repository : IDisposable
    {
        private SqlConnection _connection;
        private SqlTransaction _transaction;

        public Repository()
        {
            this._connection = new SqlConnection(Constants.ConnectionString);
        }
        public Repository(string connectionString)
        {
            this._connection = new SqlConnection(connectionString);
        }

        public Datapoint GetNextDatapoint()
        {
            var datapoint = new Datapoint();
            
            using (var reader = Query("SELECT TOP 1 [id],[name],[country],[count],[ratio] FROM [datapoint] WHERE [ratio] > 0 AND [use] = (SELECT TOP 1 [use] FROM [datapoint] WHERE [ratio] > 0 ORDER BY [use] ASC) ORDER BY NEWID()"))
            {
                if (!reader.Read())
                    return null;
                
                datapoint.ID = reader.GetInt32(0);
                datapoint.Name = reader.GetString(1);
                datapoint.Country = reader.GetString(2);
                datapoint.Count = reader.GetInt32(3);
                datapoint.Ratio = reader.GetInt32(4);
            }

            using (var reader = Query("SELECT D.area, Q.text, Q.type FROM dataset D JOIN question Q ON Q.dataset = D.name WHERE name = @name;", new SqlParameter("@name", datapoint.Name)))
            {
                var result = new List<Question>();

                while (reader.Read())
                {
                    var area = reader.GetString(0);
                    var text = reader.GetString(1);
                    var type = reader.GetInt32(2);

                    var question = new Question();
                    question.Area = area;
                    question.Text = text;
                    question.Type = type;
                    result.Add(question);
                }

                datapoint.Questions = result;
            }

            using (var reader = Query("SELECT id, uri, caption, credit FROM endplate WHERE datapoint = @id;", new SqlParameter("@id", datapoint.ID)))
            {
                var result = new List<Endplate>();

                while (reader.Read())
                {
                    var endplate = new Endplate();
                    endplate.ID = reader.GetInt32(0);
                    endplate.Uri = reader.GetString(1);
                    endplate.Caption = reader.GetString(2);
                    endplate.Credit = reader.GetString(3);

                    result.Add(endplate);
                }

                datapoint.Endplates = result;
            }

            return datapoint;
        }

        public IEnumerable<Datapoint> GetRelatedDatapoints(int id)
        {
            using (var reader = StoredProcedure("GetRelatedDatapoints", new SqlParameter("@id", id)))
            {
                var result = new List<Datapoint>();

                while (reader.Read())
                {
                    var datapoint = new Datapoint();
                    datapoint.ID = reader.GetInt32(0);
                    datapoint.Name = reader.GetString(1);
                    datapoint.Country = reader.GetString(2);
                    datapoint.Count = reader.GetInt32(3);
                    datapoint.Ratio = reader.GetInt32(4);

                    result.Add(datapoint);
                }

                return result;
            }
        }

        public void SetDatapointUsed(int id)
        {
            this.Execute("UPDATE [datapoint] SET [use] = [use] + 1 WHERE id = @id", new SqlParameter("@id", id));
        }

        public void BeginTransaction()
        {
            if (this._transaction != null)
                this._transaction = this._connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            if (this._transaction != null)
            {
                this._transaction.Commit();
                this._transaction = null;
            }
        }

        public void RollbackTransaction()
        {
            if (this._transaction != null)
            {
                this._transaction.Rollback();
                this._transaction = null;
            }
        }

        private SqlDataReader Query(string sql, params SqlParameter[] args)
        {
            this.CheckConnection();

            var query = _connection.CreateCommand();
            if (this._transaction != null)
                query.Transaction = this._transaction;

            query.CommandType = System.Data.CommandType.Text;
            query.CommandText = sql;
            query.Parameters.AddRange(args);

            return query.ExecuteReader();
        }

        private void Execute(string sql, params SqlParameter[] args)
        {
            this.CheckConnection();

            var query = _connection.CreateCommand();

            if (this._transaction != null)
                query.Transaction = this._transaction;

            query.CommandType = System.Data.CommandType.Text;
            query.CommandText = sql;
            query.Parameters.AddRange(args);

            query.ExecuteNonQuery();
        }

        private SqlDataReader StoredProcedure(string name, params SqlParameter[] args)
        {
            this.CheckConnection();

            var query = _connection.CreateCommand();

            if (this._transaction != null)
                query.Transaction = this._transaction;

            query.CommandType = System.Data.CommandType.StoredProcedure;
            query.CommandText = name;
            query.Parameters.AddRange(args);

            return query.ExecuteReader();
        }

        private void CheckConnection()
        {
            switch (this._connection.State)
            {
                case System.Data.ConnectionState.Closed:
                    this._connection.Open();
                    return;
                case System.Data.ConnectionState.Broken:
                    throw new Exception("Connection is in a broken state.");
            }
        }

        public void Dispose()
        {
            if (this._connection != null)
            {
                this._connection.Dispose();
                this._connection = null;
            }
        }
    }
}