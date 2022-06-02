using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanNuoc.DAL;
using WebBanNuoc.Models.Home;

namespace WebBanNuoc.Controllers
{
    public class HomeController : Controller
    {
        DrinksStoreEntities1 ctx = new DrinksStoreEntities1();
        public ActionResult Index(string search, int? page)
        {
            HomeIndexViewModel model = new HomeIndexViewModel();
            return View(model.CreateModel(search,4,page));
        }

        public ActionResult Checkout()
        {
            return View();
        }

        public ActionResult CheckoutDetails()
        {
            return View();
        }
        public ActionResult AddToCart(int productId, string url)
        {
            if (Session["cart"] == null)
            {
                List<Item> cart = new List<Item>();
                var product = ctx.Tbl_Product.Find(productId);
                cart.Add(new Item()
                {
                    Product = product,
                    Quantity = 1
                });
                Session["cart"] = cart;
            }
            else
            {
                List<Item> cart = (List<Item>)Session["cart"];
                var count = cart.Count();
                var product = ctx.Tbl_Product.Find(productId);
                for (int i = 0; i < count; i++)
                {
                    if (cart[i].Product.ProductId == productId)
                    {
                        int prevQty = cart[i].Quantity;
                        cart.Remove(cart[i]);
                        cart.Add(new Item()
                        {
                            Product = product,
                            Quantity = prevQty + 1
                        });
                        break;
                    }
                    else
                    {
                        var prd = cart.Where(x => x.Product.ProductId == productId).SingleOrDefault();
                        if (prd == null)
                        {
                            cart.Add(new Item()
                            {
                                Product = product,
                                Quantity = 1
                            });
                        }
                    }
                }
                Session["cart"] = cart;
            }
            return Redirect(url);
        }

        public ActionResult DecreaseQty(int productId, string url)
        {
            if (Session["cart"] != null)
            {
                List<Item> cart = (List<Item>)Session["cart"];
                var count = cart.Count();
                var product = ctx.Tbl_Product.Find(productId);
                for (int i = 0; i < count; i++)
                {
                    if (cart[i].Product.ProductId == productId)
                    {
                        if (cart[i].Quantity > 1)
                        {
                            int prevQty = cart[i].Quantity;
                            cart.Remove(cart[i]);
                            cart.Add(new Item()
                            {
                                Product = product,
                                Quantity = prevQty - 1
                            });
                            break;
                        }
                        else
                        {
                            cart.Remove(cart[i]);
                            break;
                        }
                    }
                    
                }
                Session["cart"] = cart;
            }
            return Redirect(url);
        }
        
        public int SumProduct()
        {
            int s = 0;
            List<Item> lstItem = Session["cart"] as List<Item>;
            if(lstItem != null)
            {
                s = lstItem.Count;
            }
            return s;
        }

        public ActionResult RemoveFromCart(int productId)
        {
            List<Item> cart = (List<Item>)Session["cart"];
            Item item = cart.SingleOrDefault(n => n.Product.ProductId == productId);
            if(item != null)
            {
                cart.RemoveAll(n => n.Product.ProductId == productId);
                 return RedirectToAction("Index");
            }
            
            return RedirectToAction("Index");

        }

        public ActionResult Remove(int productId)
        {
            List<Item> cart = (List<Item>)Session["cart"];
            Item item = cart.SingleOrDefault(n => n.Product.ProductId == productId);
            if (item != null)
            {
                cart.RemoveAll(n => n.Product.ProductId == productId);
                return RedirectToAction("Checkout");
            }
            cart = null;
            return RedirectToAction("Checkout");

        }

        [HttpGet]
        public ActionResult Payment()
        {
            if(Session["UserName"] == null || Session["UserName"].ToString()== "")
            {
                return RedirectToAction("TestLogin", "AccountTest");
            }
            if(Session["cart"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            List<Item> cart = (List<Item>)Session["cart"];
            return View(cart);
        }

        public ActionResult Payment(FormCollection collection)
        {
            Shipping dh = new Shipping();
            Tbl_Members kh = (Tbl_Members)Session["Account"];
            Tbl_Product p = new Tbl_Product();

            List<Item> cart = (List<Item>)Session["cart"];
            var deliverydate = String.Format("{0:MM/dd/yyyy}", collection["DeliveryDate"]);

            dh.MemberId = kh.MemberId;
            dh.OrderDate = DateTime.Now;
            dh.DeliveryDate = DateTime.Parse(deliverydate);
            dh.Ship = false;
            dh.Payment = false;
        
            ctx.Shippings.Add(dh);
            ctx.SaveChanges();

            foreach(var item in cart)
            {
                ShippingDetail shippingDetail = new ShippingDetail();
                shippingDetail.ShippingDetailId = dh.ShippingDetailId;
                shippingDetail.ProductId = item.Product.ProductId;
                shippingDetail.Quantity = item.Quantity;
                p = ctx.Tbl_Product.Single(n => n.ProductId == item.Product.ProductId);
                p.Quantity -= shippingDetail.Quantity;
                ctx.SaveChanges();

                ctx.ShippingDetails.Add(shippingDetail);
            }
            ctx.SaveChanges();
            Session["cart"] = null;
            
            return RedirectToAction("ConfirmOrder", "Home");
            
        }

        public ActionResult ConfirmOrder()
        {
            return View();
        }
    }
}
