using Newtonsoft.Json;
using SSA_mHelpDesk.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSA_mHelpDesk.API
{
    public sealed class ApiManager
    {
        private const string prodApiBase = "https://connect.mhelpdesk.com/api/v1.0/";
        private const string preprodApiBase = "https://preprod-secure1.mhelpdesk.com/api/api/v1.0/";

        private static readonly Lazy<ApiManager> lazy =
            new Lazy<ApiManager>(() => new ApiManager());

        public static ApiManager Instance { get { return lazy.Value; } }

        private readonly AuthenticationManager mAuthManager = AuthenticationManager.Instance;

        private Exception mLastError = null;
        private String mRawOutput;

        private ApiManager()
        {

        }

        public Exception GetLastError()
        {
            return mLastError;
        }

        public void ClearLastError()
        {
            mLastError = null;
        }

        private void SetLastError(Exception value)
        {
            mLastError = value;
        }

        public String GetLastRawOutput()
        {
            lock(this)
            {
                return mRawOutput;
            }
        }

        public async Task<List<Ticket>> GetTicketsAsync(int? statusId = null, 
            DateTime? appointmentStart = null,
            DateTime? appointmentEnd = null,
            DateTime? createStart = null,
            DateTime? createEnd = null,
            string fields = "ticketId,statusId,subject,assignedTo,customerId,typeId,ticketNumber,appointmentCount,appointments{id,startUTC,endUTC,teamName},customStatusId,typeName,ticketStatus,customer{name},serviceLocation{name,fulladdress}")
        {
            var uriParams = new List<Tuple<string, string>>();

            const string dateFormat = "yyyy-MM-ddTHH:mm:ss+00:00";

            if (appointmentStart.HasValue)
                uriParams.Add(new Tuple<string, string>("appointmentStart", appointmentStart.Value.ToUniversalTime().ToString(dateFormat)));

            if (appointmentEnd.HasValue)
                uriParams.Add(new Tuple<string, string>("appointmentEnd", appointmentEnd.Value.ToUniversalTime().ToString(dateFormat)));

            if (createStart.HasValue)
                uriParams.Add(new Tuple<string, string>("createStart", createStart.Value.ToUniversalTime().ToString(dateFormat)));

            if (createEnd.HasValue)
                uriParams.Add(new Tuple<string, string>("createEnd", createEnd.Value.ToUniversalTime().ToString(dateFormat)));

            if (statusId.HasValue)
                uriParams.Add(new Tuple<string, string>("statusId", statusId.ToString()));

            if (fields != null)
                uriParams.Add(new Tuple<string, string>("fields", fields));

            return await GetTicketsAsync(uriParams);
        }

        private async Task<List<Ticket>> GetTicketsAsync(List<Tuple<string, string>> uriParams)
        {
            string apiReqUri = "portal/" + UserSettings.PortalId + "/tickets";
           
            if (uriParams.Count > 0)
            {
                int i = 0;
                foreach (var entry in uriParams)
                {
                    apiReqUri += (i==0 ? "?" : "&");
                    apiReqUri += entry.Item1 + "=" + entry.Item2;
                    ++i;
                }
            }

            Exception prevError = GetLastError();
            ClearLastError();

            string resultStr = await ApiRequestAsync(apiReqUri);

            try
            {
                ResultList<Ticket> ticketList = JsonConvert.DeserializeObject<ResultList<Ticket>>(resultStr);
                var ret = ticketList.results;
                SetLastError(prevError);
                return ret;
            }
            catch (Exception ex)
            {
                //leave error from apiRequest
                if (mLastError == null)
                    mLastError = ex;

                return null;
            }
        }

        public async Task<Customer> GetCustomerAsync(int customerId)
        {
            string apiReqUri = "portal/" + UserSettings.PortalId + "/customers/" + customerId;

            Exception prevError = GetLastError();
            ClearLastError();

            string resultStr = await ApiRequestAsync(apiReqUri);

            try
            {
                Customer customer = JsonConvert.DeserializeObject<Customer>(resultStr);
                SetLastError(prevError);
                return customer;
            }
            catch (Exception ex)
            {
                //leave error from apiRequest
                if (mLastError == null)
                    mLastError = ex;

                return null;
            }
        }

        public async Task<ServiceLocation> GetServiceLocationAsync(int customerId, int serviceLocationId)
        {
            string apiReqUri = "portal/" + UserSettings.PortalId + "/customers/" + customerId + "/servicelocations/" + serviceLocationId;

            Exception prevError = GetLastError();
            ClearLastError();

            string resultStr = await ApiRequestAsync(apiReqUri);

            try
            {
                ServiceLocation customer = JsonConvert.DeserializeObject<ServiceLocation>(resultStr);
                SetLastError(prevError);
                return customer;
            }
            catch (Exception ex)
            {
                //leave error from apiRequest
                if (mLastError == null)
                    mLastError = ex;

                return null;
            }
        }

        //public?
        public async Task<string> ApiRequestAsync(string uri)
        {
            var authInfo = await mAuthManager.GetAuthInfoAsync();

            string apiBase = UserSettings.Production ? prodApiBase : preprodApiBase;
            string requestUri = apiBase + uri;
            Console.WriteLine(requestUri);
            int retryCount = 0;
            Exception errToRethrow = null;

            while (errToRethrow == null) // Retry
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);

                if (UserSettings.Production && UserSettings.Bearer_Workaround)
                    request.Headers.Add("Authorization: bearer " + authInfo?.AccessToken); //lowercase 'b' to work around issue
                else
                    request.Headers.Add("Authorization: Bearer " + authInfo?.AccessToken);
                //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

                try
                {
                    using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var tmp = await reader.ReadToEndAsync();

                        lock (this)
                        {
                            mRawOutput = tmp;
                        }

                        return mRawOutput;
                    }
                }
                catch (WebException webException)
                {
                    if (webException.Response is HttpWebResponse &&
                        (int)((HttpWebResponse)webException.Response).StatusCode == 429 && // Only retry on 429 error
                        retryCount < 3)
                    {
                        retryCount++;
                        Console.WriteLine("429 Error - Trying again in 250ms - Count=" + retryCount);
                        Thread.Sleep(250);
                    }
                    else
                        errToRethrow = webException;
                }
            } //while errToRethrow is null

            //This needs to be outside while look or compiler will complain
            throw errToRethrow;
        }
    }
}
