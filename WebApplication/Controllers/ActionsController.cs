using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebApplication.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication.Controllers
{
    public class ActionsController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Hospital");
        }

        public IActionResult CleanMainQueue()
        {
            string sql = @"
                            DELETE FROM queue;
INSERT INTO patient_archive  
    SELECT Patient_id, Queue_id, Name, Nric, Gender, Date_of_birth, Race, Height, Weight, Allergy, Smoke, Alcohol, Has_travel, Has_flu, Has_following_symptoms, Address, Postal_code, Phone_no, Email, Remarks, Registered_datetime, Is_Urgent 
    FROM patient; 
                            DELETE FROM patient;
                            DELETE FROM category1;
                            DELETE FROM category2;
                            DELETE FROM category3;
                            DELETE FROM category4;
                            DELETE FROM patientcheck;";
            if (DBUtl.ExecSQL(sql) == 1)
            {
                TempData["Message"] = "Reset Main Queue Successful";
                TempData["MsgType"] = "success";
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
            }
            return RedirectToAction("Index");
        }

        //public IActionResult CleanMissedQueue()
        //{
        //    return View();
        //}

        public IActionResult CleanMedicineBatch()
        {
            string sql = @"DELETE FROM medicine;";
            if (DBUtl.ExecSQL(sql) == 1)
            {
                TempData["Message"] = "Reset Medicine Inventory Successful";
                TempData["MsgType"] = "success";
            }
            else
            {
                TempData["Message"] = DBUtl.DB_Message;
                TempData["MsgType"] = "danger";
            }
            return RedirectToAction("Index");
        }

        public IActionResult CleanAll()
        {
            string sql = @"
INSERT INTO prescription_archive  
    SELECT Prescription_id, Patient_id, Medicine_id, Dosage_id, Doctor_mcr, Doctor_name, Practicing_place_name, Practicing_address, Booking_appointment, Case_notes, Duration, Dosage_quantity, Instructions, Total_price
    FROM prescription;  
                            DELETE FROM prescription;
                            DELETE FROM queue;
INSERT INTO patient_archive  
    SELECT Patient_id, Queue_id, Name, Nric, Gender, Date_of_birth, Race, Height, Weight, Allergy, Smoke, Alcohol, Has_travel, Has_flu, Has_following_symptoms, Address, Postal_code, Phone_no, Email, Remarks, Registered_datetime, Is_Urgent 
    FROM patient; 
                            DELETE FROM patient;
                            DELETE FROM category1;
                            DELETE FROM category2;
                            DELETE FROM category3;
                            DELETE FROM category4;
                            DELETE FROM checkflag;";
            if (DBUtl.ExecSQL(sql) == 1)
            {
                TempData["Message"] = "Reset All Successful";
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
}
