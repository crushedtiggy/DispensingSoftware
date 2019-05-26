using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.text.html.simpleparser;
using System.IO;
using System.Text;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication.Controllers
{
    public class HospitalController : Controller
    {
        // GET: /<controller>/

        [Authorize]
        public IActionResult Index()
        {
            string userid = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            List<User> model = DBUtl.GetList<User>(
                                                 @"SELECT * FROM [user] 
                                                WHERE User_id = {0}",
                                                       userid);
            return View(model);
        }

        [Authorize]
        public IActionResult Patients()
        {
            List<Patient> model = DBUtl.GetList<Patient>(
                                                 "SELECT * FROM patient ORDER BY Registered_datetime");
            return View(model);
        }

        [Authorize]
        public IActionResult Medicines()
        {
            List<Medicine> model = DBUtl.GetList<Medicine>(
                                    "SELECT Medicine_id, Category_type, Subcategory_type, Medicine_name, Brand, Quantity, Unit, Threshold FROM medicine ORDER BY Medicine_id, Category_type, Subcategory_type");
            return View(model);
        }

        [Authorize]
        public IActionResult Categories()
        {
            List<Category> model = DBUtl.GetList<Category>(
                        "SELECT * FROM category ORDER BY Category_type");
            return View(model);
        }

        [Authorize]
        public IActionResult Subcategories()
        {
            List<Subcategory> model = DBUtl.GetList<Subcategory>(
                                    "SELECT * FROM subcategory ORDER BY Category_type, Subcategory_type");
            return View(model);
        }

        [Authorize]
        [HttpGet]
        public IActionResult AddPrescription()
        {
            List<Queue> queues = DBUtl.GetList<Queue>(
                                    " SELECT TOP 1 Queue_id, Patient_id, Serve_status_id, Queue_category_id, Queue_datetime, Waiting_minutes FROM queue WHERE queue.serve_status_id = '1' ORDER BY CASE WHEN Queue_category_id = '1' THEN Queue_category_id ELSE Queue_datetime END, Queue_category_id, CASE WHEN Waiting_minutes > 60 AND Serve_status_id = '1' THEN Waiting_minutes ELSE Queue_datetime END; ");
            ViewData["PatientNumber"] = queues.FirstOrDefault().Patient_id;
            var medicines = DBUtl.GetList<Medicine>(
                                     "SELECT * FROM medicine ORDER BY Medicine_id");
            ViewData["medicine"] = new SelectList(medicines, "Medicine_id", "Medicine_name");

            var dosages = DBUtl.GetList<Dosage>(
                         "SELECT * FROM dosage ORDER BY Dosage_id");
            ViewData["dosage"] = new SelectList(dosages, "Dosage_id", "Long_form_description");
            return View();
        }

        [Authorize]
        public IActionResult Prescriptions()
        {
            List<Prescription> model = DBUtl.GetList<Prescription>(
                                     "SELECT * FROM prescription ORDER BY Prescription_id");
            ViewData["patients"] = DBUtl.GetList<Patient>(
                                                 "SELECT * FROM patient ORDER BY Patient_id");
            return View(model);
        }

        public IActionResult AddPatient()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddPatient(Patient newPatient)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("AddPatient");
            }
            else
            {
                int queueNo = CheckQueueNumber(GetQueueCategoryId(newPatient));
                string sql = @"BEGIN TRANSACTION;  
  
BEGIN TRY  
    -- Generate a constraint violation error.  
    SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;  
    INSERT INTO patientcheck (Name, Nric, Gender, Date_of_birth, Race, Height, Weight, Allergy, Smoke, Alcohol, Has_travel, Has_flu, Has_following_symptoms, Address, Postal_code, Phone_no, Email, Remarks, Registered_datetime, Is_Urgent) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', GETDATE(), '{18}');
    INSERT INTO category{42} (Queue_no, Created_by) VALUES ('{43}', '{44}');
    INSERT INTO patient (Queue_id, Name, Nric, Gender, Date_of_birth, Race, Height, Weight, Allergy, Smoke, Alcohol, Has_travel, Has_flu, Has_following_symptoms, Address, Postal_code, Phone_no, Email, Remarks, Registered_datetime, Is_Urgent)
                                    VALUES ('{19}', '{20}', '{21}', '{22}', '{23}', '{24}', '{25}', '{26}', '{27}', '{28}', '{29}', '{30}', '{31}', '{32}', '{33}', '{34}', '{35}', '{36}', '{37}', GETDATE(), '{38}');
    INSERT INTO queue (Queue_id, Patient_id, Serve_status_id, Queue_category_id, Queue_datetime) VALUES ('{39}', (SELECT Patient_id FROM patient WHERE Patient_id = @@IDENTITY), '{40}', '{41}', GETDATE());
    DELETE from patientcheck WHERE Check_id = (SELECT MIN(Check_id) FROM patientcheck);
END TRY  
BEGIN CATCH  
    SELECT   
        ERROR_NUMBER() AS ErrorNumber  
        ,ERROR_SEVERITY() AS ErrorSeverity  
        ,ERROR_STATE() AS ErrorState  
        ,ERROR_PROCEDURE() AS ErrorProcedure  
        ,ERROR_LINE() AS ErrorLine  
        ,ERROR_MESSAGE() AS ErrorMessage;  
  
    IF @@TRANCOUNT > 0  
        ROLLBACK TRANSACTION;  
END CATCH;  
  
IF @@TRANCOUNT > 0  
    COMMIT TRANSACTION;";

                if (DBUtl.ExecSQL(sql, newPatient.Name, newPatient.Nric,
                                                       newPatient.Gender, $"{newPatient.Date_of_birth:yyyy-MM-dd}", newPatient.Race,
                                                       newPatient.Height, newPatient.Weight, newPatient.Allergy, newPatient.Smoke, newPatient.Alcohol, newPatient.Has_travel, newPatient.Has_flu, newPatient.Has_following_symptoms, newPatient.Address, newPatient.Postal_code, newPatient.Phone_no, newPatient.Email, newPatient.Remarks, newPatient.Is_Urgent, queueNo, newPatient.Name, newPatient.Nric,
                                                       newPatient.Gender, $"{newPatient.Date_of_birth:yyyy-MM-dd}", newPatient.Race,
                                                       newPatient.Height, newPatient.Weight, newPatient.Allergy, newPatient.Smoke, newPatient.Alcohol, newPatient.Has_travel, newPatient.Has_flu, newPatient.Has_following_symptoms, newPatient.Address, newPatient.Postal_code, newPatient.Phone_no, newPatient.Email, newPatient.Remarks, newPatient.Is_Urgent, queueNo, 1, GetQueueCategoryId(newPatient), GetQueueCategoryId(newPatient), queueNo, User.Identity.Name) > 0)
                {
                    ViewData["QueueNumber"] = queueNo;
                    return View("ShowQueueNumber");
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                    return View("Index");
                }
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult AddPrescription(Prescription prescription)
        {
            List<Queue> queues = DBUtl.GetList<Queue>(" SELECT TOP 1 Queue_id, Patient_id, Serve_status_id, Queue_category_id, Queue_datetime, Waiting_minutes FROM queue WHERE queue.serve_status_id = '1' ORDER BY CASE WHEN Queue_category_id = '1' THEN Queue_category_id ELSE Queue_datetime END, Queue_category_id, CASE WHEN Waiting_minutes > 60 AND Serve_status_id = '1' THEN Waiting_minutes ELSE Queue_datetime END; ");

            if (!ModelState.IsValid)
            {
                ViewData["Message"] = DBUtl.DB_Message;
                ViewData["MsgType"] = "warning";
                return View("AddPrescription");
            }
            else
            {


                string sql =
                       @"INSERT INTO prescription WITH (TABLOCKX)  (Patient_id, Medicine_id, Dosage_id, Doctor_mcr, Doctor_name, Practicing_place_name, Practicing_address, Booking_appointment, Case_notes, Duration, Dosage_quantity, Instructions)
                                    VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}');

UPDATE queue WITH (TABLOCKX) SET Serve_status_id = '2' WHERE Patient_id = '{12}' AND Queue_id = '{13}'";

                if (DBUtl.ExecSQL(sql, prescription.Patient_id, prescription.Medicine_id, prescription.Dosage_id, prescription.Doctor_mcr, prescription.Doctor_name, prescription.Practicing_place_name, prescription.Practicing_address, $"{prescription.Booking_appointment:yyyy-MM-dd HH:mm}", prescription.Case_notes, prescription.Duration, prescription.Dosage_quantity, prescription.Instructions, queues.FirstOrDefault().Patient_id, queues.FirstOrDefault().Queue_id) > 0)
                {
                    TempData["Message"] = "Prescription Added";
                    TempData["MsgType"] = "success";
                    return RedirectToAction("Index");

                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                    return View("Index");
                }
            }
        }

        [Authorize]
        public IActionResult EditPrescription(String id)
        {
            string sql = "SELECT * FROM Prescription WHERE Prescription_id={0}";
            string select = String.Format(sql, id);
            DataTable dt = DBUtl.GetTable(select);
            if (dt.Rows.Count == 1)
            {
                Prescription prep = new Prescription
                {
                    Prescription_id = (int)dt.Rows[0]["Prescription_id"],
                    Patient_id = (int)dt.Rows[0]["Patient_id"],
                    Medicine_id = (int)dt.Rows[0]["Medicine_id"],
                    Dosage_id = (int)dt.Rows[0]["Dosage_id"],
                    Doctor_mcr = dt.Rows[0]["Doctor_mcr"].ToString(),
                    Doctor_name = dt.Rows[0]["Doctor_name"].ToString(),
                    Practicing_place_name = dt.Rows[0]["Practicing_place_name"].ToString(),
                    Practicing_address = dt.Rows[0]["Practicing_address"].ToString(),
                    Booking_appointment = (DateTime)dt.Rows[0]["Booking_appointment"],
                    Duration = (int)dt.Rows[0]["Duration"],
                    Dosage_quantity = (int)dt.Rows[0]["Dosage_quantity"],
                    Instructions = dt.Rows[0]["Instructions"].ToString()
                };
                var patients = DBUtl.GetList<Patient>(
                                     "SELECT * FROM patient ORDER BY Patient_id");
                ViewData["patient"] = new SelectList(patients, "Patient_id", "Name");

                var medicines = DBUtl.GetList<Medicine>(
                                         "SELECT * FROM medicine ORDER BY Medicine_id");
                ViewData["medicine"] = new SelectList(medicines, "Medicine_id", "Medicine_name");

                var dosages = DBUtl.GetList<Dosage>(
                             "SELECT * FROM dosage ORDER BY Dosage_id");
                ViewData["dosage"] = new SelectList(dosages, "Dosage_id", "Long_form_description");

                return View(prep);
            }
            else
            {
                TempData["Message"] = "Prescription Not Found";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Prescriptions");
            }
        }

        // Show queue number after patient is registered (Patient)
        [Authorize]
        public IActionResult ShowQueueNumber()
        {
            return View();
        }

        // Display queue number based on first come first serve (Prescription)
        [Authorize]
        public IActionResult GetQueueNumber()
        {
            var queues = DBUtl.GetTable(
                                    "SELECT TOP 1 Queue_id, Patient_id, Serve_status_id, Queue_category_id, Queue_datetime, Waiting_minutes FROM queue WHERE queue.serve_status_id = '1' ORDER BY CASE WHEN Queue_category_id = '1' THEN Queue_category_id ELSE Queue_datetime END, Queue_category_id, CASE WHEN Waiting_minutes > 60 AND Serve_status_id = '1' THEN Waiting_minutes ELSE Queue_datetime END; ");
            if (queues.Rows.Count == 0)
            {
                TempData["Message"] = "No queue number available";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
            int queue = (int)queues.Rows[0]["Queue_id"];
            int patient = (int)queues.Rows[0]["Patient_id"];
            ViewData["QueueNumber"] = queue;
            ViewData["PatientNumber"] = patient;
            return View();
        }

        [Authorize]
        [HttpGet]
        public IActionResult DoPayment()
        {
            var select = DBUtl.GetTable("SELECT TOP 1 Bill_transaction_id, bill_transaction.Prescription_id, bill_transaction.Queue_id, patient.Name, patient.Nric, Medicine_name, Dosage_quantity, Price, Dosage_quantity * Price AS Subtotal FROM bill_transaction, medicine, prescription, patient, queue WHERE queue.Serve_status_id = '4' AND queue.Patient_id = patient.Patient_id AND prescription.Patient_id = queue.Patient_id AND prescription.Medicine_id = medicine.Medicine_id AND bill_transaction.Prescription_id = prescription.Prescription_id;");

            var types = DBUtl.GetList<Payment_Type>(
                         "SELECT * FROM payment_type ORDER BY Payment_type");


            if (select.Rows.Count == 1)
            {
                ViewData["Transaction"] = (int)select.Rows[0]["Bill_transaction_id"];
                ViewData["Prescription"] = (int)select.Rows[0]["Prescription_id"];
                ViewData["Queue"] = (int)select.Rows[0]["Queue_id"];
                ViewData["Date"] = DateTime.Now.ToString();
                ViewData["Name"] = select.Rows[0]["Name"].ToString();
                ViewData["NRIC"] = select.Rows[0]["Nric"].ToString();
                ViewData["Medicine"] = select.Rows[0]["Medicine_name"].ToString();
                ViewData["Qty"] = (int)select.Rows[0]["Dosage_quantity"];
                ViewData["UnitPrice"] = Convert.ToDouble(select.Rows[0]["Price"]);
                ViewData["Subtotal"] = Convert.ToDouble(select.Rows[0]["Subtotal"]);
                ViewData["Types"] = new SelectList(types, "Payment_type", "Payment_type_description");
                return View();
            }
            else
            {
                TempData["Message"] = "No transaction available";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult DoPayment(Bill_transaction bill_Transaction)
        {
            string subtotal = HttpContext.Request.Form["subtotal"].ToString();

            string update = @"UPDATE bill_transaction WITH (TABLOCKX) SET Payment_type = '{0}', Subtotal = '{1}', Payment_datetime = GETDATE() WHERE Prescription_id = '{2}' AND Bill_transaction_id = '{3}' AND Queue_id = '{4}' ;

UPDATE queue WITH (TABLOCKX) SET Serve_status_id = '5' WHERE Queue_id = '{4}' AND Serve_status_id = '4';";
            if (DBUtl.ExecSQL(update, bill_Transaction.Payment_type, Double.Parse(subtotal), bill_Transaction.Prescription_id, bill_Transaction.Bill_transaction_id, bill_Transaction.Queue_id) > 0)
            {
                var select = DBUtl.GetTable("SELECT TOP 1 Bill_transaction_id, bill_transaction.Prescription_id, bill_transaction.Queue_id, patient.Name, patient.Nric, Medicine_name, Dosage_quantity, Price, Dosage_quantity * Price AS Subtotal, payment_type.Payment_type_description FROM bill_transaction, medicine, prescription, patient, queue, payment_type WHERE queue.Serve_status_id = '5' AND queue.Patient_id = patient.Patient_id AND prescription.Patient_id = queue.Patient_id AND prescription.Medicine_id = medicine.Medicine_id AND bill_transaction.Prescription_id = prescription.Prescription_id AND bill_transaction.Payment_type = payment_type.Payment_type AND Bill_transaction_id = '{0}'; ", bill_Transaction.Bill_transaction_id);

                ViewData["Transaction"] = (int)select.Rows[0]["Bill_transaction_id"];
                ViewData["Prescription"] = (int)select.Rows[0]["Prescription_id"];
                ViewData["Queue"] = (int)select.Rows[0]["Queue_id"];
                ViewData["Date"] = DateTime.Now.ToString();
                ViewData["Name"] = select.Rows[0]["Name"].ToString();
                ViewData["NRIC"] = select.Rows[0]["Nric"].ToString();
                ViewData["Medicine"] = select.Rows[0]["Medicine_name"].ToString();
                ViewData["Qty"] = (int)select.Rows[0]["Dosage_quantity"];
                ViewData["UnitPrice"] = Convert.ToDouble(select.Rows[0]["Price"]);
                ViewData["Subtotal"] = Convert.ToDouble(select.Rows[0]["Subtotal"]);
                ViewData["Types"] = select.Rows[0]["Payment_type_description"].ToString();

                return View("TransactionBill", select.Rows[0]["Bill_transaction_id"]);
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
                return View("Index");
            }
        }

        public int GetQueueCategoryId(Patient patient)
        {
            int queueid = 0;
            int age = DateTime.Today.Year - patient.Date_of_birth.Year;

            if (patient.Is_Urgent.Equals("true"))
            {
                queueid = 1;
            }
            else
            {
                if (age > 65)
                {
                    queueid = 2;
                }
                else if (age >= 15 && age <= 64)
                {
                    queueid = 4;
                }
                else if (age >= 0 && age <= 14)
                {
                    queueid = 3;
                }
            }
            return queueid;
        }


        public int CheckQueueNumber(int queueid)
        {
            List<Category1> category1s = DBUtl.GetList<Category1>(
                                     "SELECT * FROM category1 ORDER BY Queue_no");
            List<Category2> category2s = DBUtl.GetList<Category2>(
                                     "SELECT * FROM category2 ORDER BY Queue_no");
            List<Category3> category3s = DBUtl.GetList<Category3>(
                                     "SELECT * FROM category3 ORDER BY Queue_no");
            List<Category4> category4s = DBUtl.GetList<Category4>(
                                     "SELECT * FROM category4 ORDER BY Queue_no");
            int queueNo;

            if (queueid == 1)
            {
                if (category1s.Count == 0)
                {
                    queueNo = 1000;
                }
                else
                {
                    queueNo = category1s.LastOrDefault().Queue_no + 1;
                }
            }
            else if (queueid == 2)
            {
                if (category2s.Count == 0)
                {
                    queueNo = 2000;
                }
                else
                {
                    queueNo = category2s.LastOrDefault().Queue_no + 1;
                }
            }
            else if (queueid == 3)
            {
                if (category3s.Count == 0)
                {
                    queueNo = 3000;
                }
                else
                {
                    queueNo = category3s.LastOrDefault().Queue_no + 1;
                }
            }
            else
            {
                if (category4s.Count == 0)
                {
                    queueNo = 4000;
                }
                else
                {
                    queueNo = category4s.LastOrDefault().Queue_no + 1;
                }
            }
            return queueNo;
        }

        [Authorize]
        [HttpPost]
        public IActionResult ModifyCaseNotes()
        {
            string id = HttpContext.Request.Form["prescriptionid"].ToString();
            string notes = HttpContext.Request.Form["text"].ToString().Trim();
            string update = @"UPDATE prescription WITH (TABLOCKX) SET Case_notes = '{0}' WHERE Prescription_id = '{1}'";
            if (DBUtl.ExecSQL(update, notes, id) == 1)
            {
                TempData["Message"] = "Case Notes modified";
                TempData["MsgType"] = "success";
                return RedirectToAction("Prescriptions");
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
                return View("Prescriptions");
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult ModifyMedicineName()
        {
            string id = HttpContext.Request.Form["medicineid"].ToString();
            string name = HttpContext.Request.Form["text"].ToString().Trim();
            string update = @"UPDATE medicine WITH (TABLOCKX) SET Medicine_name = '{0}' WHERE Medicine_id = '{1}'";
            if (DBUtl.ExecSQL(update, name, id) == 1)
            {
                TempData["Message"] = "Medicine Name modified";
                TempData["MsgType"] = "success";
                return RedirectToAction("Medicines");
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
                return View("Medicines");
            }
        }

        [HttpPost]
        public IActionResult EditPrescription(Prescription prescription)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = DBUtl.DB_Message;
                ViewData["MsgType"] = "warning";
                return View("EditPrescription");
            }
            else
            {

                string update = @"UPDATE prescription WITH (TABLOCKX) SET Medicine_id = '{0}', Dosage_id = '{1}', Doctor_mcr = '{2}', Doctor_name = '{3}', Practicing_place_name = '{4}', Practicing_address = '{5}', Booking_appointment = '{6}', Duration = '{7}', Dosage_quantity = '{8}', Instructions = '{9}' WHERE Prescription_id = '{10}' AND Patient_id = '{11}'";

                if (DBUtl.ExecSQL(update, prescription.Medicine_id, prescription.Dosage_id, prescription.Doctor_mcr, prescription.Doctor_name, prescription.Practicing_place_name, prescription.Practicing_address, $"{prescription.Booking_appointment:yyyy-MM-dd HH:mm}", prescription.Duration, prescription.Dosage_quantity, prescription.Instructions, prescription.Prescription_id, prescription.Patient_id) == 1)
                {
                    TempData["Message"] = "Prescription Edited";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("Prescriptions");
            }
        }

        [Authorize]
        [HttpGet] // display the dispensing details before confirming dispensed
        public IActionResult PackStatus()
        {
            var select = DBUtl.GetTable("SELECT TOP 1 patient.Name, queue.Queue_id, queue.Patient_id, prescription.Prescription_id, prescription.Medicine_id, prescription.Dosage_quantity, prescription.Dosage_id FROM queue, patient, prescription WHERE queue.Serve_status_id = '2' AND queue.Patient_id = patient.Patient_id AND prescription.Patient_id = queue.Patient_id; ");
            var queueId = select.Rows[0]["Queue_id"];
            var patientId = select.Rows[0]["Patient_id"];
            var update = @"UPDATE queue SET Serve_status_id = '3' WHERE queue.Queue_id = '{0}' AND queue.Patient_id = '{1}'";

            if (select.Rows.Count == 1 && DBUtl.ExecSQL(update, queueId, patientId) == 1)
            {
                ViewData["Name"] = select.Rows[0]["Name"].ToString();
                ViewData["QueueNumber"] = (int)select.Rows[0]["Queue_id"];
                ViewData["PatientNumber"] = (int)select.Rows[0]["Patient_id"];
                ViewData["PrescriptionNumber"] = (int)select.Rows[0]["Prescription_id"];
                ViewData["MedicineNumber"] = (int)select.Rows[0]["Medicine_id"];
                ViewData["DosageNumber"] = (int)select.Rows[0]["Dosage_id"];
                return View();
            }
            else
            {
                TempData["Message"] = "No prescription ready to dispense";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
        }

        // Display queue number based on first come first serve (Pack Status)
        public IActionResult GetPrescription()
        {
            var queues = DBUtl.GetTable(
                                    "SELECT TOP 1 patient.Name, queue.Queue_id, queue.Patient_id, prescription.Prescription_id, prescription.Medicine_id, prescription.Dosage_quantity, prescription.Dosage_id FROM queue, patient, prescription WHERE queue.Serve_status_id = '2' AND queue.Patient_id = patient.Patient_id AND prescription.Patient_id = queue.Patient_id;");
            if (queues.Rows.Count == 0)
            {
                TempData["Message"] = "No queue number available";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
            int queue = (int)queues.Rows[0]["Queue_id"];
            ViewData["QueueNumber"] = queue;
            return View();
        }

        [Authorize]
        [HttpPost] // pack medicines based on prescription before proceeding to dispenser system
        public IActionResult PackForDispense()
        {
            string queue = HttpContext.Request.Form["queueid"];
            string prescription = HttpContext.Request.Form["prescriptionid"];

            string insert = @"INSERT INTO bill_transaction WITH (TABLOCKX) (Prescription_id, Queue_id) VALUES('{0}','{1}');

UPDATE queue WITH (TABLOCKX) SET Serve_status_id = '7' WHERE Queue_id = '{1}' AND Serve_status_id = '3';";
            if (DBUtl.ExecSQL(insert, prescription, queue) > 0)
            {
                TempData["Message"] = "Packed and ready for dispense!";
                TempData["MsgType"] = "success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
                return View("Index");
            }
        }

        // Display queue number based on first come first serve (Dispenser System)
        [Authorize]
        public IActionResult GetPack()
        {
            var queues = DBUtl.GetTable(
                                    "SELECT TOP 1 patient.Name, queue.Queue_id, queue.Patient_id, prescription.Prescription_id, prescription.Medicine_id, prescription.Dosage_quantity, prescription.Dosage_id FROM queue, patient, prescription WHERE queue.Serve_status_id = '7' AND queue.Patient_id = patient.Patient_id AND prescription.Patient_id = queue.Patient_id;");
            if (queues.Rows.Count == 0)
            {
                TempData["Message"] = "No queue number available";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
            var queueId = queues.Rows[0]["Queue_id"];
            var patientId = queues.Rows[0]["Patient_id"];
            var medicineid = queues.Rows[0]["Medicine_id"];
            var qty = queues.Rows[0]["Dosage_quantity"];
            ViewData["QueueNumber"] = queueId;
            ViewData["Medicine"] = medicineid;
            ViewData["Qty"] = qty;
            ViewData["PatientNumber"] = patientId;
            return View();
        }

        [HttpPost] // To properly dispense medicine and update quantity
        [Authorize]
        public IActionResult Dispense()
        {
            string medicine = HttpContext.Request.Form["medicine"];
            string qty = HttpContext.Request.Form["qty"];
            string queue = HttpContext.Request.Form["queue"];
            string patient = HttpContext.Request.Form["patient"];

            string update = @"UPDATE medicine WITH (TABLOCKX) SET Quantity = Quantity - {0} WHERE Medicine_id = '{1}';";
            var updateStatus = @"UPDATE queue WITH (TABLOCKX) SET Serve_status_id = '4' WHERE queue.Queue_id = '{0}' AND queue.Patient_id = '{1}'";
            if (DBUtl.ExecSQL(update, Int32.Parse(qty), medicine) == 1 && DBUtl.ExecSQL(updateStatus, queue, patient) == 1)
            {
                TempData["Message"] = "Medicine has been dispensed";
                TempData["MsgType"] = "success";
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
                return View("Index");
            }
        }

        // Display queue number based on first come first serve (Payment)
        [Authorize]
        public IActionResult GetPayment()
        {
            var queues = DBUtl.GetTable(
                                   "SELECT TOP 1 patient.Name, queue.Queue_id, queue.Patient_id, prescription.Prescription_id, prescription.Medicine_id, prescription.Dosage_quantity, prescription.Dosage_id FROM queue, patient, prescription WHERE queue.Serve_status_id = '4' AND queue.Patient_id = patient.Patient_id AND prescription.Patient_id = queue.Patient_id;");
            if (queues.Rows.Count == 0)
            {
                TempData["Message"] = "No queue number available";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
            int queue = (int)queues.Rows[0]["Queue_id"];
            ViewData["QueueNumber"] = queue;
            return View();
        }

        // To check status of patient
        [Authorize]
        public IActionResult CheckDispensaryStatus()
        {
            return View();
        }

        [Authorize]
        public IActionResult ShowDispensaryStatus()
        {
            return View();
        }

        // To return result of the patient which user has requested to check
        [Authorize]
        [HttpPost]
        public IActionResult ShowDispensaryStatus(Queue queue)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = DBUtl.DB_Message;
                ViewData["MsgType"] = "warning";
                return View("CheckDispensaryStatus");
            }
            else
            {

                var search = DBUtl.GetTable("SELECT Name, NRIC, Serve_status_description FROM patient, queue, serve_status WHERE queue.Queue_id = patient.Queue_id AND queue.Serve_status_id = serve_status.Serve_status_id AND queue.Queue_id = '{0}';", queue.Queue_id);

                if (search.Rows.Count == 1)
                {
                    ViewData["Name"] = search.Rows[0]["Name"].ToString();
                    ViewData["NRIC"] = search.Rows[0]["NRIC"].ToString();
                    ViewData["Status"] = search.Rows[0]["Serve_status_description"].ToString();
                    TempData["Message"] = String.Format("{0} search result found", search.Rows.Count);
                    TempData["MsgType"] = "success";
                    return View();
                }
                else
                {
                    TempData["Message"] = "Search result not found";
                    TempData["MsgType"] = "danger";
                    return View("CheckDispensaryStatus");
                }
            }
        }

        [Authorize]
        public IActionResult QueueCategories()
        {
            return View();
        }

        [Authorize]
        public IActionResult CheckWaitingDuration()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult ShowWaitingDuration(Queue queue)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = DBUtl.DB_Message;
                ViewData["MsgType"] = "warning";
                return View("CheckWaitingDuration");
            }
            else
            {
                var search = DBUtl.GetTable("SELECT Name, NRIC, Serve_status_description, Waiting_minutes FROM patient, queue, serve_status WHERE queue.Queue_id = patient.Queue_id AND queue.Serve_status_id = serve_status.Serve_status_id AND (Waiting_minutes >= 60 * {0} OR Waiting_minutes <= 60 * {0});", queue.Waiting_minutes);

                if (search.Rows.Count > 0)
                {
                    TempData["Message"] = String.Format("{0} search result found", search.Rows.Count);
                    TempData["MsgType"] = "success";
                    return View(search);
                }
                else
                {
                    TempData["Message"] = "Search result not found";
                    TempData["MsgType"] = "danger";
                    return View("CheckWaitingDuration");
                }
            }
        }

        [Authorize]
        public IActionResult Transactions()
        {
            var transactions = DBUtl.GetTable("SELECT Bill_transaction_id, bill_transaction.Prescription_id, bill_transaction.Queue_id, patient.Name, patient.Nric, Medicine_name, Dosage_quantity, Price, Dosage_quantity * Price AS Subtotal, payment_type.Payment_type_description FROM bill_transaction, medicine, prescription, patient, queue, payment_type WHERE queue.Serve_status_id = '5' AND queue.Patient_id = patient.Patient_id AND prescription.Patient_id = queue.Patient_id AND prescription.Medicine_id = medicine.Medicine_id AND bill_transaction.Prescription_id = prescription.Prescription_id AND bill_transaction.Payment_type = payment_type.Payment_type;");
            return View(transactions);
        }

        [Authorize]
        [Obsolete]
        // To download receipt as PDF for user to print out
        public FileResult Export(int id)
        {
            var transactions = DBUtl.GetTable("SELECT Bill_transaction_id, bill_transaction.Prescription_id, bill_transaction.Queue_id, patient.Name, patient.Nric, Medicine_name, Dosage_quantity, Price, Dosage_quantity * Price AS Subtotal, payment_type.Payment_type_description, bill_transaction.payment_datetime FROM bill_transaction, medicine, prescription, patient, queue, payment_type WHERE queue.Serve_status_id = '5' AND queue.Patient_id = patient.Patient_id AND prescription.Patient_id = queue.Patient_id AND prescription.Medicine_id = medicine.Medicine_id AND bill_transaction.Prescription_id = prescription.Prescription_id AND bill_transaction.Payment_type = payment_type.Payment_type AND Bill_transaction_id = '{0}';", id);

            StringBuilder sb = new StringBuilder();
            sb.Append("<header class='clearfix'>");
            sb.Append("<h1>RECEIPT</h1>");
            sb.Append("<div id='company' class='clearfix'>");
            sb.Append("<div>Republic Polytechnic One-Stop Dispensing Software</div>");
            sb.Append("<div>9 Woodlands Ave 9,<br /> Singapore 738964</div>");
            sb.Append("</div><br><br>");
            sb.Append("<div id='project'>");
            sb.Append(String.Format("<div><span>TRANSACTION ID: </span> {0}</div>", transactions.Rows[0]["Bill_transaction_id"]));
            sb.Append(String.Format("<div><span>PATIENT NAME: </span> {0}, {1}</div>", transactions.Rows[0]["Name"], transactions.Rows[0]["Nric"]));
            sb.Append(String.Format("<div><span>QUEUE NO: </span> {0}</div>", transactions.Rows[0]["Queue_id"]));
            sb.Append(String.Format("<div><span>DATE AND TIME OF TRANSACTION: </span> {0}</div>", transactions.Rows[0]["Payment_datetime"]));
            sb.Append("</div><br><br>");
            sb.Append("</header>");
            sb.Append("<main>");
            sb.Append("<table>");
            sb.Append("<thead>");
            sb.Append("<tr>");
            sb.Append("<th>MEDICINE</th>");
            sb.Append("<th>QUANTITY</th>");
            sb.Append("<th>UNIT PRICE</th>");
            sb.Append("<th>SUBTOTAL</th>");
            sb.Append("<th>PAYMENT MODE</th>");
            sb.Append("</tr>");
            sb.Append("</thead>");
            sb.Append("<tbody>");
            sb.Append("<tr>");
            sb.Append(String.Format("<td>{0}</td>", transactions.Rows[0]["Medicine_name"]));
            sb.Append(String.Format("<td>{0}</td>", transactions.Rows[0]["Dosage_quantity"]));
            sb.Append(String.Format("<td>{0}</td>", transactions.Rows[0]["Price"]));
            sb.Append(String.Format("<td>{0}</td>", transactions.Rows[0]["Subtotal"]));
            sb.Append(String.Format("<td>{0}</td>", transactions.Rows[0]["Payment_type_description"].ToString().Substring(0, 1).ToUpper() + transactions.Rows[0]["Payment_type_description"].ToString().Substring(1)));
            sb.Append("</tr>");
            sb.Append("</tbody>");
            sb.Append("</table>");
            sb.Append("</main><br><br>");
            sb.Append("<footer>");
            sb.Append("Invoice was created on a computer and is valid without the signature and seal.");
            sb.Append("</footer>");

            StringReader sr = new StringReader(sb.ToString());
            Document PdfFile = new Document(PageSize.A5);
            HTMLWorker htmlparser = new HTMLWorker(PdfFile);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                PdfWriter writer = PdfWriter.GetInstance(PdfFile, memoryStream);
                PdfFile.Open();
                htmlparser.Parse(sr);
                PdfFile.Close();
                return File(memoryStream.ToArray(), "application/pdf", "Transaction.pdf");
            }
        }

        [Authorize]
        // To show the transaction (successful) before exporting for print
        public IActionResult TransactionBill(Bill_transaction id)
        {
            var select = DBUtl.GetTable("SELECT TOP 1 Bill_transaction_id, bill_transaction.Prescription_id, bill_transaction.Queue_id, patient.Name, patient.Nric, Medicine_name, Dosage_quantity, Price, Dosage_quantity * Price AS Subtotal, payment_type.Payment_type_description FROM bill_transaction, medicine, prescription, patient, queue, payment_type WHERE queue.Serve_status_id = '5' AND queue.Patient_id = patient.Patient_id AND prescription.Patient_id = queue.Patient_id AND prescription.Medicine_id = medicine.Medicine_id AND bill_transaction.Prescription_id = prescription.Prescription_id AND bill_transaction.Payment_type = payment_type.Payment_type AND Bill_transaction_id = '{0}';", id);

            if (select.Rows.Count == 1)
            {
                ViewData["Transaction"] = (int)select.Rows[0]["Bill_transaction_id"];
                ViewData["Prescription"] = (int)select.Rows[0]["Prescription_id"];
                ViewData["Queue"] = (int)select.Rows[0]["Queue_id"];
                ViewData["Date"] = DateTime.Now.ToString();
                ViewData["Name"] = select.Rows[0]["Name"].ToString();
                ViewData["NRIC"] = select.Rows[0]["Nric"].ToString();
                ViewData["Medicine"] = select.Rows[0]["Medicine_name"].ToString();
                ViewData["Qty"] = (int)select.Rows[0]["Dosage_quantity"];
                ViewData["UnitPrice"] = Convert.ToDouble(select.Rows[0]["Price"]);
                ViewData["Subtotal"] = Convert.ToDouble(select.Rows[0]["Subtotal"]);
                ViewData["Types"] = select.Rows[0]["Payment_type_description"].ToString();
                return View();
            }
            else
            {
                TempData["Message"] = "No transaction available to show";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Index");
            }
        }
    }
}