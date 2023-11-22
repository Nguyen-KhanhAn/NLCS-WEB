using Demo_Hangfire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebsiteBanHang.Models;

namespace Demo_Hangfire.Controllers
{

    public class ProductController : Controller
    {
        AnNguyenEntities db = new AnNguyenEntities();
        // GET: Product
        public ActionResult Index()
        {
            List<Product> listPro = db.Products.Where(n => n.IsDeleted == false).ToList();
            return View(listPro);
        }
        public ActionResult DetailProduct(int id)
        {
            Product product = db.Products.SingleOrDefault(n=>n.ID == id);
            return View(product);
        }
        public ActionResult CheckOut()
        {
            List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];
            ViewBag.Total = TongTien();
            return View(listGioHang);
        }

        public List<GioHang> SessionGioHang()
        {
            List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];
            if (listGioHang == null)
            {
                listGioHang = new List<GioHang>();
                Session["GioHang"] = listGioHang;
            }
            return listGioHang;
        }
        public ActionResult ThemGioHang(int? MaSP)
        {
            Product sp = db.Products.SingleOrDefault(n => n.ID == MaSP);
            if (sp == null)
            {
                Response.StatusCode = 404;
                return null;
            }
            else
            {
                //List<GioHang> listGioHang = SessionGioHang();
                List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];

                if (listGioHang != null)
                {
                    // check sản phẩm đã tồn tại hay chưa?
                    GioHang gh = listGioHang.SingleOrDefault(n => n.ID == MaSP);
                    //TH1 sản phẩm đã tồn tại 
                    if (gh != null)
                    {
                        gh.Quantity++;
                        gh.Total = gh.Quantity * gh.Price;
                        //return Redirect(strURL);
                        //return PartialView("GioHangPartial");
                        return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        GioHang newgh = new GioHang((int)MaSP);
                        if (newgh.Quantity < sp.Remaining)
                        {
                            //Sau khi tạo xong thì add item vào listGioHang đã tạo truo72c đó
                            listGioHang.Add(newgh);
                            Session["GioHang"] = listGioHang;
                            //return PartialView("GioHangPartial");
                            return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                        }
                    }

                }
                else
                {
                    listGioHang = new List<GioHang>();
                    GioHang newgh = new GioHang((int)MaSP);
                    if (newgh.Quantity < sp.Remaining)
                    {
                        //Sau khi tạo xong thì add item vào listGioHang đã tạo truo72c đó
                        listGioHang.Add(newgh);
                        Session["GioHang"] = listGioHang;
                        //return PartialView("GioHangPartial");
                        return Json(new { mess = "success" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

                    }
                }

            }


        }
        public int TongSL()
        {
            List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];
            if (listGioHang == null)
            {
                return 0;
            }
            int sl = listGioHang.Sum(n => n.Quantity);
            return sl;

        }

        public decimal TongTien()
        {
            List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];
            if (listGioHang == null)
            {
                return 0;
            }
            decimal TongDonGia = listGioHang.Sum(n => n.Total);
            return TongDonGia;

        }
        public ActionResult XoaGioHang(int MaSP)
        {

            // Kiem tra xem co ton tai list gio hang chua
            List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];
            // neu gio hang trong thi thoi
            if (listGioHang == null)
            {
                return null;
            }
            //nguoc lai, neu ton tai gio hang thi kiem tra xem mat hang muon xoa có nam trong gio hang khong
            GioHang gh = listGioHang.SingleOrDefault(n => n.ID == MaSP);
            //neu san pham khong ton tai trong gio hang
            if (gh == null)
            {
                return null;
            }
            //nguoc lai, san pham ton tai trong gio hang, thi remove no khoi list san pham trong gio
            listGioHang.Remove(gh);
            ViewBag.listGioHang = listGioHang;
            ViewBag.TongSoLuong = TongSL();
            ViewBag.TongTien = TongTien();
            return RedirectToAction("CheckOut");
        }
        public ActionResult ThemSl(int MaSP)
        {
            int id = MaSP;
            List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];
            if (listGioHang == null)
            {
                return null;
            }
            else
            {
                GioHang sp = listGioHang.SingleOrDefault(n => n.ID == MaSP);
                if (sp != null)
                {
                    Product check = db.Products.SingleOrDefault(n => n.ID == MaSP);
                    if (sp.Quantity < check.Remaining)
                    {
                        sp.Quantity++;
                        sp.Total = sp.Quantity * sp.Price;
                    }
                    else
                    {
                        return null;
                    }
                }

                return RedirectToAction("CheckOut");
            }
        }

        public ActionResult GiamSL(int MaSP)
        {

            List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];
            if (listGioHang == null)
            {
                return null;
            }
            else
            {
                GioHang sp = listGioHang.SingleOrDefault(n => n.ID == MaSP);
                if (sp != null)
                {

                    if (sp.Quantity == 1)
                    {
                        listGioHang.Remove(sp);
                        return RedirectToAction("CheckOut");
                    }
                    else
                    {
                        sp.Quantity--;
                        sp.Total = sp.Quantity * sp.Price;
                    }
                }
                return RedirectToAction("CheckOut");
            }
        }



    }

}