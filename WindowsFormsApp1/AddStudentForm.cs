using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AddStudentForm : Form
    {
        private string connectionString;
        private bool isEditMode;
        private int studentId;

        public AddStudentForm()
        {
            InitializeComponent();
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            this.Load += new System.EventHandler(this.AddStudentForm_Load);
            textBox4.KeyPress += new KeyPressEventHandler(textBox4_KeyPress);
        }

        // Constructor for editing
        public AddStudentForm(int id, string nume, string prenume, string oras, string telefon, int idClasa) : this()
        {
            isEditMode = true;
            studentId = id; // Set the student ID for editing
            textBox1.Text = nume;
            textBox2.Text = prenume;
            textBox3.Text = oras;
            textBox4.Text = telefon;
            comboBox1.SelectedItem = idClasa;
        }

        private void AddStudentForm_Load(object sender, EventArgs e)
        {
            PopulateComboBox();
        }

        private void PopulateComboBox()
        {
            comboBox1.Items.Clear();

            for (int i = 1; i <= 10; i++)
            {
                comboBox1.Items.Add(i);
            }

            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        private int GetNextStudentId()
        {
            int nextId = 1; // Default ID if table is empty
            string query = "SELECT MAX(id_elev) FROM ELEVI";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        object result = command.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            nextId = Convert.ToInt32(result) + 1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while getting the next student ID: " + ex.Message);
                }
            }
            return nextId;
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only numbers and control keys
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string nume = textBox1.Text;
            string prenume = textBox2.Text;
            string oras = textBox3.Text;
            string telefon = textBox4.Text;

            int idClasa = (int)comboBox1.SelectedItem;

            if (string.IsNullOrWhiteSpace(nume) || string.IsNullOrWhiteSpace(prenume) || string.IsNullOrWhiteSpace(oras) || string.IsNullOrWhiteSpace(telefon))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            string query;

            if (isEditMode)
            {
                query = "UPDATE ELEVI SET nume = @nume, prenume = @prenume, oras = @oras, telefon = @telefon, id_clasa = @id_clasa WHERE id_elev = @id_elev";
            }
            else
            {
                query = "INSERT INTO ELEVI (id_elev, nume, prenume, oras, telefon, id_clasa) VALUES (@id_elev, @nume, @prenume, @oras, @telefon, @id_clasa)";
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        if (isEditMode)
                        {
                            command.Parameters.AddWithValue("@id_elev", studentId); // Use existing ID for update
                        }
                        else
                        {
                            int newStudentId = GetNextStudentId(); // Get next ID for new student
                            command.Parameters.AddWithValue("@id_elev", newStudentId);
                        }
                        command.Parameters.AddWithValue("@nume", nume);
                        command.Parameters.AddWithValue("@prenume", prenume);
                        command.Parameters.AddWithValue("@oras", oras);
                        command.Parameters.AddWithValue("@telefon", telefon);
                        command.Parameters.AddWithValue("@id_clasa", idClasa);

                        int rowsAffected = command.ExecuteNonQuery();

                        MessageBox.Show($"{rowsAffected} row(s) {(isEditMode ? "updated" : "added")} successfully.");

                        this.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }
    }
}
