using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplication.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication.Controllers
{
    public class HospitalController : Controller
    {
        // GET: /<controller>/

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Patients()
        {
            List<Patient> model = DBUtl.GetList<Patient>(
                                                 "SELECT * FROM patient ORDER BY Registered_datetime");
            return View(model);
        }

        public IActionResult Medicines()
        {
            List<Medicine> model = DBUtl.GetList<Medicine>(
                                    "SELECT Medicine_id, Category_type, Subcategory_type, Medicine_name, Brand, Quantity, Unit, Threshold FROM medicine ORDER BY Medicine_id, Category_type, Subcategory_type");
            return View(model);
        }

        public IActionResult Categories()
        {
            List<Category> model = DBUtl.GetList<Category>(
                        "SELECT * FROM category ORDER BY Category_type");
            return View(model);
        }

        public IActionResult Subcategories()
        {
            List<Subcategory> model = DBUtl.GetList<Subcategory>(
                                    "SELECT * FROM subcategory ORDER BY Category_type, Subcategory_type");
            return View(model);
        }

        public IActionResult AddPrescription()
        {
            var patients = DBUtl.GetList<Patient>(
                                     "SELECT * FROM patient ORDER BY Patient_id");
            ViewData["patient"] = new SelectList(patients, "Patient_id", "Name");

            var medicines = DBUtl.GetList<Medicine>(
                                     "SELECT * FROM medicine ORDER BY Medicine_id");
            ViewData["medicine"] = new SelectList(medicines, "Medicine_id", "Medicine_name");

            var dosages = DBUtl.GetList<Dosage>(
                         "SELECT * FROM dosage ORDER BY Dosage_id");
            ViewData["dosage"] = new SelectList(dosages, "Dosage_id", "Long_form_description");
            return View();
        }


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

        public IActionResult DoPayment()
        {
            return View();
        }

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
                List<Patient> patients = DBUtl.GetList<Patient>(
                                    "SELECT * FROM patient ORDER BY patient_id");
                List<PatientCheck> checkPatients = DBUtl.GetList<PatientCheck>("SELECT * FROM patientcheck ORDER BY Check_id");
                int check;
                int patientid;
                if (checkPatients.Count == 0)
                {
                    check = 1;
                }
                else
                {
                    check = checkPatients.Count + 1;
                }
                if (patients.Count == 0)
                {
                    patientid = 1;
                }
                else
                {
                    patientid = patients.Count + 1;
                }

                string sql = @"INSERT INTO patientcheck (Check_id, Name, Nric, Gender, Date_of_birth, Race, Height, Weight, Allergy, Smoke, Alcohol, Has_travel, Has_flu, Has_following_symptoms, Address, Postal_code, Phone_no, Email, Remarks, Registered_datetime, Is_Urgent) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', GETDATE(), '{19}');

INSERT INTO patient WITH (TABLOCKX) (Patient_id, Queue_id, Name, Nric, Gender, Date_of_birth, Race, Height, Weight, Allergy, Smoke, Alcohol, Has_travel, Has_flu, Has_following_symptoms, Address, Postal_code, Phone_no, Email, Remarks, Registered_datetime, Is_Urgent)
                                    VALUES ('{20}', '{21}', '{22}', '{23}', '{24}', '{25}', '{26}', '{27}', '{28}', '{29}', '{30}', '{31}', '{32}', '{33}', '{34}', '{35}', '{36}', '{37}', '{38}', '{39}', GETDATE(), '{40}');

INSERT INTO queue WITH (TABLOCKX) (Queue_id, Patient_id, Serve_status_id, Queue_category_id, Queue_datetime) VALUES ('{41}', '{42}', '{43}', '{44}', GETDATE());

INSERT INTO category{45} WITH (TABLOCKX) (Queue_no, Created_by) VALUES ('{46}', '{47}');

DELETE from patientcheck WITH (TABLOCKX) WHERE Check_id = MIN(Check_id);

COMMIT;
";

                if (DBUtl.ExecSQL(sql, check, newPatient.Name, newPatient.Nric,
                                                       newPatient.Gender, $"{newPatient.Date_of_birth:yyyy-MM-dd}", newPatient.Race,
                                                       newPatient.Height, newPatient.Weight, newPatient.Allergy, newPatient.Smoke, newPatient.Alcohol, newPatient.Has_travel, newPatient.Has_flu, newPatient.Has_following_symptoms, newPatient.Address, newPatient.Postal_code, newPatient.Phone_no, newPatient.Email, newPatient.Remarks, newPatient.Is_Urgent, patientid, queueNo, newPatient.Name, newPatient.Nric,
                                                       newPatient.Gender, $"{newPatient.Date_of_birth:yyyy-MM-dd}", newPatient.Race,
                                                       newPatient.Height, newPatient.Weight, newPatient.Allergy, newPatient.Smoke, newPatient.Alcohol, newPatient.Has_travel, newPatient.Has_flu, newPatient.Has_following_symptoms, newPatient.Address, newPatient.Postal_code, newPatient.Phone_no, newPatient.Email, newPatient.Remarks, newPatient.Is_Urgent, queueNo, patientid, 1, GetQueueCategoryId(newPatient), GetQueueCategoryId(newPatient), queueNo, "admin") == 1)
                {
                    TempData["Message"] = "Patient Added";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                ViewData["QueueNumber"] = queueNo;
                return View("ShowQueueNumber");
            }
        }

        [HttpPost]
        public IActionResult AddPrescription(Prescription prescription)
        {
            List<Prescription> preps = DBUtl.GetList<Prescription>(
                                    "SELECT * FROM prescription ORDER BY Prescription_id");
            List<Medicine> medicines = DBUtl.GetList<Medicine>("SELECT Price FROM medicine ORDER BY Medicine_id");
            List<Patient> patients = DBUtl.GetList<Patient>("SELECT Patient_id, Queue_id FROM patient ORDER BY Patient_id");

            int count;
            double price = 0;
            int queueNo = 0;

            foreach (Medicine i in medicines)
            {
                if (i.Medicine_id == prescription.Medicine_id)
                {
                    price = i.Price;
                }
            }
            double total = prescription.Dosage_quantity * price;

            foreach (Patient i in patients)
            {
                if (i.Patient_id == prescription.Patient_id)
                {
                    queueNo = i.Queue_id;
                }
            }

            if (preps.Count == 0)
            {
                count = 1;
            }
            else
            {
                count = preps.LastOrDefault().Prescription_id + 1;
            }

            if (!ModelState.IsValid)
            {
                ViewData["Message"] = DBUtl.DB_Message;
                ViewData["MsgType"] = "warning";
                return View("AddPrescription");
            }
            else
            {


                string sql =
                       @"INSERT INTO prescription WITH (TABLOCKX)  (Prescription_id, Patient_id, Medicine_id, Dosage_id, Doctor_mcr, Doctor_name, Practicing_place_name, Practicing_address, Booking_appointment, Case_notes, Duration, Dosage_quantity, Instructions, Total_price)
                                    VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}');
COMMIT;";
                string update = @"UPDATE queue WITH (TABLOCKX) SET Serve_status_id = '2' WHERE Patient_id = '{0}' AND Queue_id = '{1}'
COMMIT;";

                if (DBUtl.ExecSQL(sql, count, prescription.Patient_id, prescription.Medicine_id, prescription.Dosage_id, prescription.Doctor_mcr, prescription.Doctor_name, prescription.Practicing_place_name, prescription.Practicing_address, $"{prescription.Booking_appointment:yyyy-MM-dd HH:mm}", prescription.Case_notes, prescription.Duration, prescription.Dosage_quantity, prescription.Instructions, total) == 1 && DBUtl.ExecSQL(update, prescription.Patient_id, queueNo) == 1)
                {
                    TempData["Message"] = "Prescription Added";
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

        // Show queue number after patient is registered
        public IActionResult ShowQueueNumber()
        {
            return View();
        }

        // Display queue number based on priority handling
        public IActionResult GetQueueNumber()
        {
            List<Queue> queues = DBUtl.GetList<Queue>(
                                    "SELECT Queue_id FROM queue WHERE DATEDIFF(MINUTE, Queue_datetime, GETDATE()) > 60 AND Serve_status_id = '1' AND Queue_category_id = (SELECT MIN(Queue_category_id) FROM queue) ORDER BY Queue_category_id, Queue_datetime");
            if (queues.Count == 0)
            {
                TempData["Message"] = "No queue number that exceeds 1 hour of waiting / No queue number available";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Prescriptions");
            }
            ViewData["QueueNumber"] = queues.FirstOrDefault().Queue_id;
            return View();
        }


        public IActionResult DoPayment(String id)
        {
            string sql = "SELECT Prescription_id FROM bill_transaction WHERE Prescription_id={0}";
            string select = String.Format(sql, id);
            DataTable dt = DBUtl.GetTable(select);
            if (dt.Rows.Count == 1)
            {
                Bill_transaction bill_Transaction = new Bill_transaction
                {
                    Bill_transaction_id = (int)dt.Rows[0]["Bill_transaction_id"],
                    Prescription_id = (int)dt.Rows[0]["Prescription_id"],
                    Queue_id = (int)dt.Rows[0]["Queue_id"],
                    Payment_type = (int)dt.Rows[0]["Payment_type"],
                    Subtotal = (Double)dt.Rows[0]["Subtotal"],
                    Payment_datetime = (DateTime)dt.Rows[0]["Payment_datetime"]
                };
                var types = DBUtl.GetList<Payment_Type>(
                                     "SELECT * FROM payment_type ORDER BY Payment_type");
                ViewData["type"] = new SelectList(types, "Payment_type", "Payment_type_description");

                return View(bill_Transaction);
            }
            else
            {
                TempData["Message"] = "No prescription ID found to generate a bill transaction";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Prescriptions");
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

        [HttpPost]
        public IActionResult ModifyCaseNotes()
        {
            string id = HttpContext.Request.Form["prescriptionid"].ToString();
            string notes = HttpContext.Request.Form["text"].ToString().Trim();
            string update = @"UPDATE prescription SET Case_notes = '{0}' WHERE Prescription_id = '{1}'";
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

                string update = @"UPDATE queue WITH (TABLOCKX) SET Medicine_id = '{0}', Dosage_id = '{1}', Doctor_mcr = '{2}', Doctor_name '{3}', Practicing_place_name = '{4}', Practicing_address '{5}', Booking_appointment = '{6}', Duration = '{7}', Dosage_quantity = '{8}', Instructions = '{9}' WHERE Prescription_id = '{10}' AND Patient_id = '{11}'
COMMIT;";

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

        public IActionResult GetTransaction()
        {
            return View();
        }
    }
}