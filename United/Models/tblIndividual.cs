//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NM.Web.WebApplication.Timesheets.United.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblIndividual
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblIndividual()
        {
            this.tblIndividualPropertyRoles = new HashSet<tblIndividualPropertyRole>();
            this.tblTSEmployeeInfoes = new HashSet<tblTSEmployeeInfo>();
        }
    
        public int IndividualID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string CompanyID { get; set; }
        public Nullable<int> MasterUserID { get; set; }
        public string Cell { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public int Version { get; set; }
        public Nullable<System.DateTime> ModifiedDatetime { get; set; }
        public Nullable<int> ModifiedByMasterUserID { get; set; }
        public System.DateTime CreatedDatetime { get; set; }
        public int CreatedByMasterUserID { get; set; }
        public string SecondaryTitle { get; set; }
        public string Designations { get; set; }
        public string MailStop { get; set; }
        public string PhoneExtension { get; set; }
        public byte[] PersonalysisFile { get; set; }
        public Nullable<System.DateTime> Birthdate { get; set; }
        public Nullable<System.DateTime> StartDate { get; set; }
        public Nullable<System.DateTime> TermDate { get; set; }
        public Nullable<bool> Inactive { get; set; }
        public Nullable<int> OfficeLocationBuildingID { get; set; }
        public string Clock { get; set; }
        public Nullable<bool> ShowInPhoneLists { get; set; }
        public string LogicalDepartment { get; set; }
        public Nullable<int> OfficeLocationsID { get; set; }
        public Nullable<bool> IsEmployee { get; set; }
        public Nullable<int> LogicalDepartmentID { get; set; }
        public string PersonalEmail { get; set; }
        public string HomePhone { get; set; }
        public string PersonalCell { get; set; }
        public string HomeAddress { get; set; }
        public string HomeCity { get; set; }
        public string HomeState { get; set; }
        public string HomeZipCode { get; set; }
        public int ManagerIndividualID { get; set; }
        public string MiddleName { get; set; }
        public string PreferredFirstName { get; set; }
    
        public virtual tblMasterUser tblMasterUser { get; set; }
        public virtual tblMasterUser tblMasterUser1 { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblIndividualPropertyRole> tblIndividualPropertyRoles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblTSEmployeeInfo> tblTSEmployeeInfoes { get; set; }
    }
}
