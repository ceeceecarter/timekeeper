using NM.Lib.DataLibrary.United.Interface;
using NM.Web.WebApplication.Timesheets.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;

using NM.Lib.DataLibrary.United.Domain;
using NM.Lib.DataLibrary.United.Handler;
using System.Data;
using System.Text.RegularExpressions;
using LumenWorks.Framework.IO.Csv;
using NM.Lib.DataLibrary.DataAccess.United;

namespace NM.Web.WebApplication.Timesheets.Controllers
{
    public class HRAdministrationController : BaseController
    {
        public IHRAdministration hrAdminRepository;

        #region Constructors
        public HRAdministrationController(IHRAdministration repo)
        {
            hrAdminRepository = repo;
        }
        #endregion

        #region Methods

        public ActionResult AddNewPayPeriod()
        {
            return PartialView("_CreatePayPeriod");
        }

        // GET: HRAdministration
        public ActionResult Index()
        {
            var loggedInEmployeeIndividual = GetEmployeeIndividual();
            if (!loggedInEmployeeIndividual.IsUserTSHRAdmin)
            {
                return RedirectToAction("Index", "Timesheets");
            }
            //HRAdministration PayPeriod
            var model = new HRAdminViewModel();
            var payperiods = AutoMapper.Mapper.Map(hrAdminRepository.GetCurrentNextPreviousPayPeriod(), new List<PayPeriodViewModel>()).ToList();
            var currentPayPeriod = payperiods.FirstOrDefault(i => i.TimePeriod == Enumeration.TimePayPeriod.Current.ToString());
            var employeeIndividual = GetEmployeeIndividual();
            model.YearAndPayPeriod = new YearAndPayPeriodViewModel()
            {
                PayPeriods = payperiods,
                CurrentPayPeriod = currentPayPeriod,
                CurrentYear = currentPayPeriod.dtmPeriodEnd.Year
            };

            model.WorkingWithYear = currentPayPeriod.dtmPeriodEnd.Year;
            model.PayPeriodDisplayDates = string.Concat(model.YearAndPayPeriod.CurrentPayPeriod.dtmPeriodStart.ToShortDateString(), "-", model.YearAndPayPeriod.CurrentPayPeriod.dtmPeriodEnd.ToShortDateString());
            model.PayPeriods = AutoMapper.Mapper.Map(hrAdminRepository.GetAllPayPeriodBasedOnCurrentDate(DateTime.Now), new List<PayPeriodViewModel>());
            model.PayrollYears = GetListOfPayrollYears();

            model.AuthenticatedUser = AuthenticateUser;
            model.LoggedInUser.AuthenticatedUser = AuthenticateUser;
            model.LoggedInUser.EmployeeInfoID = employeeIndividual.EmployeeInfoId;
            model.LoggedInUser.JobTitle = employeeIndividual.JobTitle;
            model.LoggedInUser.IsUserDelegate = employeeIndividual.IsUserDelegate;
            model.LoggedInUser.IsUserTSHRAdmin = employeeIndividual.IsUserTSHRAdmin;
            model.LoggedInUser.IsUserTSManager = employeeIndividual.IsUserTSManager;
            model.LoggedInUser.IsUserTSUser = employeeIndividual.IsUserTSUser;

            return View("HRAdminView", model);
        }


        public ActionResult GetAllPayPeriods()
        {
            var model = new HRAdminViewModel();
            model.PayPeriods = GetPayPeriods();
            model.PayrollYears = GetListOfPayrollYears();
            return PartialView("_ViewAllPayPeriods", model);
        }


        [HttpPost]
        public ActionResult GetPayPeriod(int PayPeriodId)
        {
            HRAdministrationHandler handler = new HRAdministrationHandler();

            TimePayPeriodDTO dto = new TimePayPeriodDTO();
            PayPeriodViewModel payPeriodViewModel = new PayPeriodViewModel();

            dto = handler.GetPayPeriod(PayPeriodId);

            AutoMapper.Mapper.Map(dto, payPeriodViewModel);

            return PartialView("_EditPayPeriod", payPeriodViewModel);
        }


        //[HttpPost]
        public ActionResult GetPayPeriodEmployees(int PayPeriodId)
        {
            HRAdministrationHandler handler = new HRAdministrationHandler();

            List<PayPeriodEmployee> emps = new List<PayPeriodEmployee>();

            var x = handler.GetPayPeriodEmployees(PayPeriodId);

            AutoMapper.Mapper.Map((List<PayPeriodEmployeeDTO>)x, emps);

            foreach (PayPeriodEmployee emp in emps)
            {
                emp.Icon = Properties.Settings.Default.RootWebAddress + emp.Icon;
                emp.TimesheetErrorStatus = Properties.Settings.Default.RootWebAddress + emp.TimesheetErrorStatus;
            }

            return PartialView("_PayPeriodEmployees", emps);
        }

        [HttpPost]
        public ActionResult SavePayPeriod(NM.Web.WebApplication.Timesheets.Models.ViewModel.PayPeriodViewModel pModel)
        {
            NM.Lib.DataLibrary.United.Domain.TimePayPeriodDTO payPeriodDTO = new Lib.DataLibrary.United.Domain.TimePayPeriodDTO();

            ViewBag.AuthenticateUserId = base.AuthenticateUser.IndividualID;

            try
            {
                //TODO: Enhance UI to allow users DATE & TIME selection for reminders. Hardcode to noon per Heather H.
                if (pModel.dtm1stReminder.Value.Hour == 0)
                    pModel.dtm1stReminder = pModel.dtm1stReminder.Value.AddHours(12);

                if (pModel.dtm2ndReminder.Value.Hour == 0)
                    pModel.dtm2ndReminder = pModel.dtm2ndReminder.Value.AddHours(12);

                AutoMapper.Mapper.Map(pModel, payPeriodDTO);

                hrAdminRepository.Save(payPeriodDTO);

                AutoMapper.Mapper.Map(payPeriodDTO, pModel);

                ViewBag.PayPeriodId = pModel.PayPeriodID;

                return PartialView("_DeletePayPeriod");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public ActionResult DeletePayPeriod(int pPayPeriodId, int pIndividualId)
        {
            if (pPayPeriodId != 0)
            {
                HRAdministrationHandler handler = new HRAdministrationHandler();
                handler.DeletePayPeriod(pPayPeriodId, pIndividualId);
            }

            ViewBag.PayPeriodId = 0;

            return PartialView("_DeletePayPeriod");
        }

        public ActionResult DownloadProcessedCsv(int pPayPeriodId, bool pIsNonExempt)
        {
            byte[] file;
            string fileName = "Processed - " + (pIsNonExempt == true ? "NonExempt" : "Exempt") + ".csv";
            string contentType = MimeMapping.GetMimeMapping(fileName);
            string headerRow = "LastName,FirstName,RealFte,Co Code,Batch ID,File #,reg hours,o/t hours,hours 3 code,hours 3 amount,earnings 3 code,earnings 3 amount,adjust ded code,adjust ded amount";

            HRAdministrationHandler handler = new HRAdministrationHandler();
            List<TimesheetCSVDataDTO> csvItems = new List<TimesheetCSVDataDTO>();

            csvItems = handler.GetProcessedCsv(pPayPeriodId, pIsNonExempt);

            using (MemoryStream ms = new MemoryStream())
            {
                using (TextWriter tw = new StreamWriter(ms))
                {
                    tw.WriteLine(headerRow);

                    if (csvItems == null || csvItems.Count == 0)
                    {
                        tw.WriteLine("");
                        tw.WriteLine("NO DATA PRESENT");
                    }

                    foreach (TimesheetCSVDataDTO csv in csvItems)
                    {
                        tw.WriteLine(csv.CsvData);
                    }
                    tw.Flush();
                }

                file = ms.GetBuffer();
            }

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = fileName,
                Inline = false,
            };

            Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(file, contentType);
        }


        public ActionResult DownloadPreviewCsv(int pPayPeriodId, bool pIsNonExempt)
        {
            byte[] file;
            string fileName = "Preview - " + (pIsNonExempt == true ? "NonExempt" : "Exempt") + ".csv";
            string contentType = MimeMapping.GetMimeMapping(fileName);
            string headerRow = "LastName,FirstName,RealFte,Co Code,Batch ID,File #,reg hours,o/t hours,hours 3 code,hours 3 amount,earnings 3 code,earnings 3 amount,adjust ded code,adjust ded amount";

            HRAdministrationHandler handler = new HRAdministrationHandler();
            List<TimesheetCSVDataStagingDTO> csvItems = new List<TimesheetCSVDataStagingDTO>();

            csvItems = handler.GetPreviewCsv(pPayPeriodId, pIsNonExempt);

            using (MemoryStream ms = new MemoryStream())
            {
                using (TextWriter tw = new StreamWriter(ms))
                {
                    tw.WriteLine(headerRow);

                    if (csvItems == null || csvItems.Count == 0)
                    {
                        tw.WriteLine("");
                        tw.WriteLine("NO DATA PRESENT");
                    }

                    foreach (TimesheetCSVDataStagingDTO csv in csvItems)
                    {
                        tw.WriteLine(csv.CsvData);
                    }
                    tw.Flush();
                }

                file = ms.GetBuffer();
            }

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = fileName,
                Inline = false,
            };

            Response.AppendHeader("Content-Disposition", cd.ToString());

            return File(file, contentType);
        }


        [HttpPost]
        public ActionResult ExecutePreviewCsv(int pPayPeriodId, int pIndividualId)
        {
            HRAdministrationHandler handler = new HRAdministrationHandler();

            handler.ExecuteCsvProcess(pPayPeriodId, pIndividualId, false);

            TimePayPeriodDTO dto = handler.GetPayPeriod(pPayPeriodId);
            PayPeriodViewModel model = new PayPeriodViewModel();
            model = AutoMapper.Mapper.Map(dto, model);

            return PartialView("_CsvWidgets", model);
        }


        [HttpPost]
        public ActionResult ExecuteFinalCsv(int pPayPeriodId, int pIndividualId)
        {
            HRAdministrationHandler handler = new HRAdministrationHandler();

            handler.ExecuteCsvProcess(pPayPeriodId, pIndividualId, true);

            TimePayPeriodDTO dto = handler.GetPayPeriod(pPayPeriodId);
            PayPeriodViewModel model = new PayPeriodViewModel();
            model = AutoMapper.Mapper.Map(dto, model);

            return PartialView("_CsvWidgets", model);
        }

        [HttpPost]
        public ActionResult UndoPayPeriod(int pPayPeriodId, int pIndividualId)
        {
            HRAdministrationHandler handler = new HRAdministrationHandler();

            handler.UndoPayPeriod(pPayPeriodId, pIndividualId);

            TimePayPeriodDTO dto = handler.GetPayPeriod(pPayPeriodId);
            PayPeriodViewModel model = new PayPeriodViewModel();
            model = AutoMapper.Mapper.Map(dto, model);

            return PartialView("_CsvWidgets", model);
        }

        [HttpPost]
        public ActionResult SendEmailNotification(int pPayPeriodId, string pEmailType)
        {
            HRAdministrationHandler handler = new HRAdministrationHandler();
            string status = DateTime.Now.ToShortDateString();

            try
            {
                TimePayPeriodDTO dto = handler.GetPayPeriod(pPayPeriodId);
                if (dto.txtStatus != "Processed")
                {
                    handler.SendEmailNotification(pPayPeriodId, pEmailType);
                }
                else
                {
                    switch (pEmailType)
                    {
                        case "AllNonExempt":
                            status = dto.dtmReminderAllNonExempt.Value.ToShortDateString();
                            break;
                        case "NonSubmittedNonExempt":
                            status = dto.dtmReminderUnSubmittedNonExempt.Value.ToShortDateString();
                            break;
                        case "NonExemptUnapproved":
                            status = dto.dtmReminderUnApproved.Value.ToShortDateString();
                            break;
                        case "ExemptUnapproved":
                            status = dto.dtmReminderUnApprovedExempt.Value.ToShortDateString();
                            break;
                        default:
                            throw new Exception("The email type is unsupported");
                    }
                }
            }
            catch (Exception)
            {
                //TODO: log exception
                status = "Error";
            }

            return Content(status);
        }


        public ActionResult PTOBalanceFileUpload()
        {
            PTOBalancesDataViewModel model = new PTOBalancesDataViewModel();
            HRAdministrationHandler handler = new HRAdministrationHandler();
            model.ColumnsToImport = new List<PTOBalanceColumns>();
            model.DataTable = new DataTable();
            model.ColumnsToImport = AutoMapper.Mapper.Map(handler.GetPTOBalanceColumns(), new List<PTOBalanceColumns>());
            model.ImportAccrualRate = false;

            return PartialView("_ImportPTOBalances", model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult ReadPTOBalanceCSVFileToUpload(HttpPostedFileBase csvFileUpload, string postData)
        {
            if (ModelState.IsValid)
            {
                PTOBalancesDataViewModel model = new PTOBalancesDataViewModel();
                HRAdministrationHandler handler = new HRAdministrationHandler();
                List<PTOBalanceColumns> ptoBalanceColumns = AutoMapper.Mapper.Map(handler.GetPTOBalanceColumns(), new List<PTOBalanceColumns>());

                model.ColumnsToImport = new List<PTOBalanceColumns>();
                model.DataTable = new DataTable();

                if (csvFileUpload != null && csvFileUpload.ContentLength > 0)
                {
                    if (csvFileUpload.FileName.EndsWith(".csv"))
                    {
                        DataTable csvOriginalTable = new DataTable();
                        csvOriginalTable = GetCSVDataTable(csvFileUpload);

                        DataTable csvFilteredTable = new DataTable();
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        dictionary = GetParamColumns(postData);

                        foreach (var item in dictionary)
                        {
                            if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value) && item.Key.Contains("LastUpdate"))
                            {
                                model.LastUpdateOfPTOBalances = DateTime.Parse(item.Value.Replace("%2F", "/"));
                            }
                            else
                            {
                                var columnToAdd = ptoBalanceColumns.FirstOrDefault(i => i.PTOBalanceColumnName == item.Key);
                                columnToAdd.ColumnPositionNumber = !string.IsNullOrEmpty(item.Value) ? int.Parse(item.Value) : columnToAdd.ColumnPositionNumber;
                                model.ColumnsToImport.Add(columnToAdd);
                            }
                        }
                        //DataTable csvSaveFile = new DataTable();
                        foreach (DataColumn col in csvOriginalTable.Columns)
                        {
                            foreach (var item in dictionary.Where(i => !i.Key.Contains("LastUpdate")))
                            {
                                if (item.Value != string.Empty)
                                {
                                    if (col.Ordinal == int.Parse(item.Value))
                                    {
                                        csvFilteredTable.Columns.Add(col.ColumnName, col.DataType);
                                        break;
                                    }
                                }
                            }
                        }

                        foreach (DataRow row in csvOriginalTable.Rows)
                        {
                            var rw = csvFilteredTable.Rows.Add();
                            foreach (DataColumn col in csvOriginalTable.Columns)
                            {
                                foreach (var item in dictionary.Where(i => !i.Key.Contains("LastUpdate")))
                                {
                                    if (!string.IsNullOrEmpty(item.Value))
                                    {
                                        if (col.Ordinal == int.Parse(item.Value))
                                        {
                                            rw[col.ColumnName] = row[col.ColumnName];
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        model.DataTable = csvFilteredTable;

                        return PartialView("_PTOBalancesDisplayCSVData", model.DataTable);
                    }
                    else
                    {
                        model.ColumnsToImport = ptoBalanceColumns;
                        ModelState.AddModelError("File", "The file format is not supported.");
                        //return PartialView("_PTOBalancesCSVDataEditor", model);
                        RedirectToAction("PTOBalanceFileUpload");
                    }
                }
                else
                {
                    model.ColumnsToImport = ptoBalanceColumns;
                    ModelState.AddModelError("File", "Please select CSV file to upload");
                    //return PartialView("_PTOBalancesCSVDataEditor", model);
                    RedirectToAction("PTOBalanceFileUpload");
                }
            }
            return RedirectToAction("PTOBalanceFileUpload");
        }

        public ActionResult ProcessPTOBalanceCSVFile(HttpPostedFileBase csvFileUpload, string postData)
        {
            if (ModelState.IsValid)
            {
                PTOBalancesDataViewModel model = new PTOBalancesDataViewModel();
                HRAdministrationHandler handler = new HRAdministrationHandler();
                List<PTOBalanceColumns> ptoBalanceColumns = AutoMapper.Mapper.Map(handler.GetPTOBalanceColumns(), new List<PTOBalanceColumns>());

                model.ColumnsToImport = new List<PTOBalanceColumns>();
                model.DataTable = new DataTable();
                DateTime lastUpdate = DateTime.Now;

                if (csvFileUpload != null && csvFileUpload.ContentLength > 0)
                {
                    if (csvFileUpload.FileName.EndsWith(".csv"))
                    {
                        DataTable csvOriginalTable = new DataTable();
                        csvOriginalTable = GetCSVDataTable(csvFileUpload);

                        DataTable csvFilteredTable = new DataTable();
                        Dictionary<string, string> dictionary = new Dictionary<string, string>();
                        dictionary = GetParamColumns(postData);
                        foreach (var item in dictionary)
                        {
                            if (!string.IsNullOrEmpty(item.Key) && !string.IsNullOrEmpty(item.Value) && item.Key.Contains("LastUpdate"))
                            {
                                lastUpdate = DateTime.Parse(item.Value.Replace("%2F", "/"));
                                break;
                            }
                        }
                        //add columns to the filteredtable
                        foreach (DataColumn col in csvOriginalTable.Columns)
                        {
                            foreach (var item in dictionary.Where(i => !i.Key.Contains("LastUpdate")))
                            {
                                if (item.Value != string.Empty)
                                {
                                    if (col.Ordinal == int.Parse(item.Value))
                                    {
                                        csvFilteredTable.Columns.Add(item.Key, col.DataType);
                                        break;
                                    }
                                }
                            }
                        }
                        //add rows to the filteredtable
                        foreach (DataRow row in csvOriginalTable.Rows)
                        {
                            var rw = csvFilteredTable.Rows.Add();
                            foreach (DataColumn col in csvOriginalTable.Columns)
                            {
                                foreach (var item in dictionary.Where(i => !i.Key.Contains("LastUpdate")))
                                {
                                    if (!string.IsNullOrEmpty(item.Value))
                                    {
                                        if (col.Ordinal == int.Parse(item.Value))
                                        {
                                            rw[item.Key] = row[col.Ordinal];
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        ProcessPTOBalanceCSVData(csvFilteredTable, postData, lastUpdate);
                        return PartialView("_PTOBalancesDisplayCSVData", csvFilteredTable);
                    }
                }
                else
                {
                    model.ColumnsToImport = ptoBalanceColumns;
                    ModelState.AddModelError("File", "Please select CSV file to upload.");
                    RedirectToAction("PTOBalanceFileUpload");
                }

            }
            return RedirectToAction("PTOBalanceFileUpload");
        }


        [HttpPost]
        public ActionResult ApproveCheckedEmployees(SelectedEmployeeViewModel SelectedEmployees, string ApproverEmployeeInfoID)
        {
            if (SelectedEmployees != null)
            {
                var pPayPeriodId = int.Parse(SelectedEmployees.SelectedPayPeriodId);
                HRAdministrationHandler handler = new HRAdministrationHandler();
                if (SelectedEmployees.SelectedPayPeriodId != null && SelectedEmployees.SelectedEmployeeIndividualIds != null)
                {
                    handler.ExecuteApproveCheckedTimesheet(SelectedEmployees.SelectedEmployeeIndividualIds, int.Parse(ApproverEmployeeInfoID), int.Parse(SelectedEmployees.SelectedPayPeriodId));
                }
                return RedirectToAction("GetPayPeriodEmployees", new { PayPeriodId = pPayPeriodId });
            }
            return null;
        }

        private Dictionary<string, string> GetParamColumns(string postData)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            var paramColumns = postData.Split('&');
            foreach (var item in paramColumns)
            {
                var keyvalue = item.Split('=');
                result.Add(keyvalue[0], keyvalue[1]);
            }
            return result;
        }

        private DataTable GetCSVDataTable(HttpPostedFileBase csvFileUpload)
        {
            if (csvFileUpload != null && csvFileUpload.ContentLength > 0)
            {
                if (csvFileUpload.FileName.EndsWith(".csv"))
                {
                    Stream stream = csvFileUpload.InputStream;
                    DataTable csvTable = new DataTable();
                    using (CsvReader csvReader = new CsvReader(new StreamReader(stream), true))
                    {
                        csvTable.Load(csvReader);
                        return csvTable;
                    }
                }
            }
            return null;
        }

        private void ProcessPTOBalanceCSVData(DataTable csvSaveFile, string postData, DateTime lastUpdate)
        {
            if (csvSaveFile != null)
            {
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
                HRAdministrationHandler handler = new HRAdministrationHandler();
                List<PTOBalanceViewModel> ptoBalances = new List<PTOBalanceViewModel>();
                dictionary = GetParamColumns(postData);

                foreach (DataRow dr in csvSaveFile.Rows)
                {
                    PTOBalanceViewModel ptoVm = new PTOBalanceViewModel();
                    foreach (DataColumn col in csvSaveFile.Columns)
                    {

                        if (col.ColumnName == dictionary.FirstOrDefault(i => i.Key == "FileNumber").Key)
                        {
                            ptoVm.FileNumber = dr[col.Ordinal].ToString();
                        }
                        if (col.ColumnName == dictionary.FirstOrDefault(i => i.Key == "AccrualRate").Key)
                        {
                            ptoVm.Accrual = dr[col.Ordinal].ToString().Contains("(") ? decimal.Parse(dr[col.Ordinal].ToString().Replace("(", "").Replace(")", "")) * -1.00M : decimal.Parse(dr[col.Ordinal].ToString());
                        }
                        if (col.ColumnName == dictionary.FirstOrDefault(i => i.Key == "Allowed").Key)
                        {
                            ptoVm.Allowed = dr[col.Ordinal].ToString().Contains("(") ? decimal.Parse(dr[col.Ordinal].ToString().Replace("(", "").Replace(")", "")) * -1.00M : decimal.Parse(dr[col.Ordinal].ToString());
                        }
                        if (col.ColumnName == dictionary.FirstOrDefault(i => i.Key == "Taken").Key)
                        {
                            ptoVm.Taken = dr[col.Ordinal].ToString().Contains("(") ? decimal.Parse(dr[col.Ordinal].ToString().Replace("(", "").Replace(")", "")) * -1.00M : decimal.Parse(dr[col.Ordinal].ToString());
                        }
                        if (col.ColumnName == dictionary.FirstOrDefault(i => i.Key == "Balance").Key)
                        {
                            ptoVm.Balance = dr[col.Ordinal].ToString().Contains("(") ? decimal.Parse(dr[col.Ordinal].ToString().Replace("(", "").Replace(")", "")) * -1.00M : decimal.Parse(dr[col.Ordinal].ToString());
                        }
                        if (col.ColumnName == dictionary.FirstOrDefault(i => i.Key == "CompanyCode").Key)
                        {
                            ptoVm.CompanyCode = dr[col.Ordinal].ToString();
                        }
                    }
                    ptoVm.LastUpdated = lastUpdate;
                    ptoBalances.Add(ptoVm);
                }

                //upsert pto balances
                foreach (var item in ptoBalances)
                {
                    var data = AutoMapper.Mapper.Map(item, new PTOBalancesDTO());
                    handler.UpsertPTOBalance(AutoMapper.Mapper.Map(data, new tblTSPTOBalance()));
                }
            }
        }

        private List<PayPeriodViewModel> GetPayPeriods()
        {
            var currentDate = DateTime.Now;
            var payperiods = AutoMapper.Mapper.Map(hrAdminRepository.GetAllPayPeriodBasedOnCurrentDate(currentDate), new List<PayPeriodViewModel>());
            return payperiods;
        }

        private List<SelectListItem> GetListOfPayrollYears(DateTime? currentDate = null)
        {
            currentDate = currentDate ?? DateTime.Now;
            var currentYear = DateTime.Now.Year.ToString();
            var payrollYears = new List<SelectListItem>();
            HRAdministrationHandler handler = new HRAdministrationHandler();
            //var listOfYears = hrAdminRepository.GenerateListOfYearsBasedOnDate((DateTime)currentDate).OrderBy(i => i).ToList();
            var listOfYears = handler.GetAllPayPeriodBasedOnCurrentDate((DateTime)currentDate).Select(i => i.dtmPeriodEnd.Year.ToString()).Distinct().ToList();
            foreach (var lstYear in listOfYears) { payrollYears.Add(new SelectListItem { Text = lstYear, Value = lstYear, Selected = (lstYear == currentYear) }); }
            return payrollYears.OrderByDescending(i => i.Value).ToList();
        }

        //private void SavePayPeriod(NM.Web.WebApplication.Timesheets.Models.ViewModel.PayPeriodViewModel pModel)
        //{
        //    NM.Lib.DataLibrary.United.Domain.TimePayPeriodDTO payPeriodDTO = new Lib.DataLibrary.United.Domain.TimePayPeriodDTO();
        //    AutoMapper.Mapper.Map(pModel, payPeriodDTO);
        //    hrAdminRepository.Save(payPeriodDTO);
        //}

        #endregion

    }
}