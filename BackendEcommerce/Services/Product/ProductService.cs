using BackendEcommerce.DTOs.Product;
using Oracle.ManagedDataAccess.Client;

namespace BackendEcommerce.Services.Product
{
    public class ProductService
    {
        private readonly string _connectionString;
        public ProductService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("OracleDb");
        }

        public async Task<List<ProductResponse>> GetAllAsync()
        {
            var list = new List<ProductResponse>();
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("SELECT * FROM PRODUCTS", conn);
            await conn.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ProductResponse
                {
                    ProductId = reader.GetInt32(0),
                    ProductName = reader.GetString(1),
                    Price = reader.GetDecimal(2),
                    Description = reader.IsDBNull(3) ? null : reader.GetString(3)
                });
            }
            return list;
        }

        public async Task<int> CreateAsync(ProductRequest request)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand(
                "INSERT INTO PRODUCTS (PRODUCTNAME, PRICE, DESCRIPTION) VALUES (:name, :price, :desc)", conn);
            cmd.Parameters.Add(new OracleParameter("name", request.ProductName));
            cmd.Parameters.Add(new OracleParameter("price", request.Price));
            cmd.Parameters.Add(new OracleParameter("desc", request.Description ?? (object)DBNull.Value));

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> UpdateAsync(int id, ProductRequest request)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand(
                "UPDATE PRODUCTS SET PRODUCTNAME = :name, PRICE = :price, DESCRIPTION = :desc WHERE PRODUCTID = :id", conn);
            cmd.Parameters.Add(new OracleParameter("name", request.ProductName));
            cmd.Parameters.Add(new OracleParameter("price", request.Price));
            cmd.Parameters.Add(new OracleParameter("desc", request.Description ?? (object)DBNull.Value));
            cmd.Parameters.Add(new OracleParameter("id", id));

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }
        public async Task<int> DeleteAsync(int id)
        {
            using var conn = new OracleConnection(_connectionString);
            using var cmd = new OracleCommand("DELETE FROM PRODUCTS WHERE PRODUCTID = :id", conn);
            cmd.Parameters.Add(new OracleParameter("id", id));

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

    }
}
