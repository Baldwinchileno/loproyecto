using System;
using System.Drawing;
using System.Windows.Forms;
using AdminSERMAC.Services;
using AdminSERMAC.Services.Database;
using Microsoft.Extensions.Logging;

namespace AdminSERMAC.Forms
{
    public class InventarioForm : Form
    {
        private readonly ILogger<InventarioForm> _logger;
        private readonly SQLiteService _sqliteService;
        private readonly IInventarioDatabaseService _inventarioService;

        // Botones
        private Button comprarProductosButton;
        private Button cuadernoComprasButton;
        private Button visualizarInventarioButton;
        private Button crearProductoButton;
        private Button traspasoLocalButton;
        private Panel mainPanel;
        private Label titleLabel;

        public InventarioForm(
            ILogger<InventarioForm> logger,
            SQLiteService sqliteService,
            IInventarioDatabaseService inventarioService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _sqliteService = sqliteService ?? throw new ArgumentNullException(nameof(sqliteService));
            _inventarioService = inventarioService ?? throw new ArgumentNullException(nameof(inventarioService));

            InitializeComponents();
        }

        private void InitializeComponents()
        {
            // Configuración del formulario
            this.Text = "Gestión de Inventario - SERMAC";
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Panel principal
            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };

            // Título
            titleLabel = new Label
            {
                Text = "Gestión de Inventario",
                Font = new Font("Segoe UI", 24, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(50, 20),
                ForeColor = Color.FromArgb(0, 122, 204)
            };

            // Botones
            comprarProductosButton = CreateMenuButton("Comprar Productos", 100);
            cuadernoComprasButton = CreateMenuButton("Cuaderno de Compras", 170);
            visualizarInventarioButton = CreateMenuButton("Visualizar Inventario", 240);
            crearProductoButton = CreateMenuButton("Crear Producto", 310);
            traspasoLocalButton = CreateMenuButton("Traspaso a Local", 380);

            // Configurar eventos para los botones
            comprarProductosButton.Click += ComprarProductosButton_Click;
            cuadernoComprasButton.Click += CuadernoComprasButton_Click;
            visualizarInventarioButton.Click += VisualizarInventarioButton_Click;
            crearProductoButton.Click += CrearProductoButton_Click;
            traspasoLocalButton.Click += TraspasoLocalButton_Click;

            // Agregar controles al panel
            mainPanel.Controls.AddRange(new Control[] {
                titleLabel,
                comprarProductosButton,
                cuadernoComprasButton,
                visualizarInventarioButton,
                crearProductoButton,
                traspasoLocalButton
            });

            // Agregar panel al formulario
            this.Controls.Add(mainPanel);
        }

        private Button CreateMenuButton(string text, int top)
        {
            var button = new Button
            {
                Text = text,
                Top = top,
                Left = 50,
                Width = 250,
                Height = 45,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(0, 122, 204),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };

            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = ControlPaint.Light(button.BackColor);

            return button;
        }

        private void ComprarProductosButton_Click(object sender, EventArgs e)
        {
            try
            {
                var compraForm = new CompraInventarioForm(_sqliteService, _logger, _inventarioService);
                compraForm.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir el formulario de compra de productos");
                MessageBox.Show($"Error al abrir el formulario de compra de productos: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CuadernoComprasButton_Click(object sender, EventArgs e)
        {
            try
            {
                var cuadernoComprasForm = new CuadernoComprasForm(_sqliteService, _logger, _inventarioService);
                cuadernoComprasForm.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir el cuaderno de compras");
                MessageBox.Show($"Error al abrir el cuaderno de compras: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void VisualizarInventarioButton_Click(object sender, EventArgs e)
        {
            try
            {
                var visualizarInventarioForm = new VisualizarInventarioForm(_sqliteService, _logger);
                visualizarInventarioForm.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir el visualizador de inventario");
                MessageBox.Show($"Error al abrir el visualizador de inventario: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CrearProductoButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (var crearProductoForm = new CrearProductoForm(_logger))
                {
                    if (crearProductoForm.ShowDialog() == DialogResult.OK)
                    {
                        MessageBox.Show("Producto creado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir el formulario de creación de productos");
                MessageBox.Show($"Error al abrir el formulario de creación de productos: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TraspasoLocalButton_Click(object sender, EventArgs e)
        {
            try
            {
                var traspasoForm = new TraspasosForm(_logger, _inventarioService, _sqliteService);
                traspasoForm.Show();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al abrir el formulario de traspasos");
                MessageBox.Show($"Error al abrir el formulario de traspasos: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
