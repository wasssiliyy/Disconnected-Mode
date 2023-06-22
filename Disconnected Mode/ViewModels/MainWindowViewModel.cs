using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Disconnected_Mode.Commands;
using Disconnected_Mode.Models;
using System.Configuration;
using System.Windows;

namespace Disconnected_Mode.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {

        private int id;

        public int Id
        {
            get { return id; }
            set { id = value; OnPropertyChanged(); }
        }


        private string firstname;

        public string Firstname
        {
            get { return firstname; }
            set { firstname = value; OnPropertyChanged(); }
        }

        private string lastname;

        public string Lastname
        {
            get { return lastname; }
            set { lastname = value; OnPropertyChanged(); }
        }


        private int selectedIndex;

        public int SelectedIndex
        {
            get { return selectedIndex; }
            set { selectedIndex = value; OnPropertyChanged(); }
        }


        public RelayCommand Insert { get; set; }
        public RelayCommand Delete { get; set; }
        public RelayCommand SelectionChanged { get; set; }

        public ObservableCollection<Author> ListAuthors { get; set; }

        DataSet set = new DataSet();
        string cs = ConfigurationManager.ConnectionStrings["myConn"].ConnectionString;
        public void Connection()
        {
            var da = new SqlDataAdapter();

            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = cs;
                conn.Open();


                var query = "SELECT * FROM Authors";

                using (var command = new SqlCommand(query, conn))
                {
                    da.SelectCommand = command;
                    da.Fill(set, "a");
                }
            }
        }

        public void ListBoxRefresh(ObservableCollection<Author> authors)
        {
            SqlDataReader reader = null;
            using (var conn = new SqlConnection())
            {
                conn.ConnectionString = cs;
                conn.Open();

                var query = "SELECT * FROM Authors";

                var table = new DataTable();

                using (var command = new SqlCommand(query, conn))
                {
                    reader = command.ExecuteReader();

                    authors.Clear();

                    DataRow row = table.NewRow();

                    while (reader.Read())
                    {
                        Author author = new Author();

                        author.Id = (int)reader[0];
                        author.FirstName = reader[1].ToString();
                        author.LastName = reader[2].ToString();

                        authors.Add(author);
                    }
                }
            }
        }

        public MainWindowViewModel()
        {
            ObservableCollection<Author> authors = new ObservableCollection<Author>();

            Connection();

            ListBoxRefresh(authors);

            ListAuthors = authors;

            SelectionChanged = new RelayCommand((a) =>
            {
                if (SelectedIndex != -1)
                {
                    using (var conn = new SqlConnection())
                    {
                        conn.ConnectionString = cs;
                        conn.Open();

                        var query = "DELETE Authors WHERE Id=@id";

                        using (var command = new SqlCommand(query, conn))
                        {
                            SqlParameter paramId = new SqlParameter();
                            paramId.ParameterName = "@id";
                            paramId.SqlDbType = SqlDbType.Int;
                            paramId.Value = authors[SelectedIndex].Id;

                            command.Parameters.Add(paramId);

                            var da = new SqlDataAdapter();
                            da.DeleteCommand = command;
                            da.DeleteCommand.ExecuteNonQuery();

                            MessageBox.Show($"Deleted successfully");
                        }
                    }

                    ListBoxRefresh(authors);
                }

            });


            Insert = new RelayCommand((a) =>
            {
                bool equalId = false;

                using (var conn = new SqlConnection())
                {
                    conn.ConnectionString = cs;
                    conn.Open();

                    var query = "INSERT INTO Authors VALUES(@id,@firstname,@lastname)";

                    using (var command = new SqlCommand(query, conn))
                    {
                        SqlParameter paramId = new SqlParameter();
                        paramId.ParameterName = "@id";
                        paramId.SqlDbType = SqlDbType.Int;
                        paramId.Value = Id;

                        SqlParameter paramFirstName = new SqlParameter();
                        paramFirstName.ParameterName = "@firstname";
                        paramFirstName.SqlDbType = SqlDbType.NVarChar;
                        paramFirstName.Value = Firstname;

                        SqlParameter paramLastName = new SqlParameter();
                        paramLastName.ParameterName = "@lastname";
                        paramLastName.SqlDbType = SqlDbType.NVarChar;
                        paramLastName.Value = Lastname;

                        command.Parameters.Add(paramId);
                        command.Parameters.Add(paramFirstName);
                        command.Parameters.Add(paramLastName);


                        for (int i = 0; i < authors.Count; i++)
                        {
                            if (Id == authors[i].Id)
                            {
                                MessageBox.Show("It was unsuccessful to add");
                                equalId = true;
                                break;
                            }
                        }
                        if (!equalId)
                        {
                            var da = new SqlDataAdapter();
                            da.InsertCommand = command;
                            da.InsertCommand.ExecuteNonQuery();

                            da.Update(set, "a");
                        }
                    }
                }

                if (!equalId)
                {
                    ListBoxRefresh(authors);
                    MessageBox.Show("Added successfully");
                }
            });
        }
    }
}
