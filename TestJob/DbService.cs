using AngleSharp;
using Dapper;
using Npgsql;
using System.Text;
using TestJob.Controllers;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace TestJob
{
    public sealed class DbService : IDisposable
    {
        public DbService(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DatabaseConnection");
            _connection = new(connectionString);
        }
        ~DbService()
        {
            Dispose();
        }

        public bool IsDisposed { get; private set; }

        private readonly NpgsqlConnection _connection;

        public Task CreateElements(IEnumerable<HtmlPageParser.SelectedElement> elements)
        {
            if (!elements.Any())
            {
                return Task.CompletedTask;
            }

            StringBuilder sqlBuilder = new("INSERT INTO elements (attribute_value, html) VALUES ");
            DynamicParameters parameters = new();
            int i = 0;

            foreach (var element in elements)
            {
                if (i > 0)
                {
                    sqlBuilder.Append(',');
                }

                string attributeValueColumn = $"@AttributeValue{i}";
                string htmlColumn = $"@Html{i}";
                sqlBuilder.Append($"({attributeValueColumn}, {htmlColumn})");

                parameters.Add(attributeValueColumn, element.AttributeValue);
                parameters.Add(htmlColumn, element.Element.ToHtml());
                i++;
            }

            return _connection.ExecuteAsync(sqlBuilder.ToString(), parameters);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            _connection.Dispose();

            GC.SuppressFinalize(this);
        }

        private record struct HtmlElement(string? AttributeValue, string Html);
    }
}
