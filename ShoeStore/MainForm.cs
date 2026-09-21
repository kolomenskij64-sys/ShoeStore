using System;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ShoeStore;

namespace ShoeStore
{
    public class MainForm : Form
    {
        private Label lblUser;
        private Button btnLogout;
        private Button btnAdd;
        private Button btnOrders;
        private TextBox txtSearch;
        private ComboBox cmbSupplier;
        private ComboBox cmbSort;
        private FlowLayoutPanel panelProducts;
        private Panel topPanel;
        private ProductEditForm _editForm;
        private OrderForm _ordersForm;

        public MainForm()
        {
            Text = "Список товаров — ООО «Обувь»";
            Font = new Font("Times New Roman", 10);
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 500);

            BuildUI();

            Load += MainForm_Load;
        }

        private void BuildUI()
        {
            // 1. Список товаров (Fill)
            panelProducts = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            Controls.Add(panelProducts);

            // 2. Верхняя панель (Top)
            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 100,
                BackColor = Color.White
            };
            Controls.Add(topPanel);
            topPanel.BringToFront();

            // ФИО справа (верхняя строка)
            lblUser = new Label
            {
                AutoSize = false,
                Width = 350,
                Height = 30,
                TextAlign = ContentAlignment.MiddleRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Times New Roman", 10, FontStyle.Bold),
                Left = ClientSize.Width - 370,
                Top = 15
            };
            topPanel.Controls.Add(lblUser);

            // Кнопка Выход (правее, вторая строка)
            btnLogout = new Button
            {
                Text = "Выход",
                Width = 100,
                Height = 30,
                BackColor = Color.FromArgb(0x00, 0xFA, 0x9A),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Left = ClientSize.Width - 120,
                Top = 55
            };
            btnLogout.Click += BtnLogout_Click;
            topPanel.Controls.Add(btnLogout);

            // Кнопка "Заказы" (Менеджер и Админ)
            if (Session.Role == "Менеджер" || Session.Role == "Администратор")
            {
                btnOrders = new Button
                {
                    Text = "Заказы",
                    Width = 160,
                    Height = 30,
                    Left = 20,
                    Top = 15,
                    BackColor = Color.FromArgb(0x7F, 0xFF, 0x00)
                };
                btnOrders.Click += (s, e) => OpenOrdersForm();
                topPanel.Controls.Add(btnOrders);
            }

            // Кнопка "Добавить товар" (только Админ) — рядом с "Заказы"
            if (Session.Role == "Администратор")
            {
                btnAdd = new Button
                {
                    Text = "Добавить товар",
                    Width = 160,
                    Height = 30,
                    Left = 200,
                    Top = 15,
                    BackColor = Color.FromArgb(0x00, 0xFA, 0x9A)
                };
                btnAdd.Click += (s, e) => OpenEditForm(null);
                topPanel.Controls.Add(btnAdd);
            }

            // Поиск и фильтры — вторая строка слева
            txtSearch = new TextBox
            {
                Left = 20,
                Top = 55,
                Width = 200,
                Font = new Font("Times New Roman", 10)
            };
            topPanel.Controls.Add(txtSearch);

            cmbSupplier = new ComboBox
            {
                Left = 240,
                Top = 55,
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Times New Roman", 10)
            };
            topPanel.Controls.Add(cmbSupplier);

            cmbSort = new ComboBox
            {
                Left = 440,
                Top = 55,
                Width = 180,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Times New Roman", 10)
            };
            topPanel.Controls.Add(cmbSort);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            lblUser.Text = Session.FullName;

            bool canSearch = Session.Role == "Менеджер" || Session.Role == "Администратор";
            txtSearch.Visible = canSearch;
            cmbSupplier.Visible = canSearch;
            cmbSort.Visible = canSearch;

            cmbSort.Items.AddRange(new object[] { "Без сортировки", "Кол-во ↑", "Кол-во ↓" });
            cmbSort.SelectedIndex = 0;

            LoadSuppliers();
            LoadProducts();

            txtSearch.TextChanged += (s, ev) => LoadProducts();
            cmbSupplier.SelectedIndexChanged += (s, ev) => LoadProducts();
            cmbSort.SelectedIndexChanged += (s, ev) => LoadProducts();
        }

        private void LoadSuppliers()
        {
            cmbSupplier.Items.Clear();
            cmbSupplier.Items.Add("Все поставщики");

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT Name FROM Suppliers ORDER BY Name", conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        cmbSupplier.Items.Add(r["Name"].ToString());
            }
            cmbSupplier.SelectedIndex = 0;
        }

        private void LoadProducts()
        {
            panelProducts.Controls.Clear();

            string sql = @"SELECT p.Id, p.Name, c.Name AS Category, p.Description,
                                  m.Name AS Manufacturer, s.Name AS Supplier,
                                  p.Price, u.Name AS Unit, p.Quantity, p.Discount, p.PhotoPath
                           FROM Products p
                           JOIN Categories    c ON c.Id = p.CategoryId
                           JOIN Manufacturers m ON m.Id = p.ManufacturerId
                           JOIN Suppliers     s ON s.Id = p.SupplierId
                           JOIN Units         u ON u.Id = p.UnitId
                           WHERE 1=1";

            bool canSearch = Session.Role == "Менеджер" || Session.Role == "Администратор";

            if (canSearch && !string.IsNullOrWhiteSpace(txtSearch.Text))
                sql += @" AND (p.Name LIKE @s OR c.Name LIKE @s OR p.Description LIKE @s
                              OR m.Name LIKE @s OR s.Name LIKE @s)";

            if (canSearch && cmbSupplier.SelectedIndex > 0)
                sql += " AND s.Name = @supplier";

            if (canSearch && cmbSort.SelectedIndex == 1) sql += " ORDER BY p.Quantity ASC";
            if (canSearch && cmbSort.SelectedIndex == 2) sql += " ORDER BY p.Quantity DESC";

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (sql.Contains("@s"))
                        cmd.Parameters.AddWithValue("@s", "%" + txtSearch.Text + "%");
                    if (sql.Contains("@supplier"))
                        cmd.Parameters.AddWithValue("@supplier", cmbSupplier.SelectedItem.ToString());

                    using (var reader = cmd.ExecuteReader())
                        while (reader.Read())
                            panelProducts.Controls.Add(CreateProductCard(reader));
                }
            }
        }

        private Panel CreateProductCard(SqlDataReader r)
        {
            int discount = r["Discount"] == DBNull.Value ? 0 : (int)r["Discount"];
            int quantity = (int)r["Quantity"];
            decimal price = (decimal)r["Price"];
            int productId = (int)r["Id"];

            var card = new Panel
            {
                Width = 1100,
                Height = 140,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5)
            };

            if (discount > 15) card.BackColor = Color.FromArgb(0x2E, 0x8B, 0x57);
            else if (quantity == 0) card.BackColor = Color.LightBlue;
            else card.BackColor = Color.White;

            var pic = new PictureBox { Left = 10, Top = 10, Width = 120, Height = 120, SizeMode = PictureBoxSizeMode.Zoom };
            string photo = r["PhotoPath"] == DBNull.Value ? null : r["PhotoPath"].ToString();

            // Если фото есть — показываем его. Иначе — заглушка picture.png
            if (!string.IsNullOrEmpty(photo) && File.Exists(photo))
            {
                pic.Image = Image.FromFile(photo);
            }
            else
            {
                string stub = Path.Combine(Application.StartupPath, "picture.png");
                if (File.Exists(stub))
                    pic.Image = Image.FromFile(stub);
            }
            card.Controls.Add(pic);

            int x = 145;

            card.Controls.Add(new Label
            {
                Left = x,
                Top = 10,
                AutoSize = true,
                Font = new Font("Times New Roman", 11, FontStyle.Bold),
                Text = r["Category"] + " | " + r["Name"]
            });

            card.Controls.Add(new Label { Left = x, Top = 35, AutoSize = true, Text = "Описание: " + r["Description"] });
            card.Controls.Add(new Label { Left = x, Top = 55, AutoSize = true, Text = "Производитель: " + r["Manufacturer"] });
            card.Controls.Add(new Label { Left = x, Top = 75, AutoSize = true, Text = "Поставщик: " + r["Supplier"] });

            if (discount > 0)
            {
                decimal finalPrice = price * (100 - discount) / 100m;

                card.Controls.Add(new Label
                {
                    Left = x,
                    Top = 95,
                    AutoSize = true,
                    Text = "Цена: " + price.ToString("F2"),
                    ForeColor = Color.Red,
                    Font = new Font("Times New Roman", 10, FontStyle.Strikeout)
                });
                card.Controls.Add(new Label
                {
                    Left = x + 140,
                    Top = 95,
                    AutoSize = true,
                    Text = finalPrice.ToString("F2"),
                    ForeColor = Color.Black,
                    Font = new Font("Times New Roman", 10)
                });
            }
            else
            {
                card.Controls.Add(new Label { Left = x, Top = 95, AutoSize = true, Text = "Цена: " + price.ToString("F2") });
            }

            card.Controls.Add(new Label
            {
                Left = x,
                Top = 115,
                AutoSize = true,
                Text = "Ед. изм.: " + r["Unit"] + "   Кол-во: " + quantity
            });

            card.Controls.Add(new Label
            {
                Left = 950,
                Top = 55,
                Width = 130,
                Height = 40,
                Text = "Действующая скидка: " + discount + "%",
                TextAlign = ContentAlignment.MiddleCenter
            });

            if (Session.Role == "Администратор")
            {
                card.Cursor = Cursors.Hand;
                card.Click += (s, ev) => OpenEditForm(productId);
                foreach (Control c in card.Controls)
                    c.Click += (s, ev) => OpenEditForm(productId);
            }

            return card;
        }

        private void BtnLogout_Click(object sender, EventArgs e)
        {
            Session.UserId = 0;
            Session.FullName = null;
            Session.Role = null;

            new LoginForm().Show();
            Close();
        }

        private void OpenEditForm(int? productId)
        {
            if (_editForm != null && !_editForm.IsDisposed)
            {
                _editForm.Activate();
                return;
            }
            _editForm = new ProductEditForm(productId);
            _editForm.FormClosed += (s, ev) =>
            {
                _editForm = null;
                LoadProducts();
            };
            _editForm.Show();
        }

        private void OpenOrdersForm()
        {
            if (_ordersForm != null && !_ordersForm.IsDisposed)
            {
                _ordersForm.Activate();
                return;
            }
            _ordersForm = new OrderForm();
            _ordersForm.FormClosed += (s, ev) => _ordersForm = null;
            _ordersForm.Show();
        }
    }
}