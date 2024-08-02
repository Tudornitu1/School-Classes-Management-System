using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class AddProfesor : Form
    {
        private string connectionString;
        private bool isEditMode;
        private int professorId;

        public AddProfesor()
        {
            InitializeComponent();
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;
            this.Load += new System.EventHandler(this.AddProfesor_Load);
            textBox4.KeyPress += new KeyPressEventHandler(textBox4_KeyPress);
        }

        // Constructor for editing
        public AddProfesor(int id, string nume, string prenume, string oras, string telefon) : this()
        {
            isEditMode = true;
            professorId = id;
            textBox1.Text = nume;
            textBox2.Text = prenume;
            textBox3.Text = oras;
            textBox4.Text = telefon;
            LoadSelectedClasses();
        }

        private void AddProfesor_Load(object sender, EventArgs e)
        {
            PopulateCheckedListBox();
        }

        private void PopulateCheckedListBox()
        {
            checkedListBox1.Items.Clear();
            string query = "SELECT id_clasa FROM CLASE";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        checkedListBox1.Items.Add(reader.GetInt32(0));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void LoadSelectedClasses()
        {
            string query = "SELECT id_clasa FROM PROFESORI_CLASE WHERE id_prof = @id_prof";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id_prof", professorId);
                    SqlDataReader reader = command.ExecuteReader();

                    // Uncheck all items first
                    for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    {
                        checkedListBox1.SetItemChecked(i, false);
                    }

                    while (reader.Read())
                    {
                        int idClasa = reader.GetInt32(0);
                        int index = checkedListBox1.Items.IndexOf(idClasa);
                        if (index >= 0)
                        {
                            checkedListBox1.SetItemChecked(index, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
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

            if (string.IsNullOrWhiteSpace(nume) || string.IsNullOrWhiteSpace(prenume) || string.IsNullOrWhiteSpace(oras) || string.IsNullOrWhiteSpace(telefon))
            {
                MessageBox.Show("Please fill all fields.");
                return;
            }

            if (isEditMode)
            {
                UpdateProfesor(nume, prenume, oras, telefon);
            }
            else
            {
                AddNewProfesor(nume, prenume, oras, telefon);
            }
        }

        private void AddNewProfesor(string nume, string prenume, string oras, string telefon)
        {
            // Get the next available ID
            int newProfessorId = GetNextProfessorId();

            // Insert the new professor with the obtained ID
            string query = "INSERT INTO PROFESORI (id_prof, nume, prenume, oras, telefon) VALUES (@id_prof, @nume, @prenume, @oras, @telefon)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id_prof", newProfessorId);
                    command.Parameters.AddWithValue("@nume", nume);
                    command.Parameters.AddWithValue("@prenume", prenume);
                    command.Parameters.AddWithValue("@oras", oras);
                    command.Parameters.AddWithValue("@telefon", telefon);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        professorId = newProfessorId; // Set the professorId to the newly inserted ID
                        // Save class associations
                        SaveProfessorClasses();

                        MessageBox.Show("Added successfully.");
                        this.Close();
                    }
                    else
                    {
                        throw new Exception("Failed to add the professor.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while adding the professor: " + ex.Message);
                }
            }
        }

        private void UpdateProfesor(string nume, string prenume, string oras, string telefon)
        {
            string query = "UPDATE PROFESORI SET nume = @nume, prenume = @prenume, oras = @oras, telefon = @telefon WHERE id_prof = @id_prof";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id_prof", professorId);
                    command.Parameters.AddWithValue("@nume", nume);
                    command.Parameters.AddWithValue("@prenume", prenume);
                    command.Parameters.AddWithValue("@oras", oras);
                    command.Parameters.AddWithValue("@telefon", telefon);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        // Save class associations
                        SaveProfessorClasses();

                        MessageBox.Show("Updated successfully.");
                        this.Close();
                    }
                    else
                    {
                        throw new Exception("Failed to update the professor.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while updating the professor: " + ex.Message);
                }
            }
        }

        private int GetNextProfessorId()
        {
            int nextId = 1; // Default ID if the table is empty
            string query = "SELECT ISNULL(MAX(id_prof), 0) FROM PROFESORI";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    object result = command.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        nextId = Convert.ToInt32(result) + 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while getting the next professor ID: " + ex.Message);
                }
            }

            return nextId;
        }

        private void SaveProfessorClasses()
        {
            string deleteQuery = "DELETE FROM PROFESORI_CLASE WHERE id_prof = @id_prof";
            string insertQuery = "INSERT INTO PROFESORI_CLASE (id_prof, id_clasa) VALUES (@id_prof, @id_clasa)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // Delete existing class associations
                    using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@id_prof", professorId);
                        deleteCommand.ExecuteNonQuery();
                    }

                    // Insert new class associations
                    foreach (int idClasa in checkedListBox1.CheckedItems)
                    {
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@id_prof", professorId);
                            insertCommand.Parameters.AddWithValue("@id_clasa", idClasa);
                            insertCommand.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while saving professor classes: " + ex.Message);
                }
            }
        }
    }
}
