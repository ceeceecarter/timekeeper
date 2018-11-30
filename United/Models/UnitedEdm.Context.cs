﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class UnitedEntities : DbContext
    {
        public UnitedEntities()
            : base("name=UnitedEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tblIndividual> tblIndividuals { get; set; }
        public virtual DbSet<tblIndividualPropertyRole> tblIndividualPropertyRoles { get; set; }
        public virtual DbSet<tblMasterUser> tblMasterUsers { get; set; }
        public virtual DbSet<tblMasterUserRole> tblMasterUserRoles { get; set; }
        public virtual DbSet<tblRole> tblRoles { get; set; }
        public virtual DbSet<tblTSCompanyCode> tblTSCompanyCodes { get; set; }
        public virtual DbSet<tblTSPayPeriod> tblTSPayPeriods { get; set; }
        public virtual DbSet<tblTSStatusType> tblTSStatusTypes { get; set; }
        public virtual DbSet<tblTSEmployeeInfo> tblTSEmployeeInfoes { get; set; }
        public virtual DbSet<tblTSTimesheetHour> tblTSTimesheetHours { get; set; }
        public virtual DbSet<tblTSHoursType> tblTSHoursTypes { get; set; }
        public virtual DbSet<tblTSHoliday> tblTSHolidays { get; set; }
        public virtual DbSet<tblTSNavigation> tblTSNavigations { get; set; }
    
        [DbFunction("UnitedEntities", "fnTSDateRange")]
        public virtual IQueryable<fnTSDateRange_Result> fnTSDateRange(Nullable<System.DateTime> dtmFrom, Nullable<System.DateTime> dtmTo)
        {
            var dtmFromParameter = dtmFrom.HasValue ?
                new ObjectParameter("dtmFrom", dtmFrom) :
                new ObjectParameter("dtmFrom", typeof(System.DateTime));
    
            var dtmToParameter = dtmTo.HasValue ?
                new ObjectParameter("dtmTo", dtmTo) :
                new ObjectParameter("dtmTo", typeof(System.DateTime));
    
            return ((IObjectContextAdapter)this).ObjectContext.CreateQuery<fnTSDateRange_Result>("[UnitedEntities].[fnTSDateRange](@dtmFrom, @dtmTo)", dtmFromParameter, dtmToParameter);
        }
    }
}