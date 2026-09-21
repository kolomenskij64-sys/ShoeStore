using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace ShoeStore
{
    public class ProductEditForm : Form
    {
        private int? _productId;
        private string _photoPath;
        private string _oldPhotoPath;

        // Элементы
        private Label lblId;
        private TextBox txtId;
        private Label lblName;
        private TextBox txtName;
        private Label lblCategory;
        private ComboBox cmbCategory;
        private Label lblDescription;
        private TextBox txtDescription;
        private Label lblManufacturer;
        private ComboBox cmbManufacturer;
        private Label lblSupplier;
        private ComboBox cmbSupplier;
        private Label lblPrice;
        private TextBox txtPrice;
        private Label lblUnit;
        private ComboBox cmbUnit;
        private Label lblQuantity;
        private TextBox txtQuantity;
        private Label lblDiscount;
        private TextBox txtDiscount;
        private PictureBox picPhoto;
        private Button btnLoadPhoto;
        private Button btnSave;
        private Button btnCancel;

        public ProductEditForm(int? productId = null)
        {
            _productId = productId;

            // Настройки формы
            Text = productId == null ? "Добавить товар" : "Редактировать товар";
            Font = new Font("Times New Roman", 10);
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(720, 520);

            BuildUI();
            LoadCombos();

            if (productId == null)
            {
                lblId.Visible = false;
                txtId.Visible = false;
                txtDiscount.Text = "0";
            }
            else
            {
                txtId.ReadOnly = true;
                LoadProduct(productId.Value);
            }
        }

        private void BuildUI()
        {
            int xLabel = 20;
            int xField = 160;
            int y = 20;
            int dy = 35;

            // ID
            lblId = new Label { Text = "ID:", Left = xLabel, Top = y + 3, Width = 130 };
            txtId = new TextBox { Left = xField, Top = y, Width = 200 };
            Controls.Add(lblId); Controls.Add(txtId);
            y += dy;

            // Наименование
            Controls.Add(new Label { Text = "Наименование:", Left = xLabel, Top = y + 3, Width = 130 });
            txtName = new TextBox { Left = xField, Top = y, Width = 300 };
            Controls.Add(txtName);
            y += dy;

            // Категория
            Controls.Add(new Label { Text = "Категория:", Left = xLabel, Top = y + 3, Width = 130 });
            cmbCategory = new ComboBox { Left = xField, Top = y, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cmbCategory);
            y += dy;

            // Описание
            Controls.Add(new Label { Text = "Описание:", Left = xLabel, Top = y + 3, Width = 130 });
            txtDescription = new TextBox { Left = xField, Top = y, Width = 300, Height = 60, Multiline = true };
            Controls.Add(txtDescription);
            y += 70;

            // Производитель
            Controls.Add(new Label { Text = "Производитель:", Left = xLabel, Top = y + 3, Width = 130 });
            cmbManufacturer = new ComboBox { Left = xField, Top = y, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cmbManufacturer);
            y += dy;

            // Поставщик
            Controls.Add(new Label { Text = "Поставщик:", Left = xLabel, Top = y + 3, Width = 130 });
            cmbSupplier = new ComboBox { Left = xField, Top = y, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cmbSupplier);
            y += dy;

            // Цена
            Controls.Add(new Label { Text = "Цена:", Left = xLabel, Top = y + 3, Width = 130 });
            txtPrice = new TextBox { Left = xField, Top = y, Width = 150 };
            Controls.Add(txtPrice);
            y += dy;

            // Единица
            Controls.Add(new Label { Text = "Ед. изм.:", Left = xLabel, Top = y + 3, Width = 130 });
            cmbUnit = new ComboBox { Left = xField, Top = y, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cmbUnit);
            y += dy;

            // Кол-во
            Controls.Add(new Label { Text = "Кол-во:", Left = xLabel, Top = y + 3, Width = 130 });
            txtQuantity = new TextBox { Left = xField, Top = y, Width = 150 };
            Controls.Add(txtQuantity);
            y += dy;

            // Скидка
            Controls.Add(new Label { Text = "Скидка (%):", Left = xLabel, Top = y + 3, Width = 130 });
            txtDiscount = new TextBox { Left = xField, Top = y, Width = 150 };
            Controls.Add(txtDiscount);
            y += dy + 10;

            // Фото
            picPhoto = new PictureBox
            {
                Left = 500,
                Top = 20,
                Width = 200,
                Height = 150,
                BorderStyle = BorderStyle.FixedSingle,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.WhiteSmoke
            };
            Controls.Add(picPhoto);

            btnLoadPhoto = new Button
            {
                Text = "Загрузить фото",
                Left = 500,
                Top = 180,
                Width = 200,
                Height = 30
            };
            btnLoadPhoto.Click += BtnLoadPhoto_Click;
            Controls.Add(btnLoadPhoto);

            // Кнопки внизу
            btnSave = new Button
            {
                Text = "Сохранить",
                Left = 400,
                Top = y + 20,
                Width = 140,
                Height = 35,
                BackColor = Color.FromArgb(0x00, 0xFA, 0x9A)
            };
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "Отмена",
                Left = 560,
                Top = y + 20,
                Width = 140,
                Height = 35
            };
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnCancel);
        }

        private void LoadCombos()
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                LoadCombo(conn, "SELECT Id, Name FROM Categories", cmbCategory);
                LoadCombo(conn, "SELECT Id, Name FROM Manufacturers", cmbManufacturer);
                LoadCombo(conn, "SELECT Id, Name FROM Suppliers", cmbSupplier);
                LoadCombo(conn, "SELECT Id, Name FROM Units", cmbUnit);
            }
        }

        private void LoadCombo(SqlConnection conn, string sql, ComboBox combo)
        {
            using (var cmd = new SqlCommand(sql, conn))
            using (var r = cmd.ExecuteReader())
            {
                var dt = new DataTable();
                dt.Load(r);
                combo.DataSource = dt;
                combo.DisplayMember = "Name";
                combo.ValueMember = "Id";
            }
        }

        private void LoadProduct(int id)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM Products WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtId.Text = r["Id"].ToString();
                            txtName.Text = r["Name"].ToString();
                            cmbCategory.SelectedValue = r["CategoryId"];
                            txtDescription.Text = r["Description"] == DBNull.Value ? "" : r["Description"].ToString();
                            cmbManufacturer.SelectedValue = r["ManufacturerId"];
                            cmbSupplier.SelectedValue = r["SupplierId"];
                            txtPrice.Text = r["Price"].ToString();
                            cmbUnit.SelectedValue = r["UnitId"];
                            txtQuantity.Text = r["Quantity"].ToString();
                            txtDiscount.Text = r["Discount"].ToString();

                            _oldPhotoPath = r["PhotoPath"] == DBNull.Value ? null : r["PhotoPath"].ToString();
                            _photoPath = _oldPhotoPath;

                            if (!string.IsNullOrEmpty(_photoPath) && File.Exists(_photoPath))
                                picPhoto.Image = Image.FromFile(_photoPath);
                        }
                    }
                }
            }
        }

        private void BtnLoadPhoto_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog { Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp" })
            {
                if (ofd.ShowDialog() != DialogResult.OK) return;

                using (var img = Image.FromFile(ofd.FileName))
                {
                    if (img.Width > 300 || img.Height > 200)
                    {
                        MessageBox.Show("Изображение должно быть не более 300×200 пикселей.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                string dir = Path.Combine(Application.StartupPath, "ProductPhotos");
                Directory.CreateDirectory(dir);

                string newPath = Path.Combine(dir, Path.GetFileName(ofd.FileName));
                if (File.Exists(newPath)) File.Delete(newPath);
                File.Copy(ofd.FileName, newPath);

                if (!string.IsNullOrEmpty(_oldPhotoPath) && File.Exists(_oldPhotoPath)
                    && _oldPhotoPath != newPath)
                {
                    try { File.Delete(_oldPhotoPath); } catch { }
                }

                _photoPath = newPath;
                picPhoto.Image = Image.FromFile(newPath);
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите наименование товара.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть числом не меньше 0.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 0)
            {
                MessageBox.Show("Количество должно быть неотрицательным числом.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!int.TryParse(txtDiscount.Text, out int disc) || disc < 0 || disc > 100)
            {
                MessageBox.Show("Скидка должна быть от 0 до 100.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql;

                if (_productId == null)
                {
                    sql = @"INSERT INTO Products (Article, Name, UnitId, Price, SupplierId,
                            ManufacturerId, CategoryId, Discount, Quantity, Description, PhotoPath)
                            VALUES (@article, @name, @unit, @price, @supplier,
                            @manufacturer, @category, @discount, @qty, @desc, @photo)";
                }
                else
                {
                    sql = @"UPDATE Products SET Name=@name, UnitId=@unit, Price=@price,
                            SupplierId=@supplier, ManufacturerId=@manufacturer, CategoryId=@category,
                            Discount=@discount, Quantity=@qty, Description=@desc, PhotoPath=@photo
                            WHERE Id=@id";
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (_productId == null)
                        cmd.Parameters.AddWithValue("@article", "NEW" + DateTime.Now.Ticks.ToString().Substring(10));
                    else
                        cmd.Parameters.AddWithValue("@id", _productId.Value);

                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@unit", cmbUnit.SelectedValue);
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@supplier", cmbSupplier.SelectedValue);
                    cmd.Parameters.AddWithValue("@manufacturer", cmbManufacturer.SelectedValue);
                    cmd.Parameters.AddWithValue("@category", cmbCategory.SelectedValue);
                    cmd.Parameters.AddWithValue("@discount", disc);
                    cmd.Parameters.AddWithValue("@qty", qty);
                    cmd.Parameters.AddWithValue("@desc", txtDescription.Text);
                    cmd.Parameters.AddWithValue("@photo", (object)_photoPath ?? DBNull.Value);

                    cmd.ExecuteNonQuery();
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}