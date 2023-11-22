using Demo_Hangfire.Models;
using DemoVNPay.Others;
using MoMo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using WebsiteBanHang.Models;

namespace Demo_Hangfire.Controllers
{
    public class AllowCrossSiteJsonAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.RequestContext.HttpContext.Response.AddHeader("Access-Control-Allow-Origin", "*");
            base.OnActionExecuting(filterContext);
        }
    }
    public class OrderController : Controller
    {
        AnNguyenEntities db = new AnNguyenEntities();
        // GET: Order
        [HttpGet]
        [AllowCrossSiteJson]
        public ActionResult DatHang(int id)
        {
           
            List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];
           
            int iduser = (int)Session["idKH"];
            if (listGioHang == null)
            {
                return null;
            }
            else
            {
                Bill ddh = new Bill();
                string ngay = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                ddh.Created_at = DateTime.Parse(ngay);
                ddh.DeliveryStatus_id = 1;
                if (id == 1)
                {
                    ddh.IDDelivery = DateTime.Now.Ticks.ToString();
                    ddh.TypePayment = "COD";
                  
                    ddh.User_id = iduser;
                    ddh.TotalBill = listGioHang.Sum(n => n.Total); ;
                    ddh.TotalQuantity = listGioHang.Sum(n => n.Quantity);
              
                    db.Bills.Add(ddh);



                    for (var i = 0; i < listGioHang.Count(); i++)
                    {
                        BillDetail ct = new BillDetail();

                        ct.Bill_id = ddh.ID;

                        ct.Product_id = listGioHang[i].ID;

                        ct.Quantity = listGioHang[i].Quantity;
                        ct.Price = listGioHang[i].Price;
                        db.BillDetails.Add(ct);

                        if (ct != null)
                        {
                            Product spsl = db.Products.SingleOrDefault(n => n.ID == ct.Product_id);
                            spsl.Remaining--;
                            spsl.Sales++;
                        }



                    }

                    db.SaveChanges();
                    Session["GioHang"] = null;
                    return Json(new { Url = "" }, JsonRequestBehavior.AllowGet);
                }
                if (id == 2)
                {

                    if (listGioHang != null)
                    {
                        decimal total = 0;
                        for (int i = 0; i < listGioHang.Count(); i++)
                        {
                            decimal price = listGioHang[i].Total;
                            total = total + price;
                        }
                        string tongiten = total.ToString();


                        //ddh.TypePayment = "MoMo";
                       

                        //request params need to request to MoMo system
                        string endpoint = "https://test-payment.momo.vn/gw_payment/transactionProcessor";
                        string partnerCode = "MOMONJLR20220909";
                        string accessKey = "hWtILE8L8yb1vzVz";
                        string serectkey = "ktQfGrAtjnGWlAUo6Ea2SP7fVhBbzrhK";
                        string orderInfo = "An Store";
                        string returnUrl = "https://localhost:44389/Product/ReturnUrl";
                        string notifyurl = "https://4c8d-2001-ee0-5045-50-58c1-b2ec-3123-740d.ap.ngrok.io/Home/SavePayment"; //lưu ý: notifyurl không được sử dụng localhost, có thể sử dụng ngrok để public localhost trong quá trình test

                        string amount = tongiten;
                        string orderid = DateTime.Now.Ticks.ToString(); //mã đơn hàng
                        string requestId = DateTime.Now.Ticks.ToString();
                        string extraData = "";
                        Session["orderid"] = orderid;

                        //Before sign HMAC SHA256 signature
                        string rawHash = "partnerCode=" +
                            partnerCode + "&accessKey=" +
                            accessKey + "&requestId=" +
                            requestId + "&amount=" +
                            amount + "&orderId=" +
                            orderid + "&orderInfo=" +
                            orderInfo + "&returnUrl=" +
                            returnUrl + "&notifyUrl=" +
                            notifyurl + "&extraData=" +
                            extraData;

                        MoMoSecurity crypto = new MoMoSecurity();
                        //sign signature SHA256
                        string signature = crypto.signSHA256(rawHash, serectkey);

                        //build body json request
                        JObject message = new JObject
            {
                { "partnerCode", partnerCode },
                { "accessKey", accessKey },
                { "requestId", requestId },
                { "amount", amount },
                { "orderId", orderid },
                { "orderInfo", orderInfo },
                { "returnUrl", returnUrl },
                { "notifyUrl", notifyurl },
                { "extraData", extraData },
                { "requestType", "captureMoMoWallet" },
                { "signature", signature }

            };
                    

                        string responseFromMomo = PaymentRequest.sendPaymentRequest(endpoint, message.ToString());

                        JObject jmessage = JObject.Parse(responseFromMomo);
                        var url = jmessage.GetValue("payUrl").ToString();
                     
                        return Json(new { Url = url }, JsonRequestBehavior.AllowGet);
                    }
                }


                if (id == 3)
                {

                    if (listGioHang != null)
                    {
                        decimal total = 0;
                        for (int i = 0; i < listGioHang.Count(); i++)
                        {
                            decimal price = listGioHang[i].Total;
                            total = (total + price) * 100;
                        }
                        string tongiten = total.ToString();
                        //ddh.TypePayment = "VNPay";
                     
                        string url = ConfigurationManager.AppSettings["Url"];
                        string returnUrl = "https://localhost:44389/Order/PaymentConfirm";
                        //string returnUrl = ConfigurationManager.AppSettings["ReturnUrl"];
                        string tmnCode = ConfigurationManager.AppSettings["TmnCode"];
                        string hashSecret = ConfigurationManager.AppSettings["HashSecret"];



                        PayLib pay = new PayLib();

                        pay.AddRequestData("vnp_Version", "2.1.0"); //Phiên bản api mà merchant kết nối. Phiên bản hiện tại là 2.1.0
                        pay.AddRequestData("vnp_Command", "pay"); //Mã API sử dụng, mã cho giao dịch thanh toán là 'pay'
                        pay.AddRequestData("vnp_TmnCode", tmnCode); //Mã website của merchant trên hệ thống của VNPAY (khi đăng ký tài khoản sẽ có trong mail VNPAY gửi về)
                        pay.AddRequestData("vnp_Amount", tongiten); //số tiền cần thanh toán, công thức: số tiền * 100 - ví dụ 10.000 (mười nghìn đồng) --> 1000000
                        pay.AddRequestData("vnp_BankCode", ""); //Mã Ngân hàng thanh toán (tham khảo: https://sandbox.vnpayment.vn/apis/danh-sach-ngan-hang/), có thể để trống, người dùng có thể chọn trên cổng thanh toán VNPAY
                        pay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss")); //ngày thanh toán theo định dạng yyyyMMddHHmmss
                        pay.AddRequestData("vnp_CurrCode", "VND"); //Đơn vị tiền tệ sử dụng thanh toán. Hiện tại chỉ hỗ trợ VND
                        pay.AddRequestData("vnp_IpAddr", Util.GetIpAddress()); //Địa chỉ IP của khách hàng thực hiện giao dịch
                        pay.AddRequestData("vnp_Locale", "vn"); //Ngôn ngữ giao diện hiển thị - Tiếng Việt (vn), Tiếng Anh (en)
                        pay.AddRequestData("vnp_OrderInfo", "Thanh toan don hang"); //Thông tin mô tả nội dung thanh toán
                        pay.AddRequestData("vnp_OrderType", "other"); //topup: Nạp tiền điện thoại - billpayment: Thanh toán hóa đơn - fashion: Thời trang - other: Thanh toán trực tuyến
                        pay.AddRequestData("vnp_ReturnUrl", returnUrl); //URL thông báo kết quả giao dịch khi Khách hàng kết thúc thanh toán
                        pay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString()); //mã hóa đơn
                        
                        string paymentUrl = pay.CreateRequestUrl(url, hashSecret);

                        //return Redirect(paymentUrl);
                        return Json(new { Url = paymentUrl }, JsonRequestBehavior.AllowGet);
                    }
                }





            }

            return RedirectToAction("CheckOut", "Product");
        }
        public ActionResult ReturnUrl()
        {
            string param = Request.QueryString.ToString().Substring(0, Request.QueryString.ToString().IndexOf("signature") - 1);
            param = Server.UrlDecode(param);
            MoMoSecurity crypto = new MoMoSecurity();
            string serectkey = "ktQfGrAtjnGWlAUo6Ea2SP7fVhBbzrhK";
            //string serectKey = ConfigurationManager.AppSettings["serectKey"].ToString();
            string signature = crypto.signSHA256(param, serectkey);
            if (signature != Request["signature"].ToString())
            {
                ViewBag.message = "Thông tin request không hợp lệ";
                ViewBag.check = "fail";
            }
            else if (!Request.QueryString["errorCode"].Equals("0"))
            {
                ViewBag.message = "Thanh toán thất bại";
                ViewBag.check = "fail";
            }
            else
            {
                List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];

                int iduser = (int)Session["idKH"];
                if (listGioHang == null)
                {
                    return null;
                }
                else
                {
                    Bill ddh = new Bill();
                    ddh.IDDelivery = Session["orderid"].ToString(); ;
                    string ngay = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    ddh.Created_at = DateTime.Parse(ngay);
                    ddh.User_id = iduser;
                    ddh.TypePayment = "MoMo";
                    ddh.TotalBill = listGioHang.Sum(n => n.Total); ;
                    ddh.TotalQuantity = listGioHang.Sum(n => n.Quantity);
                    ddh.DeliveryStatus_id = 1;
                    db.Bills.Add(ddh);
                    for (var i = 0; i < listGioHang.Count(); i++)
                    {
                        BillDetail ct = new BillDetail();

                        ct.Bill_id = ddh.ID;

                        ct.Product_id = listGioHang[i].ID;

                        ct.Quantity = listGioHang[i].Quantity;
                        ct.Price = listGioHang[i].Price;
                        db.BillDetails.Add(ct);

                        if (ct != null)
                        {
                            Product spsl = db.Products.SingleOrDefault(n => n.ID == ct.Product_id);
                            spsl.Remaining--;
                            spsl.Sales++;
                        }



                    }
                    db.SaveChanges();
                    Session["GioHang"] = null;
                    Session["orderid"] = null;
                    ViewBag.message = "Đặt hàng và thanh toán thành công";
                    ViewBag.check = "success";
                    return RedirectToAction("Checkout", "Product");
                }

                }
            return View();
        }
           
        

        public ActionResult PaymentConfirm()
        {
            if (Request.QueryString.Count > 0)
            {
                string hashSecret = ConfigurationManager.AppSettings["HashSecret"]; //Chuỗi bí mật
                var vnpayData = Request.QueryString;
                PayLib pay = new PayLib();

                //lấy toàn bộ dữ liệu được trả về
                foreach (string s in vnpayData)
                {
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("vnp_"))
                    {
                        pay.AddResponseData(s, vnpayData[s]);
                    }
                }

                long orderId = Convert.ToInt64(pay.GetResponseData("vnp_TxnRef")); //mã hóa đơn
                long vnpayTranId = Convert.ToInt64(pay.GetResponseData("vnp_TransactionNo")); //mã giao dịch tại hệ thống VNPAY
                string vnp_ResponseCode = pay.GetResponseData("vnp_ResponseCode"); //response code: 00 - thành công, khác 00 - xem thêm https://sandbox.vnpayment.vn/apis/docs/bang-ma-loi/
                string vnp_SecureHash = Request.QueryString["vnp_SecureHash"]; //hash của dữ liệu trả về

                bool checkSignature = pay.ValidateSignature(vnp_SecureHash, hashSecret); //check chữ ký đúng hay không?

                if (checkSignature)
                {
                    if (vnp_ResponseCode == "00")
                    {
                        //Thanh toán thành công
                        List<GioHang> listGioHang = (List<GioHang>)Session["GioHang"];
                      
                        int iduser = (int)Session["idKH"];
                        if (listGioHang == null)
                        {
                            return null;
                        }
                        else
                        {
                            Bill ddh = new Bill();
                            ddh.IDDelivery = DateTime.Now.Ticks.ToString();

                            ddh.TypePayment = "VNPay";
                            ddh.DeliveryStatus_id = 1;
                            string ngay = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                            ddh.Created_at = DateTime.Parse(ngay);
                            ddh.User_id = iduser;
                            ddh.TotalBill = listGioHang.Sum(n => n.Total); ;
                            ddh.TotalQuantity = listGioHang.Sum(n => n.Quantity);
                            db.Bills.Add(ddh);
                            for (var i = 0; i < listGioHang.Count(); i++)
                            {
                                BillDetail ct = new BillDetail();

                                ct.Bill_id = ddh.ID;

                                ct.Product_id = listGioHang[i].ID;

                                ct.Quantity = listGioHang[i].Quantity;
                                ct.Price = listGioHang[i].Price;
                                db.BillDetails.Add(ct);

                                if (ct != null)
                                {
                                    Product spsl = db.Products.SingleOrDefault(n => n.ID == ct.Product_id);
                                    spsl.Remaining--;
                                    spsl.Sales++;
                                }



                            }

                            db.SaveChanges();
                            Session["GioHang"] = null;

                            ViewBag.Message = "Thanh toán thành công hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId;
                            ViewBag.check = "success";
                            return RedirectToAction("Checkout", "Product");
                        }
                        }

                    }
                    else
                    {
                        //Thanh toán không thành công. Mã lỗi: vnp_ResponseCode
                        ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý hóa đơn " + orderId + " | Mã giao dịch: " + vnpayTranId + " | Mã lỗi: " + vnp_ResponseCode;
                    ViewBag.check = "fail";
                }
                }
                else
                {
                    ViewBag.Message = "Có lỗi xảy ra trong quá trình xử lý";
                ViewBag.check = "fail";
            }
            return View();
        }

            
        
    }
}