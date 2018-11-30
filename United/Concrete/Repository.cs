using NM.Web.WebApplication.Timesheets.United.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NM.Web.WebApplication.Timesheets.United.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace NM.Web.WebApplication.Timesheets.United.Concrete
{
    public class Repository : IRepository, IDisposable
    {
        private UnitedEntities _db;

        public UnitedEntities db
        {
            get
            {

                return _db = new UnitedEntities();

            }
            set { _db = value; }
        }
        public tblMasterUser GetMasterUser()
        {
            throw new NotImplementedException();
        }

        public tblMasterUser GetMasterUser(string userName)
        {
            var currentUserName = userName.Split(new string[] { "\\" }, StringSplitOptions.None)[1];
            using (var db = new UnitedEntities())
            {
                return db.tblMasterUsers.FirstOrDefault(i => i.UserName == currentUserName);
            }
        }

        public tblMasterUser GetMasterUserById(int masterUserId)
        {
            return db.tblMasterUsers.FirstOrDefault(i => i.MasterUserID == masterUserId);
        }
        public void SaveChanges()
        {
            db.SaveChanges();
        }

        public List<string> GenerateYearsBasedOnCurrentYear(int numberOfYears = 10)
        {
            List<string> stringYears = new List<string>();
            var currentYear = DateTime.Now.Year;
            stringYears.Add(currentYear.ToString());

            //set default to 10; display 5 each on future and previous years
            numberOfYears = numberOfYears > 0 ? numberOfYears : 10;

            //if number is odd, add one to make it even
            var numberToLoop = (numberOfYears % 2 == 0 ? numberOfYears : numberOfYears + 1) / 2;

            for (int i = 0; i < numberToLoop;)
            {
                //Add future year 
                i++;
                stringYears.Add((currentYear + i).ToString());
                //Add previous year
                stringYears.Add((currentYear - i).ToString());
            }
            return stringYears.ToList();
        }

        public List<tblTSHoursType> GetHoursTypeNonExempt()
        {
            var result = db.tblTSHoursTypes.Where(i => i.AddTime == true).ToList();
            return result;
        }
        public List<tblTSHoursType> GetHoursTypeExempt()
        {
            var result = db.tblTSHoursTypes.Where(i => i.BothFLSA == true && i.AddTime == true).ToList();
            return result;
        }
        public List<BusinessModel.TimePayPeriod> GetCurrentNextPreviousPayPeriod()
        {
            List<BusinessModel.TimePayPeriod> result = db.Database
                .SqlQuery<BusinessModel.TimePayPeriod>("dbo.spTSGetCurrentNextPreviousPayPeriod").ToList();
            return result;
        }

        public List<BusinessModel.EmployeeIndividual> GetAllEmployeeIndividual()
        {
            List<BusinessModel.EmployeeIndividual> result = db.Database
                .SqlQuery<BusinessModel.EmployeeIndividual>("dbo.spTSGetAllEmployeeIndividual").ToList();
            return result;
        }

        //public List<BusinessModel.EmployeeIndividual> GetEmployeeIndividualById(int id)
        //{
        //    List<BusinessModel.EmployeeIndividual> result = db.Database
        //        .SqlQuery<BusinessModel.EmployeeIndividual>("dbo.spTSGetEmployeeIndividualById @IndividualId = {0}", id).ToList();
        //    return result;
        //}

        public List<BusinessModel.EmployeeIndividual> GetManagerEmployeesById(int id)
        {

            List<BusinessModel.EmployeeIndividual> result = db.Database
                .SqlQuery<BusinessModel.EmployeeIndividual>("dbo.spTSGetManagerEmployeesById @IndividualId = {0}", id).ToList();
            return result;
        }


        public IQueryable<fnTSDateRange_Result> GetTimeDateRange(DateTime DateStart, DateTime DateEnd)
        {
            var timeDateRange = db.fnTSDateRange((DateTime)DateStart, (DateTime)DateEnd);
            return timeDateRange;
        }

        public List<Models.tblIndividual> GetManagerEmployeesListById(int id)
        {
            return db.tblIndividuals.Where(m => m.ManagerIndividualID == id).OrderBy(m => m.LastName).ToList();
        }

        public Models.tblIndividual GetIndividualByMasterUserId(int masterUserId)
        {
            return db.tblIndividuals.FirstOrDefault(i => i.MasterUserID == masterUserId);
        }

        public tblIndividual GetIndividualByIndividualId(int individualId)
        {
            return db.tblIndividuals.FirstOrDefault(i => i.IndividualID == individualId);
        }

        public BusinessModel.EmployeeIndividual GetEmployeeIndividualById(int id)
        {
            var individual = GetIndividualByIndividualId(id);
            return new BusinessModel.EmployeeIndividual(individual);
        }

        public BusinessModel.EmployeeIndividual GetEmployeeIndividual(Models.tblIndividual m)
        {
            var result = new BusinessModel.EmployeeIndividual(m);
            return result;
        }

        public tblTSEmployeeInfo GetEmployeeInformationById(int id)
        {
            var result = db.tblTSEmployeeInfoes.FirstOrDefault(i => i.EmployeeInfoId == id);
            return result;
        }

        public List<tblTSTimesheetHour> GetEmployeeCurrentPayPeriodTimeEntries(int currentPayPeriodID, int employeeInfoId)
        {
            var entries = db.tblTSTimesheetHours.Where(i => i.EmployeeInfoId == employeeInfoId && i.PayPeriodID == currentPayPeriodID).ToList();
            return entries;
        }

        public List<tblTSNavigation> GetNavigationData()
        {
            return db.tblTSNavigations.ToList();
        }

        public tblTSTimesheetHour GetTimesheetHourById(int id)
        {
            var result = db.tblTSTimesheetHours.Find(id);
            return result;
        }

        public void UpsertIndividual(tblIndividual individual)
        {
            if (individual.IndividualID == 0)
            {
                db.tblIndividuals.Add(individual);
            }
            else
            {
                tblIndividual dbEntry = db.tblIndividuals.Find(individual.IndividualID);
                if (dbEntry != null)
                {
                    dbEntry.set(individual, db);
                    //db.Entry(dbEntry).State = EntityState.Modified;
                }
            }
            //SaveChanges();
        }

        public void UpsertEmployeeInfo(tblTSEmployeeInfo employeeInfo)
        {
            using (var db = new United.Models.UnitedEntities())
            {
                if (employeeInfo.EmployeeInfoId == 0)
                {
                    db.tblTSEmployeeInfoes.Add(employeeInfo);
                }
                else
                {
                    tblTSEmployeeInfo dbEntry = db.tblTSEmployeeInfoes.Find(employeeInfo.EmployeeInfoId);
                    if (dbEntry != null)
                    {
                        dbEntry.set(employeeInfo, db);
                        db.Entry(dbEntry).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
            }
        }

        public void UpsertSingleDateEntry(tblTSTimesheetHour singleEntry)
        {
            using (var db = new UnitedEntities())
            {
                if (singleEntry.TimesheetHoursID == 0)
                {
                    db.tblTSTimesheetHours.Add(singleEntry);
                }
                else
                {
                    tblTSTimesheetHour dbEntry = db.tblTSTimesheetHours.Find(singleEntry.TimesheetHoursID);
                    if (dbEntry != null)
                    {
                        dbEntry.set(singleEntry, db);
                        db.Entry(dbEntry).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();
            }
        }

        public void DeleteTimeEntry(tblTSTimesheetHour timeEntry)
        {
            using (var db = new UnitedEntities())
            {
                try
                {
                    if (timeEntry != null)
                    {
                        var entryToDelete = db.tblTSTimesheetHours.SingleOrDefault(i => i.TimesheetHoursID == timeEntry.TimesheetHoursID);
                        if (entryToDelete != null)
                        {
                            db.tblTSTimesheetHours.Remove(entryToDelete);
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex) { throw ex; }
            }

        }

        public void Dispose()
        {
            db.Dispose();
        }
    }
}
