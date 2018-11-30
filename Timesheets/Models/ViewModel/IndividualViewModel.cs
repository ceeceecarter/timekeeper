
using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class IndividualViewModel : IMapFrom<NM.Lib.DataLibrary.United.Domain.IndividualDTO>
    {
        public int IndividualID { get; set; }

        [Required]
        [Display(Name ="First Name")]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string Title { get; set; }
        public string CompanyID { get; set; }
        public Nullable<int> MasterUserID { get; set; }
        public string Cell { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }

        [Display(Name ="Email Address")]
        [DataType(DataType.EmailAddress)]
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

        [DataType(DataType.Date)]
        public Nullable<System.DateTime> Birthdate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public Nullable<System.DateTime> StartDate { get; set; }

        [DataType(DataType.Date)]
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

        [Required]
        public int ManagerIndividualID { get; set; }

        [Display(Name ="Middle Initial")]
        public string MiddleName { get; set; }

        [Required]
        [Display(Name = "Preferred First Name")]
        public string PreferredFirstName { get; set; }
    }
} 