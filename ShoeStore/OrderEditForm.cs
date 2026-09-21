using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ShoeStore
{
    public class OrderEditForm : Form
    {
        private int? _orderId;

        private TextBox txtArticle;
        private ComboBox cmbStatus;
        private ComboBox cmbPickupPoint;
        private DateTimePicker dtpOrderDate;
        private DateTimePicker dtpDeliveryDate;
        private TextBox txtReceiveCode;
        private Button btnSave;
        private Button btnDelete;
        private Button btnCancel;

        public OrderEditForm(int? orderId = null)
        {
            _orderId = orderId;

            Text = orderId == null ? "Добавить заказ" : "Редактировать заказ";
            Font = new Font("Times New Roman", 10);
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(500, 400);

            BuildUI();
            LoadCombos();

            if (orderId != null)
                LoadOrder(orderId.Value);
        }

        private void BuildUI()
        {
            int xL = 20, xF = 180, y = 20, dy = 40;

            Controls.Add(new Label { Text = "Артикул:", Left = xL, Top = y + 3, Width = 150 });
            txtArticle = new TextBox { Left = xF, Top = y, Width = 250 };
            Controls.Add(txtArticle);
            y += dy;

            Controls.Add(new Label { Text = "Статус:", Left = xL, Top = y + 3, Width = 150 });
            cmbStatus = new ComboBox { Left = xF, Top = y, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cmbStatus);
            y += dy;

            Controls.Add(new Label { Text = "Адрес пункта выдачи:", Left = xL, Top = y + 3, Width = 150 });
            cmbPickupPoint = new ComboBox { Left = xF, Top = y, Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(cmbPickupPoint);
            y += dy;

            Controls.Add(new Label { Text = "Дата заказа:", Left = xL, Top = y + 3, Width = 150 });
            dtpOrderDate = new DateTimePicker { Left = xF, Top = y, Width = 250, Format = DateTimePickerFormat.Short };
            Controls.Add(dtpOrderDate);
            y += dy;

            Controls.Add(new Label { Text = "Дата доставки:", Left = xL, Top = y + 3, Width = 150 });
            dtpDeliveryDate = new DateTimePicker { Left = xF, Top = y, Width = 250, Format = DateTimePickerFormat.Short };
            Controls.Add(dtpDeliveryDate);
            y += dy;

            Controls.Add(new Label { Text = "Код получения:", Left = xL, Top = y + 3, Width = 150 });
            txtReceiveCode = new TextBox { Left = xF, Top = y, Width = 250 };
            Controls.Add(txtReceiveCode);
            y += dy + 10;

            btnSave = new Button
            {
                Text = "Сохранить",
                Left = 100,
                Top = y,
                Width = 120,
                Height = 35,
                BackColor = Color.FromArgb(0x00, 0xFA, 0x9A)
            };
            btnSave.Click += BtnSave_Click;
            Controls.Add(btnSave);

            btnDelete = new Button
            {
                Text = "Удалить",
                Left = 230,
                Top = y,
                Width = 100,
                Height = 35,
                BackColor = Color.LightCoral
            };
            btnDelete.Click += BtnDelete_Click;
            btnDelete.Enabled = _orderId != null; // только для существующего заказа
            Controls.Add(btnDelete);

            btnCancel = new Button
            {
                Text = "Отмена",
                Left = 340,
                Top = y,
                Width = 100,
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

                using (var cmd = new SqlCommand("SELECT Id, Name FROM OrderStatuses", conn))
                using (var r = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(r);
                    cmbStatus.DataSource = dt;
                    cmbStatus.DisplayMember = "Name";
                    cmbStatus.ValueMember = "Id";
                }

                using (var cmd = new SqlCommand("SELECT Id, Address FROM PickupPoints", conn))
                using (var r = cmd.ExecuteReader())
                {
                    var dt = new DataTable();
                    dt.Load(r);
                    cmbPickupPoint.DataSource = dt;
                    cmbPickupPoint.DisplayMember = "Address";
                    cmbPickupPoint.ValueMember = "Id";
                }
            }
        }

        private void LoadOrder(int id)
        {
            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT * FROM Orders WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            txtArticle.Text = r["Article"].ToString();
                            cmbStatus.SelectedValue = r["StatusId"];
                            cmbPickupPoint.SelectedValue = r["PickupPointId"];
                            dtpOrderDate.Value = Convert.ToDateTime(r["OrderDate"]);
                            if (r["DeliveryDate"] != DBNull.Value)
                                dtpDeliveryDate.Value = Convert.ToDateTime(r["DeliveryDate"]);
                            txtReceiveCode.Text = r["ReceiveCode"].ToString();
                        }
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtArticle.Text))
            {
                MessageBox.Show("Введите артикул заказа.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                string sql;

                if (_orderId == null)
                {
                    sql = @"INSERT INTO Orders (Article, OrderDate, DeliveryDate, PickupPointId,
                            UserId, ReceiveCode, StatusId)
                            VALUES (@art, @od, @dd, @pp, @uid, @code, @st)";
                }
                else
                {
                    sql = @"UPDATE Orders SET Article=@art, OrderDate=@od, DeliveryDate=@dd,
                            PickupPointId=@pp, ReceiveCode=@code, StatusId=@st
                            WHERE Id=@id";
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (_orderId != null) cmd.Parameters.AddWithValue("@id", _orderId.Value);
                    else cmd.Parameters.AddWithValue("@uid", Session.UserId == 0 ? 4 : Session.UserId);

                    cmd.Parameters.AddWithValue("@art", txtArticle.Text);
                    cmd.Parameters.AddWithValue("@od", dtpOrderDate.Value);
                    cmd.Parameters.AddWithValue("@dd", dtpDeliveryDate.Value);
                    cmd.Parameters.AddWithValue("@pp", cmbPickupPoint.SelectedValue);
                    cmd.Parameters.AddWithValue("@code", txtReceiveCode.Text);
                    cmd.Parameters.AddWithValue("@st", cmbStatus.SelectedValue);

                    cmd.ExecuteNonQuery();
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (_orderId == null) return;

            var result = MessageBox.Show("Удалить заказ? Данные удалятся безвозвратно.",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result != DialogResult.Yes) return;

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand("DELETE FROM Orders WHERE Id=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", _orderId.Value);
                    cmd.ExecuteNonQuery();
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}