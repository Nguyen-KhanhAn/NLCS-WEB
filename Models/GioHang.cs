using Demo_Hangfire.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace WebsiteBanHang.Models
{
    public class GioHang
    {

        public string Name { get; set; } 
        public int ID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public decimal Total { get; set; }

        public GioHang (int MaSP)
        {
            using (AnNguyenEntities db = new AnNguyenEntities()) 
            {
                
                this.ID = MaSP;
                Product sp = db.Products.Single(n => n.ID == MaSP);
                this.Name = sp.Name;
                this.Image = sp.Image1;
                this.Price = (decimal)sp.Price.Value;
                this.Quantity = 1;
                this.Total = Price * Quantity;
            }

            



        }

        public GioHang()
        {

        }





        }
    
}