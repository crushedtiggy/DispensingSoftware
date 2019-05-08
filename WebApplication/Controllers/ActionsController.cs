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
                            DELETE FROM patient;
                            DELETE FROM category1;
                            DELETE FROM category2;
                            DELETE FROM category3;
                            DELETE FROM category4;
                            DELETE FROM checkflag;";
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
                            DELETE FROM prescription;
                            DELETE FROM queue;
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
