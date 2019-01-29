using SSA_mHelpDesk.API;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SSA_mHelpDesk.Domain
{
    public class TicketListViewModel
    {
        static readonly ApiManager sApiManager = ApiManager.Instance;

        private readonly ObservableCollection<ObservableTicket> _toScheduleDataItems = new ObservableCollection<ObservableTicket>();
        private readonly ObservableCollection<ObservableTicket> _todayDataItems = new ObservableCollection<ObservableTicket>();
        private readonly ObservableCollection<ObservableTicket> _fireInspectionDataItems = new ObservableCollection<ObservableTicket>();

        public ObservableCollection<ObservableTicket> ToScheduleDataItems => _toScheduleDataItems;
        public ObservableCollection<ObservableTicket> TodayDataItems => _todayDataItems;
        public ObservableCollection<ObservableTicket> FireInspectionDataItems => _fireInspectionDataItems;

        //public class ListFilter
        //{
        //    List<String> AcceptableTicketStatus = new List<String>();
        //}

        private void ProcessUpdatedLicketList(List<Ticket> ticketList)
        {
            var displayLists = new[] { ToScheduleDataItems, TodayDataItems, FireInspectionDataItems };

            List<ObservableTicket> itemsInLists = new List<ObservableTicket>();
            foreach (var testList in displayLists)
            {
                itemsInLists.AddRange(testList);
                //foreach (var obsTicket in testList)
                //    itemsInLists.AddLast(obsTicket);
            }

            foreach (Ticket t in ticketList)
            {
                var list = DetermineTicketList(t);

                ObservableTicket prevTicket = null;
                foreach (var testList in displayLists)
                {
                    prevTicket = testList.FirstOrDefault(ot => ot.Inner.ticketId == t.ticketId);
                    if (prevTicket != null)
                    {
                        if (ReferenceEquals(testList, list))
                        {
                            prevTicket.ResetTicket(t); // update the ticket TODO check property change
                            //BeginFetchTicketServiceLocation(prevTicket);
                            list = null; //don't add to list below
                        }
                        else // this ticket no longer belongs in this list
                            testList.Remove(prevTicket);

                        //This ticket was in a list so lets remove it from itemsInLists
                        itemsInLists.Remove(prevTicket);

                        break; // tickets can't be in multiple lists
                    }
                }

                if (list != null)
                {
                    ObservableTicket obsTick = new ObservableTicket(t);

                    //fetchTicketCustomer(obsTick);
                    //if (t.serviceLocationId != t.customerId)
                       //BeginFetchTicketServiceLocation(obsTick);

                    list.Add(obsTick);
                }
            }

            //remove anything left in itemsInLists from all the lists
            foreach (ObservableTicket obsTicket in itemsInLists)
                foreach (var testList in displayLists)
                    if (testList.Remove(obsTicket))
                        break; // go to next ticket
        }

        private ObservableCollection<ObservableTicket> DetermineTicketList(Ticket ticket)
        {
            DateTime today = DateTime.Today;

            DateTime? nad = ticket.GetNextAppointmentDate();

            if (ticket.ticketStatus == "Closed: Invoices with Quickbooks")
                return null;

            if (!nad.HasValue)
            {
                if (ticket.typeName == "Fire Inspection")
                    //                    return FireInspectionDataItems;
                    return null;

                if (ticket.ticketStatus == "New" ||
                    ticket.ticketStatus == "New: Scheduled" ||
                    ticket.ticketStatus == "New: ConfirmSchedule" ||
                    ticket.ticketStatus == "New: Waiting For Parts" ||
                    ticket.ticketStatus == "Open: Confirm Schedule" ||
                    ticket.ticketStatus == "Open: En Route" ||
                    ticket.ticketStatus == "Open: In-Progress" ||
                    ticket.ticketStatus == "Open: Job Complete" ||
                    ticket.ticketStatus == "Open: Rescheduled" ||
                    ticket.ticketStatus == "Open: Return Needed")
                {
                    return ToScheduleDataItems;
                }

            }
            else
            {
                if (nad.Value.Date == today)
                    return TodayDataItems;

                //                else if (nad.Value.Date > today-7 &&
                //                       ticket.ticketStatus == "Open: Job Complete")
                //                    return ToScheduleDataItems;


                else if (nad.Value.Date < today)
                {
                    if (ticket.ticketStatus == "New" ||
                        ticket.ticketStatus == "New: Scheduled" ||
                        ticket.ticketStatus == "New: ConfirmSchedule" ||
                        ticket.ticketStatus == "New: Waiting For Parts" ||
                        ticket.ticketStatus == "Open: Confirm Schedule" ||
                        ticket.ticketStatus == "Open: En Route" ||
                        ticket.ticketStatus == "Open: In-Progress" ||
                        ticket.ticketStatus == "Open: Rescheduled" ||
                        ticket.ticketStatus == "Open: Return Needed")
                    {
                        return ToScheduleDataItems;
                    }
                }
            }
            return null;
        }

        public async Task<int> RefreshTicketsAsync()
        {
            var ticketList = await sApiManager.GetTicketsAsync( //createStart: Convert.ToDateTime("1/1/2020")
                                               appointmentStart: Convert.ToDateTime("1/1/2020")
                                               );
            if (ticketList != null)
            {
                //Clipboard.SetText(sApiManager.GetLastRawOutput());
                ProcessUpdatedLicketList(ticketList);
                return ticketList.Count;
            }
            else
            {
                throw ApiManager.Instance.GetLastError();
            }
        }

        public void BeginFetchTicketCustomer(ObservableTicket ticket)
        {
            var awaiter = sApiManager.GetCustomerAsync(ticket.Inner.customerId).GetAwaiter();

            awaiter.OnCompleted(() =>
            {
                Customer customer = awaiter.GetResult();
                ticket.Customer = customer;
            });
        }

        public void BeginFetchTicketServiceLocation(ObservableTicket ticket)
        {
            var awaiter = sApiManager.GetServiceLocationAsync(ticket.Inner.customerId, ticket.Inner.serviceLocationId).GetAwaiter();

            awaiter.OnCompleted(() =>
            {
                ServiceLocation serviceLocation = awaiter.GetResult();
                ticket.ServiceLocation = serviceLocation;
            });
        }
    }
    
    //TODO move Customer/ServiceLocation properties to not rely on ticket
    public class ObservableTicket : INotifyPropertyChanged
    {
        private Ticket _ticket;
        public ObservableTicket(Ticket ticket)
        {
            _ticket = ticket;
        }

        public void ResetTicket(Ticket newTicket)
        {
            _ticket = newTicket;
            OnPropertyChanged();
        }

        public Ticket Inner => _ticket;

        public Customer Customer
        {
            get => _ticket.customer;
            set
            {
                _ticket.customer = value;
                OnPropertyChanged("Customer");

                if (ServiceLocation == null)
                {
                    OnPropertyChanged("CustomerName");
                    OnPropertyChanged("ServiceAddress");
                }
            }
        }

        public ServiceLocation ServiceLocation
        {
            get => _ticket.serviceLocation;
            set
            {
                _ticket.serviceLocation = value;
                OnPropertyChanged("ServiceLocation");
                OnPropertyChanged("CustomerName");
                OnPropertyChanged("ServiceAddress");
            }
        }

        public String CustomerName
        {
            get
            {
                if (ServiceLocation != null)
                    return ServiceLocation.name;

                return Customer?.name; // TODO print something if null
            }
        }

        public String ServiceAddress
        {
            get
            {
                if (ServiceLocation != null)
                    return ServiceLocation.fullAddress;

                return Customer?.fullAddress; // TODO print something if null
            }
        }

        public String Subject => _ticket.subject;
        public String TicketStatus => _ticket.ticketStatus;
        public DateTime? NextAppointmentDate => _ticket.GetNextAppointmentDate();
        public String JobNumber => _ticket.ticketId;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
