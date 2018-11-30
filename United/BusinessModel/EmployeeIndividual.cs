using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.United.BusinessModel
{
    public class EmployeeIndividual
    {
        public int EmployeeInfoId { get; set; }
        public int MasterUserId { get; set; }
        public int IndividualId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Inactive { get; set; }
        public string LogicalDepartment { get; set; }
        public string OfficeLocationName { get; set; }
        public virtual Models.tblIndividual Individual { get; set; } 
        public virtual Models.tblTSEmployeeInfo EmployeeInfo { get; set; }
        public virtual List<MasterUserRole> MasterUserRoles { get; set; }
        public bool IsUserTSHRAdmin { get; set; }
        public bool IsUserTSUser { get; set; }
        public bool IsUserTSManager { get; set; }
        public EmployeeIndividual() { }
        public EmployeeIndividual(United.Models.tblIndividual m)
        {
            Individual = m;
            IndividualId = m.IndividualID;
            MasterUserId = (int)m.MasterUserID;
            FirstName = m.FirstName;
            LastName = m.LastName;
            Inactive = (bool)m.Inactive;
            IsUserTSHRAdmin = false;
            IsUserTSUser = false;
            IsUserTSManager = false;
            using (var db = new United.Models.UnitedEntities())
            {
                EmployeeInfo = db.tblTSEmployeeInfoes.FirstOrDefault(i => i.MasterUserID == m.MasterUserID);
                var masterUserRoles = db.tblMasterUserRoles.Join(db.tblRoles, i => i.RoleID, r => r.RoleID, (i, r) => new { masterUserRole = i, role = r }).Where(ii => ii.role.ApplicationID > 600 && ii.masterUserRole.MasterUserID == m.MasterUserID)
                    .Select(ii => new MasterUserRole() { MasterUserID = ii.masterUserRole.MasterUserID, RoleID = ii.masterUserRole.RoleID, Role = new MasterRole() { RoleID = ii.role.RoleID, Title = ii.role.Title } }).ToList();
                MasterUserRoles = masterUserRoles;
                EmployeeInfoId = EmployeeInfo.EmployeeInfoId;
            }
        }
    }
}
