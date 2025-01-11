using System.Collections.Generic;
using System.Threading.Tasks;
using AdminSERMAC.Models;

public interface IInventarioDatabaseService
{
    Task<List<CompraRegistro>> GetAllCompraRegistrosAsync();
    Task AddCompraRegistroAsync(CompraRegistro registro);
    Task<CompraRegistro> GetCompraRegistroByIdAsync(int id);
    Task UpdateCompraRegistroAsync(CompraRegistro registro);
    Task DeleteCompraRegistroAsync(int id);
    Task ProcesarCompraRegistroAsync(int id);
}
