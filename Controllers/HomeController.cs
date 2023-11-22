using Demo_Hangfire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Demo_Hangfire.Controllers
{
    public class HomeController : Controller
    {
        AnNguyenEntities db = new AnNguyenEntities();
        public ActionResult Index()
        {
            List<TypeProduct> list = db.TypeProducts.ToList();
            ViewBag.listNam = db.Products.Where(n=>n.TypeProduct_id == 1 && n.IsDeleted == false).OrderByDescending(n => n.Sales).ToList();
            ViewBag.listNu = db.Products.Where(n => n.TypeProduct_id == 2 && n.IsDeleted == false).OrderByDescending(p => p.Sales).ToList();
            ViewBag.listBabi = db.Products.Where(n => n.TypeProduct_id == 3 && n.IsDeleted == false).OrderByDescending(p => p.Sales).ToList();
            return View(list);
        }

        public ActionResult LoginPartial()
        {
            ViewBag.check = Session["TaiKhoanDK"];
            User user = Session["User"] as User;
            ViewBag.Province_id = new SelectList(db.Provinces, "ID", "Name");
            return PartialView(user);
        }
        public ActionResult AllTypePro()
        {
            List<TypeProduct> list = db.TypeProducts.Where(n=>n.IsDeleted == false).ToList();
            return PartialView(list);
        }
        public ActionResult GetDistricts(int Province_id)
        {
            List<District> Districts = db.Districts.Where(n => n.Province_id == Province_id).ToList();
            ViewBag.District_id = new SelectList(Districts, "ID", "Name");
            return PartialView("GetDistricts");
        }

       
        public ActionResult GetTowns(int District_id)
        {
            List<Town> Towns = db.Towns.Where(n => n.District_id == District_id).ToList();
            ViewBag.Town_id = new SelectList(Towns, "ID", "Name");
            return PartialView("GetTowns");
        }

        [HttpPost]
        public ActionResult Login(FormCollection f) {
            string email = f["Email"];
            string password = f["Password"];
            User user = db.Users.SingleOrDefault(n=>n.Email == email && n.Password == password && n.IsChecked == true);
            if(user == null)
            {
                Session["User"] = null;
                return Json(new { mess = "fail" }, JsonRequestBehavior.AllowGet);
            }
            Session["User"] = user;
            Session["idKH"] = user.ID;
            if (user.TypeUser_id == 1)
            {
                return Json(new { mess = "success", TypeUser_id = 1 }, JsonRequestBehavior.AllowGet);
            }
            //return RedirectToAction("Index","Admin");
            return Json(new { mess = "success", TypeUser_id = 2 }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Logout()
        {
            Session["User"] = null;
            return RedirectToAction("Index");
        }

        [HttpPost]

        public ActionResult Register(User tv)
        {
            Random random = new Random();
            User check = db.Users.SingleOrDefault(n => n.Email == tv.Email);
                if (check == null)
                {
                    //Random random = new Random();

                    tv.TypeUser_id = 1;
                    tv.IsChecked = false;

                    tv.Captcha = random.Next(100000, 999999);
                    db.Users.Add(tv);

                    db.SaveChanges();
                    Session["TaiKhoanDK"] = tv.Email;
                    
                    ViewBag.ThongBao = "Thêm thành công";
                try
                {
                    if (ModelState.IsValid)
                    {
                        var senderEmail = new MailAddress("anb2014549@student.ctu.edu.vn", "KhanhAn");
                        var receiverEmail = new MailAddress(tv.Email, "Receiver");
                        var password = "cvpa mutj rrwa cgqx";
                        var sub = "Mã xác nhận";
                        string body = tv.Captcha.ToString();
                        var smtp = new SmtpClient
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(senderEmail.Address, password)
                        };
                        using (var mess = new MailMessage(senderEmail, receiverEmail)
                        {
                            Subject = sub,
                            Body = body 
                        })
                        {
                            smtp.Send(mess);
                        }
                      
                    }
                }
                catch (Exception)
                {
                    ViewBag.Error = "Some Error";
                }
                return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.ThongBao = "Tài khoản bị trùng lặp";
                }

            
           


            return View();
        }

        
        public ActionResult Confirm(FormCollection f)
        {
            int captcha = Int32.Parse(f["Captcha"]);
            string TK = Session["TaiKhoanDK"] as string;
            User check = db.Users.SingleOrDefault(n=>n.Email == TK && n.Captcha == captcha);
            if(check == null)
            {
                return Json(new { mess = "fail" }, JsonRequestBehavior.AllowGet);
            }
            check.IsChecked = true;
            
            db.SaveChanges();
            Session["TaiKhoanDK"] = "";
            return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TrackOrder(DateTime? start, DateTime? end, int? Delivery)
        {
            User user = Session["User"] as User;
            ViewBag.Delivery = new SelectList(db.DeliveryStatus, "ID", "Name");
            List<Bill> bills = db.Bills.Where(n=>n.User_id == user.ID).OrderByDescending(n => n.Created_at).ToList();
            if (Delivery != null && start != null && end != null)
            {
                List<Bill> bills1 = db.Bills.Where(n => n.Created_at >= start && n.Created_at <= end && n.DeliveryStatus_id == Delivery && n.User_id == user.ID).OrderByDescending(n => n.Created_at).ToList();
                return View(bills1);
            }
            if (Delivery != null && start == null && end == null)
            {
                List<Bill> bills1 = db.Bills.Where(n => n.DeliveryStatus_id == Delivery && n.User_id == user.ID).OrderByDescending(n => n.Created_at).ToList();
                return View(bills1);
            }

            if (Delivery == null && start != null && end != null)
            {
                List<Bill> bills1 = db.Bills.Where(n => n.Created_at >= start && n.Created_at <= end && n.User_id == user.ID).OrderByDescending(n => n.Created_at).ToList();
                return View(bills1);

            }

            return View(bills);
            
        }
        public ActionResult RecivedBill(int ID)
        {
            Bill bill = db.Bills.SingleOrDefault(n => n.ID == ID);
            if (bill == null)
            {
                return Json(new { mess = "fail" }, JsonRequestBehavior.AllowGet);
            }
            bill.DeliveryStatus_id = 3;
            db.SaveChanges();
            return RedirectToAction("TrackOrder");
        }
        [HttpPost]
        public ActionResult SearchProduct(string name)
        {
            List<Product> listSearch = db.Products.Where(s => s.Name.Contains(name) && s.IsDeleted == false).ToList();
            return View(listSearch);
        }
    }
}