using DLL_Support.Email;
using DLL_Support.Email.SMTP;
using DLL_Support.ILS;
using DLL_Support.ILS.SIP2;
using DLL_Support.LDAP;
using DLL_Support.LDAP.AD;
using DLL_Support.Stats;
using DLL_Support.Stats.API;
using DLL_Support.Worker;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;
using System.Threading;

namespace APP_DeskStats.Worker
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession,
          IncludeExceptionDetailInFaults = true,
          ConcurrencyMode = ConcurrencyMode.Multiple)]
    public class WorkerService : IWorkerService
    {
        private readonly ApiStatsWrapper apiStatsWrapper;
        private readonly SmtpWrapper smtpWrapper;
        private readonly Sip2IlsWrapper sip2IlsWrapper;
        private readonly ADLdapWrapper aDLdapStorageWrapper;

        private CancellationTokenSource taskCts = new CancellationTokenSource();

        public WorkerService()
        {

        }

        public void Init()
        {
            Callback = OperationContext.Current.GetCallbackChannel<IWorkerCallback>();

        }
        public StatUser ApiGetUser(StatUser statUser) => apiStatsWrapper.GetUser(statUser);
        public StatUser ApiCreateUser(StatUser statUser) => apiStatsWrapper.CreateUser(statUser);
        public StatUser ApiUpdateUser(StatUser statUser) => apiStatsWrapper.UpdateUser(statUser);
        public bool ApiPostStat(StatMessage statMessage) => apiStatsWrapper.PostStat(statMessage);
        public List<Location> ApiGetLocations() => apiStatsWrapper.GetLocations();
        public List<report_field> ApiGetReportFields() => apiStatsWrapper.GetReportFields();
        public DataTable ApiGetMonthlyStats(StatMessage statMessage) => apiStatsWrapper.GetMonthlyStats(statMessage);
        public string ApiSetMonthlyStat(StatMessage statMessage) => apiStatsWrapper.SetMonthlyStat(statMessage);
        public int GetStatCountForMonth(StatMessage statMessage) => apiStatsWrapper.GetStatCountForMonth(statMessage);
        public int GetMonthlyStatCountForMonth(StatMessage statMessage) => apiStatsWrapper.GetMonthlyStatCountForMonth(statMessage);
        public string GetMonthlyStatComment(StatMessage statMessage) => apiStatsWrapper.GetMonthlyStatComment(statMessage);
        public IlsPatronData PatronInfo(string barcode) => sip2IlsWrapper.PatronInfo(barcode);
        public SmtpSendMailReturnCode SendSmtpEmail(EmailMessage message) => smtpWrapper.SendEmail(message);
        public List<LdapUserRecord> LdapGetAllUsers() => aDLdapStorageWrapper.LdapGetAllUsers();
        public List<LdapGroupRecord> LdapGetAllGroups() => aDLdapStorageWrapper.LdapGetAllGroups();
        public LdapUserRecord LdapGetUser(string username) => aDLdapStorageWrapper.LdapGetUser(username);
        public LdapGroupRecord LdapGetGroup(string groupname) => aDLdapStorageWrapper.LdapGetGroup(groupname);
        public bool LdapIsValidUser(string username, string password) => aDLdapStorageWrapper.LdapIsValidUser(username, password);
        public void Dispose()
        {
        }
        public IWorkerCallback Callback { get; set; }

    }
}
