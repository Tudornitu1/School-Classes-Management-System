using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private string connectionString;

        public Form1()
        {
            InitializeComponent();

            // Initialize connection string from configuration
            connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["MyDbConnection"].ConnectionString;

            // Load data into DataGridViews
            LoadData();
            LoadDataProfesori();
            LoadDataClase();

            // Populate ComboBox with class IDs
            PopulateComboBox();
            PopulateComboBox2();

            // Event handlers for DataGridViews
            this.dataGridView1.CellDoubleClick += new DataGridViewCellEventHandler(this.dataGridView1_CellDoubleClick);
            this.dataGridView2.CellDoubleClick += DataGridView2_CellDoubleClick;
            this.dataGridView3.CellDoubleClick += DataGridView3_CellDoubleClick;

            // Set initial visibility for DataGridViews
            dataGridView1.Visible = true;
            dataGridView2.Visible = false;
            dataGridView3.Visible = false;
            dataGridView4.Visible = false;
            dataGridView5.Visible = false;
            label1.Visible = true;
            comboBox1.Visible = true;
            button4.Visible = true;
            label2.Visible = false;
            comboBox2.Visible = false;
            button10.Visible = false;
            label3.Visible = false;
            label4.Visible = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Open AddStudent form
            AddStudentForm addStudentForm = new AddStudentForm();
            addStudentForm.FormClosed += new FormClosedEventHandler(AddEditForm_FormClosed);
            addStudentForm.ShowDialog();
        }

        private void LoadData()
        {
            string query = "SELECT id_elev, nume, prenume, oras, telefon, id_clasa FROM ELEVI";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGridView1.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = this.dataGridView1.Rows[e.RowIndex];

                int id = Convert.ToInt32(row.Cells["id_elev"].Value);
                string nume = row.Cells["nume"].Value.ToString();
                string prenume = row.Cells["prenume"].Value.ToString();
                string oras = row.Cells["oras"].Value.ToString();
                string telefon = row.Cells["telefon"].Value.ToString();
                int idClasa = Convert.ToInt32(row.Cells["id_clasa"].Value);

                AddStudentForm editForm = new AddStudentForm(id, nume, prenume, oras, telefon, idClasa);
                editForm.FormClosed += new FormClosedEventHandler(AddEditForm_FormClosed);
                editForm.ShowDialog();
            }
        }

        private void AddEditForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            LoadData();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                int selectedClassId = (int)comboBox1.SelectedItem;
                LoadStudentsByClass(selectedClassId);
            }
            else
            {
                MessageBox.Show("Please select a class.");
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Optionally handle ComboBox selection change
        }

        private void PopulateComboBox()
        {
            string query = "SELECT id_clasa FROM CLASE";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader.GetInt32(0));
                        }
                    }

                    if (comboBox1.Items.Count > 0)
                    {
                        comboBox1.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void LoadStudentsByClass(int classId)
        {
            string query = "SELECT id_elev, nume, prenume, oras, telefon, id_clasa FROM ELEVI WHERE id_clasa = @id_clasa";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id_clasa", classId);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridView1.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void DataGridView2_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView2.Rows[e.RowIndex];
                int professorId = Convert.ToInt32(row.Cells["id_prof"].Value);
                string nume = row.Cells["nume"].Value.ToString();
                string prenume = row.Cells["prenume"].Value.ToString();
                string oras = row.Cells["oras"].Value.ToString();
                string telefon = row.Cells["telefon"].Value.ToString();

                // Open AddProfesor form in edit mode
                AddProfesor addProfesorForm = new AddProfesor(professorId, nume, prenume, oras, telefon);
                addProfesorForm.ShowDialog();

                // Reload data after closing the edit form
                LoadDataProfesori();
            }
        }

        private void Delete_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Optional: Add code to handle opening event of delete context menu
        }

        private void deleteToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dataGridView1.SelectedRows[0];
                int id = Convert.ToInt32(selectedRow.Cells["id_elev"].Value);

                DialogResult result = MessageBox.Show("Are you sure you want to delete this student?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string query = "DELETE FROM ELEVI WHERE id_elev = @id_elev";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@id_elev", id);

                                int rowsAffected = command.ExecuteNonQuery();

                                MessageBox.Show($"{rowsAffected} row(s) deleted successfully.");

                                // Reload data to reflect changes
                                LoadData();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred: " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a student to delete.");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            label1.Visible = false;
            comboBox1.Visible = false;
            button4.Visible = false;
            label2.Visible = true;
            comboBox2.Visible = true;
            button10.Visible = true;
            button2.Visible = false;
            button9.Visible = false;
            button8.Visible = true;
            dataGridView4.Visible = false;
            label3.Visible = false;
            label4.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = true;
            dataGridView3.Visible = false;
            dataGridView5.Visible = false;

            LoadDataProfesori();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            LoadData();
            label3.Visible = false;
            label4.Visible = false;
            label1.Visible = true;
            comboBox1.Visible = true;
            button4.Visible = true;
            label2.Visible = false;
            comboBox2.Visible = false;
            button10.Visible = false;
            button2.Visible = true;
            button9.Visible = false;
            button8.Visible = false;
            dataGridView1.Visible = true;
            dataGridView2.Visible = false;
            dataGridView3.Visible = false;
            dataGridView4.Visible = false;
            dataGridView5.Visible = false;

        }

        private void button7_Click(object sender, EventArgs e)
        {
            label3.Visible = false;
            label4.Visible = false;
            label2.Visible = false;
            comboBox2.Visible = false;
            button10.Visible = false;
            label1.Visible = false;
            comboBox1.Visible = false;
            button4.Visible = false;
            button2.Visible = false;
            button9.Visible = true;
            button8.Visible = false;
            dataGridView1.Visible = false;
            dataGridView2.Visible = false;
            dataGridView3.Visible = true;
            dataGridView4.Visible = false;
            dataGridView5.Visible = false;

        }

        private void LoadDataProfesori()
        {
            string query = @"
        SELECT 
            p.id_prof, 
            p.nume, 
            p.prenume, 
            p.oras, 
            p.telefon,
            STUFF((
                SELECT ', ' + CAST(c.id_clasa AS VARCHAR)
                FROM PROFESORI_CLASE pc
                JOIN CLASE c ON pc.id_clasa = c.id_clasa
                WHERE pc.id_prof = p.id_prof
                FOR XML PATH(''), TYPE
            ).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS Classes
        FROM PROFESORI p";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Bind the DataTable to the DataGridView
                    dataGridView2.DataSource = dataTable;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading data: " + ex.Message);
                }
            }
        }

        private void LoadDataClase()
        {
            // Update the query to select only the id_clasa
            string query = "SELECT id_clasa AS ClassID FROM CLASE";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Bind the DataTable to the DataGridView
                    dataGridView3.DataSource = dataTable;

                    // Customize the DataGridView columns to display appropriate headers
                    dataGridView3.Columns["ClassID"].HeaderText = "Class ID";

                    // Optional: Hide any other columns if they exist
                    foreach (DataGridViewColumn column in dataGridView3.Columns)
                    {
                        if (column.Name != "ClassID")
                        {
                            column.Visible = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading data: " + ex.Message);
                }
            }
        }


        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Optional: Add code to handle cell content click events
        }

        private void button9_Click(object sender, EventArgs e)
        {
            CreateNewClass();
        }

        private void CreateNewClass()
        {
            // Ask the user for confirmation
            DialogResult result = MessageBox.Show("Are you sure you want to create a new empty class?" , "Confirm Creation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                int newClassId = GetNextClassID();

                if (newClassId > 0)
                {
                    string query = "INSERT INTO CLASE (id_clasa) VALUES (@id_clasa)";

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();

                            using (SqlCommand command = new SqlCommand(query, connection))
                            {
                                command.Parameters.AddWithValue("@id_clasa", newClassId);
                                command.ExecuteNonQuery();
                                MessageBox.Show("New class created successfully with ID: " + newClassId);
                                LoadDataClase(); // Refresh the class data grid
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("An error occurred: " + ex.Message);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Unable to generate a new class ID.");
                }
            }
            // If the user clicked 'No', nothing happens
        }


        private int GetNextClassID()
        {
            int maxId = 0;

            string query = "SELECT ISNULL(MAX(id_clasa), 0) FROM CLASE";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        maxId = (int)command.ExecuteScalar();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while fetching the max class ID: " + ex.Message);
                }
            }

            return maxId + 1; // Return the next ID
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // Open AddProfesor form
            AddProfesor addProfesorForm = new AddProfesor();
            addProfesorForm.ShowDialog();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            // Check if any row is selected
            if (dataGridView2.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dataGridView2.SelectedRows[0];
                int professorId = Convert.ToInt32(selectedRow.Cells["id_prof"].Value);

                // Confirm the deletion
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this professor?", "Confirm Deletion", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    DeleteProfessor(professorId);
                }
            }
            else
            {
                MessageBox.Show("Please select a professor to delete.");
            }
        }

        private void DeleteProfessor(int professorId)
        {
            string query = "DELETE FROM PROFESORI WHERE id_prof = @id_prof";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@id_prof", professorId);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Professor deleted successfully.");
                        LoadDataProfesori(); // Reload the data to reflect changes
                    }
                    else
                    {
                        MessageBox.Show("No professor found with the given ID.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while deleting the professor: " + ex.Message);
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (comboBox2.SelectedItem != null)
            {
                int selectedClassId = (int)comboBox2.SelectedItem;
                LoadProfessorsByClass(selectedClassId);
            }
            else
            {
                MessageBox.Show("Please select a class.");
            }
        }

        private void LoadProfessorsByClass(int classId)
        {
            string query = @"
        SELECT 
            p.id_prof, 
            p.nume, 
            p.prenume, 
            p.oras, 
            p.telefon
        FROM 
            PROFESORI p
        INNER JOIN 
            PROFESORI_CLASE pc ON p.id_prof = pc.id_prof
        WHERE 
            pc.id_clasa = @id_clasa";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id_clasa", classId);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridView2.DataSource = dataTable;
                        
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading professors: " + ex.Message);
                }
            }
        }

        private void PopulateComboBox2()
        {
            string query = "SELECT id_clasa FROM CLASE";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            comboBox2.Items.Add(reader.GetInt32(0));
                        }
                    }

                    if (comboBox2.Items.Count > 0)
                    {
                        comboBox2.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred: " + ex.Message);
                }
            }
        }

        private void Delete2_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            // Check if any row is selected
            if (dataGridView3.SelectedRows.Count > 0)
            {
                // Get the selected row
                DataGridViewRow selectedRow = dataGridView3.SelectedRows[0];
                int classId = Convert.ToInt32(selectedRow.Cells["ClassID"].Value);

                // Confirm the deletion
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this class?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    DeleteClass(classId);
                }
            }
            else
            {
                MessageBox.Show("Please select a class to delete.");
            }
        }

        private void DeleteClass(int classId)
        {
            string query = "DELETE FROM CLASE WHERE id_clasa = @id_clasa";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id_clasa", classId);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Class deleted successfully.");
                            LoadDataClase(); // Refresh the class data grid
                        }
                        else
                        {
                            MessageBox.Show("No class found with the given ID.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while deleting the class: " + ex.Message);
                }
            }
        }

        private void DataGridView3_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) // Ensure the double-clicked cell is within the valid range
            {
                // Retrieve selected row
                DataGridViewRow row = dataGridView3.Rows[e.RowIndex];
                int classId = Convert.ToInt32(row.Cells["ClassID"].Value);

                // Load teachers for the selected class
                LoadTeachersByClass(classId);
                LoadStudentsByClass2(classId);
                dataGridView5.Visible = true;
                dataGridView4.Visible = true;
                dataGridView3.Visible = false;
                label3.Visible = true;
                label4.Visible = true;
            }
        }

        private void LoadStudentsByClass2(int classId)
        {
            string query = @"
        SELECT 
            e.id_elev AS StudentID,
            e.nume AS StudentName,
            e.prenume AS StudentSurname,
            e.telefon AS StudentPhone
        FROM 
            ELEVI e
        WHERE 
            e.id_clasa = @id_clasa";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id_clasa", classId);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        dataGridView5.DataSource = dataTable;
                        dataGridView5.Visible = true; // Make the DataGridView visible if it's hidden
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading students: " + ex.Message);
                }
            }
        }

        private void LoadTeachersByClass(int classId)
        {
            string query = @"
        SELECT 
            p.id_prof AS ProfessorID,
            p.nume AS ProfessorName,
            p.prenume AS ProfessorSurname
        FROM 
            PROFESORI p
        JOIN 
            PROFESORI_CLASE pc ON p.id_prof = pc.id_prof
        WHERE 
            pc.id_clasa = @id_clasa";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id_clasa", classId);

                        SqlDataAdapter adapter = new SqlDataAdapter(command);
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Assuming you have a DataGridView named dataGridView4 to display teachers
                        // If you don't have one, you'll need to add it to your form.
                        dataGridView4.DataSource = dataTable;
                        dataGridView4.Visible = true; // Make the DataGridView visible if it's hidden
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while loading teachers: " + ex.Message);
                }
            }
        }



    }
}
