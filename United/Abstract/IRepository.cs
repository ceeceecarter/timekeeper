using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NM.Web.WebApplication.Timesheets.United.Models;

namespace NM.Web.WebApplication.Timesheets.United.Abstract
{
    public interface IRepository
    {
        void SaveChanges();
        tblMasterUser GetMasterUser();
        tblMasterUser GetMasterUser(string userName);
        tblMasterUser GetMasterUserById(int masterUserId);
        List<string> GenerateYearsBasedOnCurrentYear(int numberOfYears);
        List<United.BusinessModel.TimePayPeriod> GetCurrentNextPreviousPayPeriod();
        List<tblTSHoursType> GetHoursTypeNonExempt();
        List<tblTSHoursType> GetHoursTypeExempt();
        List<United.BusinessModel.EmployeeIndividual> GetAllEmployeeIndividual();
        United.BusinessModel.EmployeeIndividual GetEmployeeIndividualById(int id);
        Models.tblIndividual GetIndividualByMasterUserId(int masterUserId);
        List<United.BusinessModel.EmployeeIndividual> GetManagerEmployeesById(int id);
        List<Models.tblIndividual> GetManagerEmployeesListById(int id);
        BusinessModel.EmployeeIndividual GetEmployeeIndividual(Models.tblIndividual m);
        IQueryable<fnTSDateRange_Result> GetTimeDateRange(DateTime DateStart, DateTime DateEnd);
        List<tblTSTimesheetHour> GetEmployeeCurrentPayPeriodTimeEntries(int currentPayPeriodID, int employeeInfoId);
        tblTSTimesheetHour GetTimesheetHourById(int id);
        tblTSEmployeeInfo GetEmployeeInformationById(int id);
        List<tblTSNavigation> GetNavigationData();
        tblIndividual GetIndividualByIndividualId(int individualId);



        void DeleteTimeEntry(tblTSTimesheetHour timeEntry);
        void UpsertSingleDateEntry(tblTSTimesheetHour singleEntry);
        void UpsertEmployeeInfo(tblTSEmployeeInfo employeeInfo);
        void UpsertIndividual(tblIndividual individual);
    }
}
