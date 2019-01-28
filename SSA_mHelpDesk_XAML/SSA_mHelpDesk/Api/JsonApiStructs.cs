using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SSA_mHelpDesk.API
{
    public class ResultList<T>
    {
        public int totalRows;
        public List<T> results;
    }

    public class Ticket
    {
        public string ticketId;
        public string assignedBy;
        public string assignedTo;
        public string assigneeName;
        public int categoryId;
        public int statusId;
        public bool isPublic;
        public DateTime? creationDate;
        public DateTime? lastModDate;
        public int priority;
        public string summary;
        // neededBy
        public DateTime? scheduledDate;
        // moduleId
        public string submitterUsername;
        public string subject;
        // estimatedTime
        public int customerId;
        public Customer customer;
        // comment
        public bool fromEmail;
        // newReplyFromEndUser
        // newReplyFromStaff
        public int? typeId;
        // customerStatusId
        public bool hasInvoice;
        // parentTicketId
        // dataGroupId
        // businessUnitId
        public bool deleted;
        // submitterRole
        // lastOpenedBy
        // lastOpenedTime
        // lastOpenedLocation
        // originalTicketId
        // recurringRuleId
        // generatedByRecurring
        // previousOrgId
        // appointmentCount
        // lastCalledDate
        // nextCallDate
        public int portalId;
        // contactId
        // poNumber
        public int ticketNumber;
        public DateTime createdTicket;
        // createdEstimate
        // createdInvoice
        public int serviceLocationId;
        public ServiceLocation serviceLocation;
        public float subTotal;
        public float totalAmount;
        public float totalTax;
        public float totalProfitMargin;
        public float totalROI;
        public float totalProfit;
        // items
        // customFields
        public string ticketStatus;
        public List<Appointment> appointments;
        public string typeName;

        private DateTime? _cachedNextApptDate = null;
        public DateTime? GetNextAppointmentDate()
        {
            if (_cachedNextApptDate.HasValue)
                return _cachedNextApptDate.Value;

            if (appointments.Count == 0)
                return null;

            var today = DateTime.Today.ToLocalTime();

            Appointment found = null;
            appointments.ForEach(appt => {
                if (found == null)
                    found = appt;
                else if (found.startUTC.ToLocalTime() < today)
                {
                    if (appt.startUTC > found.startUTC)
                        found = appt;
                }
                else
                {
                    if (appt.startUTC.ToLocalTime() >= today && appt.startUTC < found.startUTC)
                        found = appt;
                }
            });

            if (found != null)
            {
                _cachedNextApptDate = found.startUTC.ToLocalTime();
                return _cachedNextApptDate.Value;
            }
            else // This should never happen because we made sure count > 0
                return null;
        }
    }

    public class Appointment
    {
        public int id;
        public DateTime startUTC;
        public DateTime endUTC;
        public string teamName;
    }

    public class Customer
    {
        public int customerId;
        public String industryTypeId;
        public String name;
        public bool isServiceLocation;
        public String address1;
        public String address2;
        public String city;
        public String state;
        public String zip;
        public String primaryPhone;
        public String secondaryPhone;
        public String fax;
        public DateTime? creationDate;
        public DateTime? lastModDate;
        public String email;
        public String notes;
        public String countryId;
        public bool billToParent;
        public Double? latitude;
        public Double? longitude;
        public String fullAddress;
        public bool doNotText;
        public String cleanPrimaryPhone;
        public String cleanSecondaryPhone;
    }

    public class ServiceLocation : Customer
    {
        public int serviceLocationId = 0;

        ServiceLocation()
        {
            isServiceLocation = true;
        }
    }
}