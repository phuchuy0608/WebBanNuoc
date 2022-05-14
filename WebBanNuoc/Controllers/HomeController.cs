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
            cart = null;
            return RedirectToAction("Index");

        }
    }
}
