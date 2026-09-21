using System;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace ShoeStore
{
    public class OrdersForm : Form
    {
        private FlowLayoutPanel panelOrders;
        private Panel topPanel;
        private Button btnAdd;
        private Label lblTitle;
        private OrderEditForm _editForm;

        public OrdersForm()
        {
            Text = "Заказы — ООО «Обувь»";
            Font = new Font("Times New Roman", 10);
            BackColor = Color.White;
            StartPosition = FormStartPosition.CenterParent;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(900, 500);

            BuildUI();
            Load += (s, e) => LoadOrders();
        }

        private void BuildUI()
        {
            // Список заказов
            panelOrders = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.White,
                Padding = new Padding(10)
            };
            Controls.Add(panelOrders);

            // Верхняя панель
            topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White
            };
            Controls.Add(topPanel);
            topPanel.BringToFront();

            lblTitle = new Label
            {
                Text = "Список заказов",
                Left = 20,
                Top = 15,
                Width = 300,
                Height = 30,
                Font = new Font("Times New Roman", 12, FontStyle.Bold)
            };
            topPanel.Controls.Add(lblTitle);

            // Кнопка "Добавить заказ" (только Админ)
            if (Session.Role == "Администратор")
            {
                btnAdd = new Button
                {
                    Text = "Добавить заказ",
                    Width = 160,
                    Height = 30,
                    Left = 340,
                    Top = 15,
                    BackColor = Color.FromArgb(0x00, 0xFA, 0x9A)
                };
                btnAdd.Click += (s, e) => OpenEditForm(null);
                topPanel.Controls.Add(btnAdd);
            }
        }

        private void LoadOrders()
        {
            panelOrders.Controls.Clear();

            string sql = @"SELECT o.Id, o.Article, o.OrderDate, o.DeliveryDate,
                                  o.ReceiveCode, s.Name AS StatusName,
                                  p.Address AS PickupAddress
                           FROM Orders o
                           JOIN OrderStatuses s ON s.Id = o.StatusId
                           JOIN PickupPoints  p ON p.Id = o.PickupPointId
                           ORDER BY o.Id";

            using (var conn = DbConnection.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                using (var r = cmd.ExecuteReader())
                    while (r.Read())
                        panelOrders.Controls.Add(CreateOrderCard(r));
            }
        }

        private Panel CreateOrderCard(SqlDataReader r)
        {
            int orderId = (int)r["Id"];

            var card = new Panel
            {
                Width = 1100,
                Height = 120,
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                BackColor = Color.White
            };

            card.Controls.Add(new Label
            {
                Left = 20,
                Top = 10,
                AutoSize = true,
                Font = new Font("Times New Roman", 11, FontStyle.Bold),
                Text = "Артикул заказа: " + r["Article"]
            });

            card.Controls.Add(new Label
            {
                Left = 20,
                Top = 35,
                AutoSize = true,
                Text = "Статус заказа: " + r["StatusName"]
            });

            card.Controls.Add(new Label
            {
                Left = 20,
                Top = 55,
                AutoSize = true,
                Text = "Адрес пункта выдачи: " + r["PickupAddress"]
            });

            card.Controls.Add(new Label
            {
                Left = 20,
                Top = 75,
                AutoSize = true,
                Text = "Дата заказа: " + Convert.ToDateTime(r["OrderDate"]).ToString("dd.MM.yyyy")
            });

            card.Controls.Add(new Label
            {
                Left = 700,
                Top = 10,
                Width = 350,
                AutoSize = false,
                TextAlign = ContentAlignment.TopRight,
                Text = "Дата доставки: " + (r["DeliveryDate"] == DBNull.Value ? "—" :
                       Convert.ToDateTime(r["DeliveryDate"]).ToString("dd.MM.yyyy"))
            });

            // Админ: клик по карточке — редактирование
            if (Session.Role == "Администратор")
            {
                card.Cursor = Cursors.Hand;
                card.Click += (s, e) => OpenEditForm(orderId);
                foreach (Control c in card.Controls)
                    c.Click += (s, e) => OpenEditForm(orderId);
            }

            return card;
        }

        private void OpenEditForm(int? orderId)
        {
            if (_editForm != null && !_editForm.IsDisposed)
            {
                _editForm.Activate();
                return;
            }
            _editForm = new OrderEditForm(orderId);
            _editForm.FormClosed += (s, e) =>
            {
                _editForm = null;
                LoadOrders();
            };
            _editForm.Show();
        }
    }
}