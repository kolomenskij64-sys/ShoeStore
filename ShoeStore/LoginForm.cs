using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ShoeStore
{
    public partial class LoginForm : Form
    {
        // Строка подключения к БД
        private string connectionString =
            @"Server=DESKTOP-HLP1JKG;Database=[ShoeStore];Integrated Security=True;TrustServerCertificate=True;";

        // Публичные свойства для передачи данных в MainForm
        public static int CurrentUserId { get; private set; }
        public static string CurrentUserName { get; private set; }
        public static string CurrentUserRole { get; private set; }

        public LoginForm()
        {
            InitializeComponent();
        }

        // 🔵 КНОПКА "ВОЙТИ" - проверка логина и пароля
        private void btnLogin_Click(object sender, EventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Введите логин и пароль!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"SELECT u.UserID, u.FullName, r.RoleName 
                                     FROM dbo.Users u 
                                     JOIN dbo.Roles r ON u.RoleID = r.RoleID 
                                     WHERE u.Login = @login AND u.Password = @password";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@login", login);
                        cmd.Parameters.AddWithValue("@password", password);

                        using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);

                            if (dt.Rows.Count > 0)
                            {
                                // ✅ Успешная авторизация
                                CurrentUserId = Convert.ToInt32(dt.Rows[0]["UserID"]);
                                CurrentUserName = dt.Rows[0]["FullName"].ToString();
                                CurrentUserRole = dt.Rows[0]["RoleName"].ToString();

                                MessageBox.Show($"Добро пожаловать, {CurrentUserName}!",
                                    "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Открываем главную форму
                                MainForm mainForm = new MainForm();
                                mainForm.Show();
                                this.Hide();
                            }
                            else
                            {
                                MessageBox.Show("Неверный логин или пароль!", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения к БД: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 🟢 КНОПКА "ГОСТЬ" - вход без авторизации
        private void btnGuest_Click(object sender, EventArgs e)
        {
            // Устанавливаем данные для гостя
            CurrentUserId = 0;
            CurrentUserName = "Гость";
            CurrentUserRole = "Гость";

            MessageBox.Show("Вы вошли как Гость", "Гость",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Открываем главную форму
            MainForm mainForm = new MainForm();
            mainForm.Show();
            this.Hide();
        }

        private void lblLogin_Click(object sender, EventArgs e)
        {

        }
    }
}