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
    
    public partial class tblRole
    {
        public int RoleID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int ApplicationID { get; set; }
        public int Version { get; set; }
        public Nullable<int> ModifiedByMasterUserID { get; set; }
        public Nullable<System.DateTime> ModifiedDatetime { get; set; }
        public int CreatedByMasterUserID { get; set; }
        public System.DateTime CreatedDatetime { get; set; }
    
        public virtual tblMasterUser tblMasterUser { get; set; }
        public virtual tblMasterUser tblMasterUser1 { get; set; }
    }
}