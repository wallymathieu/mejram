using Mejram.Model;
using Mejram.StoredProcedures;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SearchStoredProcedures
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IStoredProcedures readData;
        public MainWindow()
        {
            InitializeComponent();
            switch (ConfigurationManager.AppSettings["dbtype"])
            {
                case "pg":
                    readData = new PgSqlServer(ConfigurationManager.AppSettings["conn"]);
                    break;
                case "sqlserver":
                default:
                    readData = new SqlServer(ConfigurationManager.AppSettings["conn"]);
                    break;
            }
            SearchText.ItemsSource = readData.GetRoutines().ToArray();
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            DoSearch();
        }

        private void DoSearch()
        {
            DefinitionTextBox.Text = readData.GetRoutineDefinition((Routine)SearchText.SelectedItem);
        }

        private void SearchText_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key.Equals(Key.Enter))
            {
                DoSearch();
            }
        }
    }
}
