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

        public IActionResult Logs()
        {
            List<Log> model = DBUtl.GetList<Log>(
                        "SELECT Log_id, Patient_id, Medicine_id, Dosage_id, Booking_appointment FROM log ORDER BY Log_id");
            return View(model);
        }

        public IActionResult AddPrescription()
        {   
            var patients = DBUtl.GetList<Patient>(
                                                 "SELECT * FROM patient ORDER BY Patient_id");
            ViewData["patient"] = new SelectList(patients, "Patient_id", "Name");
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

        public IActionResult AddLog()
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
                int patientId;
                List<Patient> patients = DBUtl.GetList<Patient>(
                                    "SELECT * FROM patient ORDER BY patient_id");
                if (patients.Count == 0)
                {
                    patientId = 1;
                }
                else
                {
                    patientId = patients.LastOrDefault().Patient_id + 1;
                }

                string sql =
//                       @"INSERT INTO testcheck (test_id, Name, Nric, Gender, Date_of_birth, Race, Height, Weight, Allergy, Smoke, Alcohol, Has_travel, Has_flu, Has_following_symptoms, Address, Postal_code, Phone_no, Email, Remarks, Registered_datetime, Is_Urgent)
//                                    VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', GETDATE(), '{19}')
//LOCK TABLES patient WRITE, testcheck WRITE;

@"INSERT INTO patient (Patient_id, Queue_id, Name, Nric, Gender, Date_of_birth, Race, Height, Weight, Allergy, Smoke, Alcohol, Has_travel, Has_flu, Has_following_symptoms, Address, Postal_code, Phone_no, Email, Remarks, Registered_datetime, Is_Urgent)
                                    VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}', '{17}', '{18}', '{19}', GETDATE(), '{20}')";
//DELETE from testcheck WHERE testcheck.Name = patient.Name;
//UNLOCK TABLES;";
                string queueSQL = @"INSERT INTO queue (Queue_id, Patient_id, Serve_status_id, Queue_category_id, Queue_datetime) VALUES ('{0}', '{1}', '{2}', '{3}', GETDATE())";
                string categorySQL = @"INSERT INTO category{0} (Queue_no, Created_by) VALUES ('{1}', '{2}')";


                if (DBUtl.ExecSQL(sql,
                                        patientId, queueNo, newPatient.Name, newPatient.Nric,
                                        newPatient.Gender, $"{newPatient.Date_of_birth:yyyy-MM-dd}", newPatient.Race,
                                        newPatient.Height, newPatient.Weight, newPatient.Allergy, newPatient.Smoke, newPatient.Alcohol, newPatient.Has_travel, newPatient.Has_flu, newPatient.Has_following_symptoms, newPatient.Address, newPatient.Postal_code, newPatient.Phone_no, newPatient.Email, newPatient.Remarks, newPatient.Is_Urgent) == 1 && DBUtl.ExecSQL(queueSQL, queueNo, patientId, 1, GetQueueCategoryId(newPatient)) == 1 && DBUtl.ExecSQL(categorySQL, GetQueueCategoryId(newPatient), queueNo, "admin") == 1)
                {
                    TempData["Message"] = "Patient Added";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("Patients");
            }
        }

        [HttpPost]
        public IActionResult AddPrescription(Prescription prescription, Patient patient)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("AddPrescription");
            }
            else
            {
                List<Prescription> preps = DBUtl.GetList<Prescription>(
                                     "SELECT * FROM prescription ORDER BY Prescription_id");

                string sql =
                       @"INSERT INTO prescription (Prescription_id, Patient_id, Doctor_mcr, Doctor_name, Practicing_place_name, Practicing_address)
                                    VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}')";

                if (DBUtl.ExecSQL(sql, preps.Count + 1, patient.Patient_id, prescription.Doctor_mcr, prescription.Doctor_name, prescription.Practicing_place_name, prescription.Practicing_address) == 1)
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
                    Patient_id = (int)dt.Rows[0]["Patient_id"],
                    Prescription_id = (int)dt.Rows[0]["Prescription_id"],
                    Doctor_mcr = dt.Rows[0]["Doctor_mcr"].ToString(),
                    Doctor_name = dt.Rows[0]["Doctor_name"].ToString(),
                    Practicing_place_name = dt.Rows[0]["Practicing_place_name"].ToString(),
                    Practicing_address = dt.Rows[0]["Practicing_address"].ToString()
                };

                return View(prep);
            }
            else
            {
                TempData["Message"] = "Prescription Not Found";
                TempData["MsgType"] = "warning";
                return RedirectToAction("Prescriptions");
            }
        }

        [HttpPost]
        public IActionResult AddLog(Patient patient, Medicine medicine, Dosage dosage, Log log)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Message"] = "Invalid Input";
                ViewData["MsgType"] = "warning";
                return View("AddLog");
            }
            else
            {
                List<Log> logs = DBUtl.GetList<Log>(
                                     "SELECT * FROM log ORDER BY Log_id");

                double totalPrice = medicine.Price * log.Dosage_quantity;

                string update = @"UPDATE medicine SET Quantity = Quantity - {0} WHERE Medicine_id = {1};";
                string sql =
                       @"INSERT INTO log (Log_id, Patient_id, Medicine_id, Dosage_id, Booking_appointment, Case_notes, Duration, Dosage_quantity, Instructions, Total_price)
                                    VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}')";

                if (DBUtl.ExecSQL(sql, logs.Count + 1, patient.Patient_id, medicine.Medicine_id, dosage.Dosage_id, $"{log.Booking_apppointment:yyyy-MM-dd HH:mm}", log.Case_notes, log.Duration, log.Dosage_quantity, log.Instructions, totalPrice) == 1 && DBUtl.ExecSQL(update, log.Dosage_quantity, log.Medicine_id) == 1)
                {
                    TempData["Message"] = "Log Added";
                    TempData["MsgType"] = "success";
                }
                else
                {
                    TempData["Message"] = DBUtl.DB_Message;
                    TempData["MsgType"] = "danger";
                }
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult DoPayment(Bill_transaction bill, Prescription prescription, Queue queue, Log log, Payment_Type type)
        {
            return View();
        }

        public int GetQueueCategoryId(Patient patient)
        {
            int queueid = 0;
            int age = DateTime.Today.Year - DateTime.Parse(patient.Date_of_birth).Year;

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

            //if (queueid == 1)
            //{
            //    if (DBUtl.GetTable(sql, "category1", category1s.LastOrDefault().Queue_no).Rows.Count == 1)
            //    {
            //        queueNo = category1s.LastOrDefault().Queue_no + 1;
            //    }
            //    else 
            //    {
            //        queueNo = category1s.LastOrDefault().Queue_no + 1000;
            //    }
            //}
            //else if (queueid == 2)
            //{
            //    if (DBUtl.GetTable(sql, "category2", category2s.LastOrDefault().Queue_no).Rows.Count == 1)
            //    {
            //        queueNo = category2s.LastOrDefault().Queue_no + 1;
            //    }
            //    else 
            //    {
            //        queueNo = category2s.LastOrDefault().Queue_no + 2000;
            //    }
            //}
            //else if (queueid == 3)
            //{
            //    if (DBUtl.GetTable(sql, "category3", category3s.LastOrDefault().Queue_no).Rows.Count == 1)
            //    {
            //        queueNo = category3s.LastOrDefault().Queue_no + 1;
            //    }
            //    else 
            //    {
            //        queueNo = category3s.LastOrDefault().Queue_no + 3000;
            //    }
            //}
            //else
            //{
            //    if (DBUtl.GetTable(sql, "category4", category4s.LastOrDefault().Queue_no).Rows.Count == 1)
            //    {
            //        queueNo = category4s.LastOrDefault().Queue_no + 1;
            //    }
            //    else 
            //    {
            //        queueNo = category4s.LastOrDefault().Queue_no + 4000;
            //    }
            //}
            return queueNo;
        }
    }
 }