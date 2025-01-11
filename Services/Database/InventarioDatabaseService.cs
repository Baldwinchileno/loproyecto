using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using AdminSERMAC.Models;

namespace AdminSERMAC.Services.Database
{
    public interface IInventarioDatabaseService
    {
        Task<bool> AddProductoAsync(string codigo, string producto, int unidades, double kilos, string fechaCompra, string fechaRegistro, string fechaVencimiento);
        Task<bool> ActualizarInventarioAsync(string codigo, int unidadesVendidas, double kilosVendidos);
        Task<IEnumerable<string>> GetCategoriasAsync();
        Task<IEnumerable<string>> GetSubCategoriasAsync(string categoria);
        Task<DataTable> GetInventarioAsync();
        Task<DataTable> GetInventarioPorCodigoAsync(string codigo);
        Task<bool> ActualizarFechasInventarioAsync(string codigo, DateTime fechaIngresada);

        // Métodos relacionados con CompraInventarioForm
        Task<List<CompraRegistro>> GetAllCompraRegistrosAsync();
        Task AddCompraRegistroAsync(CompraRegistro registro);
        Task<CompraRegistro> GetCompraRegistroByIdAsync(int id);
        Task UpdateCompraRegistroAsync(CompraRegistro registro);
        Task DeleteCompraRegistroAsync(int id);
        Task ProcesarCompraRegistroAsync(int id);
    }

    public class InventarioDatabaseService : BaseSQLiteService, IInventarioDatabaseService
    {
        private const string TableName = "Inventario";
        private readonly string _connectionString;
        private readonly ILogger<InventarioDatabaseService> _logger;

        public InventarioDatabaseService(ILogger<InventarioDatabaseService> logger, string connectionString)
            : base(logger, connectionString)
        {
            _logger = logger;
            _connectionString = connectionString;
            EnsureTableExists();
        }

        private void EnsureTableExists()
        {
            const string createTableSql = @"
                CREATE TABLE IF NOT EXISTS Inventario (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Codigo TEXT NOT NULL,
                    Producto TEXT NOT NULL,
                    Unidades INTEGER NOT NULL,
                    Kilos REAL NOT NULL,
                    FechaMasAntigua TEXT NOT NULL,
                    FechaMasNueva TEXT NOT NULL,
                    FechaVencimiento TEXT,
                    Categoria TEXT,
                    SubCategoria TEXT
                );

                CREATE TABLE IF NOT EXISTS CompraRegistros (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Proveedor TEXT NOT NULL,
                    Producto TEXT NOT NULL,
                    Cantidad INTEGER NOT NULL,
                    PrecioUnitario REAL NOT NULL,
                    Total REAL NOT NULL,
                    Observaciones TEXT,
                    FechaCompra TEXT NOT NULL,
                    EstaProcesado INTEGER NOT NULL DEFAULT 0
                );";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(createTableSql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        public async Task<List<CompraRegistro>> GetAllCompraRegistrosAsync()
        {
            var registros = new List<CompraRegistro>();
            const string query = "SELECT * FROM CompraRegistros";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        registros.Add(new CompraRegistro
                        {
                            Id = reader.GetInt32(0),
                            Proveedor = reader.GetString(1),
                            Producto = reader.GetString(2),
                            Cantidad = reader.GetInt32(3),
                            PrecioUnitario = reader.GetDecimal(4),
                            Total = reader.GetDecimal(5),
                            Observaciones = reader.IsDBNull(6) ? null : reader.GetString(6),
                            FechaCompra = DateTime.Parse(reader.GetString(7)),
                            EstaProcesado = reader.GetInt32(8) == 1
                        });
                    }
                }
            }

            return registros;
        }

        public async Task AddCompraRegistroAsync(CompraRegistro registro)
        {
            const string insertSql = @"
                INSERT INTO CompraRegistros 
                (Proveedor, Producto, Cantidad, PrecioUnitario, Total, Observaciones, FechaCompra, EstaProcesado)
                VALUES
                (@Proveedor, @Producto, @Cantidad, @PrecioUnitario, @Total, @Observaciones, @FechaCompra, @EstaProcesado);";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(insertSql, connection))
                {
                    command.Parameters.AddWithValue("@Proveedor", registro.Proveedor);
                    command.Parameters.AddWithValue("@Producto", registro.Producto);
                    command.Parameters.AddWithValue("@Cantidad", registro.Cantidad);
                    command.Parameters.AddWithValue("@PrecioUnitario", registro.PrecioUnitario);
                    command.Parameters.AddWithValue("@Total", registro.Total);
                    command.Parameters.AddWithValue("@Observaciones", registro.Observaciones ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FechaCompra", registro.FechaCompra.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@EstaProcesado", registro.EstaProcesado ? 1 : 0);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<CompraRegistro> GetCompraRegistroByIdAsync(int id)
        {
            const string query = "SELECT * FROM CompraRegistros WHERE Id = @Id";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new CompraRegistro
                            {
                                Id = reader.GetInt32(0),
                                Proveedor = reader.GetString(1),
                                Producto = reader.GetString(2),
                                Cantidad = reader.GetInt32(3),
                                PrecioUnitario = reader.GetDecimal(4),
                                Total = reader.GetDecimal(5),
                                Observaciones = reader.IsDBNull(6) ? null : reader.GetString(6),
                                FechaCompra = DateTime.Parse(reader.GetString(7)),
                                EstaProcesado = reader.GetInt32(8) == 1
                            };
                        }
                    }
                }
            }

            throw new KeyNotFoundException("Registro no encontrado");
        }

        public async Task UpdateCompraRegistroAsync(CompraRegistro registro)
        {
            const string updateSql = @"
                UPDATE CompraRegistros
                SET Proveedor = @Proveedor, Producto = @Producto, Cantidad = @Cantidad,
                    PrecioUnitario = @PrecioUnitario, Total = @Total, Observaciones = @Observaciones,
                    FechaCompra = @FechaCompra, EstaProcesado = @EstaProcesado
                WHERE Id = @Id";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(updateSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", registro.Id);
                    command.Parameters.AddWithValue("@Proveedor", registro.Proveedor);
                    command.Parameters.AddWithValue("@Producto", registro.Producto);
                    command.Parameters.AddWithValue("@Cantidad", registro.Cantidad);
                    command.Parameters.AddWithValue("@PrecioUnitario", registro.PrecioUnitario);
                    command.Parameters.AddWithValue("@Total", registro.Total);
                    command.Parameters.AddWithValue("@Observaciones", registro.Observaciones ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@FechaCompra", registro.FechaCompra.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@EstaProcesado", registro.EstaProcesado ? 1 : 0);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DeleteCompraRegistroAsync(int id)
        {
            const string deleteSql = "DELETE FROM CompraRegistros WHERE Id = @Id";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(deleteSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task ProcesarCompraRegistroAsync(int id)
        {
            const string processSql = "UPDATE CompraRegistros SET EstaProcesado = 1 WHERE Id = @Id";

            using (var connection = new SQLiteConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(processSql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
