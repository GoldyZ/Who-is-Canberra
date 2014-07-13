using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    public class ImageRow
    {
        public string Country { get; set; }

        public string Caption { get; set; }

        public string Credit { get; set; }

        public string Url { get; set; }
    }

    class Program
    {

        public static Dictionary<string, List<ImageRow>> LoadImageDS()
        {
            var result = new Dictionary<string, List<ImageRow>>();

            using (var parser = new TextFieldParser(@"C:\Users\Paul\Desktop\Datasets\images.csv"))
            {
                parser.CommentTokens = new string[] { "#" };
                parser.SetDelimiters(new string[] { "," });
                parser.HasFieldsEnclosedInQuotes = true;

                parser.ReadLine();

                while (!parser.EndOfData)
                {
                    var fields = parser.ReadFields();

                    var row = new ImageRow();
                    row.Country = fields[0];
                    row.Caption = fields[1];
                    row.Credit = fields[2];
                    row.Url = fields[3];

                    if (result.ContainsKey(row.Country))
                    {
                        result[row.Country].Add(row);
                    }
                    else
                    {
                        var list = new List<ImageRow>();
                        list.Add(row);
                        result.Add(row.Country, list);
                    }
                }
            }

            return result;
        }

        static void Main(string[] args)
        {
            var importAncestry = true;
            var imageDS = LoadImageDS();

            using (var conn = new SqlConnection(@"Server=.\SQLExpress;Database=goldilocks;User Id=goldilocks;Password=entry29.org.au;"))
            {

                var files = importAncestry ?
                    Directory.GetFiles(@"C:\Users\Paul\Desktop\Datasets\Ancestry", "*.csv") :
                    Directory.GetFiles(@"C:\Users\Paul\Desktop\Datasets\Country", "*.csv");

                conn.Open();

                foreach (var path in files)
                {
                    using (var transaction = conn.BeginTransaction())
                    {
                        var name = Path.GetFileNameWithoutExtension(path);
                        var area = name.Substring(name.LastIndexOf('(') + 1).TrimEnd(')');
                        Execute(conn, transaction, "INSERT INTO dataset (name, area) VALUES (@name, @area);", 
                            new SqlParameter("@name", name),
                            new SqlParameter("@area", area));

                        if (!importAncestry)
                        {
                            Execute(conn, transaction, "INSERT INTO question (dataset, text, type) VALUES (@dataset, @text, @type);",
                                new SqlParameter("@dataset", name),
                                new SqlParameter("@text", "Among foreign-born <%=area%>, which colour represents the <%=country%> born population?"),
                                new SqlParameter("@type", Convert.ToInt32(0))
                                );
                            Execute(conn, transaction, "INSERT INTO question (dataset, text, type) VALUES (@dataset, @text, @type);",
                                new SqlParameter("@dataset", name),
                                new SqlParameter("@text", "Of foreign-born <%=area%>, <%=ratio%>% were born in..."),
                                new SqlParameter("@type", 1)
                                );
                        }
                        else
                        {
                            Execute(conn, transaction, "INSERT INTO question (dataset, text, type) VALUES (@dataset, @text, @type);",
                                new SqlParameter("@dataset", name),
                                new SqlParameter("@text", "Which colour represents the <%=area%> with <%=country%> ancestry?"),
                                new SqlParameter("@type", Convert.ToInt32(0))
                                );
                            Execute(conn, transaction, "INSERT INTO question (dataset, text, type) VALUES (@dataset, @text, @type);",
                                new SqlParameter("@dataset", name),
                                new SqlParameter("@text", "<%=ratio%>% of <%=area%> identify their ancestry as..."),
                                new SqlParameter("@type", 1)
                                );
                        }
                        int total = 0;

                        using (var parser = new TextFieldParser(path))
                        {
                            parser.CommentTokens = new string[] { "#" };
                            parser.SetDelimiters(new string[] { "," });
                            parser.HasFieldsEnclosedInQuotes = true;

                            Console.WriteLine(path);

                            while (!parser.EndOfData)
                            {
                                var fields = parser.ReadFields();
                                var country = fields[0];
                                var count = int.Parse(fields[1]);

                                var totalRowName = importAncestry ? "Total persons(c)" : "Total";

                                if (country == totalRowName)
                                {
                                    total = count;
                                    continue;
                                }

                                int datapointID = 0;

                                using (var reader = Query(conn, transaction, "INSERT INTO datapoint (name, country, [count]) OUTPUT inserted.id VALUES (@name, @country, @count);",
                                    new SqlParameter("@name", name),
                                    new SqlParameter("@country", country),
                                    new SqlParameter("@count", count)))
                                {
                                    if (!reader.Read())
                                        throw new Exception("didn't get id");

                                    datapointID = reader.GetInt32(0);
                                }

                                if (imageDS.ContainsKey(country))
                                {
                                    var images = imageDS[country];

                                    foreach (var image in images)
                                    {
                                        Execute(conn, transaction, "INSERT INTO endplate (datapoint, uri, caption, credit) VALUES (@datapoint, @uri, @caption, @credit);",
                                            new SqlParameter("@datapoint", datapointID),
                                            new SqlParameter("@uri", image.Url),
                                            new SqlParameter("@caption", image.Caption),
                                            new SqlParameter("@credit", image.Credit));
                                    }
                                   
                                }
                            }
                        }

                        StoredProcedure(conn, transaction, "SetDatapointRatios", new SqlParameter("@name", name), new SqlParameter("@total", total));

                        transaction.Commit();
                    }
                }

            }

            Console.ReadLine();
        }

        private static void StoredProcedure(SqlConnection connection, SqlTransaction transaction, string name, params SqlParameter[] args)
        {
            var query = connection.CreateCommand();
                query.Transaction = transaction;

            query.CommandType = System.Data.CommandType.StoredProcedure;
            query.CommandText = name;
            query.Parameters.AddRange(args);

            query.ExecuteNonQuery();
        }

        private static void Execute(SqlConnection connection, SqlTransaction trans, string sql, params SqlParameter[] args)
        {
            var query = connection.CreateCommand();
            query.Transaction = trans;
            query.CommandType = System.Data.CommandType.Text;
            query.CommandText = sql;
            query.Parameters.AddRange(args);

            query.ExecuteNonQuery();
        }

        private static SqlDataReader Query(SqlConnection connection, SqlTransaction trans, string sql, params SqlParameter[] args)
        {
            var query = connection.CreateCommand();
            query.Transaction = trans;
            query.CommandType = System.Data.CommandType.Text;
            query.CommandText = sql;
            query.Parameters.AddRange(args);

            return query.ExecuteReader();
        }
    }
}
