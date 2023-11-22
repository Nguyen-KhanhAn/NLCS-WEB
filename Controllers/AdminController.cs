using Demo_Hangfire.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.EnterpriseServices.CompensatingResourceManager;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebsiteBanHang.Models;

namespace Demo_Hangfire.Controllers
{
    public class AdminController : Controller
    {
        AnNguyenEntities db = new AnNguyenEntities();
        // GET: Admin
        public ActionResult Index()
        {
            int year = int.Parse(DateTime.Now.Year.ToString());
            // Data chart
            List<DataChart> data = db.Bills.Where(x => x.Created_at.Value.Year == year)
                .GroupBy(x => x.Created_at.Value.Month)
                .Select(x => new DataChart()
                {
                    Month = (x.FirstOrDefault().Created_at.Value.Month),
                    Total = x.ToList().Sum(y => y.BillDetails.Sum(b => b.Quantity * b.Price)).Value
                }).ToList();

            List<DataPoint> dataPoints = new List<DataPoint>();
            foreach (var item in data)
            {
                dataPoints.Add(new DataPoint("Tháng " + item.Month.ToString(), double.Parse(item.Total.ToString())));
            }

            ViewBag.DataPoints = JsonConvert.SerializeObject(dataPoints);
            return View();
        }

        public ActionResult ViewProduct()
        {
            List<Product> products = db.Products.Where(n => n.IsDeleted == false).ToList();
            return View(products);
        }
        public ActionResult AddNewProduct()
        {
           
            ViewBag.TypeProduct_id = new SelectList(db.TypeProducts, "ID", "Name");
            return View();
        }
        public ActionResult AddNewP(FormCollection f)
        {
            string Name = f["Name"];
            //string Image = f["Image"];
            //string Image1 = f["Image1"];
            //string Image2 = f["Image2"];
            int TypeProduct_id = Convert.ToInt32(f["TypeProduct_id"]);
            decimal Price = Decimal.Parse(f["Price"]);
            int Quantity = Int32.Parse(f["Quantity"]);
            string Description = f["Description"];
            Product pro = new Product();
            pro.Name = Name;
            pro.Remaining = Quantity;
            pro.IsDeleted = false;
            pro.Price = Price;
            pro.Description = Description;
            pro.Sales = 0;
            pro.Price_discount = 0;
            pro.TypeProduct_id = TypeProduct_id;
            db.Products.Add(pro);
          
            if (Request.Files.Count > 0)
            {
                try
                {

                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {

                            fname = file.FileName;



                        }

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Content/images/"), fname);
                        file.SaveAs(fname);
                    }
                    pro.Image1 = files[0].FileName;
                    pro.Image2 = files[1].FileName; 
                    pro.Image3 = files[2].FileName;
                    db.SaveChanges();
                    // Returns message that successfully uploaded  
                    return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);

                    //  Get all files from Request object  

                }

                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditP(FormCollection f, Product pros)
        {
            int ID = Int32.Parse(f["ID"]); 
            string Name = f["Name"];
            string Image = f["Image"];
            string Image1 = f["Image1"];
            string Image2 = f["Image2"];
            int TypeProduct_id = Convert.ToInt32(f["TypeProduct_id"]);
            int Sale = Convert.ToInt32(f["Sale"]);
            decimal Price = Decimal.Parse(f["Price"]);
            decimal Price_discount = Decimal.Parse(f["Price_discount"]);
            int Quantity = Int32.Parse(f["Quantity"]);
            string Description = f["Description"];
            Product pro = db.Products.SingleOrDefault(n=>n.ID == ID);
            if(pro == null)
            {
                return Json(new { mess = "fail" }, JsonRequestBehavior.AllowGet);
            }
            pro.Name = Name;
            
            pro.Price = Price;
            pro.Description = Description;
            pro.Sales = Sale;
            pro.Price_discount = Price_discount;
            pro.TypeProduct_id = TypeProduct_id;
            if (Request.Files.Count > 0)
            {
                try
                {

                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                           
                           
                            if(file.FileName != "")
                            {
                                fname = file.FileName;
                                // Get the complete folder path and store the file inside it.  
                                fname = Path.Combine(Server.MapPath("~/Content/images/"), fname);
                                file.SaveAs(fname);
                            }
                            


                        }

                       
                    }
                   
                    if (files[0].FileName != "")
                    {

                        pro.Image1 = files[0].FileName;
                    }
                    if (files[1].FileName != "")
                    {

                        pro.Image2 = files[1].FileName;
                    }
                    if (files[2].FileName != "")
                    {

                        pro.Image3 = files[2].FileName;
                    }
                    db.SaveChanges();
                    // Returns message that successfully uploaded  
                    return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);

                    //  Get all files from Request object  

                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }

            db.SaveChanges();
            return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditProduct(int ID)
        {
            ViewBag.TypeProduct_id = new SelectList(db.TypeProducts, "ID", "Name");
            Product pro = db.Products.SingleOrDefault(n => n.ID == ID);
            return View(pro);
        }
        public ActionResult DeleteProduct(int ID)
        {
            Product product = db.Products.SingleOrDefault(n => n.ID == ID);
            if (product == null)
            {
                return Json(new { mess = "fail" }, JsonRequestBehavior.AllowGet);
            }
            product.IsDeleted = true;
            db.SaveChanges();
            return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewTypeProduct()
        {
            List<TypeProduct> types = db.TypeProducts.Where(n => n.IsDeleted == false).ToList();
            return View(types);
        }

        public ActionResult AddNewTypeProduct()
        {
            return View();
        }
        public ActionResult EditTypeProduct(int ID)
        {
            TypeProduct type = db.TypeProducts.SingleOrDefault(n => n.ID == ID);
            return View(type);
        }
        public ActionResult EditTP(FormCollection f)
        {
            int ID = Int32.Parse(f["ID"]);
            string Name = f["Name"];
            string Image = f["Image"];
            TypeProduct type = db.TypeProducts.SingleOrDefault(n => n.ID == ID);
            if (type == null)
            {
                return Json(new { mess = "fail" }, JsonRequestBehavior.AllowGet);
            }
            type.Name = Name;
            if (Request.Files.Count > 0)
            {
                try
                {

                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                           
                            fname = file.FileName;
                           


                        }

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Content/images/"), fname);
                        file.SaveAs(fname);
                        type.Image = file.FileName;
                    }
                    db.SaveChanges();
                    // Returns message that successfully uploaded  
                    return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);

                    //  Get all files from Request object  

                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            
            return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AddNewTP(FormCollection f)
        {
            string Name = f["Name"];
            string Image = f["url"];
            TypeProduct newtype = new TypeProduct();
            newtype.Name = Name;
            newtype.IsDeleted = false;
            if (Request.Files.Count > 0)
            {
                try
                {

                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {

                            fname = file.FileName;



                        }

                        // Get the complete folder path and store the file inside it.  
                        fname = Path.Combine(Server.MapPath("~/Content/images/"), fname);
                        file.SaveAs(fname);
                        newtype.Image = file.FileName;
                    }
                    db.TypeProducts.Add(newtype);
                    db.SaveChanges();
                    // Returns message that successfully uploaded  
                    return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);

                    //  Get all files from Request object  

                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
            }
            ;
            
            
            return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteTypeProduct(int ID)
        {
            TypeProduct typeproduct = db.TypeProducts.SingleOrDefault(n => n.ID == ID);
            if (typeproduct == null)
            {
                return Json(new { mess = "fail" }, JsonRequestBehavior.AllowGet);
            }
            List<Product> listpro = db.Products.Where(p => p.TypeProduct_id == ID).ToList();
            foreach (var item in listpro)
            {
                item.IsDeleted = true;
            }
            typeproduct.IsDeleted = true;
            db.SaveChanges();
            return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewBill(DateTime? start, DateTime? end, int? Delivery)
        {
            ViewBag.Delivery = new SelectList(db.DeliveryStatus, "ID", "Name");
            List<Bill> bills = db.Bills.OrderByDescending(n => n.Created_at).ToList();
            if (Delivery != null && start != null && end != null)
            {
                List<Bill> bills1 = db.Bills.Where(n => n.Created_at >= start && n.Created_at <= end && n.DeliveryStatus_id == Delivery).OrderByDescending(n => n.Created_at).ToList();
                return View(bills1);
            }
            if (Delivery != null && start == null && end == null)
            {
                List<Bill> bills1 = db.Bills.Where(n =>  n.DeliveryStatus_id == Delivery).OrderByDescending(n => n.Created_at).ToList();
                return View(bills1);
            }
            
            if(Delivery == null && start != null && end != null)
            {
                List<Bill> bills1 = db.Bills.Where(n => n.Created_at >= start && n.Created_at <= end).OrderByDescending(n => n.Created_at).ToList();
                return View(bills1);

            }
            
            return View(bills);
        }

        public ActionResult AcceptBill(int ID)
        {
            Bill bill = db.Bills.SingleOrDefault(n=>n.ID == ID);
            if(bill == null)
            {
                return Json(new { mess = "fail" }, JsonRequestBehavior.AllowGet);
            }
            bill.DeliveryStatus_id = 2;
            db.SaveChanges();
            return RedirectToAction("ViewBill");
        }

        public ActionResult SearchBill(DateTime start, DateTime end, int status_id)
        {
            
            return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
        }

    }
}