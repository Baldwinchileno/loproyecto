using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using AdminSERMAC.Models;
using AdminSERMAC.Services.Database;
using Microsoft.Extensions.Logging;
using AdminSERMAC.Services;

namespace AdminSERMAC.Forms
{
    public class CompraInventarioForm : Form
    {
        private readonly ILogger<CompraInventarioForm> _logger;
        private readonly SQLiteService _sqliteService;
        private readonly IInventarioDatabaseService _inventarioService;

        // Controles del formulario
        private DataGridView dgvCompras;
        private TextBox txtProveedor;
        private TextBox txtProducto;
        private NumericUpDown numCantidad;
        private NumericUpDown numPrecioUnitario;
        private TextBox txtObservaciones;
        private Button btnAgregar;
        private Button btnEditar;
        private Button btnEliminar;
        private Button btnProcesar;
        private Label lblTotal;

        public CompraInventarioForm(
            SQLiteService sqliteService,
            ILogger<CompraInventarioForm> logger,
            IInventarioDatabaseService inventarioService)
        {
            _sqliteService = sqliteService ?? throw new ArgumentNullException(nameof(sqliteService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _inventarioService = inventarioService ?? throw new ArgumentNullException(nameof(inventarioService));

            InitializeComponents();
            CargarRegistros();
        }

        private void InitializeComponents()
        {
            this.Text = "Gestión de Compras - Inventario";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // DataGridView para mostrar compras
            dgvCompras = new DataGridView
            {
                Dock = DockStyle.Top,
                Height = 300,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvCompras.Columns.Add("Id", "ID");
            dgvCompras.Columns.Add("FechaCompra", "Fecha");
            dgvCompras.Columns.Add("Proveedor", "Proveedor");
            dgvCompras.Columns.Add("Producto", "Producto");
            dgvCompras.Columns.Add("Cantidad", "Cantidad");
            dgvCompras.Columns.Add("PrecioUnitario", "Precio Unitario");
            dgvCompras.Columns.Add("Total", "Total");
            dgvCompras.Columns.Add("Observaciones", "Observaciones");

            // Controles para agregar y editar compras
            txtProveedor = new TextBox { PlaceholderText = "Proveedor", Width = 200 };
            txtProducto = new TextBox { PlaceholderText = "Producto", Width = 200 };
            numCantidad = new NumericUpDown { Width = 100, Minimum = 1, Maximum = 1000 };
            numPrecioUnitario = new NumericUpDown { Width = 100, Minimum = 1, Maximum = 100000, DecimalPlaces = 2 };
            txtObservaciones = new TextBox { PlaceholderText = "Observaciones", Width = 300 };

            btnAgregar = new Button { Text = "Agregar", Width = 100 };
            btnEditar = new Button { Text = "Editar", Width = 100 };
            btnEliminar = new Button { Text = "Eliminar", Width = 100 };
            btnProcesar = new Button { Text = "Procesar", Width = 100 };

            lblTotal = new Label { Text = "Total: $0.00", AutoSize = true };

            btnAgregar.Click += BtnAgregar_Click;
            btnEditar.Click += BtnEditar_Click;
            btnEliminar.Click += BtnEliminar_Click;
            btnProcesar.Click += BtnProcesar_Click;

            // Layout
            var controlsPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                AutoScroll = true
            };

            controlsPanel.Controls.AddRange(new Control[]
            {
                txtProveedor,
                txtProducto,
                numCantidad,
                numPrecioUnitario,
                txtObservaciones,
                btnAgregar,
                btnEditar,
                btnEliminar,
                btnProcesar,
                lblTotal
            });

            this.Controls.Add(dgvCompras);
            this.Controls.Add(controlsPanel);
        }

        private async void CargarRegistros()
        {
            try
            {
                dgvCompras.Rows.Clear();
                var registros = await _inventarioService.GetAllCompraRegistrosAsync();

                foreach (var registro in registros)
                {
                    dgvCompras.Rows.Add(
                        registro.Id,
                        registro.FechaCompra.ToString("dd/MM/yyyy"),
                        registro.Proveedor,
                        registro.Producto,
                        registro.Cantidad,
                        registro.PrecioUnitario.ToString("C"),
                        registro.Total.ToString("C"),
                        registro.Observaciones
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los registros de compras");
                MessageBox.Show("Error al cargar los registros de compras.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnAgregar_Click(object sender, EventArgs e)
        {
            try
            {
                var nuevoRegistro = new CompraRegistro
                {
                    Proveedor = txtProveedor.Text,
                    Producto = txtProducto.Text,
                    Cantidad = (int)numCantidad.Value,
                    PrecioUnitario = (double)numPrecioUnitario.Value,
                    Observaciones = txtObservaciones.Text,
                    FechaCompra = DateTime.Now
                };

                await _inventarioService.AddCompraRegistroAsync(nuevoRegistro);
                MessageBox.Show("Compra agregada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarRegistros();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar una nueva compra");
                MessageBox.Show("Error al agregar la compra.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnEditar_Click(object sender, EventArgs e)
        {
            if (dgvCompras.CurrentRow == null) return;

            try
            {
                var id = Convert.ToInt32(dgvCompras.CurrentRow.Cells["Id"].Value);
                var registro = await _inventarioService.GetCompraRegistroByIdAsync(id);

                registro.Proveedor = txtProveedor.Text;
                registro.Producto = txtProducto.Text;
                registro.Cantidad = (int)numCantidad.Value;
                registro.PrecioUnitario = (double)numPrecioUnitario.Value;
                registro.Observaciones = txtObservaciones.Text;

                await _inventarioService.UpdateCompraRegistroAsync(registro);
                MessageBox.Show("Compra actualizada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarRegistros();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al editar la compra");
                MessageBox.Show("Error al editar la compra.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvCompras.CurrentRow == null) return;

            try
            {
                var id = Convert.ToInt32(dgvCompras.CurrentRow.Cells["Id"].Value);
                await _inventarioService.DeleteCompraRegistroAsync(id);
                MessageBox.Show("Compra eliminada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarRegistros();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar la compra");
                MessageBox.Show("Error al eliminar la compra.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnProcesar_Click(object sender, EventArgs e)
        {
            if (dgvCompras.CurrentRow == null) return;

            try
            {
                var id = Convert.ToInt32(dgvCompras.CurrentRow.Cells["Id"].Value);
                await _inventarioService.ProcesarCompraRegistroAsync(id);
                MessageBox.Show("Compra procesada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                CargarRegistros();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar la compra");
                MessageBox.Show("Error al procesar la compra.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
