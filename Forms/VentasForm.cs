using AdminSERMAC.Services;
using System.Data.SQLite;
using AdminSERMAC.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using ClosedXML.Excel;
using System.Drawing.Printing;
using System.Windows.Forms;


public class VentasForm : Form
{
    private Label numeroGuiaLabel;
    private TextBox numeroGuiaTextBox;
    private Label rutLabel;
    private ComboBox rutComboBox;
    private Label clienteLabel;
    private ComboBox clienteComboBox;
    private Label direccionLabel;
    private TextBox direccionTextBox;
    private Label giroLabel;
    private TextBox giroTextBox;
    private Label fechaEmisionLabel;
    private DateTimePicker fechaEmisionPicker;
    private Label totalVentaLabel;
    private TextBox totalVentaTextBox;
    private Label stockLabel;

    private Button exportarExcelButton;
    private Button imprimirButton;


    private DataGridView ventasDataGridView;
    private Button finalizarButton;
    private CheckBox pagarConCreditoCheckBox;

    private SQLiteService sqliteService;
    private double totalVenta = 0;
    private Dictionary<string, Cliente> clientesDict = new Dictionary<string, Cliente>();

    public VentasForm(ILogger<SQLiteService> logger)
    {
        this.Text = "Gestión de Ventas";
        this.Width = 1000;
        this.Height = 800;

        sqliteService = new SQLiteService(logger);

        InitializeComponents(); // Primero inicializar componentes

        // Después configurar el estado de los botones
        exportarExcelButton.Enabled = false;
        imprimirButton.Enabled = false;

        ConfigureEvents();
        CargarNumeroGuia();
        CargarClientes();
    }

    private void InitializeComponents()
    {
        // Número de guía
        numeroGuiaLabel = new Label { Text = "Número de Guía:", Location = new Point(20, 20), Width = 100 };
        numeroGuiaTextBox = new TextBox { Location = new Point(150, 20), Width = 100, ReadOnly = true };

        // Fecha de emisión (mover a la derecha)
        fechaEmisionLabel = new Label { Text = "Fecha de Emisión:", Location = new Point(400, 20), Width = 120 };
        fechaEmisionPicker = new DateTimePicker { Location = new Point(520, 20), Width = 200 };

        // RUT (ajustar posición y estilo)
        rutLabel = new Label { Text = "RUT:", Location = new Point(20, 60), Width = 100 };
        rutComboBox = new ComboBox
        {
            Location = new Point(150, 60),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.CustomSource
        };

        // Cliente
        clienteLabel = new Label { Text = "Nombre:", Location = new Point(20, 90), Width = 100 };
        clienteComboBox = new ComboBox
        {
            Location = new Point(150, 90),
            Width = 200,
            DropDownStyle = ComboBoxStyle.DropDown,
            AutoCompleteMode = AutoCompleteMode.SuggestAppend,
            AutoCompleteSource = AutoCompleteSource.CustomSource
        };

        // Dirección
        direccionLabel = new Label { Text = "Dirección:", Location = new Point(20, 120), Width = 100 };
        direccionTextBox = new TextBox { Location = new Point(150, 120), Width = 200, ReadOnly = true };

        // Giro
        giroLabel = new Label { Text = "Giro:", Location = new Point(20, 150), Width = 100 };
        giroTextBox = new TextBox { Location = new Point(150, 150), Width = 200, ReadOnly = true };

        // DataGridView
        ventasDataGridView = new DataGridView
        {
            Location = new Point(20, 190),
            Size = new Size(940, 350),
            AllowUserToAddRows = true,
            AllowUserToDeleteRows = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        };

        stockLabel = new Label
        {
            Location = new Point(20, 550),
            Width = 300,
            Height = 30,
            Font = new Font("Arial", 12, FontStyle.Bold),
            ForeColor = Color.DarkGreen
        };

        this.Controls.Add(stockLabel);

        exportarExcelButton = new Button
        {
            Text = "Exportar a Excel",
            Location = new Point(600, 620),
            Width = 150,
            Height = 30,
            BackColor = Color.FromArgb(76, 175, 80),
            ForeColor = Color.White
        };

        imprimirButton = new Button
        {
            Text = "Imprimir Guía",
            Location = new Point(800, 620),
            Width = 150,
            Height = 30,
            BackColor = Color.FromArgb(63, 81, 181),
            ForeColor = Color.White
        };

        this.Controls.AddRange(new Control[] { exportarExcelButton, imprimirButton });




        // En InitializeComponents, actualizar las columnas del DataGridView
        ventasDataGridView.Columns.Add("Codigo", "Código");
        ventasDataGridView.Columns.Add("Descripcion", "Descripción");
        ventasDataGridView.Columns.Add("Unidades", "Unidades");
        ventasDataGridView.Columns.Add("Bandejas", "Bandejas");
        ventasDataGridView.Columns.Add("KilosBruto", "Kilos Bruto");
        ventasDataGridView.Columns.Add("KilosNeto", "Kilos Neto");
        ventasDataGridView.Columns.Add("Precio", "Precio");
        ventasDataGridView.Columns.Add("Total", "Total");

        // Estilo del DataGridView
        ventasDataGridView.ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        ventasDataGridView.EnableHeadersVisualStyles = false;
        ventasDataGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;

        // Controles inferiores
        pagarConCreditoCheckBox = new CheckBox
        {
            Text = "Pago con crédito",
            Location = new Point(600, 560),
            Width = 150
        };

        totalVentaLabel = new Label { Text = "Total Venta:", Location = new Point(400, 560), Width = 100 };
        totalVentaTextBox = new TextBox { Location = new Point(500, 560), Width = 150, ReadOnly = true };

        finalizarButton = new Button
        {
            Text = "Finalizar Venta",
            Location = new Point(800, 560),
            Width = 150,
            Height = 40,
            BackColor = Color.FromArgb(0, 122, 204),
            ForeColor = Color.White
        };

        // Configurar formato de columnas
        ConfigurarColumnasGrid();

        // Agregar todos los controles al formulario
        this.Controls.AddRange(new Control[]
        {
            numeroGuiaLabel, numeroGuiaTextBox,
            rutLabel, rutComboBox,
            clienteLabel, clienteComboBox,
            direccionLabel, direccionTextBox,
            giroLabel, giroTextBox,
            fechaEmisionLabel, fechaEmisionPicker,
            ventasDataGridView,
            finalizarButton, pagarConCreditoCheckBox,
            totalVentaLabel, totalVentaTextBox
        });
    }

    private void ConfigureEvents()
    {
        rutComboBox.SelectedIndexChanged += RutComboBox_SelectedIndexChanged;
        clienteComboBox.SelectedIndexChanged += ClienteComboBox_SelectedIndexChanged;
        ventasDataGridView.CellEndEdit += VentasDataGridView_CellEndEdit;
        finalizarButton.Click += FinalizarButton_Click;
        ventasDataGridView.CellValueChanged += VentasDataGridView_CellValueChanged;
        exportarExcelButton.Click += ExportarExcelButton_Click;
        imprimirButton.Click += ImprimirButton_Click;
    }

    private void ExportarExcelButton_Click(object sender, EventArgs e)
    {
        if (!ValidarVenta()) return;

        using (SaveFileDialog saveDialog = new SaveFileDialog())
        {
            saveDialog.Filter = "Excel files (*.xlsx)|*.xlsx";
            saveDialog.FileName = $"Guia_{numeroGuiaTextBox.Text}.xlsx";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var excelService = new ExcelExportService();
                    var cliente = clientesDict[rutComboBox.SelectedItem.ToString()];

                    excelService.ExportarGuia(
                        numeroGuiaTextBox.Text,
                        ventasDataGridView,
                        cliente,
                        fechaEmisionPicker.Value,
                        saveDialog.FileName
                    );

                    MessageBox.Show("Guía exportada exitosamente", "Éxito",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private void ImprimirButton_Click(object sender, EventArgs e)
    {
        if (!ValidarVenta()) return;

        try
        {
            var cliente = clientesDict[rutComboBox.SelectedItem.ToString()];
            var venta = new Venta
            {
                NumeroGuia = int.Parse(numeroGuiaTextBox.Text), // Convert string to int
                FechaVenta = fechaEmisionPicker.Value.ToString("dd/MM/yyyy"),
                RUT = cliente.RUT,
                ClienteNombre = cliente.Nombre,
                PagadoConCredito = pagarConCreditoCheckBox.Checked ? 1 : 0
            };

            var printService = new PrintService(ventasDataGridView, venta, cliente);
            printService.PrintGuia();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al imprimir: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }


    private void CargarNumeroGuia()
    {
        int numeroGuia = sqliteService.GetUltimoNumeroGuia() + 1;
        numeroGuiaTextBox.Text = numeroGuia.ToString();
    }

    private void CargarClientes()
    {
        clientesDict.Clear();
        rutComboBox.Items.Clear();
        clienteComboBox.Items.Clear();

        // Crear las colecciones de autocompletado
        var rutAutoComplete = new AutoCompleteStringCollection();
        var nombreAutoComplete = new AutoCompleteStringCollection();

        var clientes = sqliteService.GetClientes();
        foreach (var cliente in clientes)
        {
            clientesDict[cliente.RUT] = cliente;
            rutComboBox.Items.Add(cliente.RUT);
            clienteComboBox.Items.Add(cliente.Nombre);

            // Agregar a las colecciones de autocompletado
            rutAutoComplete.Add(cliente.RUT);
            nombreAutoComplete.Add(cliente.Nombre);
        }

        // Asignar las colecciones de autocompletado
        rutComboBox.AutoCompleteCustomSource = rutAutoComplete;
        clienteComboBox.AutoCompleteCustomSource = nombreAutoComplete;
    }

    private void RutComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (rutComboBox.SelectedItem != null)
        {
            string rut = rutComboBox.SelectedItem.ToString();
            if (clientesDict.ContainsKey(rut))
            {
                var cliente = clientesDict[rut];
                clienteComboBox.SelectedItem = cliente.Nombre;
                ActualizarDatosCliente(cliente);
            }
        }
    }

    private void ClienteComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (clienteComboBox.SelectedItem != null)
        {
            string nombre = clienteComboBox.SelectedItem.ToString();
            var cliente = clientesDict.Values.FirstOrDefault(c => c.Nombre == nombre);
            if (cliente != null)
            {
                rutComboBox.SelectedItem = cliente.RUT;
                ActualizarDatosCliente(cliente);
            }
        }
    }

    private void ActualizarDatosCliente(Cliente cliente)
    {
        direccionTextBox.Text = cliente.Direccion;
        giroTextBox.Text = cliente.Giro;
    }

    private async void VentasDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
    {
        if (e.ColumnIndex == ventasDataGridView.Columns["Codigo"].Index)
        {
            var row = ventasDataGridView.Rows[e.RowIndex];
            string codigo = row.Cells["Codigo"].Value?.ToString();

            if (!string.IsNullOrEmpty(codigo))
            {
                var producto = sqliteService.GetProductoPorCodigo(codigo);
                if (producto != null)
                {
                    row.Cells["Descripcion"].Value = producto.Nombre;
                    stockLabel.Text = $"Stock disponible: {producto.Kilos:N2} kg";
                    stockLabel.ForeColor = producto.Kilos > 0 ? Color.DarkGreen : Color.Red;
                }
                else
                {
                    row.Cells["Codigo"].Value = null;
                    row.Cells["Descripcion"].Value = null;
                    stockLabel.Text = "Producto no encontrado";
                    stockLabel.ForeColor = Color.Red;
                    MessageBox.Show("Producto no encontrado", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private void VentasDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            var row = ventasDataGridView.Rows[e.RowIndex];

            // Si cambió el valor de Bandejas o Kilos Bruto
            if (e.ColumnIndex == ventasDataGridView.Columns["Bandejas"].Index ||
                e.ColumnIndex == ventasDataGridView.Columns["KilosBruto"].Index)
            {
                // Calcular Kilos Neto
                if (double.TryParse(row.Cells["KilosBruto"].Value?.ToString(), out double kilosBruto) &&
                    double.TryParse(row.Cells["Bandejas"].Value?.ToString(), out double bandejas))
                {
                    double descuentoBandejas = bandejas * 1.5;
                    double kilosNeto = kilosBruto - descuentoBandejas;

                    // Actualizar Kilos Neto
                    row.Cells["KilosNeto"].Value = kilosNeto.ToString("N2");

                    // Si hay precio, actualizar el total
                    if (double.TryParse(row.Cells["Precio"].Value?.ToString(), out double precio))
                    {
                        double total = kilosNeto * precio;
                        row.Cells["Total"].Value = total.ToString("N2");
                    }
                }
            }
            // Si cambió el Precio
            else if (e.ColumnIndex == ventasDataGridView.Columns["Precio"].Index)
            {
                if (double.TryParse(row.Cells["KilosNeto"].Value?.ToString(), out double kilosNeto) &&
                    double.TryParse(row.Cells["Precio"].Value?.ToString(), out double precio))
                {
                    double total = kilosNeto * precio;
                    row.Cells["Total"].Value = total.ToString("N2");
                }
            }

            // Recalcular el total general de la venta
            CalcularTotalVenta();
        }
    }

    private void ConfigurarColumnasGrid()
    {
        // Formato para columnas numéricas
        ventasDataGridView.Columns["Bandejas"].DefaultCellStyle.Format = "N0";
        ventasDataGridView.Columns["KilosBruto"].DefaultCellStyle.Format = "N2";
        ventasDataGridView.Columns["KilosNeto"].DefaultCellStyle.Format = "N2";
        ventasDataGridView.Columns["Precio"].DefaultCellStyle.Format = "N2";
        ventasDataGridView.Columns["Total"].DefaultCellStyle.Format = "N2";

        // Alineación a la derecha para columnas numéricas
        ventasDataGridView.Columns["Bandejas"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        ventasDataGridView.Columns["KilosBruto"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        ventasDataGridView.Columns["KilosNeto"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        ventasDataGridView.Columns["Precio"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        ventasDataGridView.Columns["Total"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

        // Hacer algunas columnas de solo lectura
        ventasDataGridView.Columns["KilosNeto"].ReadOnly = true;
        ventasDataGridView.Columns["Total"].ReadOnly = true;
    }

    private void CalcularTotalVenta()
    {
        totalVenta = 0;
        foreach (DataGridViewRow row in ventasDataGridView.Rows)
        {
            if (!row.IsNewRow && row.Cells["Total"].Value != null)
            {
                if (double.TryParse(row.Cells["Total"].Value.ToString(), out double total))
                {
                    totalVenta += total;
                }
            }
        }
        totalVentaTextBox.Text = totalVenta.ToString("C");
    }

    private async void FinalizarButton_Click(object sender, EventArgs e)
    {
        if (!ValidarVenta())
            return;

        SQLiteConnection connection = null;
        SQLiteTransaction transaction = null;

        try
        {
            // Deshabilitar el botón mientras se procesa
            finalizarButton.Enabled = false;

            connection = new SQLiteConnection(sqliteService.connectionString);
            await connection.OpenAsync();
            transaction = connection.BeginTransaction();

            var numeroGuia = int.Parse(numeroGuiaTextBox.Text);
            var rut = rutComboBox.SelectedItem.ToString();
            var fechaVenta = fechaEmisionPicker.Value.ToString("yyyy-MM-dd");
            var pagadoConCredito = pagarConCreditoCheckBox.Checked ? 1 : 0;
            double totalVentaActual = 0;

            // Registrar cada línea de la venta
            foreach (DataGridViewRow row in ventasDataGridView.Rows)
            {
                if (row.IsNewRow || row.Cells["Codigo"].Value == null)
                    continue;

                var codigo = row.Cells["Codigo"].Value.ToString();
                var descripcion = row.Cells["Descripcion"].Value?.ToString();
                var bandejas = Convert.ToInt32(row.Cells["Bandejas"].Value);
                var kilosNeto = Convert.ToDouble(row.Cells["KilosNeto"].Value);
                var total = Convert.ToDouble(row.Cells["Total"].Value);
                totalVentaActual += total;

                // Insertar venta
                var command = new SQLiteCommand(@"
                INSERT INTO Ventas (
                    NumeroGuia, CodigoProducto, Descripcion, Bandejas, 
                    KilosNeto, FechaVenta, PagadoConCredito, RUT, Total
                ) VALUES (
                    @numeroGuia, @codigo, @descripcion, @bandejas, 
                    @kilosNeto, @fechaVenta, @pagadoConCredito, @rut, @total
                )", connection, transaction);

                command.Parameters.AddWithValue("@numeroGuia", numeroGuia);
                command.Parameters.AddWithValue("@codigo", codigo);
                command.Parameters.AddWithValue("@descripcion", descripcion);
                command.Parameters.AddWithValue("@bandejas", bandejas);
                command.Parameters.AddWithValue("@kilosNeto", kilosNeto);
                command.Parameters.AddWithValue("@fechaVenta", fechaVenta);
                command.Parameters.AddWithValue("@pagadoConCredito", pagadoConCredito);
                command.Parameters.AddWithValue("@rut", rut);
                command.Parameters.AddWithValue("@total", total);

                await command.ExecuteNonQueryAsync();

                // Actualizar inventario
                var updateInventarioCmd = new SQLiteCommand(@"
                UPDATE Inventario 
                SET Kilos = Kilos - @kilosNeto 
                WHERE Codigo = @codigo", connection, transaction);

                updateInventarioCmd.Parameters.AddWithValue("@kilosNeto", kilosNeto);
                updateInventarioCmd.Parameters.AddWithValue("@codigo", codigo);
                await updateInventarioCmd.ExecuteNonQueryAsync();
            }

            // Si es venta a crédito, actualizar deuda del cliente
            if (pagadoConCredito == 1)
            {
                var updateDeudaCmd = new SQLiteCommand(@"
                UPDATE Clientes 
                SET Deuda = Deuda + @monto 
                WHERE RUT = @rut", connection, transaction);

                updateDeudaCmd.Parameters.AddWithValue("@monto", totalVentaActual);
                updateDeudaCmd.Parameters.AddWithValue("@rut", rut);
                await updateDeudaCmd.ExecuteNonQueryAsync();
            }

            // Incrementar número de guía
            var updateGuiaCmd = new SQLiteCommand(@"
            UPDATE Configuracion 
            SET Valor = CAST((CAST(Valor AS INTEGER) + 1) AS TEXT) 
            WHERE Clave = 'UltimoNumeroGuia'", connection, transaction);

            await updateGuiaCmd.ExecuteNonQueryAsync();

            await transaction.CommitAsync();
            MessageBox.Show("Venta registrada exitosamente", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            LimpiarFormulario();
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            MessageBox.Show($"Error al registrar la venta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            finalizarButton.Enabled = true;
            transaction?.Dispose();
            connection?.Close();
            connection?.Dispose();
        }
    }

    private bool ValidarVenta()
    {
        if (rutComboBox.SelectedItem == null)
        {
            MessageBox.Show("Debe seleccionar un cliente", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        if (ventasDataGridView.Rows.Count <= 1)
        {
            MessageBox.Show("Debe agregar al menos un producto", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        foreach (DataGridViewRow row in ventasDataGridView.Rows)
        {
            if (row.IsNewRow) continue;

            if (row.Cells["Codigo"].Value == null || string.IsNullOrEmpty(row.Cells["Codigo"].Value.ToString()))
            {
                MessageBox.Show($"Falta el código en la línea {row.Index + 1}", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (row.Cells["KilosNeto"].Value == null ||
                !double.TryParse(row.Cells["KilosNeto"].Value.ToString(), out double kilos) ||
                kilos <= 0)
            {
                MessageBox.Show($"Los kilos en la línea {row.Index + 1} deben ser un número mayor a 0",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (row.Cells["Precio"].Value == null ||
    !double.TryParse(row.Cells["Precio"].Value.ToString(), out double precio) ||
    precio <= 0)
            {
                MessageBox.Show($"El precio en la línea {row.Index + 1} debe ser un número mayor a 0",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Validar stock disponible
            string codigo = row.Cells["Codigo"].Value.ToString();
            var producto = sqliteService.GetProductoPorCodigo(codigo);
            if (producto == null)
            {
                MessageBox.Show($"El producto con código {codigo} no existe",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (producto.Kilos < kilos)
            {
                MessageBox.Show($"No hay suficiente stock para el producto {codigo}. " +
                    $"Stock disponible: {producto.Kilos:N2} kg",
                    "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
        }

        return true;
    }

    private void LimpiarFormulario()
    {
        // Cargar nuevo número de guía
        CargarNumeroGuia();

        stockLabel.Text = "";

        // Limpiar selecciones
        rutComboBox.SelectedIndex = -1;
        clienteComboBox.SelectedIndex = -1;
        direccionTextBox.Clear();
        giroTextBox.Clear();

        // Reiniciar fecha
        fechaEmisionPicker.Value = DateTime.Now;

        // Limpiar grid
        ventasDataGridView.Rows.Clear();

        // Reiniciar totales
        totalVenta = 0;
        totalVentaTextBox.Text = "0";

        // Desmarcar crédito
        pagarConCreditoCheckBox.Checked = false;
    }
}
