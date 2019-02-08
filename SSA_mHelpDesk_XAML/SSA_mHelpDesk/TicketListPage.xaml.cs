using SSA_mHelpDesk.Domain;
using System;
using System.Collections.Generic;
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
namespace SSA_mHelpDesk
{
    /// <summary>
    /// Interaction logic for TicketListPage.xaml
    /// </summary>
    public partial class TicketListPage : UserControl
    {
        public TicketListPage(TicketListViewModel viewModel)
        {
            InitializeComponent();

            DataContext = viewModel;
        }

        private void ItemsControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.TextBlock Txt = e.OriginalSource as TextBlock;
            ObservableTicket Ticket = Txt.DataContext as ObservableTicket;
            if (Ticket != null)
            {
                Clipboard.SetText(Ticket.JobNumber);
                Console.Beep();
            }

        }
    }
}
